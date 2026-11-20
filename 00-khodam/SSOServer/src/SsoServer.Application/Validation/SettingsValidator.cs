using SsoServer.Domain.Models;

namespace SsoServer.Application.Validation;

/// <summary>Validates hot-reloadable settings updates before persisting them.</summary>
public static class SettingsValidator
{
    public static IReadOnlyList<string> Validate(UpdateSettingsDto dto)
    {
        var errors = new List<string>();

        if (dto.MaxFailedLoginAttempts is < 1 or > 100)
        {
            errors.Add("حداکثر تعداد تلاش ناموفق ورود باید بین ۱ تا ۱۰۰ باشد.");
        }

        if (dto.LockoutMinutes is < 1 or > 1440)
        {
            errors.Add("مدت قفل شدن حساب باید بین ۱ تا ۱۴۴۰ دقیقه باشد.");
        }

        if (dto.TokenRateLimitPerMinute is < 1 or > 100000)
        {
            errors.Add("سقف نرخ درخواست باید بین ۱ تا ۱۰۰٬۰۰۰ در دقیقه باشد.");
        }

        if (dto.CorsOrigins is not null)
        {
            foreach (var raw in dto.CorsOrigins.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!IsValidOrigin(raw, out var reason))
                {
                    errors.Add($"مبدأ CORS نامعتبر است ({raw}): {reason}");
                }
            }
        }

        if (dto.LoginTitle?.Length > 200)
        {
            errors.Add("عنوان صفحه ورود حداکثر ۲۰۰ کاراکتر است.");
        }

        if (dto.LoginSubtitle?.Length > 400)
        {
            errors.Add("زیرعنوان صفحه ورود حداکثر ۴۰۰ کاراکتر است.");
        }

        return errors;
    }

    private static bool IsValidOrigin(string origin, out string reason)
    {
        reason = string.Empty;
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var parsed))
        {
            reason = "باید یک آدرس مطلق باشد.";
            return false;
        }

        if (parsed.Scheme is not ("http" or "https"))
        {
            reason = "شِما فقط http یا https مجاز است.";
            return false;
        }

        if (string.IsNullOrEmpty(parsed.Host) || parsed.AbsolutePath is not ("" or "/"))
        {
            reason = "مبدأ باید scheme + host (+ port) باشد و مسیر/کوئری نداشته باشد.";
            return false;
        }

        return true;
    }
}
