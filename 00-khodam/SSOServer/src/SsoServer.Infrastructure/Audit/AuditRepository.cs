using Microsoft.EntityFrameworkCore;
using SsoServer.Domain.Entities;
using SsoServer.Domain.Enums;
using SsoServer.Domain.Interfaces;
using SsoServer.Infrastructure.Persistence;

namespace SsoServer.Infrastructure.Audit;

public sealed class AuditRepository : IAuditRepository
{
    private readonly AppDbContext _context;

    public AuditRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(AuditLog entry, CancellationToken ct = default)
    {
        _context.AuditLogs.Add(entry);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<AuditLog>> QueryAsync(int skip, int take, AuditAction? action, string? userId, CancellationToken ct = default)
    {
        var query = _context.AuditLogs.AsNoTracking().AsQueryable();
        if (action.HasValue)
        {
            query = query.Where(a => a.Action == action.Value);
        }

        if (!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(a => a.ActorUserId == userId);
        }

        return await query.OrderByDescending(a => a.Id).Skip(skip).Take(take).ToListAsync(ct);
    }
}
