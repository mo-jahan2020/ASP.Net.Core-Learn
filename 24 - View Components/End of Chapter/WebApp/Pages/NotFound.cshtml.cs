// =====================================================================
// NotFound.cshtml.cs - PageModel خالی
// =====================================================================
// این فایل حاوی یک PageModel خالی است.
// منطق اصلی در خود فایل .cshtml با استفاده از @functions تعریف شده.
// =====================================================================

using Microsoft.AspNetCore.Mvc; // برای PageModel
using Microsoft.AspNetCore.Mvc.RazorPages; // فضای نام Razor Pages

// تعریف namespace
namespace WebApp.Pages
{
    // تعریف کلاس PageModel (خالی)
    public class NotFoundModel : PageModel
    {
        // Handler خالی - منطق در خود View تعریف شده
        public void OnGet()
        {
        }
    }
}
