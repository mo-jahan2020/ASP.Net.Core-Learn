// =====================================================================
// PageSize.cs - ViewComponent برای نمایش اندازه‌ی صفحه‌ی وب
// =====================================================================
// این ViewComponent یک درخواست HTTP به آدرس http://apress.com ارسال
// می‌کند و اندازه‌ی پاسخ (Content-Length) را برمی‌گرداند.
// هدف: نمایش استفاده از HttpClient در ViewComponent به صورت ناهمزمان (async).
// =====================================================================

 using Microsoft.AspNetCore.Mvc; // برای استفاده از ViewComponent

// تعریف namespace
namespace WebApp.Components {

    // تعریف کلاس PageSize که از ViewComponent ارث‌بری می‌کند
    public class PageSize : ViewComponent {

        // متد InvokeAsync: نسخه‌ی ناهمزمان (async) متد اصلی ViewComponent
        // از async زمانی استفاده می‌شود که عملیات I/O (مانند درخواست HTTP) داریم
        // این متد IViewComponentResult را به صورت Task برمی‌گرداند
        public async Task<IViewComponentResult> InvokeAsync() {
            // ساختن یک نمونه از HttpClient برای ارسال درخواست HTTP
            // توجه: در پروژه‌های واقعی بهتر است از IHttpClientFactory استفاده شود
            HttpClient client = new HttpClient();

            // ارسال درخواست GET به آدرس http://apress.com به صورت ناهمزمان
            // await: منتظر می‌ماند تا درخواست کامل شود
            // response: شامل کد وضعیت، سرصفحه‌ها (Headers) و محتوا (Content) پاسخ است
            HttpResponseMessage response
                = await client.GetAsync("http://apress.com");

            // برگرداندن View به همراه Content-Length صفحه
            // Content-Length تعداد بایت‌های پاسخ را نشان می‌دهد
            // این مقدار در View به صورت long در دسترس خواهد بود
            return View(response.Content.Headers.ContentLength);
        }
    }
}
