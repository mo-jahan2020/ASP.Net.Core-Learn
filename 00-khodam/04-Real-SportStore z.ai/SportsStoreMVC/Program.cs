using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SportsStore.Models;

// ============================================================================
// کلاس Program - نقطه شروع برنامه ASP.NET Core MVC
// ============================================================================
// در ASP.NET Core از متد Main برنامه شروع می‌شود.
// در نسخه‌های جدید، از "Top-level statements" استفاده می‌شود که نیازی به
// تعریف صریح کلاس Program و متد Main نیست.
// ============================================================================

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------------------------------
// مرحله 1: ثبت Service ها در Dependency Injection Container
// ----------------------------------------------------------------------------

// افزودن سرویس‌های MVC (Controllers + Views)
// نکته: در نسخه Razor Pages از AddRazorPages() استفاده می‌کردیم
builder.Services.AddControllersWithViews();

// پیکربندی Session برای ذخیره سبد خرید
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ----------------------------------------------------------------------------
// ثبت DbContext ها با ConnectionString از appsettings.json
// ----------------------------------------------------------------------------
builder.Services.AddDbContext<StoreDbContext>(options =>
    options.UseSqlServer(builder.Configuration["Data:StoreProducts:ConnectionStrings"]));

builder.Services.AddDbContext<AppIdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration["Data:Identity:ConnectionStrings"]));

// ----------------------------------------------------------------------------
// پیکربندی Identity با نقش‌ها (Roles) و سیاست رمز عبور قوی
// ----------------------------------------------------------------------------
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => {

    // --- تنظیمات سیاست رمز عبور قوی ---
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // --- تنظیمات Lockout ---
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers = true;

    options.User.RequireUniqueEmail = false;
    options.SignIn.RequireConfirmedEmail = false;

}).AddEntityFrameworkStores<AppIdentityDbContext>()
  .AddDefaultTokenProviders();

// ثبت Repository ها به صورت Scoped
builder.Services.AddScoped<IStoreRepository, EFStoreRepository>();
builder.Services.AddScoped<IOrderRepository, EFOrderRepository>();

// ثبت سبد خرید به صورت Scoped (یک نمونه به ازای هر کاربر)
builder.Services.AddScoped<Cart>(sp => SessionCart.GetCart(sp));
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// پیکربندی مسیرهای Identity (Login، Logout، ...)
builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;
});

// ----------------------------------------------------------------------------
// مرحله 2: Build کردن Application
// ----------------------------------------------------------------------------
var app = builder.Build();

// ----------------------------------------------------------------------------
// مرحله 3: پیکربندی Pipeline پردازش Request ها
// ----------------------------------------------------------------------------

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// ----------------------------------------------------------------------------
// پیکربندی مسیریابی (Routing) برای MVC
// ----------------------------------------------------------------------------
// الگوی پیش‌فرض: {controller=Home}/{action=Index}/{id?}
// این یعنی:
// - /                    → HomeController.Index
// - /Products            → ProductsController.Index
// - /Products/Details/5  → ProductsController.Details(5)
// - /Account/Login       → AccountController.Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ----------------------------------------------------------------------------
// مرحله 4: مقداردهی اولیه دیتابیس‌ها
// ----------------------------------------------------------------------------

// پر کردن دیتابیس محصولات با داده‌های اولیه
//SeedData.EnsurePopulated(app);

// ایجاد کاربر Admin پیش‌فرض و نقش‌ها
//IdentitySeedData.EnsurePopulatedAsync(app).GetAwaiter().GetResult();

// اجرای برنامه
app.Run();
