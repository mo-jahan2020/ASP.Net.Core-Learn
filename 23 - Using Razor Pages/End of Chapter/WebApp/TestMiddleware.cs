// =============================================================================
// فایل TestMiddleware.cs — مثال Middleware سفارشی در ASP.NET Core
// =============================================================================
// Middleware کامپوننتی است که در Pipeline درخواست‌های HTTP قرار می‌گیرد و
// می‌تواند هر درخواست را قبل از رسیدن به کنترلر بررسی و پردازش کند.
// الگوی Middleware بر اساس زنجیره مسئولیت (Chain of Responsibility) کار می‌کند.
// =============================================================================

using WebApp.Models;

namespace WebApp {
    // -------------------------------------------------------------------------
    // کلاس TestMiddleware — Middleware سفارشی
    // -------------------------------------------------------------------------
    public class TestMiddleware {
        // ---------------------------------------------------------------------
        // RequestDelegate: اشاره‌گر به Middleware بعدی در Pipeline
        // ---------------------------------------------------------------------
        private RequestDelegate nextDelegate;

        // ---------------------------------------------------------------------
        // Constructor — تزریق خودکار RequestDelegate توسط ASP.NET Core
        // ---------------------------------------------------------------------
        public TestMiddleware(RequestDelegate next) {
            nextDelegate = next;
        }

        // ---------------------------------------------------------------------
        // متد Invoke — نقطه اجرای Middleware (برای هر درخواست HTTP)
        // ---------------------------------------------------------------------
        // HttpContext: اطلاعات درخواست و امکان ارسال پاسخ
        // DataContext: تزریق خودکار از DI Container برای دسترسی به دیتابیس
        public async Task Invoke(HttpContext context, DataContext dataContext) {
            // اگر مسیر "/test" باشد، تعداد رکوردها را نمایش بده و زنجیره را متوقف کن
            if (context.Request.Path == "/test") {
                await context.Response.WriteAsync(
                    $"There are {dataContext.Products.Count()} products\n");
                await context.Response.WriteAsync(
                    $"There are {dataContext.Categories.Count()} categories\n");
                await context.Response.WriteAsync(
                    $"There are {dataContext.Suppliers.Count()} suppliers\n");
            } else {
                // انتقال درخواست به Middleware بعدی در Pipeline
                await nextDelegate(context);
            }
        }
    }
}
