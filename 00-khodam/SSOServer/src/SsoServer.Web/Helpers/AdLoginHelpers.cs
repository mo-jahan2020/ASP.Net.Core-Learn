using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SsoServer.Domain.Entities;
using SsoServer.Domain.Models;

namespace SsoServer.Web.Helpers;

/// <summary>Shared helpers for the Active Directory login path (UI + token endpoints).</summary>
public static class AdLoginHelpers
{
    /// <summary>
    /// Finds a user by name, retrying with the bare username when the user typed a
    /// domain-qualified form (CORP\ali or ali@corp.local) but the local account was
    /// created with the plain sAMAccountName (or vice versa).
    /// </summary>
    /// فلو ۲/۴ — یافتن کاربر با پشتیبانی از فرم دامنه‌دار.
    /// اگر کاربر CORP\ali یا ali@corp.local تایپ کند ولی حساب محلی با نام ساده ali
    /// ساخته شده باشد (یا برعکس)، این متد نام ساده را هم امتحان می‌کند.
    public static async Task<ApplicationUser?> FindByNameWithDomainFallbackAsync(
        this UserManager<ApplicationUser> users, string username)
    {
        var user = await users.FindByNameAsync(username);
        if (user is not null)
        {
            return user;
        }

        var backslash = username.LastIndexOf('\\');
        if (backslash >= 0 && backslash < username.Length - 1)
        {
            user = await users.FindByNameAsync(username[(backslash + 1)..]);
            if (user is not null)
            {
                return user;
            }
        }

        var at = username.IndexOf('@');
        if (at > 0 && at < username.Length - 1)
        {
            user = await users.FindByNameAsync(username[..at]);
        }

        return user;
    }

    /// <summary>
    /// Best-effort refresh of the local profile (display name / email) from the
    /// Active Directory entry captured during login, so tokens stay in sync with
    /// the directory. Failures never block the login itself.
    /// </summary>
    /// فلو ۲/۴ — همگام‌سازی پروفایل کاربر (نام نمایشی/ایمیل) بعد از ورود موفق AD.
    public static async Task SyncProfileFromAdAsync(
        this UserManager<ApplicationUser> users,
        ILogger logger,
        ApplicationUser user,
        AdAuthResult ad)
    {
        try
        {
            var changed = false;

            if (!string.IsNullOrWhiteSpace(ad.DisplayName) &&
                !string.Equals(ad.DisplayName, user.DisplayName, StringComparison.Ordinal))
            {
                user.DisplayName = ad.DisplayName;
                changed = true;
            }

            if (!string.IsNullOrWhiteSpace(ad.Email) && ad.Email.Contains('@') &&
                !string.Equals(ad.Email, user.Email, StringComparison.Ordinal))
            {
                user.Email = ad.Email;
                changed = true;
            }

            if (changed)
            {
                var result = await users.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    logger.LogWarning("Profile sync from Active Directory failed for {UserId}: {Errors}",
                        user.Id, string.Join(", ", result.Errors.Select(e => e.Code)));
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Profile sync from Active Directory failed for {UserId}", user.Id);
        }
    }
}
