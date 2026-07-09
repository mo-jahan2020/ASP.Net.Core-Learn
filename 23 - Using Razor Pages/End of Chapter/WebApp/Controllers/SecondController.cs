// =============================================================================
// فایل Controllers/SecondController.cs — کنترلر ساده با View اشتراکی
// =============================================================================
// این کنترلر نشان می‌دهد که چند کنترلر می‌توانند از یک View مشترک استفاده کنند.
// SecondController و HomeController هر دو از Common.cshtml استفاده می‌کنند.
// =============================================================================

using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers {

    public class SecondController : Controller {

        // GET /Second/Index — نمایش View مشترک
        // View("Common"): فایل Views/Shared/Common.cshtml
        // ASP.NET Core ابتدا در Views/Second/ جستجو می‌کند، سپس در Views/Shared/
        public IActionResult Index() {
            return View("Common");
        }
    }
}
