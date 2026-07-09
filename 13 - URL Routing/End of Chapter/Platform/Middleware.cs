using Microsoft.Extensions.Options;

namespace Platform 
{

    // این کلاس نمونه‌ای از Middleware کلاس‌محور است.
    // Middleware بین درخواست و پاسخ قرار می‌گیرد و می‌تواند
    // قبل یا بعد از اجرای بخش‌های بعدی pipeline منطق خود را اجرا کند.
    public class QueryStringMiddleWare 
    {
        // next اشاره به Middleware یا Endpoint بعدی در pipeline دارد.
        private RequestDelegate? next;

        // سازنده بدون پارامتر
        public QueryStringMiddleWare() 
        {
            // do nothing
        }

        // در این سازنده، بخش بعدی pipeline دریافت می‌شود.
        public QueryStringMiddleWare(RequestDelegate nextDelegate) 
        {
            next = nextDelegate;
        }

        // متد Invoke متد اصلی اجرای Middleware است.
        public async Task Invoke(HttpContext context)
        {
            // اگر درخواست GET باشد و QueryString شامل custom=true باشد،
            // متن مشخصی در پاسخ نوشته می‌شود.
            if (context.Request.Method == HttpMethods.Get  && context.Request.Query["custom"] == "true") 
            {
                // اگر پاسخ هنوز شروع نشده باشد، می‌توان نوع محتوا را تنظیم کرد.
                if (!context.Response.HasStarted) 
                {
                    context.Response.ContentType = "text/plain";
                }
                await context.Response.WriteAsync("Class-based Middleware \n");
            }
            // در پایان، اجرای pipeline به بخش بعدی واگذار می‌شود.
            if (next != null) 
            {
                await next(context);
            }
        }
    }

    // این Middleware از IOptions برای دریافت تنظیمات استفاده می‌کند.
    // مزیت این روش این است که مقادیر پیکربندی به‌صورت strongly-typed در دسترس هستند.
    public class LocationMiddleware 
    {
        private RequestDelegate next;
        private MessageOptions options;

        // در سازنده، هم بخش بعدی pipeline و هم تنظیمات تزریق می‌شوند.
        public LocationMiddleware(RequestDelegate nextDelegate, IOptions<MessageOptions> opts) 
        {
            next = nextDelegate;
            options = opts.Value;
        }

        // اگر مسیر دقیقاً /location باشد، مقدار تنظیمات شهر و کشور نمایش داده می‌شود.
        // در غیر این صورت درخواست به بخش بعدی ارسال می‌شود.
        public async Task Invoke(HttpContext context) 
        {
            if (context.Request.Path == "/location") 
            {
                await context.Response.WriteAsync($"{options.CityName}, {options.CountryName}");
            } 
            else 
            {
                await next(context);
            }
        }
    }

}
