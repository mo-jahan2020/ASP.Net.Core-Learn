using LearnMVC.Interfaces;
using LearnMVC.Middleware;
using LearnMVC.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// بخش ۱: ثبت سرویس‌ها در DI Container
// ============================================================
//
// سه نوع Lifetime برای سرویس‌ها وجود دارد:
//
// 🔵 Singleton  → یک بار ساخته می‌شود، تا پایان برنامه زنده می‌ماند
//                 مثال: تنظیمات کلی برنامه، Cache
//
// 🔵 Scoped     → یک بار در هر HTTP Request ساخته می‌شود
//                 مثال: DbContext، سرویس‌های معمولی
//
// 🔵 Transient  → هر بار که نیاز باشد، یک نمونه جدید ساخته می‌شود
//                 مثال: سرویس‌های سبک و stateless

builder.Services.AddControllersWithViews();

// ثبت سرویس با Lifetime مشخص
// DI: هر وقت کسی IGreetingService خواست → یک GreetingService بده (Scoped)
builder.Services.AddScoped<IGreetingService, GreetingService>();


// ============================================================
var app = builder.Build();
// ============================================================

// بخش ۲: تنظیم Middleware Pipeline
// ترتیب مهم است! هر Middleware به ترتیب اجرا می‌شود
// ============================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();       // فایل‌های CSS/JS/Image
app.UseRouting();           // تشخیص مسیر URL

// ✅ Middleware سفارشی ما - بین UseRouting و Controller‌ها
app.UseRequestLogging();
//OR
//app.UseMiddleware<RequestLoggingMiddleware>();

app.UseAuthorization();

// تعریف مسیریابی پیش‌فرض
// Pattern: /Controller/Action/Id
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
