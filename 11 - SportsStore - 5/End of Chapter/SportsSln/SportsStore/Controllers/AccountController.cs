using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SportsStore.Models.ViewModels;

namespace SportsStore.Controllers {

    public class AccountController : Controller {
        private UserManager<IdentityUser> userManager;
        private SignInManager<IdentityUser> signInManager;

        public AccountController(UserManager<IdentityUser> userMgr, SignInManager<IdentityUser> signInMgr) {
            userManager = userMgr;
            signInManager = signInMgr;
        }
        //For HttpGet
        public ViewResult Login(string returnUrl) {
            return View(new LoginModel {
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel loginModel) {
            if (ModelState.IsValid) 
            {
                IdentityUser user = await userManager.FindByNameAsync(loginModel.Name);
                if (user != null && loginModel.Password != null)
                {
                    await signInManager.SignOutAsync();
                    bool isPersistent = false;//آیا سشن بعد از بستن مرورگر باقی بماند؟
                    bool lockoutOnFailure = false;//با با چند بار خطا در ورود کاربر قفل شود؟
                    if ((await signInManager.PasswordSignInAsync(user, loginModel.Password, isPersistent, lockoutOnFailure)).Succeeded) 
                    {
                        return Redirect(loginModel?.ReturnUrl ?? "/Admin");
                    }
                }
                ModelState.AddModelError("", "Invalid name or password");
            }
            return View(loginModel);
        }

        [Authorize]
        public async Task<RedirectResult> Logout(string returnUrl = "/") {
            await signInManager.SignOutAsync();
            return Redirect(returnUrl);
        }
    }
}
