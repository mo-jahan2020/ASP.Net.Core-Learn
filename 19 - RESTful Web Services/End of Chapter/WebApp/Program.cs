// =============================================================================
// فایل Program.cs — نقطه ورود (Entry Point) برنامه ASP.NET Core
// =============================================================================
// این فایل قلب تپنده هر پروژه ASP.NET Core است. در نسخه‌های جدید (از .NET 6 به بعد)،
// دیگر نیازی به کلاس Startup جداگانه نیست و همه تنظیمات در همین فایل انجام می‌شود.
// این الگو "Top-Level Statements" نام دارد.
// =============================================================================

// فضاهای نام (Namespaces) مورد نیاز:
// - Microsoft.EntityFrameworkCore: برای کار با Entity Framework Core (ORM)
// - WebApp.Models: برای دسترسی به مدل‌های دیتابیس (DataContext و غیره)
// - Microsoft.AspNetCore.Mvc: برای استفاده از ویژگی‌های کنترلرها و API
// - System.Text.Json.Serialization: برای تنظیمات سریالایز JSON (مثلاً نادیده گرفتن مقادیر null)
using Microsoft.EntityFrameworkCore;
using WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
//using System.Security.Claims;
//using Microsoft.AspNetCore.Builder;

// -----------------------------------------------------------------------------
// مرحله ۱: ساخت Builder (WebApplicationBuilder)
// -----------------------------------------------------------------------------
// CreateBuilder یک نمونه از WebApplicationBuilder می‌سازد که شامل تنظیمات پیش‌فرض
// و پیکربندی‌های خوانده شده از فایل‌های appsettings.json است.
// args آرگومان‌های خط فرمان را دریافت می‌کند.
var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// ثبت سرویس Swagger
// -----------------------------------------------------------------------------
// Swagger ابزاری است که مستندات API شما را به صورت خودکار تولید می‌کند و
// یک رابط کاربری گرافیکی (Swagger UI) برای تست API فراهم می‌کند.
// این سرویس باید قبل از فراخوانی app.Build() ثبت شود.
 builder.Services.AddSwaggerGen(); //مهم

// -----------------------------------------------------------------------------
// مرحله ۲: ثبت سرویس‌ها در سیستم تزریق وابستگی (DI Container)
// -----------------------------------------------------------------------------
// AddDbContext: ثبت DataContext به عنوان سرویس دیتابیس
// - opts.UseSqlServer: مشخص می‌کند که از SQL Server به عنوان دیتابیس استفاده می‌کنیم
// - builder.Configuration["ConnectionStrings:ProductConnection"]: رشته اتصال (Connection String)
//   را از فایل appsettings.json می‌خواند. این روش به شما اجازه می‌دهد رشته اتصال را
//   بدون تغییر کد، از فایل پیکربندی تغییر دهید.
// - EnableSensitiveDataLogging(true): در لاگ‌ها مقادیر واقعی پارامترها نمایش داده می‌شود.
//   ⚠️ این گزینه فقط در محیط Development باید فعال باشد و در Production خاموش باشد
//   زیرا ممکن است اطلاعات حساس (مثل رمز عبور) در لاگ‌ها نمایش داده شود.
builder.Services.AddDbContext<DataContext>(opts => {
    opts.UseSqlServer(builder.Configuration[
        "ConnectionStrings:ProductConnection"]);
    opts.EnableSensitiveDataLogging(true);
});

// -----------------------------------------------------------------------------
// AddControllers: ثبت تمام کنترلرها در سیستم DI
// -----------------------------------------------------------------------------
// این متد تمام کلاس‌هایی که از ControllerBase ارث‌بری کرده‌اند و دارای ویژگی
// [ApiController] هستند را پیدا کرده و به عنوان سرویس ثبت می‌کند.
// همچنین تنظیمات پیش‌فرض برای Model Binding، Validation و فرمت‌دهی JSON را اعمال می‌کند.
builder.Services.AddControllers();

