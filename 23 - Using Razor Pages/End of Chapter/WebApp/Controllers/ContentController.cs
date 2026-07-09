// =============================================================================
// فایل Controllers/ContentController.cs — کنترلر API محتوا و فرمت
// =============================================================================
// این کنترلر مفاهیم مهم زیر را آموزش می‌دهد:
// ۱. بازگرداندن رشته ساده از API
// ۲. Content Negotiation (مذاکره محتوا) — JSON و XML
// ۳. [FormatFilter] — انتخاب فرمت از مسیر URL
// ۴. [Consumes] — محدود کردن نوع محتوای ورودی
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;

namespace WebApp.Controllers {

    [ApiController]
    [Route("/api/[controller]")]
    public class ContentController : ControllerBase {
        private DataContext context;

        public ContentController(DataContext dataContext) {
            context = dataContext;
        }

        // -----------------------------------------------------------------
        // GET api/Content/string — بازگرداندن رشته ساده
        // -----------------------------------------------------------------
        // وقتی نوع بازگشت string باشد، ASP.NET Core آن را به صورت text/plain
        // برمی‌گرداند (نه JSON). این تفاوت مهمی با بازگرداندن شیء دارد.
        [HttpGet("string")]
        public string GetString() => "This is a string response";

        // -----------------------------------------------------------------
        // GET api/Content/object/json یا /object/xml — Content Negotiation
        // -----------------------------------------------------------------
        // [FormatFilter]: فرمت را از بخش آخر مسیر URL می‌خواند.
        //   مثال: /api/Content/object/json → فرمت json
        //         /api/Content/object/xml  → فرمت xml
        // [Produces]: فرمت‌های خروجی مجاز را مشخص می‌کند.
        //   ASP.NET Core بر اساس Accept header درخواست، فرمت مناسب را انتخاب می‌کند.
        // بازگشت ProductBindingTarget: خودکار به JSON یا XML تبدیل می‌شود.
        [HttpGet("object/{format?}")]
        [FormatFilter]
        [Produces("application/json", "application/xml")]
        public async Task<ProductBindingTarget> GetObject() {
            Product p = await context.Products.FirstAsync();
            return new ProductBindingTarget() {
                Name = p.Name, Price = p.Price, CategoryId = p.CategoryId,
                SupplierId = p.SupplierId
            };
        }

        // -----------------------------------------------------------------
        // POST api/Content — ذخیره محصول (فقط JSON)
        // -----------------------------------------------------------------
        // [Consumes("application/json")]: فقط درخواست‌های JSON قبول می‌شود.
        //   اگر کلاینت XML ارسال کند، خطای 415 Unsupported Media Type دریافت می‌کند.
        [HttpPost]
        [Consumes("application/json")]
        public string SaveProductJson(ProductBindingTarget product) {
            return $"JSON: {product.Name}";
        }

        // مثال کامنت‌شده: دریافت XML
        // [Consumes("application/xml")]: فقط XML قبول می‌شود
        //[HttpPost]
        //[Consumes("application/xml")]
        //public string SaveProductXml(ProductBindingTarget product) {
        //    return $"XML: {product.Name}";
        //}
    }
}
