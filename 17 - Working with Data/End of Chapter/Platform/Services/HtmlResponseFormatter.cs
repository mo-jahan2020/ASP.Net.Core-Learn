namespace Platform.Services {
    // پیاده‌سازی IResponseFormatter که پاسخ را به صورت یک صفحه HTML کامل برمی‌گرداند
    public class HtmlResponseFormatter : IResponseFormatter {

        // متد Format که محتوای ورودی را داخل یک قالب HTML قرار داده و در پاسخ می‌نویسد
        public async Task Format(HttpContext context, string content) {
            // تنظیم نوع محتوای پاسخ به عنوان HTML
            context.Response.ContentType = "text/html";
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

        // این فرمت‌دهنده خروجی غنی (HTML) تولید می‌کند، بنابراین مقدار true برمی‌گرداند
        public bool RichOutput => true;
    }
}
