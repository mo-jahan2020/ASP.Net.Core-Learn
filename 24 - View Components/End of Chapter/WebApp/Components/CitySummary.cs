// =====================================================================
// CitySummary.cs - ViewComponent برای نمایش خلاصه‌ی شهرها
// =====================================================================
// ViewComponent در ASP.NET Core MVC مشابه یک Partial View است، اما
// شامل منطق (Logic) هم می‌شود. این کامپوننت تعداد شهرها و مجموع
// جمعیت آن‌ها را به صورت یک جدول کوچک نمایش می‌دهد.
// مزیت ViewComponent: قابلیت استفاده مجدد، تست‌پذیری و جداسازی بهتر.
// =====================================================================

 using Microsoft.AspNetCore.Mvc; // برای استفاده از ViewComponent
 using WebApp.Models; // برای دسترسی به مدل‌ها
 using Microsoft.AspNetCore.Mvc.ViewComponents; // فضای نام ViewComponent
 using Microsoft.AspNetCore.Html; // برای کار با HTML

// تعریف namespace مربوط به ViewComponentها
namespace WebApp.Components {

    // تعریف کلاس CitySummary که از ViewComponent ارث‌بری می‌کند
    // نام کلاس (CitySummary) به طور پیش‌فرض، نام ViewComponent است
    // یعنی در Razor می‌توان از <vc:city-summary /> استفاده کرد
    public class CitySummary : ViewComponent {
        // فیلد خصوصی برای نگهداری داده‌های شهرها
        // این فیلد از طریق سازنده (Constructor) تزریق می‌شود
        private CitiesData data;

        // سازنده‌ی کلاس - CitiesData از طریق Dependency Injection دریافت می‌شود
        // به یاد داشته باشید: CitiesData به صورت Singleton ثبت شده است
        public CitySummary(CitiesData cdata) {
            // ذخیره‌ی نمونه‌ی CitiesData دریافتی در فیلد data
            data = cdata;
        }

        // متد Invoke: متد اصلی ViewComponent که نتیجه را برمی‌گرداند
        // این متد می‌تواند پارامترهایی داشته باشد که از Razor پاس داده می‌شوند
        // themeName: نام تم بوت‌استرپ برای رنگ‌بندی جدول (پیش‌فرض: "success")
        public IViewComponentResult Invoke(string themeName = "success") {
            // ViewBag.Theme: تنظیم تم برای استفاده در View
            // ViewBag یک شیء داینامیک است که اطلاعات را به View منتقل می‌کند
            ViewBag.Theme = themeName;

            // برگرداندن یک View به همراه یک شیء CityViewModel
            // CityViewModel شامل تعداد شهرها و مجموع جمعیت است
            // return View(...): از View پیش‌فرض (Default.cshtml) استفاده می‌کند
            return View(new CityViewModel {
                // Cities.Count(): تعداد کل شهرها
                Cities = data.Cities.Count(),
                // Sum(c => c.Population): مجموع جمعیت تمام شهرها
                // lambda expression: c => c.Population یعنی برای هر شهر c، جمعیت آن را برگردان
                Population = data.Cities.Sum(c => c.Population)
            });
        }
    }
}
