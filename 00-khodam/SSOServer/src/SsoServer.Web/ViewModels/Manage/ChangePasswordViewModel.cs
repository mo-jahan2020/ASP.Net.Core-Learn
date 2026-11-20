using System.ComponentModel.DataAnnotations;

namespace SsoServer.Web.ViewModels.Manage;

public sealed class ChangePasswordViewModel
{
    [Required(ErrorMessage = "رمز عبور فعلی الزامی است.")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور جدید الزامی است.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "رمز عبور جدید باید حداقل ۸ کاراکتر باشد.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "تکرار رمز عبور جدید مطابقت ندارد.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
