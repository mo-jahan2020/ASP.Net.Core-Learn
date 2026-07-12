// =====================================================================
// ContentController.cs - کنترلر API برای محتوا
// =====================================================================
// این یک Web API Controller است که با استفاده از Attributeهای مسیردهی
// (Routing Attributes) عمل می‌کند. این کنترلر نحوه‌ی استفاده از ویژگی‌هایی
// مانند [FormatFilter] برای پشتیبانی همزمان JSON و XML را نشان می‌دهد.
// =====================================================================

 using Microsoft.AspNetCore.Mvc; // برای کنترلرها
 using Microsoft.EntityFrameworkCore; // برای FirstAsync و دیگر متدهای EF Core
 using WebApp.Models; // برای ProductBindingTarget و DataContext

// تعریف namespace
namespace WebApp.Controllers {

    // [ApiController]: این ویژگی نشان می‌دهد که این کنترلر یک Web API است
    //   مزایا: اعتبارسنجی خودکار، کدهای وضعیت HTTP مناسب، Content Negotiation
    // [Route("/api/[controller]")]: مسیر پایه‌ی کنترلر
    //   [controller] یک placeholder است که با نام کلاس (بدون "Controller") جایگزین می‌شود
    //   در اینجا مسیر نهایی برابر "/api/content" خواهد بود
    [ApiController]
    [Route("/api/[controller]")]
    public class ContentController : ControllerBase {
        // فیلد خصوصی برای نگهداری DataContext
        // این فیلد از طریق سازنده (Constructor) تزریق می‌شود
        private DataContext context;

        // سازنده‌ی کنترلر
        public ContentController(DataContext dataContext) {
            // ذخیره‌ی DataContext در فیلد برای استفاده در Actionها
            context = dataContext;
        }

        // [HttpGet("string")]: متد GET روی مسیر "/api/content/string"
        // این متد فقط یک رشته (string) ساده برمی‌گرداند
        // Expression-Bodied: استفاده از => برای تعریف خلاصه‌ی متد
        [HttpGet("string")]
        public string GetString() => "This is a string response";

        // [HttpGet("object/{format?}")]: متد GET روی مسیر "/api/content/object"
        //   {format?}: پارامتر اختیاری در URL (مثلاً json یا xml)
        // [FormatFilter]: این فیلتر به کلاینت اجازه می‌دهد تا فرمت پاسخ
        //   را با استفاده از query string یا URL تعیین کند
        //   مثلاً: /api/content/object.json یا /api/content/object?format=xml
        // [Produces("application/json", "application/xml")]:
        //   این کنترلر فقط می‌تواند خروجی JSON یا XML تولید کند
        [HttpGet("object/{format?}")]
        [FormatFilter]
        [Produces("application/json", "application/xml")]
        public async Task<ProductBindingTarget> GetObject() {
            // FirstAsync: اولین محصول از جدول Products را به صورت ناهمزمان برمی‌گرداند
            Product p = await context.Products.FirstAsync();
            // برگرداندن یک شیء ProductBindingTarget که فقط شامل فیلدهای مشخص است
            // این کار به جداسازی (decoupling) مدل داخلی و DTO خارجی کمک می‌کند
            return new ProductBindingTarget() {
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId,
                SupplierId = p.SupplierId
            };
        }

        // [HttpPost]: متد POST روی مسیر "/api/content" برای ایجاد/ذخیره‌ی محصول
        // [Consumes("application/json")]: این متد فقط داده‌های JSON دریافت می‌کند
        //   اگر Content-Type درخواست متفاوت باشد، خطای 415 (Unsupported Media Type) برمی‌گردد
        [HttpPost]
        [Consumes("application/json")]
        public string SaveProductJson(ProductBindingTarget product) {
            // برگرداندن یک پاسخ ساده متنی که نام محصول دریافتی را نشان می‌دهد
            // داده‌های binding شده در پارامتر product در دسترس هستند
            return $"JSON: {product.Name}";
        }

        // این بخش کد (مربوط به XML) کامنت شده است
        // برای فعال‌سازی پشتیبانی از XML، خطوط زیر را از حالت کامنت خارج کنید
        //[HttpPost]
        //[Consumes("application/xml")]
        //public string SaveProductXml(ProductBindingTarget product) {
        //    return $"XML: {product.Name}";
        //}
    }
}
