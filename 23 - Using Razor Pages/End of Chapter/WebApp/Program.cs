// =============================================================================
// فایل Program.cs — نقطه ورود (Entry Point) برنامه ASP.NET Core
// =============================================================================
// این پروژه شامل سه الگو مختلف است:
// ۱. MVC (Model-View-Controller) — کنترلرها و Views
// ۲. Razor Pages — صفحات مستقل با مدل پشت‌صحنه
// ۳. Web API — کنترلرهای API (Products, Suppliers, Content)
// =============================================================================

// فضاهای نام مورد نیاز:
// - Microsoft.EntityFrameworkCore: برای Entity Framework Core (ORM)
// - WebApp.Models: برای دسترسی به مدل‌های دیتابیس
// - Microsoft.AspNetCore.Mvc.RazorPages: برای پیکربندی Razor Pages
using Microsoft.EntityFrameworkCore;
using WebApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

// -----------------------------------------------------------------------------
// مرحله ۱: ساخت Builder
// -----------------------------------------------------------------------------
var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// ثبت سرویس دیتابیس (DbContext) در DI Container
// -----------------------------------------------------------------------------
// AddDbContext: ثبت DataContext با SQL Server Provider.
// EnableSensitiveDataLogging: نمایش مقادیر واقعی در لاگ (فقط Development).
builder.Services.AddDbContext<DataContext>(opts => {
    opts.UseSqlServer(builder.Configuration[
        "ConnectionStrings:ProductConnection"]);
    opts.EnableSensitiveDataLogging(true);
});

// -----------------------------------------------------------------------------
// AddControllersWithViews: ثبت کنترلرهای MVC + API + Views
// -----------------------------------------------------------------------------
// این متد سه چیز را همزمان فعال می‌کند:
// ۱. کنترلرهای MVC (از Controller ارث‌بری کنند) برای سرو صفحات HTML
// ۲. کنترلرهای API (از ControllerBase با [ApiController]) برای REST API
// ۳. سیستم Views (فایل‌های .cshtml در پوشه Views)
// تفاوت با AddControllers: AddControllers فقط API را فعال می‌کند.
builder.Services.AddControllersWithViews();

// -----------------------------------------------------------------------------
// AddRazorPages: فعال‌سازی سیستم Razor Pages
// -----------------------------------------------------------------------------
// Razor Pages الگویی جایگزین MVC است که در آن هر صفحه یک فایل مستقل
// با پسوند .cshtml و یک PageModel دارد (در پوشه Pages).
// مزیت: کد و View در یک فایل یا فایل‌های هم‌نام کنار هم هستند.
builder.Services.AddRazorPages();

// -----------------------------------------------------------------------------
// پیکربندی Session (نشست)
// -----------------------------------------------------------------------------
// AddDistributedMemoryCache: ذخیره Session در حافظه (برای توسعه).
// در Production باید از Redis یا SQL Server Cache استفاده شود.
// AddSession: فعال‌سازی Middleware Session.
// Cookie.IsEssential = true: کوکی Session همیشه ارسال شود (حتی GDPR).
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => {
    options.Cookie.IsEssential = true;
});

// -----------------------------------------------------------------------------
// پیکربندی سفارشی Razor Pages — AddPageRoute
// -----------------------------------------------------------------------------
// AddPageRoute: ایجاد مسیر سفارشی برای یک Razor Page.
// "/Index": صفحه مورد نظر (Pages/Index.cshtml).
// "/extra/page/{id:long?}": مسیر اضافه‌ای. حالا صفحه Index از دو مسیر قابل دسترسی است:
//   - /Index (مسیر پیش‌فرض)
//   - /extra/page/5 (مسیر سفارشی با پارامتر id اختیاری)
// {id:long?}: پارامتر مسیر از نوع long و اختیاری (?).
builder.Services.Configure<RazorPagesOptions>(opts => {
    opts.Conventions.AddPageRoute("/Index", "/extra/page/{id:long?}");
});

// -----------------------------------------------------------------------------
// مرحله ۳: ساخت برنامه (Build)
// -----------------------------------------------------------------------------
var app = builder.Build();

// -----------------------------------------------------------------------------
// مرحله ۴: پیکربندی Middleware Pipeline
// -----------------------------------------------------------------------------
// UseStaticFiles: سرو فایل‌های استاتیک از پوشه wwwroot (CSS, JS, عکس و...).
// بدون این خط، فایل‌های Bootstrap و سایر فایل‌های استاتیک در دسترس نخواهند بود.
app.UseStaticFiles();

// UseSession: فعال‌سازی Session Middleware.
// باید بعد از UseStaticFiles و قبل از MapControllers/MapRazorPages باشد.
app.UseSession();

// MapControllers: اتصال درخواست‌های API به کنترلرهای [ApiController].
app.MapControllers();

// MapDefaultControllerRoute: مسیردهی پیش‌فرض MVC.
// الگوی مسیر: {controller=Home}/{action=Index}/{id?}
// مثال: /Home/Index/1 → کنترلر Home، اکشن Index، پارامتر id=1.
// اگر فقط / وارد شود → Home/Index اجرا می‌شود.
app.MapDefaultControllerRoute();

// MapRazorPages: اتصال درخواست‌ها به Razor Pages.
// Razor Pages مسیردهی مبتنی بر فایل دارند (مثلاً /Index → Pages/Index.cshtml).
app.MapRazorPages();

// -----------------------------------------------------------------------------
// مرحله ۵: مقداردهی اولیه دیتابیس (Seeding)
// -----------------------------------------------------------------------------
var context = app.Services.CreateScope().ServiceProvider
    .GetRequiredService<DataContext>();
//SeedData.SeedDatabase(context);

// -----------------------------------------------------------------------------
// مرحله ۶: اجرای برنامه
// -----------------------------------------------------------------------------
app.Run();
