using SsoServer.Domain.Interfaces;

namespace SsoServer.Infrastructure.Persistence;

/// <summary>
/// <see cref="IClientAccessStore"/> implementation — the single source of truth for the
/// SSO-flow access rule over the per-user whitelist (<see cref="IClientUserAssignmentRepository"/>):
/// a user with no assignment rows may access every enabled client (open access);
/// a user with at least one row may only access the assigned clients.
/// </summary>
public sealed class ClientAccessStore : IClientAccessStore
{
    private readonly IClientUserAssignmentRepository _assignments;

    public ClientAccessStore(IClientUserAssignmentRepository assignments)
    {
        _assignments = assignments;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<string>> GetAllowedClientIdsAsync(string userId, CancellationToken ct = default)
        => _assignments.ListClientIdsByUserAsync(userId, ct);

    /// <inheritdoc />
    public async Task<bool> IsRestrictedAsync(string userId, CancellationToken ct = default)
        => (await _assignments.ListClientIdsByUserAsync(userId, ct)).Count > 0;

    /// <inheritdoc />
    /// فلو ۳ گام ۳ و فلو ۴ گام ۵ — چک لیست‌سفید دسترسی کاربر به کلاینت.
    /// در هر دو مسیر تعاملی (authorize) و مستقیم (password grant) رعایت می‌شود.
    public async Task<bool> CanAccessAsync(string userId, string clientId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(clientId))
        {
            return false;
        }

        var allowed = await _assignments.ListClientIdsByUserAsync(userId, ct);
        return allowed.Count == 0 || allowed.Contains(clientId);
    }
}
