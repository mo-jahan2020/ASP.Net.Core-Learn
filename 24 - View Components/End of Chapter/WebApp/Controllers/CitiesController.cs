// =====================================================================
// CitiesController.cs - کنترلر هیبریدی (Hybrid) شهرها
// =====================================================================
// این کنترلر هم به عنوان یک Controller معمولی (با متد Index)
// و هم به عنوان یک ViewComponent عمل می‌کند.
// [ViewComponent(Name = "CitiesControllerHybrid")] نام سفارشی
// برای ViewComponent ایجاد می‌کند.
// =====================================================================

 using Microsoft.AspNetCore.Mvc; // برای استفاده از Controller و ActionResult
 using Microsoft.AspNetCore.Mvc.ViewComponents; // برای ViewComponent
 using Microsoft.AspNetCore.Mvc.ViewFeatures; // برای ViewData و ViewDataDictionary
 using WebApp.Models; // برای دسترسی به City و CityViewModel

// تعریف namespace مربوط به Controllerها
namespace WebApp.Controllers {

    // [ViewComponent(Name = "CitiesControllerHybrid")]:
    //   این ویژگی (Attribute) کلاس را به یک ViewComponent تبدیل می‌کند
    //   با نام سفارشی "CitiesControllerHybrid"
    //   یعنی در Razor از <vc:cities-controller-hybrid /> استفاده می‌شود
    [ViewComponent(Name = "CitiesControllerHybrid")]
    public class CitiesController : Controller {
        // فیلد خصوصی برای نگهداری داده‌های شهرها
        private CitiesData data;

        // سازنده‌ی کنترلر - CitiesData از طریق DI تزریق می‌شود
        public CitiesController(CitiesData cdata) {
            data = cdata;
        }

        // Action Index: متد اصلی کنترلر
        // وقتی به /Cities/Index درخواست ارسال شود، این متد فراخوانی می‌شود
        // این متد، لیست شهرها را به View پاس می‌دهد
        public IActionResult Index() {
            // برگرداندن View به همراه لیست شهرها
            return View(data.Cities);
        }

        // متد Invoke: این متد به ViewComponent اجازه می‌دهد تا این کنترلر
        // را به عنوان یک ViewComponent فراخوانی کند
        // یعنی این کنترلر دو نقش دارد: هم کنترلر و هم ViewComponent
        public IViewComponentResult Invoke() {
            // ایجاد یک ViewViewComponentResult سفارشی
            return new ViewViewComponentResult() {
                // ViewData: شامل داده‌هایی است که به View پاس داده می‌شود
                // ViewDataDictionary: کلاسی برای نگهداری داده‌های View
                // پارامتر اول: ViewData فعلی (برای حفظ اطلاعات)
                // پارامتر دوم: مدل CityViewModel که شامل تعداد و جمعیت است
                ViewData = new ViewDataDictionary<CityViewModel>(
                    ViewData,
                    new CityViewModel {
                        Cities = data.Cities.Count(),
                        Population = data.Cities.Sum(c => c.Population)
                    })
            };
        }
    }
}
