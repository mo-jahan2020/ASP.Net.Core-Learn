using System.ComponentModel.DataAnnotations;

namespace SportsStore.Models.ViewModels {

    /// <summary>
    /// ViewModel صفحه ثبت‌نام کاربر جدید
    /// در MVC از ViewModel برای انتقال داده بین Controller و View استفاده می‌شود.
    /// </summary>
    public class RegisterViewModel {

        [Required(ErrorMessage = "لطفاً نام کاربری را وارد کنید")]
        [StringLength(50, MinimumLength = 3,
            ErrorMessage = "نام کاربری باید بین ۳ تا ۵۰ کاراکتر باشد")]
        [Display(Name = "نام کاربری")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "لطفاً ایمیل را وارد کنید")]
        [EmailAddress(ErrorMessage = "ایمیل معتبر نیست")]
        [Display(Name = "ایمیل")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "لطفاً رمز عبور را وارد کنید")]
        [StringLength(100, MinimumLength = 8,
            ErrorMessage = "رمز عبور باید حداقل ۸ کاراکتر باشد")]
        [DataType(DataType.Password)]
        [Display(Name = "رمز عبور")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "لطفاً تأیید رمز عبور را وارد کنید")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "رمز عبور و تأیید آن یکسان نیستند")]
        [Display(Name = "تکرار رمز عبور")]
        public string? ConfirmPassword { get; set; }
    }
}
