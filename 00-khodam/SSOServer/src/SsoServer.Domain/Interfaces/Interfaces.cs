using System.Security.Cryptography.X509Certificates;
using SsoServer.Domain.Entities;
using SsoServer.Domain.Enums;
using SsoServer.Domain.Models;

namespace SsoServer.Domain.Interfaces;

/// <summary>Persists and reads <see cref="AppSetting"/> rows.</summary>
public interface ISettingsRepository
{
    Task<IReadOnlyDictionary<string, string>> GetAllAsync(CancellationToken ct = default);

    Task SetAsync(string key, string value, string updatedBy, CancellationToken ct = default);
}

/// <summary>
/// Hot-reloadable settings service: config defaults + DB overrides, cached with
/// invalidation on save. Exposes a synchronous <see cref="Current"/> snapshot for
/// middleware/options that cannot await.
/// </summary>
public interface ISettingsService
{
    /// <summary>Latest known effective settings (synchronous snapshot; refreshed on load/save).</summary>
    EffectiveSettings Current { get; }

    Task<EffectiveSettings> GetEffectiveAsync(CancellationToken ct = default);

    Task<ServiceResult> UpdateAsync(UpdateSettingsDto dto, string actorUserId, CancellationToken ct = default);

    /// <summary>Raised after settings were successfully persisted.</summary>
    event Action? SettingsChanged;
}

/// <summary>Audit trail service: persists security events.</summary>
public interface IAuditService
{
    Task LogAsync(AuditAction action, string? actorUserId = null, string? actorUserName = null,
        string? clientId = null, string? ip = null, object? data = null, CancellationToken ct = default);

    Task<IReadOnlyList<AuditLog>> QueryAsync(int skip, int take, AuditAction? action = null,
        string? userId = null, CancellationToken ct = default);
}

/// <summary>Writes audit rows to the persistence layer.</summary>
public interface IAuditRepository
{
    Task AddAsync(AuditLog entry, CancellationToken ct = default);

    Task<IReadOnlyList<AuditLog>> QueryAsync(int skip, int take, AuditAction? action, string? userId, CancellationToken ct = default);
}

/// <summary>Information about the JWT signing certificate.</summary>
public record SigningCertificateInfo(SigningKeyStatus Status, string Subject, DateTimeOffset NotAfter, string Source);

public enum SigningKeyStatus
{
    NotConfigured,
    Ephemeral,
    FileLoaded,
    Generated,
    NearExpiry,
}

/// <summary>Provides the X.509 certificate used to sign JWT tokens (RS256).</summary>
public interface ISigningCertificateProvider
{
    X509Certificate2? GetSigningCertificate();

    SigningCertificateInfo Describe();
}

/// <summary>Gateway over the OpenIddict application store + client profiles.</summary>
public interface IClientRepository
{
    Task<ClientDto?> FindAsync(string clientId, CancellationToken ct = default);

    Task<IReadOnlyList<ClientDto>> ListAsync(CancellationToken ct = default);

    Task<ServiceResult<string>> CreateAsync(SaveClientDto dto, string? clientSecret, CancellationToken ct = default);

    Task<ServiceResult> UpdateAsync(SaveClientDto dto, CancellationToken ct = default);

    Task<ServiceResult> DeleteAsync(string clientId, CancellationToken ct = default);

    /// <summary>Generates and persists a new secret for a confidential client; returns it once.</summary>
    Task<ServiceResult<string>> RegenerateSecretAsync(string clientId, CancellationToken ct = default);

    Task<bool> IsEnabledAsync(string clientId, CancellationToken ct = default);

    Task<IReadOnlyDictionary<string, string>> GetPersianDisplayNamesAsync(IReadOnlyCollection<string> clientIds, CancellationToken ct = default);
}

/// <summary>Reads and revokes OpenIddict authorizations/tokens as "active sessions".</summary>
public interface ISessionTokenStore
{
    Task<IReadOnlyList<ActiveSessionDto>> ListByUserAsync(string userId, CancellationToken ct = default);

    /// <summary>All active sessions across every user (admin session-management page).</summary>
    Task<IReadOnlyList<ActiveSessionDto>> ListAllAsync(CancellationToken ct = default);

    Task<ActiveSessionDto?> FindAsync(string authorizationId, CancellationToken ct = default);

    Task<ServiceResult> RevokeAsync(string authorizationId, CancellationToken ct = default);

    Task<ServiceResult> RevokeAllByUserAsync(string userId, CancellationToken ct = default);
}

/// <summary>User management use-cases (admin operations).</summary>
public interface IUserAdminService
{
    Task<IReadOnlyList<UserSummaryDto>> ListAsync(CancellationToken ct = default);

