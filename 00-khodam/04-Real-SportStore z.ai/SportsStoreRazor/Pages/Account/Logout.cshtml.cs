using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SportsStore.Pages.Account {

    /// <summary>
    /// PageModel صفحه خروج (Logout)
    /// خروج کاربر از سیستم با متد POST (برای امنیت - جلوگیری از CSRF).
    /// </summary>
    public class LogoutModel : PageModel {

        private readonly SignInManager<IdentityUser> _signInManager;

        public LogoutModel(SignInManager<IdentityUser> signInManager) {
            _signInManager = signInManager;
        }

        /// <summary>
        /// نمایش صفحه تأیید خروج
        /// </summary>
        public void OnGet() { }

        /// <summary>
        /// خروج کاربر از سیستم
        /// </summary>
        public async Task<IActionResult> OnPostAsync() {
            if (_signInManager.IsSignedIn(User)) {
                await _signInManager.SignOutAsync();
            }
            TempData["SuccessMessage"] = "شما با موفقیت از سیستم خارج شدید.";
            return RedirectToPage("/Account/Login");
        }
    }
}
