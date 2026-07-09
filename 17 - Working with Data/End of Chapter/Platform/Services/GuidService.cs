namespace Platform.Services {

    // پیاده‌سازی IResponseFormatter که یک شناسه یکتا (GUID) را همراه با محتوا در پاسخ می‌نویسد
    public class GuidService : IResponseFormatter {
        // یک GUID که در زمان ساخت نمونه، فقط یک بار تولید می‌شود
        private Guid guid = Guid.NewGuid();

        // نوشتن GUID و محتوای ورودی در پاسخ
        public async Task Format(HttpContext context, string content) {
            await context.Response.WriteAsync($"Guid: {guid}\n{content}");
        }
    }
}
