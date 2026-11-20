using System.ComponentModel.DataAnnotations;

namespace SsoServer.Web.Controllers.Api.Requests;

public sealed class UpdateSettingsRequest
{
    public string? CorsOrigins { get; set; }

    public bool? RequireConsentByDefault { get; set; }

    [Range(1, 100)] public int? MaxFailedLoginAttempts { get; set; }

    [Range(1, 1440)] public int? LockoutMinutes { get; set; }

    [Range(1, 100000)] public int? TokenRateLimitPerMinute { get; set; }

    public bool? SessionCookieSliding { get; set; }

    public string? LoginTitle { get; set; }

    public string? LoginSubtitle { get; set; }
}
