using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SsoServer.Domain.Entities;
using SsoServer.Domain.Enums;
using SsoServer.Domain.Interfaces;
using SsoServer.Domain.Models;
using SsoServer.Domain.Options;

namespace SsoServer.Application.Services;

/// <summary>Admin use-cases over users and their claims (no self-registration anywhere).</summary>
public sealed class UserAdminService : IUserAdminService
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly ISessionTokenStore _sessions;
    private readonly IAuditService _audit;
    private readonly ILogger<UserAdminService> _logger;

    public UserAdminService(
        UserManager<ApplicationUser> users,
        ISessionTokenStore sessions,
        IAuditService audit,
        ILogger<UserAdminService> logger)
    {
        _users = users;
        _sessions = sessions;
        _audit = audit;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UserSummaryDto>> ListAsync(CancellationToken ct = default)
    {
        // Note: ordering happens client-side because SQLite (used by the integration tests)
        // cannot translate ORDER BY over DateTimeOffset columns; SQL Server handles both.
        var users = _users.Users.Take(500).ToList().OrderByDescending(u => u.CreatedAt).ToList();
        var result = new List<UserSummaryDto>(users.Count);
        foreach (var user in users)
        {
            result.Add(new UserSummaryDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                DisplayName = user.DisplayName,
                Email = user.Email ?? string.Empty,
                IsActive = user.IsActive,
                IsAdUser = user.IsAdUser,
                Roles = [.. await _users.GetRolesAsync(user)],
                CreatedAt = user.CreatedAt,
            });
        }

        return result;
    }

    public async Task<UserDetailDto?> GetAsync(string id, CancellationToken ct = default)
    {
        var user = await _users.FindByIdAsync(id);
        if (user is null)
        {
            return null;
        }

        return new UserDetailDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            DisplayName = user.DisplayName,
            Email = user.Email ?? string.Empty,
            IsActive = user.IsActive,
            IsAdUser = user.IsAdUser,
            Roles = [.. await _users.GetRolesAsync(user)],
            CreatedAt = user.CreatedAt,
            Claims = [.. (await _users.GetClaimsAsync(user)).Select(c => new ClaimDto { Type = c.Type, Value = c.Value })],
        };
    }

    public async Task<ServiceResult<UserSummaryDto>> CreateAsync(CreateUserDto dto, string actorUserId, CancellationToken ct = default)
    {
        if (!PersianMessages.AllowedRoles.Contains(dto.Role, StringComparer.Ordinal))
        {
            return ServiceResult<UserSummaryDto>.Fail(PersianMessages.InvalidRole);
        }

        var user = new ApplicationUser
        {
            UserName = dto.UserName.Trim(),
            DisplayName = dto.DisplayName.Trim(),
            Email = dto.Email.Trim(),
            EmailConfirmed = true,
            IsActive = dto.IsActive,
            IsAdUser = dto.IsAdUser,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        // AD users authenticate against the domain: no local password is stored.
        // Local users always need a password (enforced here for API/other callers too).
        IdentityResult result;
        if (dto.IsAdUser)
        {
            result = await _users.CreateAsync(user);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                return ServiceResult<UserSummaryDto>.Fail("رمز عبور برای کاربران غیر اکتیو دایرکتوری الزامی است.");
            }

            result = await _users.CreateAsync(user, dto.Password);
        }

        if (!result.Succeeded)
        {
            return ServiceResult<UserSummaryDto>.Fail(PersianIdentityErrors.Map(result.Errors));
        }

        await _users.AddToRoleAsync(user, dto.Role);
        await _audit.LogAsync(AuditAction.UserCreated, actorUserId, actorUserId, data: new { user.Id, user.UserName, dto.Role, dto.IsAdUser }, ct: ct);

        return ServiceResult<UserSummaryDto>.Ok(new UserSummaryDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            DisplayName = user.DisplayName,
            Email = user.Email ?? string.Empty,
            IsActive = user.IsActive,
            IsAdUser = user.IsAdUser,
            Roles = [dto.Role],
            CreatedAt = user.CreatedAt,
        });
    }

    public async Task<ServiceResult> UpdateAsync(string id, UpdateUserDto dto, string actorUserId, CancellationToken ct = default)
    {
        var user = await _users.FindByIdAsync(id);
        if (user is null)
        {
            return ServiceResult.Fail(PersianMessages.UserNotFound);
        }

        if (!PersianMessages.AllowedRoles.Contains(dto.Role, StringComparer.Ordinal))
        {
            return ServiceResult.Fail(PersianMessages.InvalidRole);
        }

        if (string.Equals(id, actorUserId, StringComparison.Ordinal) &&
            !string.Equals(dto.Role, SsoRoles.Admin, StringComparison.Ordinal))
        {
            return ServiceResult.Fail(PersianMessages.CannotChangeOwnAccount);
        }

        user.DisplayName = dto.DisplayName.Trim();
        user.Email = dto.Email.Trim();
        user.IsActive = dto.IsActive;

        var wasAdUser = user.IsAdUser;
        user.IsAdUser = dto.IsAdUser;

        var result = await _users.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return ServiceResult.Fail(PersianIdentityErrors.Map(result.Errors));
        }

        // Local -> Active Directory: drop the local password so the account is domain-only.
        if (!wasAdUser && dto.IsAdUser && await _users.HasPasswordAsync(user))
        {
            await _users.RemovePasswordAsync(user);
        }

        var currentRoles = await _users.GetRolesAsync(user);
        if (!currentRoles.Contains(dto.Role, StringComparer.Ordinal))
        {
            await _users.RemoveFromRolesAsync(user, currentRoles);
            await _users.AddToRoleAsync(user, dto.Role);
        }

        await _audit.LogAsync(AuditAction.UserUpdated, actorUserId, actorUserId, data: new { user.Id, user.UserName, dto.Role, dto.IsActive, dto.IsAdUser }, ct: ct);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> SetPasswordAsync(string id, string newPassword, string actorUserId, CancellationToken ct = default)
    {
        var user = await _users.FindByIdAsync(id);
        if (user is null)
        {
            return ServiceResult.Fail(PersianMessages.UserNotFound);
        }

        if (user.IsAdUser)
        {
            return ServiceResult.Fail("این کاربر اکتیو دایرکتوری است و احراز هویت او از طریق دامنه انجام می‌شود؛ تعیین رمز عبور محلی مجاز نیست.");
        }

        await _users.RemovePasswordAsync(user);
        var result = await _users.AddPasswordAsync(user, newPassword);
        if (!result.Succeeded)
        {
            return ServiceResult.Fail(PersianIdentityErrors.Map(result.Errors));
        }

        await _audit.LogAsync(AuditAction.UserUpdated, actorUserId, actorUserId, data: new { user.Id, Action = "PasswordReset" }, ct: ct);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> SetStatusAsync(string id, bool isActive, string actorUserId, CancellationToken ct = default)
    {
        var user = await _users.FindByIdAsync(id);
        if (user is null)
        {
            return ServiceResult.Fail(PersianMessages.UserNotFound);
        }

        if (string.Equals(id, actorUserId, StringComparison.Ordinal) && !isActive)
        {
            return ServiceResult.Fail(PersianMessages.CannotChangeOwnAccount);
        }

        user.IsActive = isActive;
        var result = await _users.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return ServiceResult.Fail(PersianIdentityErrors.Map(result.Errors));
        }

        if (!isActive)
        {
            await _sessions.RevokeAllByUserAsync(id);
        }

        await _audit.LogAsync(AuditAction.UserUpdated, actorUserId, actorUserId, data: new { user.Id, Action = isActive ? "Activated" : "Deactivated" }, ct: ct);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> SetClaimsAsync(string id, IReadOnlyList<ClaimDto> claims, string actorUserId, CancellationToken ct = default)
    {
        var user = await _users.FindByIdAsync(id);
        if (user is null)
        {
            return ServiceResult.Fail(PersianMessages.UserNotFound);
        }

        var reserved = new HashSet<string>(StringComparer.Ordinal) { "sub", "name", "preferred_username", "email", "role" };
        if (claims.Any(c => reserved.Contains(c.Type)))
        {
            return ServiceResult.Fail(PersianMessages.ClaimTypeReserved);
        }

        foreach (var existing in await _users.GetClaimsAsync(user))
        {
            await _users.RemoveClaimAsync(user, existing);
        }

        foreach (var claim in claims.Where(c => !string.IsNullOrWhiteSpace(c.Type)))
        {
            await _users.AddClaimAsync(user, new System.Security.Claims.Claim(claim.Type.Trim(), claim.Value));
        }

        await _audit.LogAsync(AuditAction.UserUpdated, actorUserId, actorUserId, data: new { user.Id, Action = "ClaimsUpdated", Count = claims.Count }, ct: ct);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(string id, string actorUserId, CancellationToken ct = default)
    {
        var user = await _users.FindByIdAsync(id);
        if (user is null)
        {
            return ServiceResult.Fail(PersianMessages.UserNotFound);
        }

        if (string.Equals(id, actorUserId, StringComparison.Ordinal))
        {
            return ServiceResult.Fail(PersianMessages.CannotChangeOwnAccount);
        }

        await _sessions.RevokeAllByUserAsync(id);
        var result = await _users.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return ServiceResult.Fail(PersianIdentityErrors.Map(result.Errors));
        }

        await _audit.LogAsync(AuditAction.UserDeleted, actorUserId, actorUserId, data: new { user.Id, user.UserName }, ct: ct);
        return ServiceResult.Ok();
    }
}
