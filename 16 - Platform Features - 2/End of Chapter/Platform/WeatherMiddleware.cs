// =============================================================================
// WeatherMiddleware.cs - میان‌افزار آب و هوا
// این میان‌افزار در مسیر /middleware/class سه فرمت‌کننده پاسخ مختلف را
// به صورت پشت سر هم فراخوانی می‌کند تا نحوه تزریق چندین سرویس هم‌نام
// در میان‌افزار کلاس‌محور نمایش داده شود
// =============================================================================

using Platform.Services; // وارد کردن سرویس‌های قالب‌بندی پاسخ

namespace Platform {
    /// <summary>
    /// WeatherMiddleware - میان‌افزار نمایش آب و هوا
    /// نحوه تزریق چندین نمونه IResponseFormatter متفاوت را در یک میان‌افزار نشان می‌دهد
    /// </summary>
    public class WeatherMiddleware {
        // ارجاع به میان‌افزار بعدی در زنجیره پردازش
        private RequestDelegate next;

        /// <summary>
        /// سازنده - دریافت ارجاع میان‌افزار بعدی
        /// </summary>
        /// <param name="nextDelegate">دلیگیت میان‌افزار بعدی در خط لوله</param>
        public WeatherMiddleware(RequestDelegate nextDelegate) {
            next = nextDelegate;
        }

        /// <summary>
        /// متد Invoke - پردازش درخواست HTTP
        /// در مسیر /middleware/class سه فرمت‌کننده مختلف (تزریق‌شده از DI Container)
        /// پشت سر هم فراخوانی می‌شوند تا خروجی‌های متنوع تولید شود
        /// </summary>
        /// <param name="context">زمینه درخواست HTTP جاری</param>
        /// <param name="formatter1">اولین فرمت‌کننده پاسخ (مثلاً HtmlResponseFormatter)</param>
        /// <param name="formatter2">دومین فرمت‌کننده پاسخ (مثلاً TextResponseFormatter)</param>
        /// <param name="formatter3">سومین فرمت‌کننده پاسخ (مثلاً TimeResponseFormatter)</param>
        public async Task Invoke(HttpContext context, IResponseFormatter formatter1,
                IResponseFormatter formatter2, IResponseFormatter formatter3) {
            if (context.Request.Path == "/middleware/class") {
                // فراخوانی سه فرمت‌کننده مختلف پشت سر هم
                // هر فرمت‌کننده محتوای خود را با فرمت متفاوت به پاسخ اضافه می‌کند
                await formatter1.Format(context, string.Empty);
                await formatter2.Format(context, string.Empty);
                await formatter3.Format(context, string.Empty);
            } else {
                // ارجاع درخواست به میان‌افزار بعدی برای سایر مسیرها
                await next(context);
            }
        }
    }
}
