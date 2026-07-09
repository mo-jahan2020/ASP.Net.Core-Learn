// =============================================================================
// Capital.cs - اندپوینت پایتخت کشورها
// این کلاس یک اندپوینت برای دریافت نام پایتخت یک کشور از طریق مسیر URL فراهم می‌کند
// همچنین قابلیت ریدایرکت برای کشور موناکو را دارد
// =============================================================================

namespace Platform {
    /// <summary>
    /// کلاس Capital - مدیریت اندپوینت پایتخت کشورها
    /// شامل یک متد استاتیک Endpoint که نام کشور را از مسیر URL خوانده
    /// و پایتخت مربوطه را برمی‌گرداند
    /// </summary>
    public class Capital {

        /// <summary>
        /// متد اندپوینت - پردازش درخواست‌های مربوط به پایتخت کشورها
        /// کشور از پارامتر مسیر "country" استخراج شده و پایتخت مربوطه بازگردانده می‌شود
        /// </summary>
        /// <param name="context">زمینه درخواست HTTP جاری شامل اطلاعات مسیر و پاسخ</param>
        public static async Task Endpoint(HttpContext context) {
            string? capital = null; // متغیر برای ذخیره نام پایتخت، در ابتدا null است

            // استخراج نام کشور از مقادیر مسیر (Route Values)
            string? country = context.Request.RouteValues["country"] as string;

            // بررسی نام کشور و تعیین پایتخت متناظر
            switch ((country ?? "").ToLower()) {
                case "uk":
                    capital = "London"; // پایتخت بریتانیا
                    break;
                case "france":
                    capital = "Paris"; // پایتخت فرانسه
                    break;
                case "monaco":
                    // برای موناکو، به جای نمایش پایتخت، به اندپوینت جمعیت ریدایرکت می‌شود
                    // استفاده از LinkGenerator برای ساخت URL به اندپوینت "population"
                    LinkGenerator? generator =
                        context.RequestServices.GetService<LinkGenerator>();
                    // ساخت مسیر با پارامتر city = نام کشور (monaco)
                    string? url = generator?.GetPathByRouteValues(context,
                        "population", new { city = country });
                    if (url != null) {
                        // ریدایرکت مرورگر به URL اندپوینت جمعیت
                        context.Response.Redirect(url);
                    }
                    return; // خروج زودهنگام - نیازی به ادامه پردازش نیست
            }

            // اگر پایتخت پیدا شده بود، آن را در پاسخ بنویس
            if (capital != null) {
                await context.Response
                    .WriteAsync($"{capital} is the capital of {country}");
            } else {
                // اگر کشور نامعتبر بود، کد 404 بازگردانده شود
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            }
        }
    }
}
