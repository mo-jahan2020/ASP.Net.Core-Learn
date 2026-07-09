namespace Platform {
    // این کلاس Endpoint مربوط به مسیر population را پیاده‌سازی می‌کند
    // partial است چون از LoggerMessage source generator استفاده می‌کند
    public partial class Population {

        // متد اصلی Endpoint که برای هر درخواست فراخوانی می‌شود
        // context اطلاعات درخواست/پاسخ HTTP و logger برای ثبت وقایع است
        public static async Task Endpoint(HttpContext context,
                ILogger<Population> logger) {
            //logger.LogDebug($"Started processing for {context.Request.Path}");
            // ثبت لاگ شروع پردازش با استفاده از متد تولید شده توسط source generator
            StartingResponse(logger, context.Request.Path);
            // خواندن مقدار پارامتر مسیر "city"؛ در صورت نبودن مقدار پیش‌فرض "london" است
            string city
                = context.Request.RouteValues["city"] as string ?? "london";
            int? pop = null;
            // بررسی نام شهر (بدون توجه به بزرگی/کوچکی حروف) و تعیین جمعیت متناظر
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
            // اگر جمعیت شهر پیدا شد، آن را در پاسخ بنویس
            if (pop.HasValue) {
                await context.Response
                    .WriteAsync($"City: {city}, Population: {pop}");
            } else {
                // در غیر این صورت کد وضعیت 404 (پیدا نشد) برگردان
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            }
            // ثبت لاگ پایان پردازش درخواست
            logger.LogDebug($"Finished processing for {context.Request.Path}");
        }

        // متد لاگ‌گیری تولید شده توسط source generator (LoggerMessage)
        // برای ثبت پیام "Starting response for {path}" با سطح Debug
        [LoggerMessage(0, LogLevel.Debug, "Starting response for {path}")]
        public static partial void StartingResponse(ILogger logger, string path);
    }
}
