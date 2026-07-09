using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SportsStore.Pages.Account {

    /// <summary>
    /// PageModel صفحه ثبت‌نام کاربر جدید (Register)
    /// این صفحه:
    /// - کاربران جدید را در سیستم ثبت می‌کند
    /// - رمز عبور بر اساس سیاست قوی اعتبارسنجی می‌شود (تنظیمات در Program.cs)
    /// - کاربر جدید به طور خودکار به نقش "Customer" اضافه می‌شود
    /// - پس از ثبت‌نام موفق، کاربر وارد سیستم شده و به صفحه اصلی هدایت می‌شود
    /// </summary>
    public class RegisterModel : PageModel {

        private readonly UserManager<IdentityUser>   _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole>   _roleManager;

        public RegisterModel(UserManager<IdentityUser> userManager,
                             SignInManager<IdentityUser> signInManager,
                             RoleManager<IdentityRole> roleManager) {
            _userManager   = userManager;
            _signInManager = signInManager;
            _roleManager   = roleManager;
        }

        // --- فیلدهای فرم ---

        [BindProperty]
        [Required(ErrorMessage = "لطفاً نام کاربری را وارد کنید")]
        [StringLength(50, MinimumLength = 3,
            ErrorMessage = "نام کاربری باید بین ۳ تا ۵۰ کاراکتر باشد")]
        [Display(Name = "نام کاربری")]
        public string? UserName { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "لطفاً ایمیل را وارد کنید")]
        [EmailAddress(ErrorMessage = "ایمیل معتبر نیست")]
        [Display(Name = "ایمیل")]
        public string? Email { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "لطفاً رمز عبور را وارد کنید")]
        [StringLength(100, MinimumLength = 8,
            ErrorMessage = "رمز عبور باید حداقل ۸ کاراکتر باشد")]
        [DataType(DataType.Password)]
        [Display(Name = "رمز عبور")]
        public string? Password { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "لطفاً تأیید رمز عبور را وارد کنید")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "رمز عبور و تأیید آن یکسان نیستند")]
        [Display(Name = "تکرار رمز عبور")]
        public string? ConfirmPassword { get; set; }

        // ReturnUrl برای هدایت پس از ثبت‌نام
        public string? ReturnUrl { get; set; }

        /// <summary>
        /// نمایش فرم ثبت‌نام
        /// </summary>
        public void OnGet(string? returnUrl = null) {
            ReturnUrl = returnUrl;
        }

        /// <summary>
        /// پردازش فرم ثبت‌نام
        /// </summary>
        public async Task<IActionResult> OnPostAsync(string? returnUrl = null) {
            returnUrl ??= "/";

            if (!ModelState.IsValid) {
                return Page();
            }

            // ساخت شیء کاربر جدید
            var user = new IdentityUser {
                UserName = UserName,
                Email = Email,
                EmailConfirmed = true // برای سادگی پروژه آموزشی
            };

            // ساخت کاربر در سیستم Identity
            IdentityResult result = await _userManager.CreateAsync(user, Password ?? "");

            if (result.Succeeded) {
                // --- اضافه کردن کاربر به نقش Customer ---
                // ابتدا اطمینان از وجود نقش Customer
                if (!await _roleManager.RoleExistsAsync("Customer")) {
                    await _roleManager.CreateAsync(new IdentityRole("Customer"));
                }
                await _userManager.AddToRoleAsync(user, "Customer");

                // ورود خودکار کاربر پس از ثبت‌نام
                await _signInManager.SignInAsync(user, isPersistent: false);

                TempData["SuccessMessage"] =
                    $"خوش آمدید {user.UserName}! حساب شما با موفقیت ساخته شد.";

                return LocalRedirect(returnUrl);
            }

            // در صورت بروز خطا (مثلاً نام کاربری تکراری)، خطاها را به ModelState اضافه کن
            foreach (var error in result.Errors) {
                // ترجمه پیام‌های رایج Identity به فارسی
                var msg = error.Code switch {
                    "DuplicateUserName" => "این نام کاربری قبلاً استفاده شده است.",
                    "DuplicateEmail"    => "این ایمیل قبلاً استفاده شده است.",
                    "PasswordTooShort"  => "رمز عبور بسیار کوتاه است (حداقل ۸ کاراکتر).",
                    "PasswordRequiresNonAlphanumeric" =>
                        "رمز عبور باید شامل حداقل یک کاراکتر ویژه (!@#$%...) باشد.",
                    "PasswordRequiresDigit" =>
                        "رمز عبور باید شامل حداقل یک عدد باشد.",
                    "PasswordRequiresUpper" =>
                        "رمز عبور باید شامل حداقل یک حرف بزرگ باشد.",
                    "PasswordRequiresLower" =>
                        "رمز عبور باید شامل حداقل یک حرف کوچک باشد.",
                    _ => error.Description
                };
                ModelState.AddModelError(string.Empty, msg);
            }

            return Page();
        }
    }
}
