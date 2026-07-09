// =============================================================================
// فایل Pages/Index.cshtml.cs — PageModel صفحه اصلی
// =============================================================================
// PageModel: کلاس پشت‌صحنه Razor Page (مشابه Controller در MVC اما مختص یک صفحه).
// هر Razor Page می‌تواند یک PageModel دارد که منطق صفحه در آن قرار می‌گیرد.
// =============================================================================

using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Pages {
    public class IndexModel : PageModel {
        private DataContext context;

        // محصولی که باید نمایش داده شود
        public Product? Product { get; set; }

        // Constructor Injection
        public IndexModel(DataContext ctx) {
            context = ctx;
        }

        // OnGetAsync: متد Handler برای درخواست‌های GET
        // نام‌گذاری: On + [HttpMethod] + Async
        // id: پارامتر مسیر با مقدار پیش‌فرض ۱
        // Page(): رندر کردن Razor Page متناظر
        // RedirectToPage("NotFound"): Redirect اگر محصول وجود نداشته باشد
        public async Task<IActionResult> OnGetAsync(long id = 1) {
            Product = await context.Products.FindAsync(id);
            if (Product == null) {
                return RedirectToPage("NotFound");
            }
            return Page();
        }
    }
}
