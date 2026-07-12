// =====================================================================
// HandlerSelector.cshtml.cs - PageModel خالی
// =====================================================================
// این فایل حاوی یک PageModel خالی است.
// نکته‌ی جالب: منطق اصلی این صفحه در داخل فایل .cshtml (با @functions)
// تعریف شده است. این الگو برای صفحات کوچک کاربرد دارد.
// =====================================================================

using Microsoft.AspNetCore.Mvc; // برای PageModel
using Microsoft.AspNetCore.Mvc.RazorPages; // فضای نام Razor Pages

// تعریف namespace
namespace WebApp.Pages
{
    // تعریف کلاس PageModel (خالی)
    public class HandlerSelectorModel : PageModel
    {
        // Handler خالی - هیچ کاری انجام نمی‌دهد
        // منطق واقعی در خود .cshtml فایل با @functions نوشته شده
        public void OnGet()
        {
        }
    }
}
