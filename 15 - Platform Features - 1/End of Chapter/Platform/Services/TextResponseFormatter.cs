namespace Platform.Services {
    // پیاده‌سازی IResponseFormatter که هر بار یک شمارنده را افزایش داده
    // و همراه با محتوای ورودی در پاسخ می‌نویسد
    public class TextResponseFormatter : IResponseFormatter {
        // شمارنده تعداد پاسخ‌های تولید شده توسط این نمونه
        private int responseCounter = 0;
        // نمونه مشترک (Singleton) که به صورت دستی پیاده‌سازی شده است
        private static TextResponseFormatter? shared;

        // افزایش شمارنده و نوشتن شماره پاسخ به همراه محتوا
        public async Task Format(HttpContext context, string content) {
            await context.Response.
                WriteAsync($"Response {++responseCounter}:\n{content}");
        }

        // خاصیتی برای دسترسی به نمونه Singleton؛ در صورت نبودن، یک نمونه جدید ساخته می‌شود
        public static TextResponseFormatter Singleton {
            get {
                if (shared == null) {
                    shared = new TextResponseFormatter();
                }
                return shared;
            }
        }
    }
}
