// =====================================================================
// Data.cshtml.cs - PageModel ساده برای صفحه‌ی Data
// =====================================================================
// این یک PageModel خالی است که فقط ساختار پایه‌ی Razor Page را نشان می‌دهد.
// Handler آن (OnGet) خالی است و کاری انجام نمی‌دهد.
// منطق اصلی این صفحه در خود فایل .cshtml (Data.cshtml) نوشته شده است.
// =====================================================================

using Microsoft.AspNetCore.Mvc; // برای PageModel
using Microsoft.AspNetCore.Mvc.RazorPages; // فضای نام Razor Pages

// تعریف namespace
namespace WebApp.Pages
{
    // تعریف کلاس PageModel
    public class DataModel : PageModel
    {
        // Handler مربوط به درخواست GET
        // این متد خالی است و هیچ کاری انجام نمی‌دهد
        // صفحه با استفاده از @inject در خود View، به DataContext دسترسی پیدا می‌کند
        public void OnGet()
        {
        }
    }
}
