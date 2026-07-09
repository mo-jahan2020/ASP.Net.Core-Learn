// =============================================================================
// فایل Controllers/HomeController.cs — کنترلر API برای مدیریت محصولات
// =============================================================================
// این کنترلر عملیات CRUD (ایجاد، خواندن، بروزرسانی، حذف) را روی محصولات انجام می‌دهد.
// ما از الگوی "API Controller" استفاده می‌کنیم که مخصوص ساخت RESTful API است.
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
//using WebApp.Models.ViewModels;

namespace WebApp.Controllers
{

    // -------------------------------------------------------------------------
    // ویژگی [ApiController]
    // -------------------------------------------------------------------------
    // این ویژگی رفتارهای زیر را به صورت خودکار فعال می‌کند:
    // ۱. اعتبارسنجی خودکار Model State (اگر مدل نامعتبر باشد، 400 برمی‌گرداند)
    // ۲. استنتاج منبع پارامترها (FromBody, FromRoute, FromQuery به صورت خودکار)
    // ۳. فرمت‌دهی پیش‌فرض Problem Details برای خطاها
    [ApiController]

    // -------------------------------------------------------------------------
    // ویژگی [Route] — مسیردهی (Routing)
    // -------------------------------------------------------------------------
    // چند الگوی مسیردهی در اینجا وجود دارد (برخی کامنت شده‌اند):
    // - "api/Home1": مسیر ثابت — همه درخواست‌ها به api/Home1 می‌روند
    // - "api": فقط پیشوند api
    // - "api/[controller]": مسیر پویا — [controller] با نام کنترلر جایگزین می‌شود
    //   در اینجا "Home" می‌شود، پس مسیر پایه "api/Home" خواهد بود
    // - "api/_Products": می‌توانید نام دلخواه بگذارید (با _ شروع نشود)
    //[Route("api/Home1")]
    //[Route("api")]
    [Route("api/[controller]")]
    //[Route("api/_Products")]

    // -------------------------------------------------------------------------
    // کلاس HomeController — کنترلر API
    // -------------------------------------------------------------------------
    // از ControllerBase ارث‌بری می‌کند (نه Controller).
    // ControllerBase مخصوص API است و شامل متدهای کمکی مثل Ok(), NotFound(), BadRequest() است.
    // Controller برای MVC با View است و ControllerBase برای API خالص.
    public class HomeController : ControllerBase
    {

        // ---------------------------------------------------------------------
        // فیلد context — دسترسی به دیتابیس
        // ---------------------------------------------------------------------
        // DataContext نمونه EF Core DbContext است که عملیات دیتابیس از طریق آن انجام می‌شود.
        private DataContext context;

        // ---------------------------------------------------------------------
        // Constructor — تزریق وابستگی (Dependency Injection)
        // ---------------------------------------------------------------------
        // ASP.NET Core به صورت خودکار نمونه DataContext را از سیستم DI تزریق می‌کند.
        // این الگو "Constructor Injection" نام دارد و بهترین روش تزریق وابستگی است.
        public HomeController(DataContext ctx)
        {
            context = ctx;
        }

        // =====================================================================
        // GET api/Home — دریافت همه محصولات
        // =====================================================================
        // [HttpGet] بدون پارامتر: این متد به درخواست‌های GET به مسیر پایه پاسخ می‌دهد.
        // IAsyncEnumerable: بازده استریم (Streaming) نتایج.
        //   به جای اینکه همه محصولات در حافظه بارگذاری شوند، یکی‌یکی ارسال می‌شوند.
        //   این روش برای داده‌های زیاد بسیار کارآمدتر است.
        // AsAsyncEnumerable(): کوئری EF Core را به IAsyncEnumerable تبدیل می‌کند.
        [HttpGet]
        public IAsyncEnumerable<Product> GetPoroducts()
        {
            return context.Products.AsAsyncEnumerable();
        }

