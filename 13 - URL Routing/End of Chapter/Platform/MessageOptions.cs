namespace Platform 
{

    // این کلاس برای نگهداری تنظیمات پیام استفاده می‌شود.
    // چنین کلاس‌هایی معمولاً با الگوی Options در ASP.NET Core به کار می‌روند.
    public class MessageOptions 
    {

        // نام شهر پیش‌فرض
        public string CityName { get; set; } = "New York";

        // نام کشور پیش‌فرض
        public string CountryName { get; set; } = "USA";
    }
}