// -----------------------------------------------------------------------------
// پیکربندی گزینه‌های JSON (JsonOptions)
// -----------------------------------------------------------------------------
// این تنظیم مشخص می‌کند که خصوصیاتی که مقدار null دارند، در خروجی JSON
// نادیده گرفته شوند (_serialized نشوند). این کار حجم پاسخ JSON را کاهش می‌دهد
// و از ارسال داده‌های بیهوده جلوگیری می‌کند.
builder.Services.Configure<JsonOptions>(opts => {
    opts.JsonSerializerOptions.DefaultIgnoreCondition
        = JsonIgnoreCondition.WhenWritingNull;
});

// -----------------------------------------------------------------------------
// مرحله ۳: ساخت برنامه (Build)
// -----------------------------------------------------------------------------
// بعد از ثبت تمام سرویس‌ها، متد Build() نمونه نهایی WebApplication را می‌سازد.
// از این به بعد نمی‌توان سرویس جدیدی ثبت کرد.
var app = builder.Build();

// -----------------------------------------------------------------------------
// مرحله ۴: پیکربندی Pipeline درخواست‌ها (Request Pipeline / Middleware)
// -----------------------------------------------------------------------------
// Middleware ها به ترتیبی که نوشته می‌شوند روی هر درخواست HTTP اجرا می‌شوند.
// هر Middleware می‌تواند درخواست را پردازش کند و یا به Middleware بعدی منتقل کند.

// UseMiddleware<WebApp.TestMiddleware>: ثبت Middleware سفارشی ما
// این Middleware قبل از اجرای کنترلرها اجرا می‌شود و اگر مسیر "/test" باشد،
// تعداد محصولات، دسته‌بندی‌ها و تأمین‌کنندگان را نمایش می‌دهد.
app.UseMiddleware<WebApp.TestMiddleware>();//call middleware

// -----------------------------------------------------------------------------
// فعال‌سازی Swagger فقط در محیط Development
// -----------------------------------------------------------------------------
// IsDevelopment() بررسی می‌کند که آیا متغیر محیطی ASPNETCORE_ENVIRONMENT برابر
// "Development" است یا خیر. این رویکرد امنیتی تضمین می‌کند که Swagger
// در محیط Production در دسترس نباشد (زیرا مستندات API اطلاعات حساس را فاش می‌کند).
if (app.Environment.IsDevelopment())
{
    // UseSwagger: فعال‌سازی میان‌افزار Swagger که.Endpoint های مربوط به مستندات را فراهم می‌کند
    //مهم
    app.UseSwagger();
    // UseSwaggerUI: فعال‌سازی رابط کاربری گرافیکی Swagger برای تست API در مرورگر
    app.UseSwaggerUI();
}

// -----------------------------------------------------------------------------
// MapControllers: اتصال درخواست‌های HTTP به کنترلرها
// -----------------------------------------------------------------------------
// این متد ویژگی‌های [Route] روی کنترلرها و اکشن‌متدها را می‌خواند و
// مسیردهی (Routing) را تنظیم می‌کند. بدون این خط، هیچ درخواستی به کنترلرها
// نمی‌رسد و خطای 404 دریافت خواهید کرد.
app.MapControllers();


//app.MapGet("/hello", () => "Hello World!");

// -----------------------------------------------------------------------------
// مرحله ۵: مقداردهی اولیه دیتابیس (Seeding)
// -----------------------------------------------------------------------------
// CreateScope(): یک محدوده (Scope) جدید برای سرویس‌ها ایجاد می‌کند.
// GetRequiredService<DataContext>: نمونه DataContext را از سیستم DI دریافت می‌کند.
// SeedData.SeedDatabase(context): اگر دیتابیس خالی باشد، داده‌های اولیه را وارد می‌کند.
// نکته: این روش در برنامه‌های واقعی توصیه نمی‌شود. بهتر است از Extension Method
// مشابه app.MigrateDatabase() استفاده کنید.
var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<DataContext>();
//SeedData.SeedDatabase(context);
//Data Source=.;Initial Catalog=SportsStore;Integrated Security=True;Trust Server Certificate=True
//Data Source=.;Initial Catalog=SportsStore;Integrated Security=True;Trust Server Certificate=True

// -----------------------------------------------------------------------------
// مرحله ۶: اجرای برنامه
// -----------------------------------------------------------------------------
// app.Run(): برنامه را شروع کرده و منتظر درخواست‌های HTTP می‌ماند.
// این خط برنامه را متوقف می‌کند و سرور HTTP شروع به گوش دادن می‌کند.
app.Run();