using Microsoft.AspNetCore.Identity;
using SsoServer.Domain.Models;
using SsoServer.Domain.Options;

namespace SsoServer.Application.Services;

/// <summary>Maps ASP.NET Core Identity error codes to Persian messages.</summary>
public static class PersianIdentityErrors
{
    public static IReadOnlyList<string> Map(IEnumerable<IdentityError> errors)
        => errors.Select(Map).ToList();

    public static string Map(IdentityError error) => error.Code switch
    {
        "DuplicateUserName" => "این نام کاربری قبلاً ثبت شده است.",
        "InvalidUserName" => "نام کاربری نامعتبر است (فقط حروف انگلیسی، عدد، نقطه، خط تیره و زیرخط).",
        "DuplicateEmail" => "این نشانی ایمیل قبلاً ثبت شده است.",
        "InvalidEmail" => "نشانی ایمیل نامعتبر است.",
        "PasswordTooShort" => "رمز عبور باید حداقل ۸ کاراکتر باشد.",
        "PasswordRequiresNonAlphanumeric" => "رمز عبور باید حداقل یک کاراکتر خاص (مثل ! یا @) داشته باشد.",
        "PasswordRequiresDigit" => "رمز عبور باید حداقل یک رقم داشته باشد.",
        "PasswordRequiresUpper" => "رمز عبور باید حداقل یک حرف بزرگ انگلیسی داشته باشد.",
        "PasswordRequiresLower" => "رمز عبور باید حداقل یک حرف کوچک انگلیسی داشته باشد.",
        "PasswordMismatch" => "رمز عبور فعلی نادرست است.",
        "UserAlreadyHasPassword" => "برای این کاربر قبلاً رمز عبور تعیین شده است.",
        "ConcurrencyFailure" => "اطلاعات هم‌زمان توسط کاربر دیگری تغییر کرده است؛ دوباره تلاش کنید.",
        _ => string.IsNullOrWhiteSpace(error.Description) ? "خطای نامشخص در عملیات کاربر." : error.Description,
    };
}

/// <summary>Shared Persian result messages.</summary>
public static class PersianMessages
{
    public const string UserNotFound = "کاربر یافت نشد.";
    public const string ClientNotFound = "کلاینت یافت نشد.";
    public const string SessionNotFound = "نشست یافت نشد.";
    public const string InvalidRole = "نقش نامعتبر است؛ فقط Admin یا User مجاز است.";
    public const string CannotChangeOwnAccount = "تغییر نقش یا حذف حساب کاربری خودتان مجاز نیست.";
    public const string ClaimTypeReserved = "نوع کلیم رزروشده است و نمی‌توان آن را دستی تعریف کرد.";

    public static readonly string[] AllowedRoles = [SsoRoles.Admin, SsoRoles.User];
}
