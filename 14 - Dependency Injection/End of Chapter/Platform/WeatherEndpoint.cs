// این کلاس نمونه‌ای دیگر از Endpoint مبتنی بر کلاس است، با این تفاوت که
// به جای متد static، از یک متد نمونه‌ای (Instance Method) استفاده می‌کند
// و وابستگی IResponseFormatter را به‌صورت پارامتر متد دریافت می‌کند
// (به‌جای تزریق در سازنده‌ی کلاس - که در کامنت‌های زیر نمونه‌ی جایگزین آن دیده می‌شود).
﻿using Platform.Services;

namespace Platform {
    public class WeatherEndpoint {
        // نمونه‌ی جایگزین (غیرفعال/کامنت‌شده) که تزریق وابستگی را از طریق سازنده (Constructor Injection) انجام می‌دهد:
        //private IResponseFormatter formatter;

        //public WeatherEndpoint(IResponseFormatter responseFormatter) {
        //    formatter = responseFormatter;
        //}

        // در این نسخه، IResponseFormatter مستقیماً به عنوان پارامتر متد Endpoint دریافت می‌شود
        // که این الگو "Method Injection" یا تزریق وابستگی در سطح متد نام دارد
        // و توسط EndpointExtensions.MapEndpoint<T>() از طریق DI Container پر می‌شود.
        public async Task Endpoint(HttpContext context,
                IResponseFormatter formatter) {
            // فراخوانی سرویس فرمت‌دهنده‌ی پاسخ برای نوشتن یک پیام آب‌وهوایی نمونه.
            await formatter.Format(context, "Endpoint Class: It is cloudy in Milan");
        }
    }
}
