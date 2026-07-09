namespace Platform 
{

    // این کلاس یک Endpoint کمکی برای نمایش پایتخت کشورهاست.
    // در پروژه‌های آموزشی، جدا کردن منطق هر Endpoint در یک کلاس،
    // خوانایی برنامه را بیشتر می‌کند.
    public class Capital 
    {
        // این متد از HttpContext استفاده می‌کند و پاسخ مناسب را برمی‌گرداند.
        public static async Task Endpoint(HttpContext context) 
        {
            string? capital = null;

            // مقدار country از RouteValues خوانده می‌شود.
            // یعنی بخشی از URL قبلاً توسط سیستم Routing استخراج شده است.
            string? country = context.Request.RouteValues["country"] as string;

            // با switch بررسی می‌کنیم کاربر چه کشوری را درخواست کرده است.
            switch ((country ?? "").ToLower()) 
            {
                case "uk":
                    capital = "London";
                    break;
                case "france":
                    capital = "Paris";
                    break;
                case "monaco":
                    // LinkGenerator برای ساختن URL به شکل برنامه‌نویسی استفاده می‌شود.
                    // این روش از hard-code کردن آدرس‌ها بهتر است.
                    LinkGenerator? generator =
                        context.RequestServices.GetService<LinkGenerator>();
                    string? url = generator?.GetPathByRouteValues(context,
                        "population", new { city = country });
                    if (url != null) 
                    {
                        // اگر URL ساخته شد، کاربر به مسیر دیگر هدایت می‌شود.
                        context.Response.Redirect(url);
                    }
                    return;
            }

            // اگر پایتخت پیدا شده باشد، در پاسخ نمایش داده می‌شود.
            if (capital != null) 
            {
                await context.Response.WriteAsync($"{capital} is the capital of {country}");
            } 
            else 
            {
                // اگر کشور ناشناخته باشد، وضعیت 404 برمی‌گردانیم.
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            }
        }
    }
}
