// این فایل شامل دو نمونه از "Middleware مبتنی بر کلاس" (Class-Based Middleware) است.
// Middleware ها قطعاتی از کد هستند که در Pipeline پردازش درخواست HTTP به ترتیب اجرا می‌شوند
// و هر کدام می‌توانند قبل/بعد از فراخوانی Middleware بعدی (next) کاری انجام دهند
// یا حتی مسیر اجرای بقیه‌ی Pipeline را متوقف کنند.
using Microsoft.Extensions.Options;

namespace Platform
{

    // Middleware ای که بررسی می‌کند آیا Query String به نام "custom" با مقدار "true" ارسال شده است.
    public class QueryStringMiddleWare
    {
        // ارجاع به Middleware بعدی در زنجیره‌ی Pipeline. با فراخوانی next(context)
        // کنترل به Middleware/Endpoint بعدی منتقل می‌شود.
        private RequestDelegate? next;

        // سازنده‌ی بدون پارامتر (برای مواردی که ممکن است این کلاس بدون تزریق next ساخته شود).
        public QueryStringMiddleWare()
        {
            // do nothing
        }

        // سازنده‌ای که توسط زیرساخت ASP.NET Core فراخوانی می‌شود و مرجع Middleware بعدی را دریافت می‌کند.
        public QueryStringMiddleWare(RequestDelegate nextDelegate)
        {
            next = nextDelegate;
        }

        // متد Invoke (یا InvokeAsync) به صورت قراردادی توسط Pipeline برای هر درخواست فراخوانی می‌شود.
        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Method == HttpMethods.Get && context.Request.Query["custom"] == "true")
            {
                // بررسی این‌که پاسخ هنوز شروع به ارسال نکرده باشد، پیش از تنظیم Header ها (مثل ContentType).
                if (!context.Response.HasStarted)
                {
                    context.Response.ContentType = "text/plain";
                }
                await context.Response.WriteAsync("Class-based Middleware \n");
            }
            // انتقال کنترل به Middleware بعدی در زنجیره (در صورت وجود).
            if (next != null)
            {
                await next(context);
            }
            await context.Response.WriteAsync("After Middelware QueryStringMiddleWare Called Response to User\n");
        }
    }

    // Middleware ای که برای مسیر "/location" اطلاعات موقعیت مکانی (شهر و کشور) را
    // از یک تنظیمات پیکربندی‌شده (Options Pattern) خوانده و در پاسخ می‌نویسد.
    public class LocationMiddleware
    {
        private RequestDelegate next;
        private MessageOptions options;

        // تزریق وابستگی (Dependency Injection) در سازنده:
        // IOptions<MessageOptions> نمونه‌ای از تنظیمات ثبت‌شده در Program.cs را فراهم می‌کند.
        public LocationMiddleware(RequestDelegate nextDelegate, IOptions<MessageOptions> opts)
        {
            next = nextDelegate;
            options = opts.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            // اگر مسیر درخواست دقیقاً "/location" باشد، پاسخ مستقیم داده می‌شود
            // و دیگر به Middleware/Endpoint بعدی منتقل نمی‌شود (Short-Circuit کردن Pipeline).
            if (context.Request.Path == "/location")
            {
                await context.Response.WriteAsync($"{options.CityName}, {options.CountryName}\n");
            }
            else
            {
                // در غیر این صورت، درخواست به Middleware بعدی ارسال می‌شود.
                await next(context);
            }
            await context.Response.WriteAsync("After Middelware LocationMiddleware Called Response to User\n");
        }
    }

}
