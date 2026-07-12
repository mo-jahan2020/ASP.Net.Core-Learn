// =====================================================================
// SecondController.cs - کنترلر ساده برای نمایش View مشترک
// =====================================================================
// این کنترلر نشان می‌دهد چگونه می‌توان یک View مشترک (Shared) را
// از کنترلرهای مختلف نمایش داد. View "Common" در Views/Shared قرار دارد.
// =====================================================================

 using Microsoft.AspNetCore.Mvc; // برای کنترلرها

// تعریف namespace
namespace WebApp.Controllers {

    // تعریف کلاس SecondController
    public class SecondController : Controller {

        // Action Index: تنها Action این کنترلر
        // مسیر پیش‌فرض: /Second/Index
        public IActionResult Index() {
            // View("Common"): نمایش View به نام "Common"
            // ASP.NET Core مسیر را به ترتیب در این مکان‌ها جستجو می‌کند:
            //   1. Views/Second/Common.cshtml (مختص همین کنترلر)
            //   2. Views/Shared/Common.cshtml (مشترک بین کنترلرها)
            // در اینجا چون View اختصاصی وجود ندارد، View مشترک استفاده می‌شود
            return View("Common");
        }
    }
}
