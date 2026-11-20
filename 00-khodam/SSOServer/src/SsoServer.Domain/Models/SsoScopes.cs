namespace SsoServer.Domain.Models;

/// <summary>The canonical list of scopes this SSO can grant.</summary>
public static class SsoScopes
{
    public const string OpenId = "openid";
    public const string Profile = "profile";
    public const string Roles = "roles";
    public const string Email = "email";
    public const string Api = "api";
    public const string OfflineAccess = "offline_access";

    public static readonly string[] All = [OpenId, Profile, Roles, Email, Api, OfflineAccess];

    /// <summary>Persian description for the consent screen and admin UI.</summary>
    public static string PersianDescription(string scope) => scope switch
    {
        OpenId => "احراز هویت و شناسایی کاربر",
        Profile => "نمایش اطلاعات پروفایل پایه",
        Roles => "دسترسی به نقش‌های کاربر",
        Email => "دسترسی به نشانی ایمیل",
        Api => "فراخوانی APIهای محافظت‌شده",
        OfflineAccess => "دریافت توکن بازیابی برای ورود خودکار",
        _ => scope,
    };
}
