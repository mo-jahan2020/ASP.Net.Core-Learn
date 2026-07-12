// =====================================================================
// SuppliersController.cs - کنترلر API برای تأمین‌کنندگان
// =====================================================================
// این کنترلر شامل متدهایی برای دریافت و به‌روزرسانی تأمین‌کنندگان است.
// نکته‌ی مهم: این کنترلر از JsonPatch پشتیبانی می‌کند.
// JsonPatch استانداردی برای توصیف تغییرات جزئی روی یک منبع JSON است.
// =====================================================================

 using Microsoft.AspNetCore.Mvc; // برای کنترلرها
 using WebApp.Models; // برای Supplier, DataContext
 using Microsoft.EntityFrameworkCore; // برای Include, FirstAsync, FindAsync
 using Microsoft.AspNetCore.JsonPatch; // برای استفاده از JsonPatchDocument

// تعریف namespace
namespace WebApp.Controllers {

    // [ApiController]: این یک Web API Controller است
    // [Route("api/[controller]")]: مسیر پایه = "api/suppliers"
    [ApiController]
    [Route("api/[controller]")]
    public class SuppliersController : ControllerBase {
        // فیلد خصوصی برای نگهداری DataContext
        private DataContext context;

        // سازنده
        public SuppliersController(DataContext ctx) {
            context = ctx;
        }

        // READ - دریافت تأمین‌کننده به همراه محصولاتش
        // [HttpGet("{id}")]: متد GET با پارامتر id در URL
        // مثلاً: GET /api/suppliers/1
        [HttpGet("{id}")]
        public async Task<Supplier?> GetSupplier(long id) {
            // Include(s => s.Products): بارگذاری اطلاعات محصولات مرتبط (Eager Loading)
            //   بدون Include، Products خالی می‌ماند (Lazy Loading)
            // FirstAsync(s => s.SupplierId == id): اولین تأمین‌کننده با شناسه‌ی مشخص
            Supplier supplier = await context.Suppliers.Include(s => s.Products)
                .FirstAsync(s => s.SupplierId == id);
            // حذف ارجاع معکوس برای جلوگیری از حلقه‌ی بی‌نهایت در JSON
            // هر محصول دارای Supplier است و هر Supplier دارای Products
            //   که هر کدام Supplier دارند (حلقه!)
            // با null کردن Supplier در هر محصول، این حلقه شکسته می‌شود
            if (supplier.Products != null) {
                foreach (Product p in supplier.Products) {
                    p.Supplier = null;
                };
            }
            return supplier;
        }

        // UPDATE - به‌روزرسانی جزئی تأمین‌کننده با استفاده از JSON Patch
        // [HttpPatch("{id}")]: متد HTTP PATCH با پارامتر id
        // مثلاً: PATCH /api/suppliers/1
        // JsonPatch استاندارد RFC 6902 برای توصیف تغییرات روی سند JSON است
        // patchDoc: سند JSON Patch که شامل لیست عملیات (op) است
        [HttpPatch("{id}")]
        public async Task<Supplier?> PatchSupplier(long id,
                JsonPatchDocument<Supplier> patchDoc) {
            // پیدا کردن تأمین‌کننده با شناسه‌ی مشخص
            Supplier? s = await context.Suppliers.FindAsync(id);
            // اگر تأمین‌کننده پیدا شد، تغییرات را اعمال کن
            if (s != null) {
                // ApplyTo: اعمال عملیات‌های JSON Patch بر روی شیء s
                //   مثلاً: [{ "op": "replace", "path": "/Name", "value": "New Name" }]
                patchDoc.ApplyTo(s);
                // ذخیره‌ی تغییرات در پایگاه داده
                await context.SaveChangesAsync();
            }
            return s;
        }
    }
}
