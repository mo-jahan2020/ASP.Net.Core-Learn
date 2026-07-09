using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SportsStore.Models;

// ============================================================================
// کلاس Program - نقطه شروع برنامه ASP.NET Core
// ============================================================================
// در ASP.NET Core از متد Main برنامه شروع می‌شود.
// در نسخه‌های جدید، از "Top-level statements" استفاده می‌شود که نیازی به
// تعریف صریح کلاس Program و متد Main نیست.
// ============================================================================

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------------------------------
// مرحله 1: ثبت Service ها در Dependency Injection Container
// ----------------------------------------------------------------------------

// افزودن سرویس‌های Razor Pages (برای صفحات Razor)
builder.Services.AddRazorPages();

// پیکربندی Session برای ذخیره سبد خرید
// نکته: Session برای ذخیره سبد خرید کاربر بین درخواست‌ها استفاده می‌شود.
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30); // مدت اعتبار Session
    options.Cookie.HttpOnly = true; // جلوگیری از دسترسی JavaScript به کوکی
    options.Cookie.IsEssential = true; // لازم برای عملکرد برنامه
});

// ----------------------------------------------------------------------------
// ثبت StoreDbContext با استفاده از ConnectionString از appsettings.json
// ----------------------------------------------------------------------------
// ConnectionString از کلید "Data:StoreProducts:ConnectionStrings" خوانده می‌شود.
// این کلید در فایل appsettings.json (یا appsettings.Development.json) تعریف شده.
builder.Services.AddDbContext<StoreDbContext>(options =>
    options.UseSqlServer(builder.Configuration["Data:StoreProducts:ConnectionStrings"]));

// ثبت AppIdentityDbContext برای احراز هویت
builder.Services.AddDbContext<AppIdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration["Data:Identity:ConnectionStrings"]));

// ----------------------------------------------------------------------------
// پیکربندی Identity با نقش‌ها (Roles) و سیاست رمز عبور قوی
// ----------------------------------------------------------------------------
// AddIdentity<IdentityUser, IdentityRole>: تزریق نقش‌ها به سیستم Identity
// این کار باعث می‌شود RoleManager قابل استفاده باشد
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => {
        // --- تنظیمات سیاست رمز عبور قوی ---
    // این تنظیمات از رمزهای ضعیف جلوگیری می‌کنند
    options.Password.RequiredLength = 8;        // حداقل طول ۸ کاراکتر
    options.Password.RequireNonAlphanumeric = true; // حداقل یک کاراکتر ویژه (!@#$%)
    options.Password.RequireDigit = true;       // حداقل یک عدد
    options.Password.RequireUppercase = true;   // حداقل یک حرف بزرگ
    options.Password.RequireLowercase = true;   // حداقل یک حرف کوچک
    // --- تنظیمات Lockout (قفل اکانت پس از تلاش‌های ناموفق) ---
    options.Lockout.MaxFailedAccessAttempts = 5; // حداکثر ۵ تلاش ناموفق
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // قفل ۱۵ دقیقه
    options.Lockout.AllowedForNewUsers = true;
        // --- تنظیمات کاربر ---
    options.User.RequireUniqueEmail = false; // چون در IdentitySeedData ایمیل یکسان داریم
    options.SignIn.RequireConfirmedEmail = false; // برای سادگی پروژه آموزشی

}).AddEntityFrameworkStores<AppIdentityDbContext>()
  .AddDefaultTokenProviders();

// ثبت Repository ها به صورت Scoped (یک نمونه به ازای هر Request)
builder.Services.AddScoped<IStoreRepository, EFStoreRepository>();
builder.Services.AddScoped<IOrderRepository, EFOrderRepository>();

// ----------------------------------------------------------------------------
// ثبت سبد خرید (Cart) به صورت Scoped
// ----------------------------------------------------------------------------
// برای هر کاربر یک سبد جدید ساخته می‌شود.
// SessionCart.GetCart از Session کاربر فعلی، سبد را بارگذاری می‌کند.
// اگر سبدی در Session نباشد، یک سبد خالی جدید می‌سازد.
builder.Services.AddScoped<Cart>(sp => SessionCart.GetCart(sp));
// IHttpAccessor برای دسترسی به HttpContext (و Session) در سرویس‌ها ضروری است
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// پیکربندی مسیرهای Identity (Login، Logout، ...)
builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/Account/Login"; // مسیر ورود به سیستم
    options.LogoutPath = "/Account/Logout"; // مسیر خروج
    options.AccessDeniedPath = "/Account/AccessDenied"; // مسیر دسترسی ممنوع
    options.ExpireTimeSpan = TimeSpan.FromHours(2); // مدت اعتبار کوکی ورود
    options.SlidingExpiration = true; // تمدید خودکار کوکی
});

// ----------------------------------------------------------------------------
// مرحله 2: Build کردن Application
// ----------------------------------------------------------------------------
var app = builder.Build();

// ----------------------------------------------------------------------------
// مرحله 3: پیکربندی Pipeline پردازش Request ها
// ----------------------------------------------------------------------------

// فعال‌سازی فایل‌های استاتیک (CSS, JS, Images از wwwroot)
app.UseStaticFiles();

// فعال‌سازی Routing (مسیریابی)
app.UseRouting();

// فعال‌سازی Session (باید قبل از UseAuthentication باشد)
app.UseSession();

// فعال‌سازی احراز هویت و authorization
// ترتیب مهم است: ابتدا Authentication سپس Authorization
app.UseAuthentication();
app.UseAuthorization();

// فعال‌سازی صفحات Razor با مسیر پیش‌فرض
app.MapRazorPages();

// مسیر پیش‌فرض - هدایت کاربر به صفحه اصلی محصولات
app.MapGet("/", context => {
    context.Response.Redirect("/Products");
    return Task.CompletedTask;
});

// ----------------------------------------------------------------------------
// مرحله 4: مقداردهی اولیه دیتابیس‌ها
// ----------------------------------------------------------------------------

// پر کردن دیتابیس محصولات با داده‌های اولیه (در صورت نیاز)
SeedData.EnsurePopulated(app);

// ایجاد کاربر Admin پیش‌فرض و نقش‌ها (در صورت نیاز)
IdentitySeedData.EnsurePopulatedAsync(app).GetAwaiter().GetResult();

// اجرای برنامه
app.Run();
