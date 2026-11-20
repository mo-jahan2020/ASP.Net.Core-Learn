namespace SsoServer.Domain.Models;

/// <summary>
/// Effective runtime settings: hot-reloadable values (DB-backed) layered over
/// startup-loaded values (appsettings.json, changing requires a restart).
/// </summary>
public class EffectiveSettings
{
    // --- hot-reloadable ---
    public IReadOnlyList<string> CorsOrigins { get; set; } = [];

    public bool RequireConsentByDefault { get; set; }

    public int MaxFailedLoginAttempts { get; set; } = 5;

    public int LockoutMinutes { get; set; } = 15;

    public int TokenRateLimitPerMinute { get; set; } = 10;

    public bool SessionCookieSliding { get; set; } = true;

    public string LoginTitle { get; set; } = string.Empty;

    public string LoginSubtitle { get; set; } = string.Empty;

    // --- startup-loaded (restart required) ---
    public string IssuerUri { get; set; } = "http://localhost:5001";

    public int AccessTokenLifetimeMinutes { get; set; } = 60;

    public int IdentityTokenLifetimeMinutes { get; set; } = 20;

    public int RefreshTokenLifetimeDays { get; set; } = 14;

    public int RefreshTokenAbsoluteLifetimeDays { get; set; } = 90;

    public bool UseSlidingRefreshTokens { get; set; } = true;

    public int SsoSessionCookieHours { get; set; } = 168;
}

/// <summary>Payload for updating the hot-reloadable settings.</summary>
public class UpdateSettingsDto
{
    public string? CorsOrigins { get; set; }

    public bool? RequireConsentByDefault { get; set; }

    public int? MaxFailedLoginAttempts { get; set; }

    public int? LockoutMinutes { get; set; }

    public int? TokenRateLimitPerMinute { get; set; }

    public bool? SessionCookieSliding { get; set; }

    public string? LoginTitle { get; set; }

    public string? LoginSubtitle { get; set; }
}

/// <summary>Keys of the DB-backed (hot-reloadable) settings.</summary>
public static class SettingKeys
{
    public const string CorsOrigins = "Security:CorsOrigins";
    public const string MaxFailedLoginAttempts = "Security:MaxFailedLoginAttempts";
    public const string LockoutMinutes = "Security:LockoutMinutes";
    public const string TokenRateLimitPerMinute = "Security:TokenRateLimitPerMinute";
    public const string RequireConsentByDefault = "Sso:RequireConsentByDefault";
    public const string SessionCookieSliding = "Sso:SessionCookieSliding";
    public const string LoginTitle = "Sso:LoginTitle";
    public const string LoginSubtitle = "Sso:LoginSubtitle";
}
