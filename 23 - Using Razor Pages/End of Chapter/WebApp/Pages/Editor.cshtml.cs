// =============================================================================
// فایل Pages/Editor.cshtml.cs — PageModel ویرایش محصول
// =============================================================================
// این صفحه هم GET (نمایش فرم) و هم POST (ذخیره تغییرات) را مدیریت می‌کند.
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Models;

namespace WebApp.Pages {
    public class EditorModel : PageModel {
        private DataContext context;

        public Product? Product { get; set; }

        public EditorModel(DataContext ctx) {
            context = ctx;
        }

        // OnGetAsync: نمایش فرم ویرایش با داده‌های فعلی محصول
        public async Task OnGetAsync(long id) {
            Product = await context.Products.FindAsync(id);
        }

        // OnPostAsync: ذخیره قیمت جدید
        // id: از مسیر URL  |  price: از بدنه فرم
        // RedirectToPage(): Redirect به همان صفحه
        public async Task<IActionResult> OnPostAsync(long id, decimal price) {
            Product? p = await context.Products.FindAsync(id);
            if (p != null) {
                p.Price = price;
            }
            await context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}
