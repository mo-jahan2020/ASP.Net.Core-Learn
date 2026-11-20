using System.ComponentModel.DataAnnotations;

namespace SsoServer.Web.ViewModels.Admin;

public sealed class UpdateSettingsViewModel
{
    public string? CorsOrigins { get; set; }

    public bool RequireConsentByDefault { get; set; }

    [Range(1, 100, ErrorMessage = "بین ۱ تا ۱۰۰ وارد کنید.")]
    public int MaxFailedLoginAttempts { get; set; }

    [Range(1, 1440, ErrorMessage = "بین ۱ تا ۱۴۴۰ دقیقه وارد کنید.")]
    public int LockoutMinutes { get; set; }

    [Range(1, 100000, ErrorMessage = "بین ۱ تا ۱۰۰٬۰۰۰ وارد کنید.")]
    public int TokenRateLimitPerMinute { get; set; }

    public bool SessionCookieSliding { get; set; }

    public string? LoginTitle { get; set; }

    public string? LoginSubtitle { get; set; }
}
