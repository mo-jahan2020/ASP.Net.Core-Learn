using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SportsStore.Pages.Account {

    /// <summary>
    /// PageModel صفحه ورود (Login)
    /// این کلاس منطق پردازش فرم ورود را مدیریت می‌کند:
    /// - OnGet: نمایش فرم ورود
    /// - OnPostAsync: پردازش اطلاعات وارد شده و ورود کاربر
    /// </summary>
    public class LoginModel : PageModel {

        // SignInManager: برای ورود و خروج کاربران
        private readonly SignInManager<IdentityUser> _signInManager;
        // UserManager: برای جستجوی کاربر
        private readonly UserManager<IdentityUser> _userManager;

        public LoginModel(SignInManager<IdentityUser> signInMgr,
                          UserManager<IdentityUser> userMgr) {
            _signInManager = signInMgr;
            _userManager = userMgr;
        }

        // فیلد نام کاربری - اجباری
        [BindProperty]
        [Required(ErrorMessage = "لطفاً نام کاربری را وارد کنید")]
        public string? Name { get; set; }

        // فیلد رمز عبور - اجباری
        [BindProperty]
        [Required(ErrorMessage = "لطفاً رمز عبور را وارد کنید")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        // آدرسی که کاربر پس از ورود باید به آن برگردد
        // اگر کاربر بدون ورود به صفحه محافظت‌شده برود، به Login هدایت می‌شود
        // و ReturnUrl آن صفحه محافظت‌شده خواهد بود
        [BindProperty]
        public string ReturnUrl { get; set; } = "/";

        /// <summary>
        /// نمایش فرم ورود (متد GET)
        /// </summary>
        public void OnGet(string? returnUrl = null) {
            ReturnUrl = returnUrl ?? "/";
        }

        /// <summary>
        /// پردازش فرم ورود (متد POST)
        /// </summary>
        public async Task<IActionResult> OnPostAsync(string? returnUrl = null) {
            returnUrl ??= ReturnUrl ?? "/";

            // اعتبارسنجی فرم سمت سرور
            if (!ModelState.IsValid) {
                return Page();
            }

            // جستجوی کاربر با نام وارد شده
            IdentityUser? user = await _userManager.FindByNameAsync(Name ?? "");
            if (user == null) {
                ModelState.AddModelError("", "نام کاربری یا رمز عبور نادرست است.");
                return Page();
            }

            // تلاش برای ورود با رمز عبور
            // isPersistent=false: کوکی ورود پس از بستن مرورگر پاک شود
            // lockoutOnFailure=false: قفل اکانت پس از تلاش‌های ناموفق فعال نشود
            Microsoft.AspNetCore.Identity.SignInResult result =
                await _signInManager.PasswordSignInAsync(
                    Name ?? "",
                    Password ?? "",
                    isPersistent: false,
                    lockoutOnFailure: false);

            if (result.Succeeded) {
                // ورود موفق - هدایت به ReturnUrl
                // LocalRedirect جلوگیری می‌کند از Redirect به سایت خارجی (امنیت)
                return LocalRedirect(returnUrl);
            }

            // ورود ناموفق - نمایش خطا
            ModelState.AddModelError("", "نام کاربری یا رمز عبور نادرست است.");
            return Page();
        }
    }
}
