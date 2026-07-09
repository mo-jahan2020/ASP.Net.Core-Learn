// =============================================================================
// MessageOptions.cs - کلاس تنظیمات پیام
// این کلاس برای نگهداری تنظیمات مربوط به نام شهر و کشور استفاده می‌شود
// مقادیر آن از فایل پیکربندی (appsettings.json) خوانده می‌شوند
// =============================================================================

namespace Platform {
    /// <summary>
    /// MessageOptions - کلاس مدل تنظیمات پیام
    /// شامل نام شهر و کشور که می‌توان از طریق تنظیمات اپلیکیشن مقداردهی شد
    /// این کلاس با الگوی Options برای تزریق وابستگی استفاده می‌شود
    /// </summary>
    public class MessageOptions {
        /// <summary>
        /// نام شهر - مقدار پیش‌فرض: New York
        /// از بخش Location:CityName در appsettings.json خوانده می‌شود
        /// </summary>
        public string CityName { get; set; } = "New York";

        /// <summary>
        /// نام کشور - مقدار پیش‌فرض: USA
        /// از بخش Location:CountryName در appsettings.json خوانده می‌شود
        /// </summary>
        public string CountryName { get; set; } = "USA";
    }
}
