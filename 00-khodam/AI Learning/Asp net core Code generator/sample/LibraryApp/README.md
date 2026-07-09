# 📚 برنامه کتابخانه - ASP.NET Core MVC

برنامه آموزشی مدیریت کتاب‌ها با ASP.NET Core 8 MVC، Entity Framework Core و SQL Server Express.

## ✨ امکانات

- ثبت‌نام و ورود کاربران (ASP.NET Core Identity)
- دسترسی به مدیریت کتاب‌ها **فقط برای کاربران واردشده**
- نمایش کتاب‌ها به‌صورت **Grid کارتی** با **Pagination**
- جستجو بر اساس عنوان یا نویسنده
- دکمه‌های **جزئیات / ویرایش / حذف** برای هر کتاب
- فرم ثبت کتاب جدید با اعتبارسنجی
- طراحی راست‌چین (RTL) با Bootstrap 5

## 🛠 پیش‌نیازها

1. **.NET 8 SDK** — دانلود: <https://dotnet.microsoft.com/download/dotnet/8.0>
2. **SQL Server Express** — دانلود: <https://www.microsoft.com/sql-server/sql-server-downloads>
3. **Visual Studio 2022** (حداقل نسخه 17.8) یا **VS Code**
4. (اختیاری) **SQL Server Management Studio (SSMS)** برای مشاهده دیتابیس

## 🚀 راهنمای نصب و اجرا

### گام ۱: استخراج پروژه
فایل ZIP را در یک پوشه (مثلاً `D:\LibraryApp`) استخراج کنید.

### گام ۲: تنظیم رشته اتصال
فایل `appsettings.json` را باز کنید و رشته اتصال را مطابق Instance خود تنظیم کنید:

```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=LibraryAppDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

- اگر نام Instance شما `SQLEXPRESS` نیست، آن را تغییر دهید.
- اگر از احراز هویت ویندوز استفاده نمی‌کنید، به‌جای `Trusted_Connection=True` از `User Id=...;Password=...` استفاده کنید.

### گام ۳: بازیابی پکیج‌ها
در ترمینال (Terminal یا CMD) داخل پوشه پروژه:

```bash
dotnet restore
```

### گام ۴: ساخت دیتابیس (Migrations)
ابزار EF Core را نصب کنید (اگر قبلاً ندارید):

```bash
dotnet tool install --global dotnet-ef
```

سپس Migration اولیه را بسازید و اعمال کنید:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

این دستور دیتابیس `LibraryAppDb` را به همراه جداول `Books` و جداول Identity می‌سازد.

### گام ۵: اجرای پروژه
```bash
dotnet run
```

یا در Visual Studio کلید `F5` را بزنید.

سپس در مرورگر باز کنید: <https://localhost:5001> یا آدرسی که در ترمینال نمایش داده می‌شود.

### گام ۶: استفاده
1. روی **ثبت‌نام (Register)** کلیک کنید و یک حساب جدید بسازید.
2. وارد شوید.
3. به منوی **لیست کتاب‌ها** بروید و کتاب اضافه کنید.

## 📂 ساختار پروژه

```
LibraryApp/
├── Controllers/
│   ├── HomeController.cs       # صفحه اصلی
│   └── BooksController.cs      # CRUD کتاب‌ها (محافظت‌شده با [Authorize])
├── Data/
│   └── ApplicationDbContext.cs # DbContext + Identity
├── Models/
│   └── Book.cs                 # مدل کتاب
├── Views/
│   ├── Shared/_Layout.cshtml   # قالب اصلی (RTL + Bootstrap)
│   ├── Home/
│   └── Books/                  # Index, Create, Edit, Details, Delete
├── appsettings.json            # تنظیمات + رشته اتصال
├── Program.cs                  # نقطه ورود برنامه
└── LibraryApp.csproj
```

## 🔐 نکات امنیتی

- اعتبارسنجی هم سمت کلاینت (jQuery Validate) و هم سمت سرور (Model Validation) انجام می‌شود.
- توکن ضد CSRF (`ValidateAntiForgeryToken`) در تمام فرم‌های POST فعال است.
- کلمات عبور توسط Identity به‌صورت Hash ذخیره می‌شوند.

## 📖 منابع آموزشی

- [مستندات ASP.NET Core MVC](https://learn.microsoft.com/aspnet/core/mvc)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)
- [ASP.NET Core Identity](https://learn.microsoft.com/aspnet/core/security/authentication/identity)

---
موفق باشید! 🎉
