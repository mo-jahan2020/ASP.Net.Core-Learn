// همانند کلاس Capital، این کلاس نیز یک Endpoint مبتنی بر کلاس است
// که توسط EndpointExtensions.MapEndpoint<T>() به مسیر "population" متصل می‌شود
// و از طریق LinkGenerator در کلاس Capital نیز به آن ارجاع داده می‌شود (برای مورد "monaco").
﻿namespace Platform {
    public class Population {

        public static async Task Endpoint(HttpContext context) {
            // خواندن مقدار پارامتر مسیر "city"؛ در صورت نبود مقدار، پیش‌فرض "london" استفاده می‌شود.
            string city
                = context.Request.RouteValues["city"] as string ?? "london";
            // استفاده از nullable int (?) تا بتوان تشخیص داد که آیا شهر معتبری پیدا شده یا نه.
            int? pop = null;
            switch (city.ToLower()) {
                case "london":
                    pop = 8_136_000;
                    break;
                case "paris":
                    pop = 2_141_000;
                    break;
                case "monaco":
                    pop = 39_000;
                    break;
            }
            if (pop.HasValue) {
                // نمایش نام شهر و جمعیت آن در پاسخ.
                await context.Response
                    .WriteAsync($"City: {city}, Population: {pop}");
            } else {
                // اگر شهر در لیست تعریف‌شده نبود، کد وضعیت 404 برگردانده می‌شود.
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            }
        }
    }
}
