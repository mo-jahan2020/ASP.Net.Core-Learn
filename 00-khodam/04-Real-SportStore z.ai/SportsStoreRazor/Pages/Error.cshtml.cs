using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace SportsStore.Pages {

    /// <summary>
    /// PageModel صفحه خطای عمومی (Error)
    /// این صفحه به طور خودکار در صورت بروز خطای پردازش نشده نمایش داده می‌شود.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel {

        // شناسه درخواست (برای ردیابی خطاها در Log)
        public string? RequestId { get; set; }

        // آیا شناسه درخواست نمایش داده شود؟
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public void OnGet() {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }
}
