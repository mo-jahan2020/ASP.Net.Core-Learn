using Platform.Services;

namespace Platform {
    // میان‌افزاری که برای مسیر خاص "/middleware/class" چند فرمت‌دهنده پاسخ (IResponseFormatter) را فراخوانی می‌کند
    public class WeatherMiddleware {
        // مرجع به Middleware بعدی در زنجیره پردازش
        private RequestDelegate next;

        // سازنده‌ای که Middleware بعدی را دریافت می‌کند
        public WeatherMiddleware(RequestDelegate nextDelegate) {
            next = nextDelegate;
        }

        // متد Invoke که سه نمونه از IResponseFormatter را از طریق تزریق وابستگی دریافت می‌کند
        // (این سه نمونه می‌توانند بسته به نوع ثبت سرویس، یکسان یا متفاوت باشند)
        public async Task Invoke(HttpContext context, IResponseFormatter formatter1,
                IResponseFormatter formatter2, IResponseFormatter formatter3) {
            // اگر مسیر درخواست دقیقا "/middleware/class" باشد
            if (context.Request.Path == "/middleware/class") {
                // هر سه فرمت‌دهنده به ترتیب فراخوانی می‌شوند
                await formatter1.Format(context, string.Empty);
                await formatter2.Format(context, string.Empty);
                await formatter3.Format(context, string.Empty);
            } else {
                // در غیر این صورت درخواست به Middleware بعدی پاس داده می‌شود
                await next(context);
            }
        }
    }
}
