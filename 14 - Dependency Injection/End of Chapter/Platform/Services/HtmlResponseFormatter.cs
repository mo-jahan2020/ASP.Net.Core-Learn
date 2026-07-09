// یکی دیگر از پیاده‌سازی‌های اینترفیس IResponseFormatter که به‌جای متن ساده،
// یک صفحه‌ی کامل HTML به‌عنوان پاسخ برمی‌گرداند (نمونه‌ای از الگوی Strategy Pattern،
// که در آن چند پیاده‌سازی مختلف از یک قرارداد یکسان (اینترفیس) وجود دارد).
﻿namespace Platform.Services {
    public class HtmlResponseFormatter : IResponseFormatter {

        public async Task Format(HttpContext context, string content) {
            // تنظیم نوع محتوای پاسخ به HTML تا مرورگر آن را به‌درستی رندر کند.
            context.Response.ContentType = "text/html";
            // نوشتن یک قالب HTML کامل با استفاده از Interpolated String چندخطی ($@"...")
            // و جای‌گذاری محتوای دریافتی (content) درون بدنه‌ی صفحه.
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

        // پیاده‌سازی صریح یک عضو پیش‌فرض اینترفیس (Default Interface Member)؛
        // این کلاس مقدار true را برمی‌گرداند تا اعلام کند خروجی آن "غنی" (شامل HTML) است،
        // برخلاف پیاده‌سازی پیش‌فرض در خودِ اینترفیس که false است.
        public bool RichOutput => true;
    }
}
