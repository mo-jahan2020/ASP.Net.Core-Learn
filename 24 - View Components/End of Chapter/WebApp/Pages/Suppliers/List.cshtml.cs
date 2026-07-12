// =====================================================================
// List.cshtml.cs - PageModel خالی
// =====================================================================
// منطق واقعی در داخل فایل List.cshtml با استفاده از @functions تعریف شده.
// این الگو برای صفحات ساده‌ای که فقط به یک Action نیاز دارند، مناسب است.
// =====================================================================

using Microsoft.AspNetCore.Mvc; // برای PageModel
using Microsoft.AspNetCore.Mvc.RazorPages; // فضای نام Razor Pages

// تعریف namespace
namespace WebApp.Pages.Suppliers
{
    // تعریف کلاس PageModel (خالی)
    public class ListModel : PageModel
    {
        // Handler خالی - منطق در View است
        public void OnGet()
        {
        }
    }
}
