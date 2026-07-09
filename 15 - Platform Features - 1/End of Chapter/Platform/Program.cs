using Platform;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.FileProviders;

// ساخت یک نمونه WebApplicationBuilder برای پیکربندی سرویس‌ها و تنظیمات برنامه
var builder = WebApplication.CreateBuilder(args);

// افزودن سرویس HTTP Logging برای ثبت اطلاعات درخواست‌ها و پاسخ‌ها
builder.Services.AddHttpLogging(opts => {
    // مشخص کردن اینکه کدام فیلدها لاگ شوند: متد درخواست، مسیر درخواست و کد وضعیت پاسخ
    opts.LoggingFields = HttpLoggingFields.RequestMethod
        | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode;
});

// ساخت شیء WebApplication از روی builder (پس از این نقطه دیگر نمی‌توان سرویس جدید اضافه کرد)
var app = builder.Build();

// فعال‌سازی middleware مربوط به لاگ‌گیری HTTP
app.UseHttpLogging();

// فعال‌سازی سرویس‌دهی فایل‌های استاتیک از پوشه پیش‌فرض wwwroot
app.UseStaticFiles();

// گرفتن اطلاعات محیط اجرای برنامه (Development/Production و ...)
var env = app.Environment;
// افزودن یک تنظیمات دیگر برای فایل‌های استاتیک؛ این بار از پوشه staticfiles
// و در دسترس قرار گرفتن آن‌ها از مسیر /files
app.UseStaticFiles(new StaticFileOptions {
    FileProvider = new
        PhysicalFileProvider($"{env.ContentRootPath}/staticfiles"),
    RequestPath = "/files"
});

// تعریف یک Endpoint برای درخواست‌های GET با مسیر population/{city?}
// پارامتر city اختیاری است و به متد Population.Endpoint ارسال می‌شود
app.MapGet("population/{city?}", Population.Endpoint);

// اجرای برنامه و شروع به گوش دادن به درخواست‌های ورودی
app.Run();
