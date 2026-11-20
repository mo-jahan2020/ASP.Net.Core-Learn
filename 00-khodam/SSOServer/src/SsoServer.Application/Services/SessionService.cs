using Microsoft.Extensions.Logging;
using SsoServer.Domain.Enums;
using SsoServer.Domain.Interfaces;
using SsoServer.Domain.Models;

namespace SsoServer.Application.Services;

/// <summary>Session management: self-service for users, full control for admins.</summary>
public sealed class SessionService : ISessionService
{
    private readonly ISessionTokenStore _store;
    private readonly IAuditService _audit;
    private readonly ILogger<SessionService> _logger;

    public SessionService(ISessionTokenStore store, IAuditService audit, ILogger<SessionService> logger)
    {
        _store = store;
        _audit = audit;
        _logger = logger;
    }

    public Task<IReadOnlyList<ActiveSessionDto>> ListOwnAsync(string userId, CancellationToken ct = default)
        => _store.ListByUserAsync(userId, ct);

    /// فلو ۷ — ابطال یک نشست توسط خود کاربر.
    /// ردیف authorization در store باطل و رویداد SessionRevoked ثبت می‌شود.
    public async Task<ServiceResult> RevokeOwnAsync(string userId, string authorizationId, CancellationToken ct = default)
    {
        var session = await _store.FindAsync(authorizationId, ct);
        if (session is null || !string.Equals(session.Subject, userId, StringComparison.Ordinal))
        {
            return ServiceResult.Fail(PersianMessages.SessionNotFound);
        }

        var result = await _store.RevokeAsync(authorizationId, ct);
        if (!result.Succeeded)
        {
            return result;
        }

        await _audit.LogAsync(AuditAction.SessionRevoked, userId, userId, clientId: session.ClientId, ct: ct);
        return ServiceResult.Ok();
    }

    /// فلو ۷ — ابطال همه نشست‌های کاربر (دستگاه‌های دیگر).
    public async Task<ServiceResult> RevokeAllOwnAsync(string userId, CancellationToken ct = default)
    {
        var result = await _store.RevokeAllByUserAsync(userId, ct);
        if (!result.Succeeded)
        {
            return result;
        }

        await _audit.LogAsync(AuditAction.SessionRevoked, userId, userId, data: new { Scope = "all" }, ct: ct);
        return ServiceResult.Ok();
    }

    public Task<IReadOnlyList<ActiveSessionDto>> ListForUserAsync(string targetUserId, CancellationToken ct = default)
        => _store.ListByUserAsync(targetUserId, ct);

    public async Task<ServiceResult> RevokeForUserAsync(string targetUserId, string authorizationId, string actorUserId, CancellationToken ct = default)
    {
        var session = await _store.FindAsync(authorizationId, ct);
        if (session is null || !string.Equals(session.Subject, targetUserId, StringComparison.Ordinal))
        {
            return ServiceResult.Fail(PersianMessages.SessionNotFound);
        }

        var result = await _store.RevokeAsync(authorizationId, ct);
        if (!result.Succeeded)
        {
            return result;
        }

        await _audit.LogAsync(AuditAction.SessionRevoked, actorUserId, actorUserId, clientId: session.ClientId, data: new { targetUserId }, ct: ct);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> RevokeAllForUserAsync(string targetUserId, string actorUserId, CancellationToken ct = default)
    {
        var result = await _store.RevokeAllByUserAsync(targetUserId, ct);
        if (!result.Succeeded)
        {
            return result;
        }

        await _audit.LogAsync(AuditAction.SessionRevoked, actorUserId, actorUserId, data: new { targetUserId, Scope = "all" }, ct: ct);
        return ServiceResult.Ok();
    }

    /// <summary>فهرست همه نشست‌های فعال سامانه — فقط برای صفحه «مدیریت نشست‌های فعال» ادمین.</summary>
    public Task<IReadOnlyList<ActiveSessionDto>> ListAllAsync(CancellationToken ct = default)
        => _store.ListAllAsync(ct);

    /// <summary>
    /// خاتمه هر نشست توسط ادمین (بدون محدودیت مالکیت).
    /// رویداد SessionRevoked با اطلاعات کاربر هدف و کلاینت ثبت می‌شود.
    /// </summary>
    public async Task<ServiceResult> RevokeAnyAsync(string authorizationId, string actorUserId, CancellationToken ct = default)
    {
        var session = await _store.FindAsync(authorizationId, ct);
        if (session is null)
        {
            return ServiceResult.Fail(PersianMessages.SessionNotFound);
        }

        var result = await _store.RevokeAsync(authorizationId, ct);
        if (!result.Succeeded)
        {
            return result;
        }

        await _audit.LogAsync(AuditAction.SessionRevoked, actorUserId, actorUserId,
            clientId: session.ClientId, data: new { targetUserId = session.Subject, via = "manage-sessions" }, ct: ct);
        return ServiceResult.Ok();
    }
}
