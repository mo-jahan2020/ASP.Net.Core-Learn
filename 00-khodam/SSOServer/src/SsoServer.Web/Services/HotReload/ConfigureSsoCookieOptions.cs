using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using SsoServer.Domain.Interfaces;

namespace SsoServer.Web.Services.HotReload;

/// <summary>Applies the hot-reloadable session-cookie sliding setting to the Identity cookie.</summary>
public sealed class ConfigureSsoCookieOptions : IConfigureNamedOptions<CookieAuthenticationOptions>
{
    private readonly ISettingsService _settings;

    public ConfigureSsoCookieOptions(ISettingsService settings) => _settings = settings;

    /// تنظیمات کوکی نشست SSO (فلو ۲ گام ۴):
    /// نام کوکی SsoServer.Session، HttpOnly، SameSite=Lax و انقضای کششی
    /// طبق تنظیم SsoSessionCookieHours — با تغییر تنظیمات، بدون ری‌استارت دوباره اعمال می‌شود.
    public void Configure(string? name, CookieAuthenticationOptions options)
    {
        // The Identity application cookie is registered under its own scheme name.
        if (name != Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme)
        {
            return;
        }

        options.SlidingExpiration = _settings.Current.SessionCookieSliding;
    }

    public void Configure(CookieAuthenticationOptions options) => Configure(Options.DefaultName, options);
}

/// <summary>Invalidates the cached cookie options whenever settings change.</summary>
public sealed class SettingsCookieChangeTokenSource : IOptionsChangeTokenSource<CookieAuthenticationOptions>
{
    private CancellationTokenSource _cancellationTokenSource = new();
    private readonly ISettingsService _settings;

    public SettingsCookieChangeTokenSource(ISettingsService settings)
    {
        _settings = settings;
        settings.SettingsChanged += () =>
        {
            // Swap in a fresh source first so re-subscriptions observe an active token,
            // then cancel the old one exactly once (otherwise monitors loop forever).
            var previous = Interlocked.Exchange(ref _cancellationTokenSource, new CancellationTokenSource());
            try
            {
                previous.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // Host shutting down - ignore.
            }
            finally
            {
                previous.Dispose();
            }
        };
    }

    public string Name => Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme;

    public IChangeToken GetChangeToken() => new CancellationChangeToken(Volatile.Read(ref _cancellationTokenSource).Token);
}
