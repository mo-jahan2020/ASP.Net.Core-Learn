// این کلاس نمونه‌ای از یک "Endpoint مبتنی بر کلاس" (Class-Based Endpoint) است.
// به‌جای نوشتن یک لامبدا (Lambda) داخل Program.cs، منطق Endpoint در یک متد استاتیک
// درون یک کلاس جدا نوشته شده تا کد Program.cs تمیزتر و قابل استفاده‌ی مجدد باشد.
// این متد توسط EndpointExtensions.MapEndpoint<T>() از طریق Reflection فراخوانی می‌شود.
﻿namespace Platform {

    public class Capital {

        // متد Endpoint باید امضای (Signature) مشخصی داشته باشد تا توسط MapEndpoint شناسایی شود:
        // باید public، static و بازگشتی از نوع Task باشد.
        public static async Task Endpoint(HttpContext context) {
            string? capital = null;
            // خواندن مقدار پارامتر مسیر (Route Value) با نام "country" که از URL استخراج شده است.
            // مثال: در مسیر "/capital/uk" مقدار country برابر "uk" خواهد بود.
            string? country = context.Request.RouteValues["country"] as string;
            // بر اساس نام کشور، پایتخت متناظر تعیین می‌شود (تبدیل به حروف کوچک برای مقایسه‌ی غیر حساس به بزرگی/کوچکی حروف).
            switch ((country ?? "").ToLower()) {
                case "uk":
                    capital = "London";
                    break;
                case "france":
                    capital = "Paris";
                    break;
                case "monaco":
                    // برای موناکو به‌جای نمایش مستقیم، کاربر به Endpoint دیگری (population) هدایت (Redirect) می‌شود.
                    // LinkGenerator ابزاری در ASP.NET Core برای تولید URL بر اساس نام مسیر (Route Name) است،
                    // بدون این‌که مسیر به صورت رشته‌ای Hard-code شود.
                    LinkGenerator? generator =
                        context.RequestServices.GetService<LinkGenerator>();
                    string? url = generator?.GetPathByRouteValues(context,
                        "population", new { city = country });
                    if (url != null) {
                        // ارسال پاسخ Redirect (کد وضعیت 302) به مرورگر/کلاینت به سمت URL تولید شده.
                        context.Response.Redirect(url);
                    }
                    return;
            }
            if (capital != null) {
                // نوشتن پاسخ متنی در بدنه‌ی HTTP Response.
                await context.Response
                    .WriteAsync($"{capital} is the capital of {country}");
            } else {
                // اگر کشوری پیدا نشد، کد وضعیت 404 (Not Found) برگردانده می‌شود.
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            }
        }
    }
}
