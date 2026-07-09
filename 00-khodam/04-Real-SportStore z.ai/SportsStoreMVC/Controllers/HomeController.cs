using Microsoft.AspNetCore.Mvc;

namespace SportsStore.Controllers {

    /// <summary>
    /// Controller اصلی برنامه
    /// مسئولیت: هدایت کاربر از صفحه اصلی به صفحه محصولات
    ///
    /// نکته آموزشی:
    /// در MVC، Controller کلاسی است که از Controller (یا ControllerBase) ارث می‌برد.
    /// هر متد public در Controller به عنوان یک "Action" شناخته می‌شود و
    /// می‌تواند به یک URL پاسخ دهد.
    ///
    /// مسیریابی پیش‌فرض: {controller=Home}/{action=Index}/{id?}
    /// بنابراین HomeController.Index به URL "/" پاسخ می‌دهد.
    /// </summary>
    public class HomeController : Controller {

        /// <summary>
        /// Action پیش‌فرض - هدایت به صفحه محصولات
        /// </summary>
        public IActionResult Index() {
            // RedirectToAction کاربر را به اکشن Index از ProductsController هدایت می‌کند
            return RedirectToAction("Index", "Products");
        }
    }
}
