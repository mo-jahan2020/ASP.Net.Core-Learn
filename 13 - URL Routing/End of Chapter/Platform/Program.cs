using Platform;

// این فایل نقطه شروع اجرای برنامه است.
// در الگوی Minimal API معمولاً سرویس‌ها، تنظیمات مسیرها و Endpointها در همین فایل تعریف می‌شوند.

var builder = WebApplication.CreateBuilder(args);

// RouteOptions برای تنظیم رفتار سیستم Routing استفاده می‌شود.
// اینجا یک Route Constraint سفارشی با نام countryName ثبت می‌کنیم
// تا بعداً بتوانیم در الگوهای مسیر از آن استفاده کنیم.
builder.Services.Configure<RouteOptions>(opts =>
{
    opts.ConstraintMap.Add("countryName", typeof(CountryRouteConstraint));
});

var app = builder.Build();

// این Middleware به‌صورت inline نوشته شده است.
// وظیفه آن این است که قبل از رسیدن درخواست به Endpoint نهایی،
// بررسی کند آیا Endpoint انتخاب شده یا نه.
app.Use(async (context, next) =>
{
    // GetEndpoint نتیجه Routing را برمی‌گرداند.
    Endpoint? end = context.GetEndpoint();
    if (end != null)
    {
        await context.Response.WriteAsync($"{end.DisplayName} Selected \n");
    }
    else
    {
        await context.Response.WriteAsync("No Endpoint Selected \n");
    }
    await next();
});

// این Endpoint فقط زمانی انتخاب می‌شود که بخش مسیر از نوع int باشد.
// Order پایین‌تر یعنی اولویت بیشتر در انتخاب مسیر.
app.Map("{number:int}", async context =>
{
    await context.Response.WriteAsync("Routed to the int endpoint");
}).WithDisplayName("Int Endpoint").Add(b => ((RouteEndpointBuilder)b).Order = 1);

// این Endpoint برای مقادیر double است.
// چون Order آن 2 است، بعد از endpoint عدد صحیح بررسی می‌شود.
app.Map("{number:double}", async context =>
{
    await context.Response.WriteAsync("Routed to the double endpoint");
}).WithDisplayName("Double Endpoint").Add(b => ((RouteEndpointBuilder)b).Order = 2);

// MapFallback زمانی اجرا می‌شود که هیچ مسیر دیگری با درخواست فعلی سازگار نباشد.
app.MapFallback(async context =>
{
    await context.Response.WriteAsync("Routed to fallback endpoint");
});

// اجرای برنامه
app.Run();