        // =====================================================================
        // GET api/Home/{id} — دریافت یک محصول با شناسه
        // =====================================================================
        // [HttpGet("{id}")]: این متد به درخواست‌های GET با پارامتر id در مسیر پاسخ می‌دهد.
        //   مثال: GET api/Home/1 → id برابر 1
        // long id: شناسه محصول از مسیر URL استخراج می‌شود (Model Binding).
        // FindAsync(id): محصول را با کلید اصلی (Primary Key) پیدا می‌کند.
        //   این متد ابتدا در حافظه (Change Tracker) جستجو می‌کند و اگر نبود، به دیتابیس می‌رود.
        // Product?: علامت ? یعنی این متغیر می‌تواند null باشد (Nullable Reference Type).
        // NotFound(): پاسخ HTTP 404 برمی‌گرداند.
        // Ok(p): پاسخ HTTP 200 به همراه داده محصول برمی‌گرداند.
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
        // POST api/Home — ایجاد محصول جدید
        // =====================================================================
        // [HttpPost]: این متد به درخواست‌های POST پاسخ می‌دهد.
        // ProductBindingTarget: یک DTO (Data Transfer Object) است که فقط فیلدهای
        //   مورد نیاز برای ایجاد محصول را دریافت می‌کند (نه ProductId).
        //   این کار امنیت را بالا می‌برد زیرا کاربر نمی‌تواند ProductId را دستکاری کند.
        // target.ToProduct(): DTO را به موجودیت Product تبدیل می‌کند.
        // AddAsync(p): محصول جدید را به DbContext اضافه می‌کند (هنوز در دیتابیس ذخیره نشده).
        // SaveChangesAsync(): تغییرات را در دیتابیس ذخیره می‌کند (INSERT اجرا می‌شود).
        [HttpPost]
        public async Task<IActionResult> SaveProduct(ProductBindingTarget target)
        {
            Product p = target.ToProduct();
            await context.Products.AddAsync(p);
            await context.SaveChangesAsync();
            return Ok(p);
        }

        // =====================================================================
        // PUT api/Home — بروزرسانی محصول
        // =====================================================================
        // [HttpPut]: این متد به درخواست‌های PUT پاسخ می‌دهد.
        // Update(product): موجودیت را در حالت "Modified" قرار می‌دهد.
        //   EF Core در زمان SaveChangesAsync، فقط فیلدهای تغییر کرده را بروزرسانی می‌کند.
        // نکته: این متد IActionResult برنمی‌گرداند. ASP.NET Core در این حالت
        //   پاسخ HTTP 204 (No Content) برمی‌گرداند.
        [HttpPut]
        public async Task UpdateProduct(Product product)
        {
            context.Update(product);
            await context.SaveChangesAsync();
        }

        // =====================================================================
        // DELETE api/Home/Delete({id}) — حذف محصول
        // =====================================================================
        // تغییر این بخش برای ایجاد مسیر سفارشی
        // می‌توانید الگوهای مختلف مسیر حذف را امتحان کنید:
        // - "Delete({id})": مسیر becomes api/Home/Delete/1
        // - "{id}": مسیر becomes api/Home/1
        // - "DeleteProduct({id})": مسیر becomes api/Home/DeleteProduct/1
        [HttpDelete("Delete({id})")]
        //[HttpDelete("{id}")]
        //[HttpDelete("DeleteProduct({id})")]

        // FindAsync(id): ابتدا محصول را پیدا می‌کند تا مطمئن شویم وجود دارد.
        // Remove(p): محصول را از DbContext حذف می‌کند (وضعیت Deleted).
        // SaveChangesAsync(): دستور DELETE در دیتابیس اجرا می‌شود.
        public async Task<IActionResult> DeleteProduct(long id)
        {
            Product? p = await context.Products.FindAsync(id);
            if (p == null)
            {
                return NotFound();
            }
            context.Products.Remove(p);
            await context.SaveChangesAsync();
            return Ok();
        }

        // =====================================================================
        // GET api/Home/redirect — مثال Redirect
        // =====================================================================
        // RedirectToAction: درخواست را به اکشن‌متد دیگری هدایت (Redirect) می‌کند.
        // nameof(GetProduct): از نام متد به جای رشته استفاده می‌کند تا در زمان
        //   تغییر نام متد، خطای کامپایل دریافت کنید (Refactoring-safe).
        // new { Id = 1 }: مقادیر پارامترهای مسیر مقصد را مشخص می‌کند.
        [HttpGet("redirect")]
        public IActionResult Redirect()
        {
            return RedirectToAction(nameof(GetProduct), new { Id = 1 });
        }

    }
}