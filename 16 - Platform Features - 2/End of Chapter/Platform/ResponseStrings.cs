// =============================================================================
// ResponseStrings.cs - رشته‌های پاسخ HTML پیش‌فرض
// این کلاس شامل قالب HTML پیش‌فرض برای نمایش صفحات خطا است
// از این قالب در Program.cs برای صفحات کد وضعیت استفاده می‌شود
// =============================================================================

namespace Platform {
    /// <summary>
    /// Responses - کلاس استاتیک نگهدارنده رشته‌های پاسخ
    /// شامل قالب‌های HTML پیش‌فرض برای خطاهای HTTP
    /// </summary>
    public static class Responses {
        /// <summary>
        /// DefaultResponse - قالب HTML پیش‌فرض برای صفحات خطا
        /// شامل یک الگوی فرمت با {0} برای نمایش کد خطا
        /// از فایل bootstrap.min.css برای استایل‌دهی استفاده می‌کند
        /// و لینکی به صفحه اصلی دارد
        /// </summary>
        public static string DefaultResponse = @"
        <!DOCTYPE html>
            <html lang=""en"">
            <head>
                <!-- اضافه کردن فایل CSS بوت‌استرپ -->
                <link rel=""stylesheet"" 
                   href=""/lib/bootstrap/css/bootstrap.min.css"" />
                <title>Error</title>
            </head>
            <body class=""text-center"">
                <!-- نمایش کد خطا با استفاده از الگوی فرمت {0} -->
                <h3 class=""p-2"">Error {0}</h3>
                <h6>
                    You can go back to the <a href=""/"">homepage</a> and try again
                </h6>
            </body>
        </html>";
    }
}
