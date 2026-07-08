// نقطه ورود (Entry Point) برنامه‌ی ASP.NET Core.
// در قالب Minimal API، این فایل جایگزین Startup.cs و Program.cs قدیمی می‌شود
// و پیکربندی سرویس‌ها + Pipeline درخواست همگی در یک فایل انجام می‌شود.

using Platform;
using Platform.Services;

// WebApplicationBuilder وظیفه‌ی پیکربندی برنامه پیش از اجرا را دارد:
// شامل تنظیم سرویس‌ها (DI Container)، پیکربندی (appsettings.json)، لاگ‌گیری و ...
var builder = WebApplication.CreateBuilder(args);

// ثبت یک سرویس Generic به صورت Singleton در کانتینر تزریق وابستگی (Dependency Injection).
// یعنی برای هر نوع بسته‌ی ICollection<T> درخواستی، یک نمونه از List<T> ساخته می‌شود
// و چون Singleton است، در طول عمر برنامه فقط یک نمونه از هر نوع T ساخته و برای همه درخواست‌ها به اشتراک گذاشته می‌شود.
builder.Services.AddSingleton(typeof(ICollection<>), typeof(List<>));

// متد Build() برنامه‌ی نهایی (WebApplication) را می‌سازد که برای تعریف مسیرها (Endpoints)
// و Middleware ها و در نهایت اجرای برنامه استفاده می‌شود.
var app = builder.Build();

app.UseMiddleware<QueryStringMiddleWare>();

app.UseMiddleware<LocationMiddleware>();


// تعریف یک Endpoint با متد GET روی مسیر "string".
// این Endpoint سرویس ICollection<string> را از DI Container می‌گیرد (که همان Singleton بالاست)
// و هر بار که درخواست جدیدی بیاید، یک رشته جدید به کالکشن اضافه کرده و کل کالکشن را برمی‌گرداند.
// چون سرویس Singleton است، مقادیر بین درخواست‌های مختلف حفظ می‌شوند (State نگه داشته می‌شود).
// call with http://localhost:5000/string
app.MapGet("string", async context =>
{
    ICollection<string> collection = context.RequestServices.GetRequiredService<ICollection<string>>();
    collection.Add($"Request: {DateTime.Now.ToLongTimeString()}");
    foreach (string str in collection)
    {
        await context.Response.WriteAsync($"String: {str}\n");
    }
});

// تعریف یک Endpoint مشابه بالا اما برای نوع int.
// چون Generic Singleton برای هر نوع T (اینجا int) به صورت جداگانه یک نمونه می‌سازد،
// این کالکشن کاملاً مستقل از کالکشن رشته‌ای بالا عمل می‌کند.
// call with http://localhost:5000/int
app.MapGet("int", async context =>
{
    ICollection<int> collection = context.RequestServices.GetRequiredService<ICollection<int>>();
    collection.Add(collection.Count() + 1);
    foreach (int val in collection)
    {
        await context.Response.WriteAsync($"Int: {val}\n");
    }
});

//app.MapGet("/", () => Results.Redirect("/string"));

//http://localhost:5000/  // Hello World ID :0
//http://localhost:5000/121212  //Hello World ID :121212
//app.MapGet("/{id?}", (int? id ) => $"Hello World ID :{id ?? 0}");



//http://localhost:5000/  // Hello World ID :0
//http://localhost:5000/121212 error
//http://localhost:5000/?id=1212 // Hello World ID :1212
//app.MapGet("/", (int? id) => $"Hello World ID :{id ?? 0}");
// اجرای برنامه و شروع به گوش دادن (Listen) روی درخواست‌های HTTP دریافتی.
// این متد بلاک‌کننده (Blocking) است و تا زمانی که برنامه متوقف نشود، اجرا ادامه می‌یابد.
app.Run();
