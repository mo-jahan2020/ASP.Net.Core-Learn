// =============================================================================
// TextResponseFormatter.cs - فرمت‌کننده پاسخ متنی ساده
// این کلاس محتوا را به صورت متن ساده با شماره‌گذاری پاسخ‌ها نمایش می‌دهد
// شامل یک شمارنده (Counter) برای شماره‌گذاری ترتیبی پاسخ‌ها و
// یک پیاده‌سازی Singleton برای اشتراک‌گذاری نمونه واحد است
// =============================================================================

namespace Platform.Services {
    /// <summary>
    /// TextResponseFormatter - فرمت‌کننده پاسخ متنی ساده با شمارنده
    /// هر بار که متد Format فراخوانی می‌شود، شمارنده افزایش یافته و
    /// شماره پاسخ به همراه محتوا نمایش داده می‌شود
    /// </summary>
    public class TextResponseFormatter : IResponseFormatter {
        // شمارنده تعداد پاسخ‌های تولید شده توسط این نمونه
        private int responseCounter = 0;
        // نمونه اشتراکی (Singleton) از این کلاس
        private static TextResponseFormatter? shared;

        /// <summary>
        /// Format - فرمت‌بندی محتوا به صورت متن ساده با شماره پاسخ
        /// شمارنده قبل از هر پاسخ یک واحد افزایش می‌یابد
        /// </summary>
        /// <param name="context">زمینه درخواست HTTP</param>
        /// <param name="content">محتوای متنی برای نمایش</param>
        public async Task Format(HttpContext context, string content) {
            // افزایش شمارنده و نوشتن شماره پاسخ به همراه محتوا
            await context.Response.
                WriteAsync($"Response {++responseCounter}:\n{content}");
        }

        /// <summary>
        /// Singleton - دسترسی به نمونه اشتراکی (Singleton) از TextResponseFormatter
        /// با استفاده از این ویژگی، یک نمونه واحد در سراسر اپلیکیشن به اشتراک گذاشته می‌شود
        /// الگوی Lazy Initialization: نمونه فقط در اولین دسترسی ایجاد می‌شود
        /// </summary>
        public static TextResponseFormatter Singleton {
            get {
                // اگر هنوز نمونه‌ای ایجاد نشده، یک نمونه جدید بساز
                if (shared == null) {
                    shared = new TextResponseFormatter();
                }
                return shared; // بازگرداندن نمونه اشتراکی
            }
        }
    }
}
