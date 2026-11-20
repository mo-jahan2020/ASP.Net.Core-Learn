using Microsoft.AspNetCore.Mvc;

namespace SsoServer.Web.Helpers;

/// <summary>Authorization policy names used by the management API.</summary>
public static class SsoPolicies
{
    /// <summary>Requires the "api" scope on the validated OpenIddict token.</summary>
    public const string ApiScope = "SsoApiScope";

    /// <summary>Requires the api scope AND the Admin role.</summary>
    public const string Admin = "SsoAdmin";

    /// <summary>Cookie + OpenIddict server scheme (interactive endpoints).</summary>
    public static readonly string[] AdminSchemes = [];
}

public static class LocalRedirectHelper
{
    /// <summary>Open-redirect-safe local redirect.</summary>
    public static string SafeLocal(this IUrlHelper url, string? returnUrl, string fallback = "/")
    {
        if (string.IsNullOrEmpty(returnUrl) || !returnUrl.StartsWith('/') || returnUrl.StartsWith("//") || returnUrl.Contains("://"))
        {
            return fallback;
        }

        return returnUrl;
    }
}
