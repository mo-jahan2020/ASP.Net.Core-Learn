// =====================================================================
// Cities.cshtml.cs - PageModel هیبریدی برای صفحه‌ی شهرها
// =====================================================================
// این کلاس هم به عنوان PageModel برای Razor Page عمل می‌کند
// و هم به عنوان ViewComponent (با نام CitiesPageHybrid).
// این الگوی "Hybrid" نام دارد و ترکیب Page و ViewComponent است.
// =====================================================================

using Microsoft.AspNetCore.Mvc; // برای PageModel
using Microsoft.AspNetCore.Mvc.RazorPages; // فضای نام Razor Pages
using Microsoft.AspNetCore.Mvc.ViewComponents; // برای ViewComponentContext
using Microsoft.AspNetCore.Mvc.ViewFeatures; // برای ViewDataDictionary
using WebApp.Models; // برای City, CitiesData, CityViewModel

// تعریف namespace
namespace WebApp.Pages {

    // [ViewComponent(Name = "CitiesPageHybrid")]:
    //   این کلاس به یک ViewComponent با نام سفارشی "CitiesPageHybrid" تبدیل می‌شود
    //   یعنی می‌توان از <vc:cities-page-hybrid /> در Razor استفاده کرد
    [ViewComponent(Name = "CitiesPageHybrid")]
    public class CitiesModel : PageModel {

        // سازنده - CitiesData از طریق DI تزریق می‌شود
        public CitiesModel(CitiesData cdata) {
            // ذخیره‌ی داده‌ها برای استفاده در متدها
            Data = cdata;
        }

        // ویژگی برای نگهداری داده‌های شهرها
        // CitiesData? یعنی اختیاری است (می‌تواند null باشد)
        public CitiesData? Data { get; set; }

        // [ViewComponentContext]:
        //   این ویژگی، Context مربوط به ViewComponent را به کلاس تزریق می‌کند
        //   ViewComponentContext شامل اطلاعاتی مانند ViewData و Arguments است
        //   این ویژگی فقط زمانی که این کلاس به عنوان ViewComponent فراخوانی شود، مقداردهی می‌شود
        [ViewComponentContext]
        public ViewComponentContext Context { get; set; } = new();

        // متد Invoke: منطق ViewComponent
        // این متد فقط زمانی فراخوانی می‌شود که کلاس به عنوان ViewComponent استفاده شود
        public IViewComponentResult Invoke() {
            // ساختن یک ViewViewComponentResult با ViewData سفارشی
            return new ViewViewComponentResult() {
                // ViewDataDictionary: نگهداری داده‌ها برای View
                //   Context.ViewData: ViewData فعلی ViewComponent (برای حفظ اطلاعات)
                //   new CityViewModel { ... }: مدل شامل تعداد و جمعیت شهرها
                ViewData = new ViewDataDictionary<CityViewModel>(
                    Context.ViewData,
                    new CityViewModel {
                        // Data?.Cities: اگر Data null نباشد، Cities برگردان
                        // Count(): تعداد شهرها
                        Cities = Data?.Cities.Count(),
                        // Sum(c => c.Population): مجموع جمعیت (با null check)
                        Population = Data?.Cities.Sum(c => c.Population)
                    })
            };
        }
    }
}
