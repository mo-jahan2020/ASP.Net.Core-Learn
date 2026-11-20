using SsoServer.Domain.Entities;
using SsoServer.Domain.Enums;
using SsoServer.Domain.Interfaces;
using SsoServer.Domain.Models;

namespace SsoServer.Application.Services;

/// <summary>
/// Admin use-cases over the per-user client access whitelist
/// (<see cref="ClientUserAssignment"/>): which users may sign in to which apps.
/// </summary>
public sealed class ClientAssignmentService
{
    private readonly IClientUserAssignmentRepository _assignments;
    private readonly IAuditService _audit;

    public ClientAssignmentService(IClientUserAssignmentRepository assignments, IAuditService audit)
    {
        _assignments = assignments;
        _audit = audit;
    }

    /// <summary>Client ids assigned to a user; empty = no restriction (all enabled clients).</summary>
    public Task<IReadOnlyList<string>> ListClientIdsByUserAsync(string userId, CancellationToken ct = default)
        => _assignments.ListClientIdsByUserAsync(userId, ct);

    public Task<IReadOnlyList<ClientUserAssignment>> ListByClientAsync(string clientId, CancellationToken ct = default)
        => _assignments.ListByClientAsync(clientId, ct);

    public async Task<ServiceResult> AssignAsync(string clientId, string userId, string actorUserId, CancellationToken ct = default)
    {
        var result = await _assignments.AssignAsync(userId, clientId, actorUserId, ct);
        if (result.Succeeded)
        {
            await _audit.LogAsync(AuditAction.ClientAssigned, actorUserId, actorUserId, clientId,
                data: new { userId }, ct: ct);
        }

        return result;
    }

    public async Task<ServiceResult> UnassignAsync(string clientId, string userId, string actorUserId, CancellationToken ct = default)
    {
        var result = await _assignments.UnassignAsync(userId, clientId, ct);
        if (result.Succeeded)
        {
            await _audit.LogAsync(AuditAction.ClientUnassigned, actorUserId, actorUserId, clientId,
                data: new { userId }, ct: ct);
        }

        return result;
    }

    /// <summary>
    /// Replaces the user's whitelist with <paramref name="selectedClientIds"/>:
    /// newly ticked clients are assigned, unticked ones are removed.
    /// An empty selection clears every row (= no restriction, all enabled clients).
    /// </summary>
    public async Task<ServiceResult> SyncUserAssignmentsAsync(
        string userId, IReadOnlyCollection<string> selectedClientIds, string actorUserId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return ServiceResult.Fail("شناسه کاربر الزامی است.");
        }

        var current = await _assignments.ListClientIdsByUserAsync(userId, ct);
        var selected = selectedClientIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var errors = new List<string>();
        foreach (var clientId in selected.Except(current, StringComparer.Ordinal))
        {
            var result = await AssignAsync(clientId, userId, actorUserId, ct);
            if (!result.Succeeded)
            {
                errors.AddRange(result.Errors);
            }
        }

        foreach (var clientId in current.Except(selected, StringComparer.Ordinal))
        {
            var result = await UnassignAsync(clientId, userId, actorUserId, ct);
            if (!result.Succeeded)
            {
                errors.AddRange(result.Errors);
            }
        }

        return errors.Count == 0 ? ServiceResult.Ok() : ServiceResult.Fail(errors);
    }
}
