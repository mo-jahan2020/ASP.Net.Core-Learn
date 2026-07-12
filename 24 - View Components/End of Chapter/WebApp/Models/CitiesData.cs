// =====================================================================
// CitiesData.cs - منبع داده‌ی ایستا (In-Memory) برای شهرها
// =====================================================================
// این کلاس داده‌های چند شهر معروف را در حافظه نگهداری می‌کند.
// به جای استفاده از پایگاه داده، از این کلاس به عنوان یک منبع داده ساده
// برای آزمایش ویژگی‌های مختلف ASP.NET Core استفاده می‌شود.
// =====================================================================

// تعریف namespace مربوط به مدل‌ها
namespace WebApp.Models {

    // تعریف کلاس CitiesData
    // این کلاس به صورت Singleton در Program.cs ثبت شده است
    public class CitiesData {

        // فیلد خصوصی (private) برای نگهداری لیست شهرها
        // List<City> یعنی مجموعه‌ای از اشیاء City
        // new List<City> { ... } مقداردهی اولیه (object initializer) با چند شهر نمونه
        private List<City> cities = new List<City> {
            // شهر لندن، کشور انگلیس، جمعیت 8 میلیون و 539 هزار نفر
            new City { Name = "London", Country = "UK", Population = 8539000},
            // شهر نیویورک، آمریکا، جمعیت 8 میلیون و 406 هزار نفر
            new City { Name = "New York", Country = "USA", Population = 8406000 },
            // شهر سن‌خوزه، آمریکا، جمعیت نزدیک به 1 میلیون نفر
            new City { Name = "San Jose", Country = "USA", Population = 998537 },
            // شهر پاریس، فرانسه، جمعیت حدود 2 میلیون و 244 هزار نفر
            new City { Name = "Paris", Country = "France", Population = 2244000 }
        };

        // ویژگی فقط-خواندنی (Read-Only) برای دسترسی بیرونی به لیست شهرها
        // IEnumerable<City> یک اینترفیس پایه برای تمام مجموعه‌های قابل پیمایش است
        // علامت => (Expression-Bodied) یعنی get به صورت خلاصه نوشته شده
        public IEnumerable<City> Cities => cities;

        // متد عمومی برای اضافه کردن شهر جدید به لیست
        // newCity پارامتر ورودی از نوع City است
        public void AddCity(City newCity) {
            // cities.Add: افزودن شهر جدید به انتهای لیست
            cities.Add(newCity);
        }
    }
}
