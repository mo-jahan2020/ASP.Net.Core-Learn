// ════════════════════════════════════════════════════════════════
// فایل: Program.cs
// هدف: نقطه ورود (Entry Point) برنامه — اولین فایلی که اجرا می‌شود
//
// 📌 این فایل دو کار اصلی انجام می‌دهد:
//    ۱. ثبت سرویس‌ها (Services) — تعریف اینکه برنامه از چه ابزارهایی استفاده می‌کند
//    ۲. پیکربندی Pipeline — تعریف ترتیب پردازش درخواست‌های HTTP
//
// 📌 Middleware Pipeline چیست؟
//    هر درخواست HTTP که به برنامه می‌رسد از یک "خط لوله" رد می‌شود.
//    هر قطعه از این لوله (Middleware) کاری روی درخواست انجام می‌دهد.
//    مثال: احراز هویت → مسیریابی → اجرای Controller → ارسال پاسخ
// ════════════════════════════════════════════════════════════════

using Microsoft.EntityFrameworkCore;
using RazorSample.Data;

// ── مرحله ۱: ساختن Builder ──────────────────────────────────────
// WebApplication.CreateBuilder محیط برنامه را آماده می‌کند و
// args را از خط فرمان می‌خواند (مثلاً برای تغییر پورت)
var builder = WebApplication.CreateBuilder(args);


// ════════════════════════════════════════════════════════════════
// ── مرحله ۲: ثبت سرویس‌ها (Dependency Injection Container) ─────
// هر چیزی که اینجا ثبت کنیم، می‌توانیم در Controller ها و
// سایر کلاس‌ها با تزریق وابستگی (DI) استفاده کنیم
// ════════════════════════════════════════════════════════════════

// ثبت MVC با پشتیبانی از Views
// این خط باعث می‌شود Controller ها و Razor Views کار کنند
builder.Services.AddControllersWithViews();

// ثبت DbContext با پیکربندی SQL Server
// AddDbContext → EF Core را به DI Container اضافه می‌کند
// UseSqlServer → می‌گوید از SQL Server استفاده کن
// GetConnectionString → رشته اتصال را از appsettings.json می‌خواند
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));


// ── مرحله ۳: ساختن Application ─────────────────────────────────
// پس از ثبت همه سرویس‌ها، برنامه را می‌سازیم
var app = builder.Build();


// ════════════════════════════════════════════════════════════════
// ── مرحله ۴: ساخت خودکار بانک اطلاعاتی ────────────────────────
// این بخش مخصوص محیط آموزشی است تا نیازی به Migration دستی نباشد
// EnsureCreated() اگر دیتابیس وجود نداشته باشد آن را می‌سازد
// ════════════════════════════════════════════════════════════════

// using scope → اطمینان از آزاد شدن منابع پس از استفاده
using (var scope = app.Services.CreateScope())
{
    // گرفتن DbContext از DI Container
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // ساخت دیتابیس اگر وجود نداشته باشد (بدون نیاز به Migration)
    // ⚠️ این روش برای آموزش مناسب است؛ در پروژه‌های واقعی از Migration استفاده کنید
    db.Database.EnsureCreated();
}


// ════════════════════════════════════════════════════════════════
// ── مرحله ۵: پیکربندی Middleware Pipeline ───────────────────────
// ترتیب این‌ها مهم است! هر Middleware به ترتیب اجرا می‌شود
// ════════════════════════════════════════════════════════════════

if (!app.Environment.IsDevelopment())
{
    // در محیط Production، خطاها به صفحه Error هدایت می‌شوند
    app.UseExceptionHandler("/Home/Error");

    // HSTS: مرورگر را مجبور می‌کند همیشه از HTTPS استفاده کند
    app.UseHsts();
}

// Redirect: هر درخواست HTTP را به HTTPS تبدیل می‌کند
app.UseHttpsRedirection();

// Static Files: اجازه دسترسی به فایل‌های wwwroot (CSS, JS, Images)
app.UseStaticFiles();

// Routing: درخواست‌ها را بر اساس URL به Controller مناسب هدایت می‌کند
app.UseRouting();

// Authorization: بررسی دسترسی کاربر (اینجا استفاده نشده ولی الگوی استاندارد است)
app.UseAuthorization();


// ── مرحله ۶: تعریف الگوی مسیریابی (Route Pattern) ──────────────
// این الگو می‌گوید URL ها چطور به Controller/Action/Id تبدیل شوند
//
// مثال‌های عملی:
//   /              → Controller=User, Action=Index
//   /User          → Controller=User, Action=Index
//   /User/Create   → Controller=User, Action=Create
//   /User/Edit/5   → Controller=User, Action=Edit, Id=5
//   /User/Delete/3 → Controller=User, Action=Delete, Id=3
//
// {controller=User}  → اگر Controller در URL نبود، پیش‌فرض "User" است
// {action=Index}     → اگر Action در URL نبود، پیش‌فرض "Index" است
// {id?}              → Id اختیاری است (علامت ? = Optional)
//app.MapControllerRoute(name: "default",pattern: "{controller=User}/{action=Index}/{id?}");
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

// ── مرحله ۷: اجرای برنامه ──────────────────────────────────────
// برنامه را روشن کن و منتظر درخواست‌های HTTP بمان
app.Run();
