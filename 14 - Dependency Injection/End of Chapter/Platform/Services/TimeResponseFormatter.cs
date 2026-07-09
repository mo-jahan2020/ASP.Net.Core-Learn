// این کلاس نمونه‌ای از "Constructor Injection" (تزریق وابستگی از طریق سازنده) است:
// به‌جای ساختن مستقیم یک وابستگی درون کلاس، آن وابستگی (ITimeStamper) از بیرون
// (توسط DI Container) به سازنده پاس داده می‌شود که باعث افزایش قابلیت تست‌پذیری
// و کاهش وابستگی مستقیم بین کلاس‌ها (Loose Coupling) می‌شود.
﻿namespace Platform.Services {
    public class TimeResponseFormatter : IResponseFormatter {
        // وابستگی به اینترفیس (نه پیاده‌سازی مشخص)، طبق اصل Dependency Inversion.
        private ITimeStamper stamper;

        public TimeResponseFormatter(ITimeStamper timeStamper) {
            stamper = timeStamper;
        }

        public async Task Format(HttpContext context, string content) {
            // استفاده از سرویس تزریق‌شده برای گرفتن زمان جاری و نمایش آن همراه با محتوا.
            await context.Response.WriteAsync($"{stamper.TimeStamp}: {content}");
        }
    }
}
