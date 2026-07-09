// =============================================================================
// Population.cs - اندپوینت جمعیت شهرها
// این کلاس یک اندپوینت برای دریافت جمعیت شهرها از طریق مسیر URL فراهم می‌کند
// از Source Generator لاگینگ (LoggerMessage) برای بهینه‌سازی لاگ‌ها استفاده می‌کند
// =============================================================================

namespace Platform {
    /// <summary>
    /// Population - کلاس اندپوینت جمعیت شهرها
    /// کلاس partial است زیرا Source Generator لاگینگ بخش دیگری از آن را تولید می‌کند
    /// شامل یک متد اندپوینت استاتیک و یک متد لاگ تولیدشده توسط کامپایلر
    /// </summary>
    public partial class Population {
        /// <summary>
        /// متد اندپوینت - پردازش درخواست‌های مربوط به جمعیت شهرها
        /// نام شهر از پارامتر مسیر "city" خوانده شده و جمعیت مربوطه بازگردانده می‌شود
        /// </summary>
        /// <param name="context">زمینه درخواست HTTP جاری</param>
        /// <param name="logger">نگارنده لاگ برای ثبت رویدادها</param>
        public static async Task Endpoint(HttpContext context,
                ILogger<Population> logger) {
            // لاگ شروع پردازش با استفاده از متد تولیدشده (Source Generated)
            StartingResponse(logger, context.Request.Path);

            // استخراج نام شهر از مقادیر مسیر، پیش‌فرض: london
            string city
                = context.Request.RouteValues["city"] as string ?? "london";
            int? pop = null; // متغیر جمعیت، در ابتدا null

            // تعیین جمعیت بر اساس نام شهر
            switch (city.ToLower()) {
                case "london":
                    pop = 8_136_000; // جمعیت لندن
                    break;
                case "paris":
                    pop = 2_141_000; // جمعیت پاریس
                    break;
                case "monaco":
                    pop = 39_000; // جمعیت موناکو
                    break;
            }

            // اگر جمعیت پیدا شد، آن را در پاسخ بنویس
            if (pop.HasValue) {
                await context.Response
                    .WriteAsync($"City: {city}, Population: {pop}");
            } else {
                // اگر شهر نامعتبر بود، کد 404 بازگردانده شود
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            }
            // لاگ پایان پردازش
            logger.LogDebug($"Finished processing for {context.Request.Path}");
        }

        /// <summary>
        /// متد تولیدشده توسط Source Generator لاگینگ
        /// این متد با ویژگی LoggerMessage علامت‌گذاری شده و کامپایلر
        /// پیاده‌سازی بهینه آن را در فایل partial دیگر تولید می‌کند
        /// </summary>
        [LoggerMessage(0, LogLevel.Debug, "Starting response for {path}")]
        public static partial void StartingResponse(ILogger logger, string path);
    }
}
