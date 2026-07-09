// =============================================================================
// فایل Controllers/ProductsController.cs — کنترلر API محصولات
// =============================================================================
// کنترلر RESTful API برای عملیات CRUD روی محصولات.
// مسیر پایه: api/Products (از [Route("api/[controller]")])
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

namespace WebApp.Controllers {

    // [ApiController]: فعال‌سازی اعتبارسنجی خودکار و استنتاج پارامترها
    // [Route("api/[controller]")]: مسیر پایه = api/Products
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase {
        private DataContext context;

        public ProductsController(DataContext ctx) {
            context = ctx;
        }

        // GET api/Products — دریافت همه محصولات (استریمی)
        // IAsyncEnumerable: ارسال تدریجی (Streaming) — کارآمد برای داده زیاد
        [HttpGet]
        public IAsyncEnumerable<Product> GetProducts() {
            return context.Products.AsAsyncEnumerable();
        }

        // GET api/Products/{id} — دریافت یک محصول
        // [ProducesResponseType]: مستندسازی پاسخ‌های ممکن (برای Swagger/OpenAPI)
        //   200: محصول یافت شد  |  404: محصول یافت نشد
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProduct(long id) {
            Product? p = await context.Products.FindAsync(id);
            if (p == null) {
                return NotFound(); // HTTP 404
            }
            return Ok(p); // HTTP 200 + داده
        }

        // POST api/Products — ایجاد محصول جدید
        // ProductBindingTarget: DTO — جلوگیری از Over-posting
        [HttpPost]
        public async Task<IActionResult>
                SaveProduct(ProductBindingTarget target) {
            Product p = target.ToProduct(); // تبدیل DTO به Entity
            await context.Products.AddAsync(p);
            await context.SaveChangesAsync();
            return Ok(p);
        }

        // PUT api/Products — بروزرسانی محصول
        // پاسخ: HTTP 204 No Content (چون IActionResult برنمی‌گرداند)
        [HttpPut]
        public async Task UpdateProduct(Product product) {
            context.Update(product);
            await context.SaveChangesAsync();
        }

        // DELETE api/Products/{id} — حذف محصول
        // ⚠️ بدون بررسی وجود — اگر نباشد خطا رخ می‌دهد
        [HttpDelete("{id}")]
        public async Task DeleteProduct(long id) {
            context.Products.Remove(new Product() { ProductId = id });
            await context.SaveChangesAsync();
        }

        // GET api/Products/redirect — مثال Redirect
        // nameof: ارجاع نوع‌امن (Refactoring-safe)
        [HttpGet("redirect")]
        public IActionResult Redirect() {
            return RedirectToAction(nameof(GetProduct), new { Id = 1 });
        }
    }
}
