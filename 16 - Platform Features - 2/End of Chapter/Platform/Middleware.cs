// =============================================================================
// Middleware.cs - کلاس‌های میان‌افزار سفارشی
// شامل دو میان‌افزار:
// 1. QueryStringMiddleWare - بررسی پارامتر کوئری custom و تولید پاسخ متنی
// 2. LocationMiddleware - نمایش اطلاعات مکان بر اساس تنظیمات
// =============================================================================

using Microsoft.Extensions.Options; // وارد کردن IOptions برای الگوی Options

namespace Platform {
    /// <summary>
    /// QueryStringMiddleWare - میان‌افزار بررسی رشته کوئری
    /// این میان‌افزار بررسی می‌کند آیا پارامتر "custom=true" در کوئری استرینگ وجود دارد
    /// در این صورت یک پیام متنی برمی‌گرداند
    /// </summary>
    public class QueryStringMiddleWare {
        // ارجاع به میان‌افزار بعدی در زنجیره (اختیاری - ممکن است null باشد)
        private RequestDelegate? next;

        /// <summary>
        /// سازنده بدون پارامتر - بدون تنظیم میان‌افزار بعدی
        /// این حالت برای زمانی است که این میان‌افزار آخرین در زنجیره باشد
        /// </summary>
        public QueryStringMiddleWare() {
            // بدون عملیات - next به صورت پیش‌فرض null است
        }

        /// <summary>
        /// سازنده با پارامتر - تنظیم میان‌افزار بعدی
        /// </summary>
        /// <param name="nextDelegate">دلیگیت میان‌افزار بعدی در خط لوله</param>
        public QueryStringMiddleWare(RequestDelegate nextDelegate) {
            next = nextDelegate;
        }

        /// <summary>
        /// متد Invoke - پردازش درخواست HTTP
        /// اگر متد GET باشد و پارامتر custom=true در کوئری وجود داشته باشد،
        /// پیام "Class-based Middleware" نوشته می‌شود
        /// </summary>
        /// <param name="context">زمینه درخواست HTTP جاری</param>
        public async Task Invoke(HttpContext context) {
            // بررسی اینکه آیا درخواست از نوع GET است و پارامتر custom=true دارد
            if (context.Request.Method == HttpMethods.Get
                        && context.Request.Query["custom"] == "true") {
                // بررسی اینکه آیا پاسخ هنوز شروع نشده است (می‌توان هدرها را تغییر داد)
                if (!context.Response.HasStarted) {
                    context.Response.ContentType = "text/plain"; // تنظیم نوع محتوای پاسخ
                }
                // نوشتن پیام میان‌افزار در پاسخ
                await context.Response.WriteAsync("Class-based Middleware \n");
            }
            // اگر میان‌افزار بعدی وجود دارد، درخواست به آن ارجاع داده می‌شود
            if (next != null) {
                await next(context);
            }
        }
    }

    /// <summary>
    /// LocationMiddleware - میان‌افزار نمایش اطلاعات مکان
    /// این میان‌افزار نام شهر و کشور را از تنظیمات (MessageOptions) خوانده
    /// و در مسیر /location نمایش می‌دهد
    /// </summary>
    public class LocationMiddleware {
        // ارجاع به میان‌افزار بعدی در زنجیره
        private RequestDelegate next;
        // نمونه تنظیمات شامل نام شهر و کشور
        private MessageOptions options;

        /// <summary>
        /// سازنده - دریافت میان‌افزار بعدی و تنظیمات مکان
        /// از الگوی IOptions برای تزریق تنظیمات استفاده می‌شود
        /// </summary>
        /// <param name="nextDelegate">دلیگیت میان‌افزار بعدی</param>
        /// <param name="opts">تنظیمات مکان (شهر و کشور)</param>
        public LocationMiddleware(RequestDelegate nextDelegate,
                IOptions<MessageOptions> opts) {
            next = nextDelegate;
            options = opts.Value; // استخراج مقدار واقعی از IOptions
        }

        /// <summary>
        /// متد Invoke - پردازش درخواست HTTP
        /// اگر مسیر /location باشد، نام شهر و کشور نمایش داده می‌شود
        /// </summary>
        /// <param name="context">زمینه درخواست HTTP جاری</param>
        public async Task Invoke(HttpContext context) {
            if (context.Request.Path == "/location") {
                // نمایش نام شهر و کشور از تنظیمات
                await context.Response
                    .WriteAsync($"{options.CityName}, {options.CountryName}");
            } else {
                // ارجاع به میان‌افزار بعدی برای سایر مسیرها
                await next(context);
            }
        }
    }

}
