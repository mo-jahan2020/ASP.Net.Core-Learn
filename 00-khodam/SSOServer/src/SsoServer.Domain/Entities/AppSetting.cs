namespace SsoServer.Domain.Entities;

/// <summary>Database-backed key/value setting used for hot-reloadable configuration.</summary>
public class AppSetting
{
    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string UpdatedBy { get; set; } = string.Empty;
}
