// =============================================================================
// Program.cs - نقطه ورود اصلی برنامه ASP.NET Core
// این فایل وظیفه پیکربندی و اجرای اپلیکیشن وب را بر عهده دارد
// در اینجا Host Filtering، مدیریت خطا و صفحات وضعیت تنظیم می‌شوند
// =============================================================================

using Microsoft.AspNetCore.HostFiltering; // وارد کردن سرویس فیلتر کردن هاست برای محدود کردن دامنه‌های مجاز

// ایجاد Builder اصلی برنامه با پارامترهای خط فرمان
var builder = WebApplication.CreateBuilder(args);

// پیکربندی گزینه‌های فیلتر کردن هاست
// فقط درخواست‌هایی که دامنه آن‌ها با الگو مطابقت داشته باشد پردازش می‌شوند
builder.Services.Configure<HostFilteringOptions>(opts => {
    opts.AllowedHosts.Clear(); // پاک کردن لیست هاست‌های مجاز پیش‌فرض
    opts.AllowedHosts.Add("*.example.com"); // افزودن الگوی دامنه مجاز (زیردامنه‌های example.com)
});

// ساخت نمونه نهایی اپلیکیشن از Builder
var app = builder.Build();

// تنظیمات مخصوص محیط تولید (غیر Development)
if (!app.Environment.IsDevelopment()) {
    // در محیط تولید، خطاها به صفحه error.html هدایت می‌شوند
    app.UseExceptionHandler("/error.html");
    // فعال‌سازی سرویس فایل‌های استاتیک (مانند CSS, JS, تصاویر)
    app.UseStaticFiles();
}

// تنظیم صفحات کد وضعیت HTTP
// برای هر کد وضعیت غیرموفق (مثل 404, 500) پاسخ HTML پیش‌فرض نمایش داده می‌شود
app.UseStatusCodePages("text/html", Platform.Responses.DefaultResponse);

// تعریف میان‌افزار (Middleware) سفارشی درون‌خطی
// اگر مسیر درخواست "/error" باشد، کد 404 برمی‌گرداند
app.Use(async (context, next) => {
    if (context.Request.Path == "/error") {
        // تنظیم کد وضعیت 404 (یافت نشد) برای مسیر error
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await Task.CompletedTask; // بدون انجام کار اضافی، پاسخ را کامل می‌کند
    } else {
        // برای سایر مسیرها، ادامه زنجیره میان‌افزارها
        await next();
    }
});

// تعریف یک اندپوینت (Endpoint) پیش‌فرض که همیشه خطا پرتاب می‌کند
// این اندپوینت برای هر درخواستی که با مسیرهای قبلی مطابقت نداشته باشد اجرا می‌شود
app.Run(context => {
    throw new Exception("Something has gone wrong"); // پرتاب استثنا برای تست مدیریت خطا
});

// شروع شنود درخواست‌های HTTP و اجرای اپلیکیشن
app.Run();
