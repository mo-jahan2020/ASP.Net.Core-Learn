// =====================================================================
// ProductsController.cs - کنترلر API برای محصولات
// =====================================================================
// این یک Web API Controller کامل است که عملیات CRUD را برای محصولات
// فراهم می‌کند. از Attributeهای مسیردهی برای تعریف مسیرها استفاده می‌شود.
// CRUD: Create, Read, Update, Delete
// =====================================================================

 using Microsoft.AspNetCore.Mvc; // برای کنترلرها
 using WebApp.Models; // برای Product, DataContext, ProductBindingTarget

// تعریف namespace
namespace WebApp.Controllers {

    // [ApiController]: این یک Web API Controller است
    // [Route("api/[controller]")]: مسیر پایه = "api/products"
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase {
        // فیلد خصوصی برای نگهداری DataContext
        private DataContext context;

        // سازنده
        public ProductsController(DataContext ctx) {
            context = ctx;
        }

        // READ - دریافت لیست تمام محصولات
        // [HttpGet]: متد HTTP GET (بدون مسیر اضافی = مسیر پایه)
        // IAsyncEnumerable<T>: نوعی که امکان پیمایش ناهمزمان را فراهم می‌کند
        //   AsAsyncEnumerable(): تمام محصولات را به صورت ناهمزمان برمی‌گرداند
        [HttpGet]
        public IAsyncEnumerable<Product> GetProducts() {
            return context.Products.AsAsyncEnumerable();
        }

        // READ - دریافت یک محصول با شناسه
        // [HttpGet("{id}")]: متد GET با پارامتر id در URL
        //   مثلاً: GET /api/products/5
        // [ProducesResponseType]: مستندسازی کدهای پاسخ ممکن (برای OpenAPI/Swagger)
        //   Status200OK: یعنی محصول پیدا شد (پاسخ موفق)
        //   Status404NotFound: یعنی محصولی با آن شناسه وجود ندارد
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProduct(long id) {
            // FindAsync: جستجوی محصول با کلید اصلی به صورت ناهمزمان
            Product? p = await context.Products.FindAsync(id);
            // اگر محصولی پیدا نشد (null بود)، کد 404 برگردان
            if (p == null) {
                return NotFound();
            }
            // در غیر این صورت، محصول را با کد 200 (OK) برگردان
            return Ok(p);
        }

        // CREATE - ایجاد یک محصول جدید
        // [HttpPost]: متد HTTP POST روی مسیر پایه
        // target: داده‌های ورودی که از body درخواست binding می‌شوند
        [HttpPost]
        public async Task<IActionResult>
                SaveProduct(ProductBindingTarget target) {
            // تبدیل DTO (ProductBindingTarget) به Product با استفاده از متد ToProduct
            Product p = target.ToProduct();
            // AddAsync: اضافه کردن محصول جدید به DbContext (در حافظه)
            await context.Products.AddAsync(p);
            // SaveChangesAsync: اعمال تغییرات بر روی پایگاه داده به صورت ناهمزمان
            await context.SaveChangesAsync();
            // برگرداندن محصول ایجادشده با کد 200 (OK)
            return Ok(p);
        }

        // UPDATE - به‌روزرسانی یک محصول موجود
        // [HttpPut]: متد HTTP PUT
        // محصول به‌روزرسانی‌شده از body درخواست دریافت می‌شود
        [HttpPut]
        public async Task UpdateProduct(Product product) {
            // Update: علامت‌گذاری محصول به‌عنوان Modified تا EF Core تغییرات را ذخیره کند
            context.Update(product);
            // ذخیره‌ی تغییرات در پایگاه داده
            await context.SaveChangesAsync();
        }

        // DELETE - حذف یک محصول
        // [HttpDelete("{id}")]: متد HTTP DELETE با پارامتر id
        //   مثلاً: DELETE /api/products/5
        [HttpDelete("{id}")]
        public async Task DeleteProduct(long id) {
            // ساخت یک شیء Product فقط با ProductId برای حذف
            // EF Core فقط با دانستن کلید اصلی می‌تواند رکورد را حذف کند
            context.Products.Remove(new Product() { ProductId = id });
            // اعمال حذف در پایگاه داده
            await context.SaveChangesAsync();
        }

        // READ با Redirect - این متد کاربرد آموزشی RedirectToAction را نشان می‌دهد
        // [HttpGet("redirect")]: متد GET روی مسیر "/api/products/redirect"
        [HttpGet("redirect")]
        public IActionResult Redirect() {
            // RedirectToAction: ارسال پاسخ HTTP 302 (Found) به کلاینت
            //   تا کلاینت به آدرس دیگری درخواست بفرستد
            //   nameof(GetProduct): نام متد GetProduct (refactor-safe)
            //   new { Id = 1 }: پارامترهایی که به Action پاس داده می‌شوند
            return RedirectToAction(nameof(GetProduct), new { Id = 1 });
        }
    }
}
