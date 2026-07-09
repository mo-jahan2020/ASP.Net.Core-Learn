// ============================================================
// Program.cs - نقطه ورود برنامه (Entry Point)
// در .NET 8 از مدل Minimal Hosting استفاده می‌شود
// ============================================================

using LibraryApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- 1) ثبت سرویس‌ها (Dependency Injection) ----------

// رشته اتصال به دیتابیس از فایل appsettings.json خوانده می‌شود
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// ثبت DbContext با ارائه‌دهنده SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ثبت Identity برای مدیریت کاربران (ثبت‌نام / ورود / خروج)
// RequireConfirmedAccount=false یعنی نیاز به تایید ایمیل نیست (برای سادگی آموزش)
builder.Services
    .AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// افزودن سرویس‌های MVC (Controller + View)
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // برای صفحات پیش‌فرض Identity (Login/Register)

var app = builder.Build();

// ---------- 2) پیکربندی Middleware Pipeline ----------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();          // سرو کردن فایل‌های wwwroot
app.UseRouting();

app.UseAuthentication();       // ابتدا تشخیص هویت کاربر
app.UseAuthorization();        // سپس بررسی سطح دسترسی

// مسیر پیش‌فرض: Controller=Home, Action=Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();           // فعال‌سازی روت‌های Identity

app.Run();
