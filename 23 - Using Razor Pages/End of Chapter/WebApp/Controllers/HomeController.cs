// =============================================================================
// فایل Controllers/HomeController.cs — کنترلر MVC (نه API)
// =============================================================================
// این کنترلر از Controller (نه ControllerBase) ارث‌بری می‌کند.
// Controller برای MVC با View مناسب است و متدهای کمکی مثل View() دارد.
// مسیردهی: {controller=Home}/{action=Index}/{id?}
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Controllers {

    // از Controller ارث‌بری (نه ControllerBase) — مخصوص MVC با View
    // بدون [ApiController] و [Route] — از مسیردهی پیش‌فرض MVC استفاده می‌کند
    public class HomeController : Controller {
        // دسترسی به دیتابیس از طریق DI
        private DataContext context;

        // Constructor Injection — تزریق خودکار DataContext
        public HomeController(DataContext ctx) {
            context = ctx;
        }

        // -----------------------------------------------------------------
        // GET /Home/Index/1 — نمایش یک محصول
        // -----------------------------------------------------------------
        // id: پارامتر مسیر اختیاری با مقدار پیش‌فرض ۱
        // ViewBag.AveragePrice: میانگین قیمت همه محصولات (منتقل به View)
        //   ViewBag: شیء پویا برای انتقال داده از Controller به View
        //   (تفاوت با Model: ViewBag نوع‌امن نیست اما انعطاف‌پذیرتر است)
        // View(product): فایل Views/Home/Index.cshtml با Model از نوع Product
        public async Task<IActionResult> Index(long id = 1) {
            ViewBag.AveragePrice =
                await context.Products.AverageAsync(p => p.Price);
            return View(await context.Products.FindAsync(id));
        }

        // -----------------------------------------------------------------
        // GET /Home/List — نمایش لیست همه محصولات
        // -----------------------------------------------------------------
        // View(products): فایل Views/Home/List.cshtml با Model از نوع IEnumerable<Product>
        // کل کوئری (بدون ToList) ارسال می‌شود — EF Core آن را lazy بارگذاری می‌کند
        public IActionResult List() {
            return View(context.Products);
        }

        // -----------------------------------------------------------------
        // GET /Home/Html — نمایش رشته HTML خام
        // -----------------------------------------------------------------
        // (object)"...": تبدیل صریح به object تا View<string> فراخوانی نشود
        // و View متناظر با نام اکشن (Html.cshtml) اجرا شود
        public IActionResult Html() {
            return View((object)"This is a <h3><i>string</i></h3>");
        }
    }
}
