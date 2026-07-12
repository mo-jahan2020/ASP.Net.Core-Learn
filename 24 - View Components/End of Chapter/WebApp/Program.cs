// =====================================================================
// Program.cs - نقطه شروع (Entry Point) برنامه ASP.NET Core
// =====================================================================
// در .NET 6 به بعد، این فایل به جای کلاس Program و متد Main استفاده می‌شود.
// تمام تنظیمات سرویس‌ها (Services) و خط لوله درخواست‌ها (Request Pipeline)
// در همین فایل و با استفاده از "Minimal Hosting Model" انجام می‌شود.
// =====================================================================

using Microsoft.EntityFrameworkCore; // اضافه کردن فضای نام EF Core برای کار با پایگاه داده
using WebApp.Models;                 // فضای نام مدل‌های پروژه (Product, Category, Supplier, ...)
using Microsoft.AspNetCore.Mvc.RazorPages; // فضای نام مربوط به Razor Pages

// ایجاد یک شیء WebApplicationBuilder برای پیکربندی اولیه برنامه
// این شیء شامل تنظیمات Configuration, Logging, Services و ... است
var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------
// ثبت سرویس‌ها در Container اصلی برنامه (Dependency Injection)
// ---------------------------------------------------------------------

// AddDbContext: ثبت DbContext مربوط به Entity Framework Core
// DataContext کلاس پایگاه داده‌ی برنامه است که در فولدر Models تعریف شده
// opts.UseSqlServer: مشخص می‌کند که پایگاه داده، SQL Server است
// builder.Configuration["ConnectionStrings:ProductConnection"]: خواندن رشته اتصال
//   از فایل appsettings.json با استفاده از سیستم پیکربندی (IConfiguration)
// EnableSensitiveDataLogging(true): نمایش مقادیر واقعی پارامترها در لاگ‌ها
//   (فقط در محیط توسعه استفاده شود، چون اطلاعات حساس را فاش می‌کند)
builder.Services.AddDbContext<DataContext>(opts =>
{
    opts.UseSqlServer(builder.Configuration[
        "ConnectionStrings:ProductConnection"]);
    opts.EnableSensitiveDataLogging(true);
});

// AddControllersWithViews: فعال‌سازی الگوی MVC کامل (Controllers + Views)
// این سرویس، ویژگی‌های Controllerها، Viewها و Model Binding را فراهم می‌کند
builder.Services.AddControllersWithViews();

// AddRazorPages: فعال‌سازی سیستم Razor Pages (الگوی صفحه‌ای - Page-based)
// Razor Pages ترکیبی از Controller و View در یک فایل است
builder.Services.AddRazorPages();

// AddDistributedMemoryCache: ثبت کش توزیع‌شده در حافظه RAM
// این کش برای نگهداری موقت داده‌ها مثل داده‌های Session استفاده می‌شود
builder.Services.AddDistributedMemoryCache();

// AddSession: فعال‌سازی قابلیت Session (نگهداری اطلاعات کاربر در سرور)
// Cookie.IsEssential = true: کوکی سشن ضروری است و برای GDPR نیاز به رضایت ندارد
builder.Services.AddSession(options =>
{
    options.Cookie.IsEssential = true;
});

// Configure<RazorPagesOptions>: پیکربندی سفارشی Razor Pages
// Conventions.AddPageRoute: اضافه کردن یک مسیر (Route) سفارشی به یک Page خاص
//   در اینجا، Page ای به نام Index (یعنی Pages/Index.cshtml) علاوه بر مسیر اصلی
//   از مسیر "/extra/page/{id:long?}" نیز قابل دسترسی می‌شود
//   id:long? یعنی id اختیاری است و باید از نوع long باشد
builder.Services.Configure<RazorPagesOptions>(opts =>
{
    opts.Conventions.AddPageRoute("/Index", "/extra/page/{id:long?}");
});

// AddSingleton<CitiesData>: ثبت کلاس CitiesData به صورت Singleton
// Singleton یعنی برای کل طول عمر برنامه، فقط یک نمونه از این کلاس ساخته می‌شود
// (CitiesData داده‌های شهرها را در حافظه نگهداری می‌کند)
builder.Services.AddSingleton<CitiesData>();

// ساختن شیء WebApplication از روی builder
// در این مرحله، سرویس‌ها و پیکربندی‌ها نهایی می‌شوند
var app = builder.Build();

// ---------------------------------------------------------------------
// پیکربندی خط لوله درخواست‌ها (Middleware Pipeline)
// ---------------------------------------------------------------------
// ترتیب middlewareها بسیار مهم است؛ درخواست از بالا به پایین و پاسخ از پایین به بالا جریان دارد

// UseStaticFiles: اجازه می‌دهد فایل‌های ایستا (CSS, JS, تصاویر) از wwwroot سرو شوند
app.UseStaticFiles();

// UseSession: فعال‌سازی middleware مربوط به Session (باید بعد از UseStaticFiles باشد)
app.UseSession();

// MapControllers: مسیردهی درخواست‌ها به Actionهای کنترلرها
// این دستور، Attributeهای [Route] و Conventionهای مسیردهی را فعال می‌کند
app.MapControllers();

// MapDefaultControllerRoute: مسیردهی پیش‌فرض MVC
// الگوی پیش‌فرض: "{controller=Home}/{action=Index}/{id?}"
app.MapDefaultControllerRoute();

// MapRazorPages: مسیردهی درخواست‌ها به Razor Pages
app.MapRazorPages();

// ---------------------------------------------------------------------
// ایجاد Scope برای دسترسی به سرویس‌های Scoped مثل DbContext
// ---------------------------------------------------------------------
// CreateScope(): یک Scope موقت ایجاد می‌کند (DbContextها معمولاً Scoped هستند)
// ServiceProvider: ظرف سرویس‌ها (DI Container)
// GetRequiredService<DataContext>(): گرفتن نمونه‌ای از DataContext

var context = app.Services.CreateScope().ServiceProvider
    .GetRequiredService<DataContext>();

// فراخوانی متد SeedDatabase برای پر کردن دیتابیس با داده‌های اولیه
// این متد بررسی می‌کند که آیا داده‌ای وجود دارد یا نه، و در صورت نبود، اضافه می‌کند
SeedData.SeedDatabase(context);

// app.Run(): شروع به گوش دادن به درخواست‌های HTTP و اجرای برنامه
app.Run();
