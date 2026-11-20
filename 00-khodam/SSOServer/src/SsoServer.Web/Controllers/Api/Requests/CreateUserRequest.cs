using System.ComponentModel.DataAnnotations;
using SsoServer.Domain.Options;

namespace SsoServer.Web.Controllers.Api.Requests;

public sealed class CreateUserRequest : IValidatableObject
{
    [Required] public string UserName { get; set; } = string.Empty;

    [Required] public string DisplayName { get; set; } = string.Empty;

    [EmailAddress] public string Email { get; set; } = string.Empty;

    /// <summary>الزامی مگر آنکه IsAdUser فعال باشد (حساب‌های اکتیو دایرکتوری رمزشان در دامنه می‌ماند).</summary>
    public string Password { get; set; } = string.Empty;

    [Required] public string Role { get; set; } = SsoRoles.User;

    public bool IsActive { get; set; } = true;

    /// <summary>در صورت فعال بودن، اعتبارنامه‌ها از اکتیو دایرکتوری اعتبارسنجی می‌شود.</summary>
    public bool IsAdUser { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IsAdUser)
        {
            yield break;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            yield return new ValidationResult("رمز عبور الزامی است.", [nameof(Password)]);
        }
        else if (Password.Length < 8)
        {
            yield return new ValidationResult("رمز عبور باید حداقل ۸ کاراکتر باشد.", [nameof(Password)]);
        }
    }
}
