namespace Platform {

    // کلاسی که Endpoint مربوط به دریافت پایتخت یک کشور را پیاده‌سازی می‌کند
    public class Capital {

        // متد Endpoint اصلی که برای درخواست‌های ورودی فراخوانی می‌شود
        public static async Task Endpoint(HttpContext context) {
            string? capital = null;
            // خواندن پارامتر مسیر "country" از درخواست
            string? country = context.Request.RouteValues["country"] as string;
            // بررسی نام کشور (بدون حساسیت به بزرگی/کوچکی حروف)
            switch ((country ?? "").ToLower()) {
                case "uk":
                    capital = "London";
                    break;
                case "france":
                    capital = "Paris";
                    break;
                case "monaco":
                    // برای موناکو به جای پاسخ مستقیم، کاربر را به مسیر population هدایت (Redirect) می‌کنیم
                    LinkGenerator? generator =
                        context.RequestServices.GetService<LinkGenerator>();
                    // ساخت URL مربوط به مسیر population با پارامتر city برابر با نام کشور
                    string? url = generator?.GetPathByRouteValues(context,
                        "population", new { city = country });
                    if (url != null) {
                        context.Response.Redirect(url);
                    }
                    return;
            }
            // اگر پایتخت پیدا شد آن را در پاسخ بنویس
            if (capital != null) {
                await context.Response
                    .WriteAsync($"{capital} is the capital of {country}");
            } else {
                // در غیر این صورت کد وضعیت 404 برگردان
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            }
        }
    }
}
