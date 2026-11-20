using SsoServer.Domain.Options;

namespace SsoServer.Application.Tokens;

public sealed record TokenLifetimes(
    TimeSpan AccessToken,
    TimeSpan IdentityToken,
    TimeSpan RefreshToken,
    bool SlidingRefresh);

/// <summary>
/// Maps the Sso options (startup-loaded) onto OpenIddict token lifetimes.
/// OpenIddict 7.x semantics: the refresh token lifetime is a sliding window by default;
/// passing SlidingRefresh = false maps to DisableSlidingRefreshTokenExpiration().
/// </summary>
public static class TokenLifetimeMapper
{
    /// نگاشت تنظیمات به طول عمر توکن‌ها (فلو ۴ نکته امنیتی):
    /// access / id / refresh token و فعال‌بودن انقضای کششی refresh.
    /// این مقادیر هنگام راه‌اندازی روی OpenIddict اعمال می‌شوند.
    public static TokenLifetimes Map(SsoOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return new TokenLifetimes(
            AccessToken: TimeSpan.FromMinutes(Math.Max(1, options.AccessTokenLifetimeMinutes)),
            IdentityToken: TimeSpan.FromMinutes(Math.Max(1, options.IdentityTokenLifetimeMinutes)),
            RefreshToken: TimeSpan.FromDays(Math.Max(1, options.RefreshTokenLifetimeDays)),
            SlidingRefresh: options.UseSlidingRefreshTokens);
    }
}
