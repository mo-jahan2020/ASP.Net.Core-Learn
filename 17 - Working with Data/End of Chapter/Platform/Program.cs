using Platform.Services;
using Platform.Models;
using Microsoft.EntityFrameworkCore;

// ساخت یک نمونه WebApplicationBuilder برای پیکربندی سرویس‌ها و تنظیمات برنامه
var builder = WebApplication.CreateBuilder(args);

// افزودن سرویس کش توزیع‌شده (Distributed Cache) مبتنی بر SQL Server
builder.Services.AddDistributedSqlServerCache(opts => {
    // خواندن رشته اتصال به دیتابیس کش از تنظیمات برنامه
    opts.ConnectionString
        = builder.Configuration["ConnectionStrings:CacheConnection"];
    // نام Schema جدول کش در دیتابیس
    opts.SchemaName = "dbo";
    // نام جدولی که داده‌های کش در آن نگهداری می‌شود
    opts.TableName = "DataCache";
});

// فعال‌سازی قابلیت Response Caching (کش کردن پاسخ‌های HTTP)
builder.Services.AddResponseCaching();
// ثبت HtmlResponseFormatter به عنوان پیاده‌سازی IResponseFormatter با طول عمر Singleton
builder.Services.AddSingleton<IResponseFormatter, HtmlResponseFormatter>();

// ثبت CalculationContext (DbContext) برای اتصال به دیتابیس محاسبات با استفاده از SQL Server
builder.Services.AddDbContext<CalculationContext>(opts => {
    opts.UseSqlServer(builder.Configuration["ConnectionStrings:CalcConnection"]);
    // فعال‌سازی لاگ‌گیری اطلاعات حساس (فقط مناسب محیط توسعه)
    opts.EnableSensitiveDataLogging(true);
});

// ثبت سرویس SeedData با طول عمر Transient برای مقداردهی اولیه دیتابیس
builder.Services.AddTransient<SeedData>();

// ساخت شیء WebApplication از روی builder (پس از این نقطه دیگر نمی‌توان سرویس جدید اضافه کرد)
var app = builder.Build();

// فعال‌سازی middleware مربوط به کش کردن پاسخ‌ها
app.UseResponseCaching();

// نگاشت کلاس SumEndpoint به مسیر /sum/{count}
// مقدار پیش‌فرض پارامتر count برابر 1000000000 است
app.MapEndpoint<Platform.SumEndpoint>("/sum/{count:int=1000000000}");

// تعریف یک Endpoint ساده برای مسیر ریشه "/" که پیام Hello World برمی‌گرداند
app.MapGet("/", async context => {
    await context.Response.WriteAsync("Hello World!");
});

// بررسی اینکه آیا از طریق آرگومان خط فرمان INITDB درخواست مقداردهی اولیه دیتابیس شده است
bool cmdLineInit = (app.Configuration["INITDB"] ?? "false") == "true";
// اگر محیط اجرا Development باشد یا مقداردهی اولیه از خط فرمان درخواست شده باشد
if (app.Environment.IsDevelopment() || cmdLineInit) {
    // دریافت سرویس SeedData از کانتینر تزریق وابستگی
    var seedData = app.Services.GetRequiredService<SeedData>();
    // اجرای عملیات مقداردهی اولیه (Seed) دیتابیس
    seedData.SeedDatabase();
}
// اگر برنامه فقط برای مقداردهی اولیه دیتابیس اجرا نشده باشد، سرور را اجرا کن
if (!cmdLineInit) {
    app.Run();
}
