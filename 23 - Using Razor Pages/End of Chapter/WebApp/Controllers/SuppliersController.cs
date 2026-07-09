// =============================================================================
// فایل Controllers/SuppliersController.cs — کنترلر API تأمین‌کنندگان
// =============================================================================
// این کنترلر دو مفهوم مهم را نشان می‌دهد:
// ۱. Eager Loading با Include (بارگذاری روابط)
// ۲. JSON Patch (بروزرسانی جزئی با HttpPatch)
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch; // برای JsonPatchDocument

namespace WebApp.Controllers {

    [ApiController]
    [Route("api/[controller]")]
    public class SuppliersController : ControllerBase {
        private DataContext context;

        public SuppliersController(DataContext ctx) {
            context = ctx;
        }

        // -----------------------------------------------------------------
        // GET api/Suppliers/{id} — دریافت تأمین‌کننده با محصولات
        // -----------------------------------------------------------------
        // Include(s => s.Products): Eager Loading — بارگذاری محصولات همراه تأمین‌کننده
        //   بدون Include، خاصیت Products null خواهد بود.
        // FirstAsync: اولین رکورد مطابق شرط را برمی‌گرداند (اگر نباشد خطا).
        // p.Supplier = null: حذف ارجاع معکوس برای جلوگیری از Circular Reference در JSON.
        [HttpGet("{id}")]
        public async Task<Supplier?> GetSupplier(long id) {
            Supplier supplier = await context.Suppliers.Include(s => s.Products)
                .FirstAsync(s => s.SupplierId == id);
            if (supplier.Products != null) {
                foreach (Product p in supplier.Products) {
                    p.Supplier = null; // قطع ارجاع معکوس
                };
            }
            return supplier;
        }

        // -----------------------------------------------------------------
        // PATCH api/Suppliers/{id} — بروزرسانی جزئی (JSON Patch)
        // -----------------------------------------------------------------
        // HTTP PATCH: فقط فیلدهای مشخص‌شده را بروزرسانی می‌کند (نه کل شیء).
        // JsonPatchDocument<Supplier>: فرمت استاندارد JSON Patch (RFC 6902).
        //   مثال بدنه درخواست: [{"op": "replace", "path": "/City", "value": "Tehran"}]
        // ApplyTo(s): تغییرات Patch را روی شیء اعمال می‌کند.
        // مزیت: نیاز به ارسال کل شیء نیست — فقط فیلدهای تغییری.
        [HttpPatch("{id}")]
        public async Task<Supplier?> PatchSupplier(long id,
                JsonPatchDocument<Supplier> patchDoc) {
            Supplier? s = await context.Suppliers.FindAsync(id);
            if (s != null) {
                patchDoc.ApplyTo(s);
                await context.SaveChangesAsync();
            }
            return s;
        }
    }
}
