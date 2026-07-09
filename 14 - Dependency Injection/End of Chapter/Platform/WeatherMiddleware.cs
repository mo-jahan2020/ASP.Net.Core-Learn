// این Middleware نشان می‌دهد که متد Invoke می‌تواند علاوه بر HttpContext،
// وابستگی‌های اضافی را نیز به‌عنوان پارامتر دریافت کند (Method Injection در Middleware).
// همچنین نمونه‌ای از "Multiple Registrations" را نشان می‌دهد: وقتی چند سرویس
// برای یک اینترفیس (IResponseFormatter) ثبت شده باشند، هر پارامتر می‌تواند
// یک نمونه‌ی متفاوت از آن‌ها دریافت کند (بسته به نحوه‌ی ثبت در DI Container).
﻿using Platform.Services;

namespace Platform {
    public class WeatherMiddleware {
        private RequestDelegate next;

        public WeatherMiddleware(RequestDelegate nextDelegate) {
            next = nextDelegate;
        }

        // سه پارامتر از نوع IResponseFormatter دریافت می‌شوند تا نشان داده شود
        // که چگونه چند پیاده‌سازی متفاوت از یک اینترفیس می‌توانند هم‌زمان تزریق شوند.
        public async Task Invoke(HttpContext context, IResponseFormatter formatter1,
                IResponseFormatter formatter2, IResponseFormatter formatter3) {
            if (context.Request.Path == "/middleware/class") {
                // فراخوانی هر سه فرمت‌دهنده به ترتیب روی همان پاسخ.
                await formatter1.Format(context, string.Empty);
                await formatter2.Format(context, string.Empty);
                await formatter3.Format(context, string.Empty);
            } else {
                // اگر مسیر مطابقت نداشت، درخواست به بخش بعدی Pipeline منتقل می‌شود.
                await next(context);
            }
        }
    }
}
