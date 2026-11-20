using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SsoServer.Domain.Interfaces;

namespace SsoServer.Web.Services.HotReload;

/// <summary>
/// Applies hot-reloadable security values to live framework singletons and re-applies them
/// whenever settings change: Identity lockout thresholds (applied in place on the shared
/// IdentityOptions instance used by UserManager) and the settings snapshot warm-up.
/// </summary>
public sealed class SettingsHotReloadService : IHostedService
{
    private readonly ISettingsService _settings;
    private readonly IOptions<IdentityOptions> _identityOptions;
    private readonly ILogger<SettingsHotReloadService> _logger;

    public SettingsHotReloadService(
        ISettingsService settings,
        IOptions<IdentityOptions> identityOptions,
        ILogger<SettingsHotReloadService> logger)
    {
        _settings = settings;
        _identityOptions = identityOptions;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _settings.SettingsChanged += ApplyLiveSettings;
        ApplyLiveSettings();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _settings.SettingsChanged -= ApplyLiveSettings;
        return Task.CompletedTask;
    }

    private void ApplyLiveSettings()
    {
        try
        {
            var effective = _settings.Current;
            _identityOptions.Value.Lockout.MaxFailedAccessAttempts = effective.MaxFailedLoginAttempts;
            _identityOptions.Value.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(effective.LockoutMinutes);
            _logger.LogDebug(
                "Live settings applied: MaxFailedAccessAttempts={Attempts}, Lockout={Minutes}m, CookieSliding={Sliding}",
                effective.MaxFailedLoginAttempts, effective.LockoutMinutes, effective.SessionCookieSliding);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not apply live settings");
        }
    }
}
