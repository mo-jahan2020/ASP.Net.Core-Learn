// =============================================================================
// فایل Controllers/CubedController.cs — کنترلر MVC با TempData
// =============================================================================
// این کنترلر مفهوم TempData را آموزش می‌دهد:
// TempData داده‌ای است بین یک درخواست و درخواست بعدی (Redirect) زنده می‌ماند.
// کاربرد رایج: نمایش پیام "عملیات با موفقیت انجام شد" بعد از Redirect.
// =============================================================================

using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers {
    // کنترلر MVC (از Controller ارث‌بری، نه ControllerBase)
    public class CubedController : Controller {

        // GET /Cubed/Index — نمایش فرم و نتیجه
        // View("Cubed"): فایل Views/Shared/Cubed.cshtml (در پوشه Shared)
        public IActionResult Index() {
            return View("Cubed");
        }

        // GET /Cubed/Cube?num=5 — محاسبه مکعب و Redirect
        // TempData["value"]: ذخیره عدد ورودی برای نمایش در فرم بعد از Redirect
        // TempData["result"]: ذخیره نتیجه مکعب
        // TempData از Session یا Cookie برای ذخیره‌سازی استفاده می‌کند.
        // Math.Pow(num, 3): محاسبه توان سوم (مکعب)
        // RedirectToAction(nameof(Index)): Redirect به اکشن Index
        public IActionResult Cube(double num) {
            TempData["value"] = num.ToString();
            TempData["result"] = Math.Pow(num, 3).ToString();
            return RedirectToAction(nameof(Index));
        }
    }
}
