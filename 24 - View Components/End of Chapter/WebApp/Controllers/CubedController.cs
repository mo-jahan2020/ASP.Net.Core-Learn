// =====================================================================
// CubedController.cs - کنترلر ساده برای محاسبه‌ی مکعب یک عدد
// =====================================================================
// این کنترلر نحوه‌ی استفاده از TempData را نشان می‌دهد.
// TempData برای انتقال داده‌ها بین درخواست‌ها (requests) استفاده می‌شود.
// داده‌های TempData فقط برای یک درخواست بعدی در دسترس هستند.
// =====================================================================

 using Microsoft.AspNetCore.Mvc; // برای استفاده از Controller

// تعریف namespace
namespace WebApp.Controllers {
    // تعریف کلاس CubedController که از Controller ارث‌بری می‌کند
    public class CubedController : Controller {

        // Action Index: صفحه‌ی اصلی کنترلر
        // این Action فقط View مربوط به خودش (Cubed.cshtml) را نمایش می‌دهد
        // Convention: اگر نام View مشخص نشود، View با همین نام Action جستجو می‌شود
        public IActionResult Index() {
            // View("Cubed"): نمایش View به نام Cubed (در Views/Shared/Cubed.cshtml)
            return View("Cubed");
        }

        // Action Cube: محاسبه‌ی مکعب یک عدد
        // num: عدد ورودی که از query string یا فرم دریافت می‌شود
        // مثلاً: /Cubed/Cube?num=5
        public IActionResult Cube(double num) {
            // TempData: مجموعه‌ای از داده‌ها که فقط برای درخواست بعدی در دسترس است
            // ذخیره‌ی مقدار ورودی (num) به صورت رشته در TempData با کلید "value"
            TempData["value"] = num.ToString();
            // محاسبه‌ی مکعب عدد و ذخیره در TempData با کلید "result"
            // Math.Pow(num, 3): عدد num را به توان 3 می‌رساند
            TempData["result"] = Math.Pow(num, 3).ToString();
            // RedirectToAction: تغییر مسیر (redirect) به Action Index همین کنترلر
            // nameof(Index) نام Action را به صورت امن (refactor-safe) مشخص می‌کند
            // یعنی اگر نام Action تغییر کند، اینجا هم به صورت خودکار به‌روزرسانی می‌شود
            return RedirectToAction(nameof(Index));
        }
    }
}
