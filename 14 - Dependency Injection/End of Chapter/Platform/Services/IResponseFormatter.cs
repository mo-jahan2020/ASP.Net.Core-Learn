// این اینترفیس قرارداد مشترکی برای تمام "فرمت‌دهنده‌های پاسخ" (Response Formatters)
// در پروژه تعریف می‌کند. کلاس‌هایی مانند TextResponseFormatter، HtmlResponseFormatter،
// TimeResponseFormatter و GuidService همگی این اینترفیس را پیاده‌سازی می‌کنند
// تا بتوان آن‌ها را به‌صورت قابل تعویض (Interchangeable) از طریق DI استفاده کرد.
﻿namespace Platform.Services {
    public interface IResponseFormatter {

        // متد اصلی که هر پیاده‌سازی باید تعریف کند: نحوه‌ی نوشتن محتوا در پاسخ HTTP.
        Task Format(HttpContext context, string content);

        // این یک "Default Interface Member" (ویژگی زبان C# 8+) است:
        // یعنی اگر کلاسی این پراپرتی را بازنویسی (Override) نکند،
        // به‌طور پیش‌فرض مقدار false را دریافت می‌کند بدون اینکه لازم باشد آن را صراحتاً پیاده‌سازی کند.
        public bool RichOutput => false;
    }
}
