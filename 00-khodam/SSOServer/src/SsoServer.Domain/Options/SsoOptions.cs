using SsoServer.Domain.Entities;

namespace SsoServer.Domain.Options;

public class SsoOptions
{
    public string IssuerUri { get; set; } = "http://localhost:5001";

    public SigningCertificateOptions SigningCertificate { get; set; } = new();

    public int AccessTokenLifetimeMinutes { get; set; } = 60;

    public int IdentityTokenLifetimeMinutes { get; set; } = 20;

    public int RefreshTokenLifetimeDays { get; set; } = 14;

    /// <summary>Absolute cap for sliding refresh tokens (days). Startup-loaded.</summary>
    public int RefreshTokenAbsoluteLifetimeDays { get; set; } = 90;

    public bool UseSlidingRefreshTokens { get; set; } = true;

    public bool RequireConsentByDefault { get; set; }

    public int SsoSessionCookieHours { get; set; } = 168;
}

public class SigningCertificateOptions
{
    public string Path { get; set; } = "certs/sso-signing.pfx";

    public string? Password { get; set; }

    /// <summary>Literal sentinel used in appsettings.json to mean "provide via environment".</summary>
    public const string NotSetValue = "SET_VIA_ENVIRONMENT";
}

public class SecurityOptions
{
    public int MaxFailedLoginAttempts { get; set; } = 5;

    public int LockoutMinutes { get; set; } = 15;

    public int TokenRateLimitPerMinute { get; set; } = 10;

    public string[] CorsOrigins { get; set; } = [];
}

public class SeedOptions
{
    public string AdminUsername { get; set; } = "admin";

    public string AdminEmail { get; set; } = "admin@local";

    public string AdminPassword { get; set; } = "ChangeMe!123";

    /// <summary>Dev secret assigned to the six sample clients and first-party confidential clients.</summary>
    public string SampleClientSecret { get; set; } = "sample-dev-secret";

    public List<FirstPartyClientOptions> FirstPartyClients { get; set; } = [];
}

public class FirstPartyClientOptions
{
    public string ClientId { get; set; } = string.Empty;

    public string? ClientSecret { get; set; }

    /// <summary>Comma separated: authorization_code_pkce, refresh_token, password, client_credentials.</summary>
    public string Flows { get; set; } = string.Empty;

    /// <summary>Comma separated list of allowed scopes.</summary>
    public string Scopes { get; set; } = "openid,profile,roles,email,api,offline_access";

    /// <summary>Comma separated redirect URIs.</summary>
    public string? RedirectUris { get; set; }

    /// <summary>Comma separated post-logout redirect URIs.</summary>
    public string? PostLogoutRedirectUris { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string PersianDisplayName { get; set; } = string.Empty;
}

/// <summary>Conventional role names used across the SSO.</summary>
public static class SsoRoles
{
    public const string Admin = "Admin";

    public const string User = "User";
}
