// =============================================================================
// HtmlResponseFormatter.cs - فرمت‌کننده پاسخ HTML
// این کلاس محتوا را در یک قالب HTML استاندارد با استایل‌دهی نمایش می‌دهد
// نوع محتوا را به text/html تنظیم می‌کند و خروجی غنی (Rich) تولید می‌کند
// =============================================================================

namespace Platform.Services {
    /// <summary>
    /// HtmlResponseFormatter - فرمت‌کننده پاسخ HTML
    /// محتوا را در یک صفحه HTML کامل با عنوان و استایل نمایش می‌دهد
    /// پیاده‌سازی IResponseFormatter با خروجی غنی (RichOutput = true)
    /// </summary>
    public class HtmlResponseFormatter : IResponseFormatter {
        /// <summary>
        /// Format - فرمت‌بندی محتوا به صورت HTML
        /// نوع محتوای پاسخ به text/html تغییر می‌کند و محتوا در تگ‌های HTML قرار می‌گیرد
        /// </summary>
        /// <param name="context">زمینه درخواست HTTP</param>
        /// <param name="content">محتوای متنی برای نمایش در صفحه HTML</param>
        public async Task Format(HttpContext context, string content) {
            // تنظیم نوع محتوای پاسخ به HTML
            context.Response.ContentType = "text/html";
            // نوشتن قالب HTML کامل شامل محتوای داده‌شده
            await context.Response.WriteAsync($@"
                <!DOCTYPE html>
                <html lang=""en"">
                <head><title>Response</title></head>
                <body>
                    <h2>Formatted Response</h2>
                    <span>{content}</span>
                </body>
                </html>");
        }

        /// <summary>
        /// RichOutput - مشخص می‌کند که این فرمت‌کننده خروجی غنی (HTML) تولید می‌کند
        /// مقدار true نشان می‌دهد خروجی فرمت‌شده HTML است نه متن ساده
        /// </summary>
        public bool RichOutput => true;
    }
}
