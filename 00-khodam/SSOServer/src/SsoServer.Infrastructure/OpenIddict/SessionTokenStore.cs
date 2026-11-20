using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenIddict.EntityFrameworkCore.Models;
using SsoServer.Domain.Entities;
using SsoServer.Domain.Interfaces;
using SsoServer.Domain.Models;
using SsoServer.Infrastructure.Persistence;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SsoServer.Infrastructure.OpenIddict;

/// <summary>
/// Maps OpenIddict authorizations/tokens to human-readable "active sessions" and revokes them.
/// Revocation sets both the authorization and all of its tokens to the rejected status,
/// which immediately invalidates refresh tokens and (with entry validation enabled on the
/// API) access tokens as well.
/// </summary>
public sealed class SessionTokenStore : ISessionTokenStore
{
    private readonly AppDbContext _context;
    private readonly ILogger<SessionTokenStore> _logger;

    public SessionTokenStore(AppDbContext context, ILogger<SessionTokenStore> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ActiveSessionDto>> ListByUserAsync(string userId, CancellationToken ct = default)
    {
        var authorizations = await Authorizations()
            .Where(a => a.Subject == userId && a.Status == Statuses.Valid)
            .OrderByDescending(a => a.CreationDate)
            .ToListAsync(ct);

        if (authorizations.Count == 0)
        {
            return [];
        }

        var applicationIds = authorizations.Where(a => a.Application != null).Select(a => a.Application!.Id!).Distinct().ToList();
        var applications = await Applications()
            .Where(a => applicationIds.Contains(a.Id!))
            .ToDictionaryAsync(a => a.Id!, a => a.ClientId ?? "?", ct);

        var authorizationIds = authorizations.Select(a => a.Id!).ToList();
        var tokens = await Tokens()
            .Where(t => t.Authorization != null && authorizationIds.Contains(t.Authorization.Id!))
            .ToListAsync(ct);

        var result = new List<ActiveSessionDto>(authorizations.Count);
        foreach (var authorization in authorizations)
        {
            var authorizationId = authorization.Id!;
            var sessionTokens = tokens.Where(t => t.Authorization?.Id == authorizationId).ToList();
            var clientId = authorization.Application is null
                ? applications.GetValueOrDefault(authorizationId, "?")
                : applications.GetValueOrDefault(authorization.Application.Id!, "?");

            result.Add(new ActiveSessionDto
            {
                AuthorizationId = authorizationId,
                Subject = authorization.Subject ?? userId,
                ClientId = clientId,
                ClientDisplayName = clientId,
                Type = authorization.Type ?? "ad-hoc",
                CreatedAt = ToUtc(authorization.CreationDate) ?? DateTimeOffset.UtcNow,
                LastUsedAt = sessionTokens
                    .Select(t => ToUtc(t.CreationDate))
                    .Where(t => t.HasValue)
                    .DefaultIfEmpty(ToUtc(authorization.CreationDate))
                    .Max(),
                ExpiresAt = sessionTokens
                    .Where(t => t.Status == Statuses.Valid && t.Type == TokenTypeHints.RefreshToken)
                    .Select(t => ToUtc(t.ExpirationDate))
                    .Where(t => t.HasValue)
                    .DefaultIfEmpty(null)
                    .Max(),
                Scopes = ParseScopes(authorization.Scopes),
            });
        }

        return result;
    }

    public async Task<ActiveSessionDto?> FindAsync(string authorizationId, CancellationToken ct = default)
    {
        var authorization = await Authorizations()
            .Include(a => a.Application)
            .FirstOrDefaultAsync(a => a.Id == authorizationId, ct);

        if (authorization is null)
        {
            return null;
        }

        return new ActiveSessionDto
        {
            AuthorizationId = authorization.Id!,
            Subject = authorization.Subject ?? string.Empty,
            ClientId = authorization.Application?.ClientId ?? "?",
            ClientDisplayName = authorization.Application?.ClientId ?? "?",
            Type = authorization.Type ?? "ad-hoc",
            CreatedAt = ToUtc(authorization.CreationDate) ?? DateTimeOffset.UtcNow,
            Scopes = ParseScopes(authorization.Scopes),
        };
    }

    /// <summary>
    /// فهرست تمام نشست‌های فعال سامانه (همه کاربران) — برای صفحه «مدیریت نشست‌های فعال» ادمین.
    /// هر authorization معتبر یک نشست است؛ نام کاربری صاحب نشست از جدول کاربران پیوند می‌شود.
    /// </summary>
    public async Task<IReadOnlyList<ActiveSessionDto>> ListAllAsync(CancellationToken ct = default)
    {
        var authorizations = await Authorizations()
            .Where(a => a.Status == Statuses.Valid)
            .OrderByDescending(a => a.CreationDate)
            .ToListAsync(ct);

        if (authorizations.Count == 0)
        {
            return [];
        }

        // نگاشت ApplicationId → ClientId برای همه نشست‌ها
        var applicationIds = authorizations.Where(a => a.Application != null).Select(a => a.Application!.Id!).Distinct().ToList();
        var applications = await Applications()
            .Where(a => applicationIds.Contains(a.Id!))
            .ToDictionaryAsync(a => a.Id!, a => a.ClientId ?? "?", ct);

        // نگاشت Subject (شناسه کاربر) → نام کاربری برای نمایش در ستون «کاربر»
        var subjects = authorizations.Select(a => a.Subject).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();
        var userNames = await _context.Users
            .Where(u => subjects.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.UserName ?? u.Id, ct);

        var authorizationIds = authorizations.Select(a => a.Id!).ToList();
        var tokens = await Tokens()
            .Where(t => t.Authorization != null && authorizationIds.Contains(t.Authorization.Id!))
            .ToListAsync(ct);

        var result = new List<ActiveSessionDto>(authorizations.Count);
        foreach (var authorization in authorizations)
        {
            var authorizationId = authorization.Id!;
            var sessionTokens = tokens.Where(t => t.Authorization?.Id == authorizationId).ToList();
            var clientId = applications.GetValueOrDefault(authorization.Application?.Id ?? string.Empty, "?");

            result.Add(new ActiveSessionDto
            {
                AuthorizationId = authorizationId,
                Subject = authorization.Subject ?? string.Empty,
                UserName = userNames.GetValueOrDefault(authorization.Subject ?? string.Empty, authorization.Subject ?? "?"),
                ClientId = clientId,
                ClientDisplayName = clientId,
                Type = authorization.Type ?? "ad-hoc",
                CreatedAt = ToUtc(authorization.CreationDate) ?? DateTimeOffset.UtcNow,
                LastUsedAt = sessionTokens
                    .Select(t => ToUtc(t.CreationDate))
                    .Where(t => t.HasValue)
                    .DefaultIfEmpty(ToUtc(authorization.CreationDate))
                    .Max(),
                ExpiresAt = sessionTokens
                    .Where(t => t.Status == Statuses.Valid && t.Type == TokenTypeHints.RefreshToken)
                    .Select(t => ToUtc(t.ExpirationDate))
                    .Where(t => t.HasValue)
                    .DefaultIfEmpty(null)
                    .Max(),
                Scopes = ParseScopes(authorization.Scopes),
            });
        }

        return result;
    }

    public async Task<ServiceResult> RevokeAsync(string authorizationId, CancellationToken ct = default)
    {
        var authorization = await Authorizations().FirstOrDefaultAsync(a => a.Id == authorizationId, ct);
        if (authorization is null)
        {
            return ServiceResult.Fail("نشست یافت نشد.");
        }

        authorization.Status = Statuses.Rejected;
        var tokens = await Tokens().Where(t => t.Authorization != null && t.Authorization.Id == authorizationId).ToListAsync(ct);
        foreach (var token in tokens)
        {
            token.Status = Statuses.Rejected;
        }

        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Revoked authorization {AuthorizationId} ({TokenCount} tokens)", authorizationId, tokens.Count);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> RevokeAllByUserAsync(string userId, CancellationToken ct = default)
    {
        var authorizations = await Authorizations().Where(a => a.Subject == userId).ToListAsync(ct);
        var authorizationIds = authorizations.Select(a => a.Id!).ToList();

        foreach (var authorization in authorizations)
        {
            authorization.Status = Statuses.Rejected;
        }

        var tokens = await Tokens()
            .Where(t => t.Authorization != null && authorizationIds.Contains(t.Authorization.Id!))
            .ToListAsync(ct);

        foreach (var token in tokens)
        {
            token.Status = Statuses.Rejected;
        }

        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Revoked {AuthorizationCount} authorizations ({TokenCount} tokens) for user {UserId}",
            authorizations.Count, tokens.Count, userId);
        return ServiceResult.Ok();
    }

    private static DateTimeOffset? ToUtc(DateTime? value)
        => value.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)) : null;

    private static string[] ParseScopes(string? scopes)
        => string.IsNullOrWhiteSpace(scopes) ? [] : [.. scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];

    private DbSet<OpenIddictEntityFrameworkCoreAuthorization> Authorizations() => _context.Set<OpenIddictEntityFrameworkCoreAuthorization>();

    private DbSet<OpenIddictEntityFrameworkCoreApplication> Applications() => _context.Set<OpenIddictEntityFrameworkCoreApplication>();

    private DbSet<OpenIddictEntityFrameworkCoreToken> Tokens() => _context.Set<OpenIddictEntityFrameworkCoreToken>();
}
