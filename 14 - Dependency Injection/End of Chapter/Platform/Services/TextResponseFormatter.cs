// این کلاس نمونه‌ای ساده از پیاده‌سازی IResponseFormatter است که پاسخ را
// به‌صورت متن ساده برمی‌گرداند، اما نکته‌ی جالب آن نگهداری یک شمارنده (Counter)
// برای نمایش تفاوت رفتار Lifetime های مختلف (Singleton در برابر Scoped/Transient) است.
﻿namespace Platform.Services {
    public class TextResponseFormatter : IResponseFormatter {
        // این شمارنده به ازای هر نمونه (Instance) از کلاس جداگانه است؛
        // اگر سرویس به‌صورت Transient ثبت شود، همیشه صفر شروع می‌شود؛
        // اما اگر Singleton باشد، بین درخواست‌های مختلف افزایش می‌یابد.
        private int responseCounter = 0;
        // پیاده‌سازی دستی الگوی Singleton (جدا از DI Container) برای مقایسه یا استفاده‌ی مستقل.
        private static TextResponseFormatter? shared;

        public async Task Format(HttpContext context, string content) {
            // پیش‌افزایش (Pre-increment) شمارنده و نوشتن آن به همراه محتوا در پاسخ.
            await context.Response.
                WriteAsync($"Response {++responseCounter}:\n{content}");
        }

        // پراپرتی استاتیک که همیشه یک نمونه‌ی یکسان (Singleton دستی) از این کلاس برمی‌گرداند؛
        // نمونه فقط در اولین فراخوانی ساخته می‌شود (Lazy Initialization).
        public static TextResponseFormatter Singleton {
            get {
                if (shared == null) {
                    shared = new TextResponseFormatter();
                }
                return shared;
            }
        }
    }
}
