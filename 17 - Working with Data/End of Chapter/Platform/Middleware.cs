using Microsoft.Extensions.Options;

namespace Platform {

    // میان‌افزار (Middleware) مبتنی بر کلاس که بر اساس query string عمل می‌کند
    public class QueryStringMiddleWare {
        // مرجع به Middleware بعدی در زنجیره پردازش درخواست
        private RequestDelegate? next;

        // سازنده بدون پارامتر (در این پروژه استفاده خاصی ندارد)
        public QueryStringMiddleWare() {
            // do nothing
        }

        // سازنده‌ای که Middleware بعدی را دریافت و ذخیره می‌کند
        public QueryStringMiddleWare(RequestDelegate nextDelegate) {
            next = nextDelegate;
        }

        // متدی که برای هر درخواست فراخوانی می‌شود
        public async Task Invoke(HttpContext context) {
            // اگر متد درخواست GET باشد و پارامتر custom در query string برابر "true" باشد
            if (context.Request.Method == HttpMethods.Get
                        && context.Request.Query["custom"] == "true") {
                // اگر پاسخ هنوز شروع نشده باشد نوع محتوا را متنی تنظیم کن
                if (!context.Response.HasStarted) {
                    context.Response.ContentType = "text/plain";
                }
                // نوشتن یک پیام در پاسخ
                await context.Response.WriteAsync("Class-based Middleware \n");
            }
            // در صورت وجود Middleware بعدی، درخواست را به آن پاس بده
            if (next != null) {
                await next(context);
            }
        }
    }

    // میان‌افزاری که بر اساس مسیر درخواست، اطلاعات مکان (شهر/کشور) را برمی‌گرداند
    public class LocationMiddleware {
        private RequestDelegate next;
        private MessageOptions options;

        // سازنده‌ای که Middleware بعدی و تنظیمات (Options) را از طریق تزریق وابستگی دریافت می‌کند
        public LocationMiddleware(RequestDelegate nextDelegate,
                IOptions<MessageOptions> opts) {
            next = nextDelegate;
            options = opts.Value;
        }

        // متدی که برای هر درخواست فراخوانی می‌شود
        public async Task Invoke(HttpContext context) {
            // اگر مسیر درخواست دقیقا "/location" باشد
            if (context.Request.Path == "/location") {
                // نام شهر و کشور تنظیم‌شده را در پاسخ بنویس
                await context.Response
                    .WriteAsync($"{options.CityName}, {options.CountryName}");
            } else {
                // در غیر این صورت درخواست را به Middleware بعدی پاس بده
                await next(context);
            }
        }
    }

}
