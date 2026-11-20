using Microsoft.EntityFrameworkCore;
using SsoServer.Domain.Entities;
using SsoServer.Domain.Enums;
using SsoServer.Domain.Interfaces;
using SsoServer.Infrastructure.Persistence;

namespace SsoServer.Infrastructure.Settings;

public sealed class SettingsRepository : ISettingsRepository
{
    private readonly AppDbContext _context;

    public SettingsRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyDictionary<string, string>> GetAllAsync(CancellationToken ct = default)
        => await _context.AppSettings.AsNoTracking().ToDictionaryAsync(s => s.Key, s => s.Value, ct);

    public async Task SetAsync(string key, string value, string updatedBy, CancellationToken ct = default)
    {
        var setting = await _context.AppSettings.FirstOrDefaultAsync(s => s.Key == key, ct);
        if (setting is null)
        {
            _context.AppSettings.Add(new AppSetting { Key = key, Value = value, UpdatedBy = updatedBy, UpdatedAt = DateTimeOffset.UtcNow });
        }
        else
        {
            setting.Value = value;
            setting.UpdatedBy = updatedBy;
            setting.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _context.SaveChangesAsync(ct);
    }
}
