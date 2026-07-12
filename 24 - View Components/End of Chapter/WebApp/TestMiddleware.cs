// =====================================================================
// TestMiddleware.cs - یک Middleware سفارشی برای تست
// =====================================================================
// Middleware کامپوننتی است که در خط لوله‌ی HTTP قرار می‌گیرد و می‌تواند
// درخواست‌ها را پردازش، تغییر یا متوقف کند. این Middleware فقط زمانی
// که مسیر "/test" باشد، اطلاعات دیتابیس را نمایش می‌دهد، در غیر این
// صورت درخواست را به Middleware بعدی در زنجیره ارسال می‌کند.
// =====================================================================

 using WebApp.Models; // فضای نام مدل‌ها برای دسترسی به DataContext

// تعریف namespace پروژه
namespace WebApp {
    // تعریف کلاس Middleware سفارشی
    // نام‌گذاری معمول: XxxMiddleware
    public class TestMiddleware {
        // فیلد داخلی برای نگهداری delegate مربوط به Middleware بعدی در زنجیره
        // RequestDelegate یک متد است که HttpContext را به عنوان ورودی می‌گیرد
        // و یک Task برمی‌گرداند (پاسخ HTTP)
        private RequestDelegate nextDelegate;

        // سازنده (Constructor) کلاس Middleware
        // ASP.NET Core به صورت خودکار این سازنده را با next (middleware بعدی) فراخوانی می‌کند
        public TestMiddleware(RequestDelegate next) {
            // ذخیره‌ی delegate مربوط به Middleware بعدی در فیلد کلاس
            nextDelegate = next;
        }

        // متد Invoke: متد اصلی Middleware که برای هر درخواست HTTP فراخوانی می‌شود
        // این متد می‌تواند سرویس‌های دیگر را از طریق Dependency Injection دریافت کند
        // در اینجا DataContext به عنوان پارامتر تزریق می‌شود
        public async Task Invoke(HttpContext context, DataContext dataContext) {
            // بررسی اینکه آیا مسیر درخواست برابر با "/test" است
            if (context.Request.Path == "/test") {
                // نوشتن تعداد محصولات موجود در دیتابیس در پاسخ
                // WriteAsync متن را مستقیماً به جریان پاسخ HTTP ارسال می‌کند
                await context.Response.WriteAsync(
                    $"There are {dataContext.Products.Count()} products\n");
                // نوشتن تعداد دسته‌بندی‌ها
                await context.Response.WriteAsync(
                    $"There are {dataContext.Categories.Count()} categories\n");
                // نوشتن تعداد تأمین‌کنندگان
                await context.Response.WriteAsync(
                    $"There are {dataContext.Suppliers.Count()} suppliers\n");
            } else {
                // اگر مسیر /test نبود، درخواست را به Middleware بعدی در زنجیره ارسال می‌کنیم
                // nextDelegate(context) یعنی: "من کارم با این درخواست تمام شد، برو بعدی"
                await nextDelegate(context);
            }
        }
    }
}
