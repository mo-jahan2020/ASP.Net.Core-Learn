// =====================================================================
// Index.cshtml.cs - کلاس پشت‌صحنه‌ی Razor Page با نام Index
// =====================================================================
// در الگوی Razor Pages، هر صفحه دارای یک فایل .cshtml (View) و یک فایل
// .cshtml.cs (PageModel) است. PageModel شامل منطق صفحه و Handlerهاست.
// Handlerها متدهایی هستند که به درخواست‌های HTTP (GET, POST, ...) پاسخ می‌دهند.
// نام‌گذاری Handlerها: OnGetAsync, OnPostAsync, OnPut, ...
// =====================================================================

using Microsoft.AspNetCore.Mvc.RazorPages; // برای PageModel
using WebApp.Models; // برای Product و DataContext
using Microsoft.AspNetCore.Mvc; // برای IActionResult

// تعریف namespace مربوط به Pages
namespace WebApp.Pages {
    // تعریف کلاس PageModel برای صفحه‌ی Index
    // هر PageModel از کلاس PageModel ارث‌بری می‌کند
    public class IndexModel : PageModel {
        // فیلد خصوصی برای نگهداری DataContext
        private DataContext context;

        // ویژگی عمومی برای نگهداری محصول جاری
        // در Razor می‌توان با @Model.Product به این ویژگی دسترسی داشت
        // Product? یعنی می‌تواند null باشد
        public Product? Product { get; set; }

        // سازنده - DataContext از طریق DI تزریق می‌شود
        public IndexModel(DataContext ctx) {
            context = ctx;
        }

        // Handler مربوط به درخواست GET
        // نام "OnGet" به این معنی است که این متد با درخواست GET فراخوانی می‌شود
        // "Async" پسوند اختیاری برای متدهای ناهمزمان است
        // id: پارامتر اختیاری (پیش‌فرض 1) که از URL یا Query String می‌آید
        public async Task<IActionResult> OnGetAsync(long id = 1) {
            // پیدا کردن محصول با شناسه‌ی مشخص
            Product = await context.Products.FindAsync(id);
            // اگر محصولی پیدا نشد، به صفحه‌ی NotFound هدایت می‌شویم
            // RedirectToPage("NotFound"): یک Redirect به Razor Page دیگر
            if (Product == null) {
                return RedirectToPage("NotFound");
            }
            // return Page(): نمایش همان صفحه (Index.cshtml) با Model فعلی
            return Page();
        }
    }
}
