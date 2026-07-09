# پروژه مدیریت کاربران با ASP.NET Core MVC

این نسخه، نسخه پیشرفته و آموزشی پروژه است. روی اکثر فایل‌ها کامنت فارسی اضافه شده تا هنگام مطالعه، نقش هر بخش را بهتر یاد بگیری.

## قابلیت‌های پیاده‌سازی‌شده

- معماری `ASP.NET Core MVC`
- استفاده از `Controller` و `Razor View`
- اتصال به `SQL Server Express`
- عملیات کامل `CRUD`
- نمایش کاربران در جدول
- جستجو و فیلتر
- مرتب‌سازی ستون‌ها
- صفحه‌بندی (`Pagination`)
- استفاده از `ViewModel`‌های مجزا
- پیاده‌سازی `Repository Pattern`
- استفاده از `Stored Procedure` برای لیست کاربران
- احراز هویت کاربران با `Cookie Authentication`
- طراحی با `Bootstrap RTL`

## پیش‌نیازها

- `NET 10 SDK`
- `SQL Server Express`

## رشته اتصال

فایل `appsettings.json` از نوع `JSON` است و JSON به‌صورت استاندارد کامنت واقعی ندارد. برای همین، توضیح‌های آموزشی داخل کلیدهایی مثل `__Comment` نوشته شده‌اند.

رشته اتصال فعلی:

```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=UserManagementMvcAdvancedDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

اگر نام اینستنس SQL Server شما متفاوت است، فقط مقدار `Server` را تغییر بده.

## اجرای پروژه

در پوشه پروژه این دستورها را اجرا کن:

```bash
dotnet restore
dotnet run
```

بعد از اجرا، برنامه باز می‌شود و اگر وارد نشده باشی به صفحه ورود هدایت می‌شوی.

## اطلاعات ورود پیش‌فرض

برای آموزش، یک کاربر پیش‌فرض در `DatabaseInitializer` ساخته می‌شود:

- نام کاربری: `admin`
- رمز عبور: `123456`

## نکته دیتابیس

در شروع برنامه، فایل `Program.cs` متد `DatabaseInitializer.InitializeAsync()` را اجرا می‌کند. این متد کارهای زیر را انجام می‌دهد:

1. اگر دیتابیس وجود نداشته باشد، آن را می‌سازد.
2. `Stored Procedure` را ایجاد یا به‌روزرسانی می‌کند.
3. کاربر پیش‌فرض ورود را ثبت می‌کند.
4. چند رکورد نمونه برای جدول کاربران می‌سازد.

## مسیر یادگیری پیشنهادی

اگر می‌خواهی پروژه را مرحله‌به‌مرحله یاد بگیری، این ترتیب مناسب است:

1. `Program.cs`
2. `Data/ApplicationDbContext.cs`
3. `Data/DatabaseInitializer.cs`
4. `Models/UserInputModel.cs`
5. `Models/AppUser.cs`
6. `ViewModels/*`
7. `Repositories/IUserRepository.cs`
8. `Repositories/UserRepository.cs`
9. `Repositories/AuthRepository.cs`
10. `Controllers/AccountController.cs`
11. `Controllers/UsersController.cs`
12. `Views/Shared/_Layout.cshtml`
13. `Views/Account/Login.cshtml`
14. `Views/Users/*.cshtml`
15. `SqlScripts/sp_GetUsersPaged.sql`

## نقش هر بخش

- `Program.cs`: ثبت سرویس‌ها، احراز هویت، Repositoryها و مسیرها
- `Data/ApplicationDbContext.cs`: تعریف جدول‌های دیتابیس
- `Data/DatabaseInitializer.cs`: ساخت اولیه بانک، داده نمونه و Stored Procedure
- `Models`: Entityهای اصلی دیتابیس
- `ViewModels`: مدل‌های مخصوص View و فرم‌ها
- `Repositories`: لایه دسترسی به داده و منطق ذخیره‌سازی
- `Controllers`: مدیریت درخواست‌ها و پاسخ به View
- `Views`: رابط کاربری
- `SqlScripts`: اسکریپت‌های SQL برای مطالعه و اجرا

## نکته آموزشی مهم

در این پروژه از دو سبک دسترسی به داده کنار هم استفاده شده است:

- `Entity Framework Core` برای عملیات معمولی مثل `Create`، `Edit` و `Delete`
- `Stored Procedure + ADO.NET` برای لیست کاربران، جستجو، فیلتر، مرتب‌سازی و صفحه‌بندی

این ترکیب برای آموزش خیلی مفید است، چون هم کار با EF Core را یاد می‌گیری و هم روش کلاسیک SQL Server را.
