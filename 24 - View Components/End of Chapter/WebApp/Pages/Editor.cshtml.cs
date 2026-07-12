// =====================================================================
// Editor.cshtml.cs - PageModel برای صفحه‌ی ویرایش محصول
// =====================================================================
// این صفحه یک محصول را بر اساس id دریافتی نمایش می‌دهد و امکان
// ویرایش قیمت آن را فراهم می‌کند. شامل دو Handler است:
//   1. OnGetAsync: برای نمایش محصول
//   2. OnPostAsync: برای ذخیره‌ی قیمت جدید
// =====================================================================

using Microsoft.AspNetCore.Mvc; // برای PageModel و IActionResult
using Microsoft.AspNetCore.Mvc.RazorPages; // فضای نام Razor Pages
using WebApp.Models; // برای Product و DataContext

// تعریف namespace
namespace WebApp.Pages {
    // تعریف کلاس PageModel
    public class EditorModel : PageModel {
        // فیلد خصوصی برای نگهداری DataContext
        private DataContext context;

        // ویژگی عمومی برای نگهداری محصول جاری
        public Product? Product { get; set; }

        // سازنده
        public EditorModel(DataContext ctx) {
            context = ctx;
        }

        // Handler مربوط به درخواست GET
        // id: شناسه‌ی محصول از URL دریافت می‌شود (مثلاً /Editor/5)
        public async Task OnGetAsync(long id) {
            // پیدا کردن محصول با شناسه‌ی مشخص و ذخیره در ویژگی Product
            Product = await context.Products.FindAsync(id);
        }

        // Handler مربوط به درخواست POST
        // این متد زمانی فراخوانی می‌شود که فرم HTML با method="post" ارسال شود
        // پارامترها: id از URL و price از فرم
        public async Task<IActionResult> OnPostAsync(long id, decimal price) {
            // پیدا کردن محصول با شناسه
            Product? p = await context.Products.FindAsync(id);
            // اگر محصول پیدا شد، قیمت آن را به‌روزرسانی کن
            if (p != null) {
                p.Price = price;
            }
            // ذخیره‌ی تغییرات در پایگاه داده
            await context.SaveChangesAsync();
            // RedirectToPage(): هدایت مجدد به همین صفحه (برای الگوی Post-Redirect-Get)
            //   این الگو از ارسال مجدد فرم با کلیک دکمه‌ی Refresh جلوگیری می‌کند
            return RedirectToPage();
        }
    }
}
