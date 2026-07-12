// =====================================================================
// HomeController.cs - کنترلر اصلی برنامه
// =====================================================================
// این کنترلر شامل Actionهای پایه‌ای برای نمایش محصولات است.
// از الگوی MVC کلاسیک استفاده می‌کند (Controller + View + Model).
// =====================================================================

using Microsoft.AspNetCore.Mvc; // برای کنترلرها
using WebApp.Models; // برای Product و DataContext
using Microsoft.EntityFrameworkCore; // برای AverageAsync و FindAsync

// تعریف namespace
namespace WebApp.Controllers
{

    // تعریف کلاس HomeController
    // طبق Convention، کنترلر Home به طور پیش‌فرض مسیر اصلی ("/") را مدیریت می‌کند
    public class HomeController : Controller
    {
        // فیلد خصوصی برای نگهداری DataContext
        private DataContext context;

        // سازنده‌ی کنترلر
        public HomeController(DataContext ctx)
        {
            context = ctx;
        }

        // Action Index: صفحه‌ی اصلی
        // مسیر: /Home/Index یا فقط /
        // id: پارامتر اختیاری (پیش‌فرض 1) برای شناسه‌ی محصول
        public async Task<IActionResult> Index(long id = 1)
        {
            // محاسبه‌ی میانگین قیمت محصولات به صورت ناهمزمان
            // AverageAsync: محاسبه‌ی میانگین در سطح پایگاه داده (کارآمدتر از کلاینت)
            // نتیجه در ViewBag ذخیره می‌شود تا در View قابل دسترسی باشد
            ViewBag.AveragePrice = await context.Products.AverageAsync(p => p.Price);

            // پیدا کردن محصول با شناسه‌ی مشخص‌شده به صورت ناهمزمان
            // FindAsync: ابتدا حافظه‌ی داخلی را جستجو می‌کند، سپس پایگاه داده
            // return View: برگرداندن View به همراه شیء Product به عنوان Model
            return View(await context.Products.FindAsync(id));
        }

        // Action List: نمایش لیست تمام محصولات
        public IActionResult List()
        {
            // برگرداندن View به همراه لیست محصولات (IEnumerable<Product>) به عنوان Model
            return View(context.Products);
        }

        // Action Html: نمایش یک رشته‌ی HTML به صورت خام
        // هدف: نمایش نحوه‌ی استفاده از Html.Raw برای جلوگیری از HTML Encoding
        public IActionResult Html()
        {
            // (object) cast لازم است؛ چون View(object) overloadهای متعددی دارد
            return View((object)"This is a <h3><i>string</i></h3>");
        }
    }
}
