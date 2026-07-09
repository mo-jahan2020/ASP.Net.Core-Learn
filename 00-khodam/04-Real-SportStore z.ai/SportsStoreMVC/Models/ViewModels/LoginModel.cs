using System.ComponentModel.DataAnnotations;

namespace SportsStore.Models.ViewModels {

    /// <summary>
    /// مدل ورود به سیستم
    /// برای صفحه Login استفاده می‌شود.
    /// </summary>
    public class LoginModel {

        [Required]
        public string? Name { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        // آدرسی که کاربر پس از ورود باید به آن برگردد
        public string ReturnUrl { get; set; } = "/";
    }
}
