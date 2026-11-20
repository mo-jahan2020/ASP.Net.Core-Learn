using System.ComponentModel.DataAnnotations;

namespace SsoServer.Web.Controllers.Api.Requests;

public sealed class ChangePasswordRequest
{
    [Required(ErrorMessage = "رمز عبور فعلی الزامی است.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور جدید الزامی است.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "رمز عبور جدید باید حداقل ۸ کاراکتر باشد.")]
    public string NewPassword { get; set; } = string.Empty;
}
