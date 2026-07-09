// =============================================================================
// فایل Controllers/ProductsController.cs — کنترلر API دوم برای مدیریت محصولات
// =============================================================================
// این کنترلر مشابه HomeController است اما تفاوت‌هایی دارد:
// ۱. مسیر آن "api/Products" است (نام کنترلر در [Route] جایگزین می‌شود)
// ۲. متد Delete به گونه‌ای متفاوت پیاده‌سازی شده (بدون بررسی وجود محصول)
//
// وجود دو کنترلر با عملیات مشابه برای آموزش مفاهیم مختلف Routing و
// الگوهای مختلف پیاده‌سازی CRUD است.
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

namespace WebApp.Controllers
{

    // -------------------------------------------------------------------------
    // [ApiController]: فعال‌سازی ویژگی‌های خودکار API Controller
    // [Route("api/[controller]")]: مسیر پایه این کنترلر "api/Products" خواهد بود
    //   زیرا نام کلاس "ProductsController" است و پسوند "Controller" حذف می‌شود.
    // -------------------------------------------------------------------------
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        // ---------------------------------------------------------------------
        // فیلد context — دسترسی به دیتابیس از طریق Entity Framework Core
        // ---------------------------------------------------------------------
        private DataContext context;

        // ---------------------------------------------------------------------
        // Constructor Injection — تزریق DataContext از طریق سازنده
        // ---------------------------------------------------------------------
        // سیستم DI به صورت خودکار نمونه DataContext را هنگام ساخت این کنترلر تزریق می‌کند.
        public ProductsController(DataContext ctx)
        {
            context = ctx;
        }

        // =====================================================================
        // GET api/Products — دریافت لیست همه محصولات
        // =====================================================================
        // این متد مانند HomeController.GetPoroducts() کار می‌کند.
        // AsAsyncEnumerable: برای ارسال استریمی محصولات (یک‌یکی به جای همه با هم).
        [HttpGet]
        public IAsyncEnumerable<Product> GetPoroducts()
        {
            return context.Products.AsAsyncEnumerable();
        }

        // =====================================================================
        // GET api/Products/{id} — دریافت یک محصول با شناسه
        // =====================================================================
        // FindAsync: جستجو بر اساس کلید اصلی (ProductId).
        // اگر محصول یافت نشد، NotFound() یعنی HTTP 404 برمی‌گرداند.
        // اگر یافت شد، Ok(p) یعنی HTTP 200 به همراه داده محصول برمی‌گرداند.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(long id)
        {
            Product? p = await context.Products.FindAsync(id);
            if (p == null)
            {
                return NotFound();
            }
            return Ok(p);
        }

        // =====================================================================
        // POST api/Products — ایجاد محصول جدید
        // =====================================================================
        // ProductBindingTarget به جای Product مستقیم استفاده شده.
        // دلیل: جلوگیری از Over-posting (ارسال فیلدهای غیرمجاز مثل ProductId).
        // ToProduct(): متد تبدیل DTO به Entity در کلاس ProductBindingTarget تعریف شده.
        [HttpPost]
        public async Task<IActionResult> SaveProduct(ProductBindingTarget target)
        {
            Product p = target.ToProduct();
            await context.Products.AddAsync(p);
            await context.SaveChangesAsync();
            return Ok(p);
        }

        // =====================================================================
        // PUT api/Products — بروزرسانی محصول موجود
        // =====================================================================
        // نکته مهم: در اینجا کل شیء Product به عنوان پارامتر دریافت می‌شود.
        // ASP.NET Core بدنه درخواست (Request Body) را به Product دیسریالایز می‌کند.
        // context.Update(): محصول را در حالت Modified قرار می‌دهد.
        // پاسخ: HTTP 204 No Content (چون متد IActionResult برنمی‌گرداند).
        [HttpPut]
        public async Task UpdateProduct(Product product)
        {
            context.Update(product);
            await context.SaveChangesAsync();
        }

        // =====================================================================
        // DELETE api/Products/{id} — حذف محصول
        // =====================================================================
        // ⚠️ تفاوت مهم با HomeController:
        // در اینجا ابتدا بررسی نمی‌کند که آیا محصول وجود دارد یا خیر!
        // یک شیء Product جدید با فقط ProductId ساخته می‌شود و به Remove ارسال می‌شود.
        // EF Core این شیء را با استفاده از کلید اصلی در دیتابیس پیدا و حذف می‌کند.
        // این روش کوتاه‌تر است اما اگر محصول وجود نداشته باشد، خطا رخ می‌دهد.
        // در HomeController ابتدا FindAsync صدا زده می‌شود و اگر نبود NotFound() برمی‌گردد.
        [HttpDelete("{id}")]
        public async Task DeleteProduct(long id)
        {
            context.Products.Remove(new Product() { ProductId = id });
            await context.SaveChangesAsync();
        }

        // =====================================================================
        // GET api/Products/redirect — مثال Redirect به اکشن دیگر
        // =====================================================================
        // RedirectToAction: مرورگر را به URL دیگری هدایت می‌کند (HTTP 302/307).
        // nameof(GetProduct): ارجاع نوع‌امن به متد GetProduct.
        [HttpGet("redirect")]
        public IActionResult Redirect()
        {
            return RedirectToAction(nameof(GetProduct), new { Id = 1 });
        }
    }
}