using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SportsStore.Models.ViewModels;

namespace SportsStore.Controllers {

    /// <summary>
    /// Controller احراز هویت (Account)
    /// مسئولیت: ورود، خروج و ثبت‌نام کاربران
    ///
    /// مقایسه با Razor Pages:
    /// - در Razor Pages: Login.cshtml.cs + Logout.cshtml.cs + Register.cshtml.cs (سه PageModel جدا)
    /// - در MVC: یک AccountController با سه Action (Login، Logout، Register)
    ///
    /// مسیرها (با Route پیش‌فرض):
    /// - GET  /Account/Login    → Login()
    /// - POST /Account/Login    → Login(LoginModel) (با [HttpPost])
    /// - POST /Account/Logout   → Logout()
    /// - GET  /Account/Register → Register()
    /// - POST /Account/Register → Register(RegisterViewModel)
    /// - GET  /Account/AccessDenied → AccessDenied()
    /// </summary>
    public class AccountController : Controller {

        // برای مدیریت ورود/خروج کاربران
        private readonly SignInManager<IdentityUser> _signInManager;
        // برای جستجو/ایجاد کاربر
        private readonly UserManager<IdentityUser>   _userManager;

        public AccountController(SignInManager<IdentityUser> signInMgr,
                                 UserManager<IdentityUser>   userMgr) {
            _signInManager = signInMgr;
            _userManager   = userMgr;
        }

        // --------------------------------------------------------------------
        // LOGIN (ورود)
        // --------------------------------------------------------------------

        /// <summary>
        /// نمایش فرم ورود (GET /Account/Login)
        /// </summary>
        /// <param name="returnUrl">آدرسی که کاربر پس از ورود باید به آن برگردد</param>
        [HttpGet]
        public IActionResult Login(string? returnUrl = "/") {
            return View(new LoginModel {
                ReturnUrl = returnUrl ?? "/"
            });
        }

        /// <summary>
        /// پردازش فرم ورود (POST /Account/Login)
        /// [HttpPost] نشان می‌دهد که این اکشن فقط به درخواست‌های POST پاسخ می‌دهد
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]  // جلوگیری از حملات CSRF
        public async Task<IActionResult> Login(LoginModel model) {
            if (!ModelState.IsValid) {
                return View(model);
            }

            // جستجوی کاربر با نام وارد شده
            IdentityUser? user = await _userManager.FindByNameAsync(model.Name ?? "");
            if (user == null) {
                ModelState.AddModelError("", "نام کاربری یا رمز عبور نادرست است.");
                return View(model);
            }

            // تلاش برای ورود با رمز عبور
            Microsoft.AspNetCore.Identity.SignInResult result =
                await _signInManager.PasswordSignInAsync(
                    model.Name ?? "",
                    model.Password ?? "",
                    isPersistent: false,
                    lockoutOnFailure: false);

            if (result.Succeeded) {
                // ورود موفق - هدایت به ReturnUrl
                return LocalRedirect(model.ReturnUrl ?? "/");
            }

            ModelState.AddModelError("", "نام کاربری یا رمز عبور نادرست است.");
            return View(model);
        }

        // --------------------------------------------------------------------
        // LOGOUT (خروج)
        // --------------------------------------------------------------------

        /// <summary>
        /// خروج کاربر (POST /Account/Logout)
        /// فقط با POST انجام می‌شود تا از حملات CSRF جلوگیری شود
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout() {
            if (_signInManager.IsSignedIn(User)) {
                await _signInManager.SignOutAsync();
            }
            TempData["SuccessMessage"] = "شما با موفقیت از سیستم خارج شدید.";
            return RedirectToAction("Login");
        }

        // --------------------------------------------------------------------
        // REGISTER (ثبت‌نام)
        // --------------------------------------------------------------------

        /// <summary>
        /// نمایش فرم ثبت‌نام (GET /Account/Register)
        /// </summary>
        [HttpGet]
        public IActionResult Register() {
            return View();
        }

        /// <summary>
        /// پردازش فرم ثبت‌نام (POST /Account/Register)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model) {
            if (!ModelState.IsValid) {
                return View(model);
            }

            // ساخت شیء کاربر جدید
            var user = new IdentityUser {
                UserName = model.UserName,
                Email = model.Email,
                EmailConfirmed = true
            };

            // ساخت کاربر در سیستم Identity
            IdentityResult result = await _userManager.CreateAsync(user, model.Password ?? "");

            if (result.Succeeded) {
                // اضافه کردن کاربر به نقش Customer
                await _userManager.AddToRoleAsync(user, "Customer");

                // ورود خودکار کاربر پس از ثبت‌نام
                await _signInManager.SignInAsync(user, isPersistent: false);

                TempData["SuccessMessage"] =
                    $"خوش آمدید {user.UserName}! حساب شما با موفقیت ساخته شد.";
                return RedirectToAction("Index", "Products");
            }

            // ترجمه پیام‌های خطای Identity به فارسی
            foreach (var error in result.Errors) {
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

            return View(model);
        }

        // --------------------------------------------------------------------
        // ACCESS DENIED (دسترسی ممنوع)
        // --------------------------------------------------------------------

        /// <summary>
        /// نمایش صفحه "دسترسی ممنوع" (GET /Account/AccessDenied)
        /// وقتی کاربر وارد شده اما نقش لازم را ندارد، به این صفحه هدایت می‌شود
        /// </summary>
        [HttpGet]
        public IActionResult AccessDenied() {
            return View();
        }
    }
}