    Task<UserDetailDto?> GetAsync(string id, CancellationToken ct = default);

    Task<ServiceResult<UserSummaryDto>> CreateAsync(CreateUserDto dto, string actorUserId, CancellationToken ct = default);

    Task<ServiceResult> UpdateAsync(string id, UpdateUserDto dto, string actorUserId, CancellationToken ct = default);

    Task<ServiceResult> SetPasswordAsync(string id, string newPassword, string actorUserId, CancellationToken ct = default);

    Task<ServiceResult> SetStatusAsync(string id, bool isActive, string actorUserId, CancellationToken ct = default);

    Task<ServiceResult> SetClaimsAsync(string id, IReadOnlyList<ClaimDto> claims, string actorUserId, CancellationToken ct = default);

    Task<ServiceResult> DeleteAsync(string id, string actorUserId, CancellationToken ct = default);
}

/// <summary>
/// Per-user client access whitelist backed by <see cref="ClientUserAssignment"/> rows.
/// Empty assignment list for a user means: no restriction (all enabled clients).
/// </summary>
public interface IClientUserAssignmentRepository
{
    /// <summary>Client ids assigned to the user; empty means no restriction (all enabled clients).</summary>
    Task<IReadOnlyList<string>> ListClientIdsByUserAsync(string userId, CancellationToken ct = default);

    Task<IReadOnlyList<ClientUserAssignment>> ListByClientAsync(string clientId, CancellationToken ct = default);

    Task<bool> IsAssignedAsync(string userId, string clientId, CancellationToken ct = default);

    Task<ServiceResult> AssignAsync(string userId, string clientId, string assignedBy, CancellationToken ct = default);

    Task<ServiceResult> UnassignAsync(string userId, string clientId, CancellationToken ct = default);
}

/// <summary>
/// Runtime client-access checks for the SSO flow (authorize + token endpoints).
/// Single source of truth for the whitelist rule:
/// a user with no assignment rows may access every enabled client;
/// a user with at least one row may only access the assigned clients.
/// (Admins bypass the whitelist — checked by the caller before consulting the store.)
/// </summary>
public interface IClientAccessStore
{
    /// <summary>Client ids the user is allowed to sign in to; empty = no restriction (all enabled clients).</summary>
    Task<IReadOnlyList<string>> GetAllowedClientIdsAsync(string userId, CancellationToken ct = default);

    /// <summary>True when the user has at least one assignment row (i.e. is restricted to the assigned clients).</summary>
    Task<bool> IsRestrictedAsync(string userId, CancellationToken ct = default);

    /// <summary>
    /// May <paramref name="userId"/> sign in to <paramref name="clientId"/>?
    /// Returns true when the user is unrestricted (no rows) or has an explicit assignment.
    /// </summary>
    Task<bool> CanAccessAsync(string userId, string clientId, CancellationToken ct = default);
}

/// <summary>
/// Validates username/password pairs against Active Directory (LDAP bind) and reports
/// the account state (wrong password / disabled / locked / expired / unreachable)
/// as distinct outcomes with ready-to-display Persian messages.
/// </summary>
public interface IActiveDirectoryService
{
    /// <summary>
    /// Checks <paramref name="username"/> + <paramref name="password"/> against the
    /// configured Active Directory. Never throws for expected failures.
    /// </summary>
    Task<AdAuthResult> AuthenticateAsync(string username, string password, CancellationToken ct = default);
}

/// <summary>Self-service + admin session management.</summary>
public interface ISessionService
{
    Task<IReadOnlyList<ActiveSessionDto>> ListOwnAsync(string userId, CancellationToken ct = default);

    /// <summary>Revokes one of the user's own sessions. 404-like result when not owned.</summary>
    Task<ServiceResult> RevokeOwnAsync(string userId, string authorizationId, CancellationToken ct = default);

    Task<ServiceResult> RevokeAllOwnAsync(string userId, CancellationToken ct = default);

    Task<IReadOnlyList<ActiveSessionDto>> ListForUserAsync(string targetUserId, CancellationToken ct = default);

    Task<ServiceResult> RevokeForUserAsync(string targetUserId, string authorizationId, string actorUserId, CancellationToken ct = default);

    Task<ServiceResult> RevokeAllForUserAsync(string targetUserId, string actorUserId, CancellationToken ct = default);

    /// <summary>All active sessions of every user (admin-only page «مدیریت نشست‌های فعال»).</summary>
    Task<IReadOnlyList<ActiveSessionDto>> ListAllAsync(CancellationToken ct = default);

    /// <summary>Admin terminates any session by authorization id.</summary>
    Task<ServiceResult> RevokeAnyAsync(string authorizationId, string actorUserId, CancellationToken ct = default);
}
