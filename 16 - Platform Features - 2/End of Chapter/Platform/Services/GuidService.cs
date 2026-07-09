// =============================================================================
// GuidService.cs - سرویس فرمت‌کننده پاسخ با GUID
// این کلاس پیاده‌سازی IResponseFormatter را انجام می‌دهد و
// یک شناسه یکتای GUID را به همراه محتوا در پاسخ نمایش می‌دهد
// GUID در زمان ساخت نمونه تولید می‌شود و در طول عمر شیء ثابت می‌ماند
// =============================================================================

namespace Platform.Services {
    /// <summary>
    /// GuidService - فرمت‌کننده پاسخ که یک GUID یکتا به محتوا اضافه می‌کند
    /// این سرویس می‌تواند برای ردیابی درخواست‌ها یا تولید شناسه‌های یکتا استفاده شود
    /// </summary>
    public class GuidService : IResponseFormatter {
        // شناسه یکتای GUID که در زمان ساخت شیء تولید می‌شود
        private Guid guid = Guid.NewGuid();

        /// <summary>
        /// Format - فرمت‌بندی و نوشتن پاسخ شامل GUID و محتوا
        /// GUID ثابت این نمونه به همراه محتوای داده‌شده نوشته می‌شود
        /// </summary>
        /// <param name="context">زمینه درخواست HTTP</param>
        /// <param name="content">محتوای متنی برای نمایش</param>
        public async Task Format(HttpContext context, string content) {
            // نوشتن GUID و محتوا در پاسخ HTTP
            await context.Response.WriteAsync($"Guid: {guid}\n{content}");
        }
    }
}
