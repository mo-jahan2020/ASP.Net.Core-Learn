// این کلاس یک الگوی "Options Pattern" در ASP.NET Core را نشان می‌دهد.
// کلاس‌های Options برای نگهداری تنظیمات پیکربندی (Configuration) استفاده می‌شوند
// و معمولاً از طریق appsettings.json یا کد مقداردهی و سپس با IOptions<T> تزریق می‌شوند.
namespace Platform
{

    public class MessageOptions
    {

        // مقادیر پیش‌فرض در صورتی که پیکربندی دیگری ارائه نشود.
        public string CityName { get; set; } = "New York";
        public string CountryName { get; set; } = "USA";
    }
}
