# راهنمای پروژه RazorSample — ASP.NET Core 8 MVC

## ساختار پروژه

```
RazorSample/
├── Controllers/
│   ├── HomeController.cs       ← ریدایرکت به User
│   └── UserController.cs       ← CRUD + Pagination
├── Data/
│   └── ApplicationDbContext.cs ← EF Core DbContext
├── Models/
│   └── UserInputModel.cs       ← مدل با Data Annotations
├── Views/
│   ├── Shared/
│   │   ├── _Layout.cshtml              ← Layout اصلی با Bootstrap RTL
│   │   └── _ValidationScriptsPartial.cshtml
│   ├── User/
│   │   ├── Index.cshtml    ← Grid + Pagination
│   │   ├── Create.cshtml   ← فرم ایجاد
│   │   ├── Edit.cshtml     ← فرم ویرایش
│   │   ├── Details.cshtml  ← نمایش جزئیات
│   │   └── Delete.cshtml   ← تأیید حذف
│   ├── _ViewImports.cshtml
│   └── _ViewStart.cshtml
├── appsettings.json        ← Connection String به SQL Server Express
├── Program.cs              ← نقطه ورود + DI + Routing
└── RazorSample.csproj
```

---

## پیش‌نیازها

| نرم‌افزار | نسخه |
|---|---|
| Visual Studio | 2022 یا 2026 |
| .NET SDK | 8.0 |
| SQL Server Express | 2019 یا بالاتر |

---

## مراحل راه‌اندازی

### ۱. باز کردن پروژه
فولدر `RazorSample` را به Visual Studio بکشید یا از منو:
```
File → Open → Project/Solution → RazorSample.csproj
```

### ۲. تنظیم Connection String
فایل `appsettings.json` را باز کنید:
```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=RazorSampleDb;Trusted_Connection=True;TrustServerCertificate=True"
```
اگر SQL Server Express شما نام instance متفاوتی دارد (مثلاً `MSSQLSERVER`)، آن را تغییر دهید:
```json
"Server=.;Database=RazorSampleDb;Trusted_Connection=True;TrustServerCertificate=True"
```

### ۳. ساخت دیتابیس (دو روش)

**روش الف — خودکار (پیشنهادی برای آموزش):**
پروژه را اجرا کنید. خط زیر در `Program.cs` دیتابیس را خودکار می‌سازد:
```csharp
db.Database.EnsureCreated();
```

**روش ب — Migration دستی:**
در Package Manager Console:
```
Add-Migration InitialCreate
Update-Database
```

### ۴. اجرا
کلید `F5` یا دکمه ▶ در Visual Studio.

---

## امکانات پروژه

- ✅ **لیست کاربران** با Grid واکنش‌گرا
- ✅ **Pagination** — هر صفحه ۵ رکورد (قابل تغییر در `UserController.cs`)
- ✅ **ایجاد کاربر** با Validation فارسی
- ✅ **ویرایش کاربر**
- ✅ **حذف کاربر** با صفحه تأیید
- ✅ **جزئیات کاربر**
- ✅ **Bootstrap 5 RTL** — پشتیبانی کامل از راست به چپ
- ✅ **پیام‌های موفقیت/خطا** با TempData

---

## تغییر تعداد رکورد در هر صفحه

در `Controllers/UserController.cs` خط زیر را ویرایش کنید:
```csharp
private const int PageSize = 5;  // ← عدد دلخواه
```

---

## تکنولوژی‌های استفاده‌شده

- **ASP.NET Core 8 MVC**
- **Entity Framework Core 8** — ORM برای SQL Server
- **Bootstrap 5.3 RTL** — ظاهر واکنش‌گرا فارسی
- **Bootstrap Icons** — آیکون‌ها
- **jQuery Validation Unobtrusive** — اعتبارسنجی سمت کلاینت
- **Vazirmatn** — فونت فارسی
