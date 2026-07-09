using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagementMvc.Data;
using UserManagementMvc.Models;
using UserManagementMvc.Repositories;

// این فایل نقطه شروع اجرای برنامه است.
// در ASP.NET Core معمولاً سرویس‌ها، دیتابیس، مسیرها و Middlewareها در همین فایل تنظیم می‌شوند.

var builder = WebApplication.CreateBuilder(args);

// این خط پشتیبانی از الگوی MVC را فعال می‌کند.
// یعنی برنامه می‌تواند Controller و View داشته باشد.
builder.Services.AddControllersWithViews();

// ثبت سرویس احراز هویت مبتنی بر Cookie
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
    });

// فعال‌سازی سیستم مجوزها
builder.Services.AddAuthorization();

// ثبت DbContext در سیستم DI:
// از اینجا به بعد هر جا ApplicationDbContext لازم باشد،
// فریم‌ورک آن را با تنظیمات اتصال SQL Server می‌سازد.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ثبت Repository Pattern
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();

// سرویس Hash کردن رمز عبور
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();

var app = builder.Build();

// اجرای کارهای اولیه دیتابیس مثل ساخت Stored Procedure و داده نمونه
await DatabaseInitializer.InitializeAsync(app.Services);

// اگر برنامه در محیط Development نباشد، صفحه خطای عمومی و HSTS فعال می‌شود.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Users/Error");
    app.UseHsts();
}

// هدایت درخواست‌ها از HTTP به HTTPS
app.UseHttpsRedirection();

// فعال‌سازی فایل‌های استاتیک مثل CSS و JavaScript
app.UseStaticFiles();

// فعال‌سازی سیستم Routing
app.UseRouting();

// این بخش باید قبل از Authorization بیاید.
app.UseAuthentication();

// اگر بعداً احراز هویت اضافه شود، این بخش برای کنترل مجوزها استفاده می‌شود.
app.UseAuthorization();

// مسیر پیش‌فرض برنامه:
// وقتی آدرس خاصی وارد نشود، کنترلر Users و اکشن Index اجرا می‌شود.
//app.MapControllerRoute(name: "default",pattern: "{controller=Users}/{action=Index}/{id?}");
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

// اجرای نهایی برنامه
app.Run();
