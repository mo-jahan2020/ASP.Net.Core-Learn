using Microsoft.EntityFrameworkCore;
using SsoServer.Domain.Entities;
using SsoServer.Domain.Interfaces;
using SsoServer.Domain.Models;

namespace SsoServer.Infrastructure.Persistence;

/// <summary>
/// <see cref="IClientUserAssignmentRepository"/> implementation over <see cref="AppDbContext"/>.
/// </summary>
public sealed class ClientUserAssignmentRepository : IClientUserAssignmentRepository
{
    private readonly AppDbContext _context;

    public ClientUserAssignmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<string>> ListClientIdsByUserAsync(string userId, CancellationToken ct = default)
        => await _context.ClientUserAssignments
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .Select(a => a.ClientId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ClientUserAssignment>> ListByClientAsync(string clientId, CancellationToken ct = default)
    {
        // Note: ordering happens client-side because SQLite (used by the integration tests)
        // cannot translate ORDER BY over DateTimeOffset columns; SQL Server handles both.
        var rows = await _context.ClientUserAssignments
            .AsNoTracking()
            .Where(a => a.ClientId == clientId)
            .ToListAsync(ct);

        return rows.OrderBy(a => a.AssignedAt).ToList();
    }

    public Task<bool> IsAssignedAsync(string userId, string clientId, CancellationToken ct = default)
        => _context.ClientUserAssignments.AnyAsync(a => a.UserId == userId && a.ClientId == clientId, ct);

    public async Task<ServiceResult> AssignAsync(string userId, string clientId, string assignedBy, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(clientId))
        {
            return ServiceResult.Fail("شناسه کاربر و کلاینت الزامی است.");
        }

        if (await _context.Users.FindAsync([userId], ct) is null)
        {
            return ServiceResult.Fail("کاربر یافت نشد.");
        }

        if (await _context.ClientProfiles.FindAsync([clientId], ct) is null)
        {
            return ServiceResult.Fail("کلاینت یافت نشد.");
        }

        if (await IsAssignedAsync(userId, clientId, ct))
        {
            return ServiceResult.Fail("این کاربر قبلاً به این کلاینت تخصیص یافته است.");
        }

        _context.ClientUserAssignments.Add(new ClientUserAssignment
        {
            UserId = userId,
            ClientId = clientId,
            AssignedAt = DateTimeOffset.UtcNow,
            AssignedBy = assignedBy,
        });

        await _context.SaveChangesAsync(ct);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> UnassignAsync(string userId, string clientId, CancellationToken ct = default)
    {
        var row = await _context.ClientUserAssignments
            .FirstOrDefaultAsync(a => a.UserId == userId && a.ClientId == clientId, ct);

        if (row is null)
        {
            return ServiceResult.Fail("چنین تخصیصی وجود ندارد.");
        }

        _context.ClientUserAssignments.Remove(row);
        await _context.SaveChangesAsync(ct);
        return ServiceResult.Ok();
    }
}
