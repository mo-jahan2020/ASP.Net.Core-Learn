namespace Platform {

    // کلاس تنظیمات (Options Pattern) که برای نگهداری نام شهر و کشور پیش‌فرض استفاده می‌شود
    // این کلاس معمولاً از طریق appsettings.json یا کد مقداردهی می‌شود
    public class MessageOptions {

        // نام شهر پیش‌فرض
        public string CityName { get; set; } = "New York";
        // نام کشور پیش‌فرض
        public string CountryName { get; set; } = "USA";
    }
}
