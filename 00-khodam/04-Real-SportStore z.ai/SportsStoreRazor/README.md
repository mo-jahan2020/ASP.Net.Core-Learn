# 🏪 پروژه آموزشی فروشگاه ورزشی (SportsStore)

> یک پروژه کامل ASP.NET Core 8 با Razor Pages و Entity Framework Core
> که نمونه‌ای از یک فروشگاه اینترنتی ساده با امکانات احراز هویت، مدیریت
> محصولات، ثبت سفارش، نقش‌های کاربری و داشبورد گزارش‌گیری است.

---

## 📋 فهرست مطالب

1. [معرفی پروژه](#-معرفی-پروژه)
2. [ویژگی‌های اصلی](#-ویژگی‌های-اصلی)
3. [پیش‌نیازها](#-پیش‌نیازها)
4. [نصب و راه‌اندازی گام‌به‌گام](#-نصب-و-راه‌اندازی-گام-به-گام)
5. [اطلاعات ورود پیش‌فرض](#-اطلاعات-ورود-پیش‌فرض)
6. [نقش‌های کاربری](#-نقش‌های-کاربری)
7. [ساختار پروژه](#-ساختار-پروژه)
8. [توضیح فنی کدها](#-توضیح-فنی-کدها)
9. [رفع اشکال](#-رفع-اشکال)

---

## 🎯 معرفی پروژه

این پروژه یک فروشگاه اینترنتی ورزشی است که با **ASP.NET Core 8** و **Razor Pages** ساخته شده است. تمام کدها دارای **کامنت‌های آموزشی فارسی** هستند تا برای یادگیری ASP.NET Core مناسب باشد. برنامه از **Entity Framework Core** برای کار با دیتابیس و از **SQL Server Express** به عنوان دیتابیس استفاده می‌کند.

این پروژه بر اساس پروژه کلاسیک SportsStore از کتاب *"Pro ASP.NET Core MVC"* آقای Adam Freeman طراحی شده، اما با تفاوت‌های زیر:
- به جای MVC از **Razor Pages** استفاده شده
- تمام کامنت‌ها به فارسی هستند
- Bootstrap و Chart.js به صورت محلی (نه CDN) نصب شده‌اند
- فونت فارسی **Vazirmatn** به صورت محلی بارگذاری می‌شود
- رابط کاربری راست‌چین (RTL) و فارسی شده
- دارای نقش‌های **Admin** و **Customer**
- داشبورد گزارش‌گیری با نمودار

---

## ✨ ویژگی‌های اصلی

| ویژگی | توضیح |
|---|---|
| 🔐 احراز هویت | تمام صفحات نیاز به ورود دارند (ASP.NET Core Identity) |
| 👥 نقش‌های کاربری | دو نقش Admin و Customer با دسترسی متفاوت |
| 📦 مدیریت محصولات | افزودن، ویرایش، حذف و مشاهده جزئیات محصول (فقط Admin) |
| 🖼️ تصویر محصول | هر محصول می‌تواند تصویر داشته باشد (با پیش‌نمایش) |
| 🗂️ فیلتر دسته‌بندی | فیلتر کردن محصولات بر اساس دسته‌بندی |
| 📄 صفحه‌بندی | لیست محصولات و سفارشات دارای Pagination |
| 🛒 سبد خرید | سبد خرید مبتنی بر Session |
| 📝 ثبت سفارش | کاربر می‌تواند سفارش جدید ثبت کند |
| 📊 لیست سفارشات | نمایش تمام سفارشات به صورت Grid |
| 📈 داشبورد گزارش‌ها | نمودار میله‌ای، دایره‌ای و خطی با Chart.js (فقط Admin) |
| 🔒 سیاست رمز قوی | حداقل ۸ کاراکتر شامل حرف بزرگ، کوچک، عدد و کاراکتر ویژه |
| 🚫 محدودیت تلاش ورود | قفل اکانت پس از ۵ تلاش ناموفق |
| 📝 ثبت‌نام کاربر | کاربران جدید می‌توانند به عنوان Customer ثبت‌نام کنند |
| 🎨 Bootstrap محلی | بدون نیاز به اینترنت (Bootstrap 5.3 نصب محلی) |
| 🔤 فونت Vazirmatn | فونت فارسی زیبا، نصب محلی |
| 🌐 RTL & فارسی | رابط کاربری کاملاً فارسی و راست‌چین |

---

## 🛠️ پیش‌نیازها

برای اجرای این پروژه به موارد زیر نیاز دارید:

### 1. .NET 8 SDK
- دانلود از: https://dotnet.microsoft.com/download/dotnet/8.0
- نصب بر اساس سیستم‌عامل شما (Windows، macOS یا Linux)

برای بررسی نصب بودن:
```bash
dotnet --version
# باید 8.0.x یا بالاتر باشد
```

### 2. SQL Server Express
- دانلود از: https://www.microsoft.com/sql-server/sql-server-downloads
- نسخه **Express** رایگان است
- در حین نصب، گزینه **Default instance** یا **SQLEXPRESS instance** را انتخاب کنید
- حتماً **SQL Server Authentication** را هم فعال کنید (یا از Windows Authentication استفاده کنید)

### 3. (اختیاری) Visual Studio 2022 یا VS Code
- Visual Studio 2022 نسخه Community رایگان است
- VS Code با افزونه C# Dev Kit هم کافی است

### 4. (اختیاری) SQL Server Management Studio (SSMS)
- برای مشاهده دیتابیس و جداول
- دانلود از: https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms

---

## 🚀 نصب و راه‌اندازی گام‌به‌گام

### مرحله 1: استخراج فایل ZIP
فایل ZIP را در یک مسیر دلخواه استخراج کنید (مثلاً `C:\Projects\SportsStore`).

```bash
# در ویندوز (با PowerShell)
Expand-Archive SportsStore.zip -DestinationPath C:\Projects\SportsStore

# در لینوکس/مک
unzip SportsStore.zip -d ~/Projects/SportsStore
```

### مرحله 2: ورود به پوشه پروژه
```bash
cd C:\Projects\SportsStore\SportsStore
# یا در لینوکس/مک
cd ~/Projects/SportsStore/SportsStore
```

### مرحله 3: تنظیم ConnectionString
فایل `appsettings.json` را با یک ویرایشگر متن باز کنید و `ConnectionString` را مطابق با SQL Server خود تنظیم کنید:

```json
{
  "Data": {
    "StoreProducts": {
      "ConnectionStrings": "Server=(localdb)\\MSSQLLocalDB;Database=SportsStore;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
    },
    "Identity": {
      "ConnectionStrings": "Server=(localdb)\\MSSQLLocalDB;Database=SportsStoreIdentity;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
    }
  }
}
```

#### تنظیمات رایج ConnectionString:

| نوع نصب SQL Server | ConnectionString نمونه |
|---|---|
| SQL Server LocalDB (پیش‌فرض Visual Studio) | `Server=(localdb)\MSSQLLocalDB;Database=SportsStore;Trusted_Connection=True;TrustServerCertificate=True` |
| SQL Server Express (نام نمونه SQLEXPRESS) | `Server=.\SQLEXPRESS;Database=SportsStore;Trusted_Connection=True;TrustServerCertificate=True` |
| SQL Server کامل (نام نمونه پیش‌فرض) | `Server=.;Database=SportsStore;Trusted_Connection=True;TrustServerCertificate=True` |
| SQL Server با احراز هویت SQL | `Server=.;Database=SportsStore;User Id=sa;Password=YourPassword;TrustServerCertificate=True` |
| SQL Server روی سرور remote | `Server=192.168.1.100,1433;Database=SportsStore;User Id=sa;Password=YourPassword;TrustServerCertificate=True` |

> **نکته:** `TrustServerCertificate=True` برای جلوگیری از خطای Certificate در SQL Server جدید ضروری است.

### مرحله 4: بازیابی پکیج‌های NuGet
```bash
dotnet restore
```

### مرحله 5: ساخت دیتابیس (Migration)
برنامه به صورت خودکار Migration ها را اجرا می‌کند، اما برای اطمینان می‌توانید دستی اجرا کنید:

```bash
# اجرای Migration برای دیتابیس محصولات و سفارشات
dotnet ef database update --context StoreDbContext

# اجرای Migration برای دیتابیس Identity
dotnet ef database update --context AppIdentityDbContext
```

> **نکته:** اگر ابزار `dotnet-ef` نصب نیست، آن را نصب کنید:
> ```bash
> dotnet tool install --global dotnet-ef --version 8.0.0
> ```

### مرحله 6: اجرای برنامه
```bash
dotnet run
```

### مرحله 7: باز کردن در مرورگر
پس از اجرای موفق، برنامه در آدرس زیر در دسترس است:

```
http://localhost:5000
```

مرورگر به طور خودکار به صفحه ورود هدایت می‌شود.

---

## 🔑 اطلاعات ورود پیش‌فرض

پس از اجرای برنامه، یک کاربر Admin به صورت خودکار ساخته می‌شود:

| فیلد | مقدار |
|---|---|
| **نام کاربری** | `Admin` |
| **رمز عبور** | `Secret123$` |
| **ایمیل** | admin@example.com |
| **نقش** | Admin |

> ⚠️ **هشدار امنیتی:** این اطلاعات فقط برای محیط توسعه هستند. در محیط production حتماً رمز عبور قوی و متفاوت تنظیم کنید.

برای تغییر این مقادیر، فایل `Models/IdentitySeedData.cs` را ویرایش کنید:

```csharp
private const string adminUser = "Admin";
private const string adminPassword = "Secret123$";
```

---

## 👥 نقش‌های کاربری

این پروژه از دو نقش (Role) استفاده می‌کند:

### 1. نقش Admin (مدیر)
- دسترسی کامل به همه بخش‌ها
- می‌تواند محصول جدید بسازد، ویرایش کند، حذف کند
- می‌تواند به داشبورد گزارش‌ها دسترسی داشته باشد
- می‌تواند وضعیت سفارش را به "ارسال شده" تغییر دهد
- کاربر پیش‌فرض: `Admin`

### 2. نقش Customer (مشتری)
- می‌تواند محصولات را ببیند و به سبد خرید اضافه کند
- می‌تواند سفارش ثبت کند
- می‌تواند سفارشات خود را ببیند
- **نمی‌تواند** محصول بسازد/ویرایش/حذف کند
- **نمی‌تواند** به داشبورد گزارش‌ها دسترسی داشته باشد
- کاربران جدید از طریق صفحه `/Account/Register` به این نقش اضافه می‌شوند

### جدول دسترسی صفحات

| صفحه | Admin | Customer |
|---|---|---|
| `/Products` (لیست) | ✅ با دکمه ویرایش/حذف | ✅ بدون دکمه ویرایش/حذف |
| `/Products/Create` | ✅ | ❌ (AccessDenied) |
| `/Products/Edit` | ✅ | ❌ (AccessDenied) |
| `/Products/Delete` | ✅ | ❌ (AccessDenied) |
| `/Products/Details` | ✅ | ✅ |
| `/Cart` | ✅ | ✅ |
| `/Order` | ✅ | ✅ |
| `/Order/Create` | ✅ | ✅ |
| `/Reports` | ✅ | ❌ (AccessDenied) |
| `/Account/Register` | ✅ (می‌تواند کاربر جدید بسازد) | ✅ |

---

## 📁 ساختار پروژه

```
SportsStore/
│
├── 📄 SportsStore.csproj          # فایل پروژه و پکیج‌ها
├── 📄 Program.cs                  # نقطه شروع، DI، سیاست رمز قوی، نقش‌ها
├── 📄 appsettings.json            # ConnectionString و تنظیمات
├── 📄 global.json                 # نسخه SDK
├── 📄 README.md                   # این فایل
│
├── 📁 Models/                     # مدل‌های دامنه و Repository ها
│   ├── Product.cs                 # مدل محصول (با ImageUrl)
│   ├── Order.cs                   # مدل سفارش
│   ├── Cart.cs                    # سبد خرید پایه
│   ├── SessionCart.cs             # سبد خرید مبتنی بر Session
│   ├── StoreDbContext.cs          # DbContext اصلی
│   ├── AppIdentityDbContext.cs    # DbContext احراز هویت
│   ├── IStoreRepository.cs        # اینترفیس Repository محصولات
│   ├── EFStoreRepository.cs       # پیاده‌سازی EF Repository محصولات
│   ├── IOrderRepository.cs        # اینترفیس Repository سفارشات
│   ├── EFOrderRepository.cs       # پیاده‌سازی EF Repository سفارشات
│   ├── SeedData.cs                # داده اولیه محصولات (با تصاویر)
│   ├── IdentitySeedData.cs        # کاربر Admin و نقش‌ها
│   └── 📁 ViewModels/            # View Model ها
│       ├── PagingInfo.cs          # اطلاعات صفحه‌بندی
│       └── ProductsListViewModel.cs
│
├── 📁 Infrastructure/            # کلاس‌های کمکی
│   ├── SessionExtensions.cs       # Extension Method برای Session
│   └── PaginationTagHelper.cs     # Helper ساخت لینک‌های صفحه‌بندی
│
├── 📁 Pages/                      # Razor Pages
│   ├── _ViewImports.cshtml        # فضاهای نام مشترک
│   ├── _ViewStart.cshtml          # تنظیم Layout پیش‌فرض
│   ├── Error.cshtml + .cs         # صفحه خطا
│   ├── Cart.cshtml + .cs          # سبد خرید
│   ├── 📁 Shared/                # Layout مشترک
│   │   ├── _Layout.cshtml         # قالب اصلی (شامل Bootstrap و Vazirmatn محلی)
│   │   ├── _LoginPartial.cshtml
│   │   └── _ValidationScriptsPartial.cshtml
│   ├── 📁 Account/               # صفحات احراز هویت
│   │   ├── Login.cshtml + .cs     # ورود
│   │   ├── Logout.cshtml + .cs    # خروج
│   │   ├── Register.cshtml + .cs  # ثبت‌نام کاربر جدید (نقش Customer)
│   │   └── AccessDenied.cshtml    # دسترسی ممنوع
│   ├── 📁 Products/              # صفحات محصولات
│   │   ├── Index.cshtml + .cs     # لیست محصولات با Pagination و تصویر
│   │   ├── Details.cshtml + .cs   # جزئیات محصول
│   │   ├── Create.cshtml + .cs    # افزودن محصول [Admin]
│   │   ├── Edit.cshtml + .cs      # ویرایش محصول [Admin]
│   │   └── Delete.cshtml + .cs    # حذف محصول [Admin]
│   ├── 📁 Order/                 # صفحات سفارشات
│   │   ├── Index.cshtml + .cs     # لیست سفارشات با Pagination
│   │   ├── Details.cshtml + .cs   # جزئیات سفارش
│   │   └── Create.cshtml + .cs    # ثبت سفارش جدید
│   └── 📁 Reports/               # داشبورد گزارش‌ها
│       └── Index.cshtml + .cs     # نمودارها و آمار [Admin]
│
├── 📁 wwwroot/                    # فایل‌های استاتیک
│   ├── 📁 lib/                    # کتابخانه‌های محلی (نه CDN)
│   │   ├── 📁 bootstrap/         # Bootstrap 5.3 محلی
│   │   ├── 📁 jquery/            # jQuery 3.7 محلی
│   │   ├── 📁 jquery-validate/   # jQuery Validation محلی
│   │   ├── 📁 jquery-validation-unobtrusive/
│   │   └── 📁 chartjs/           # Chart.js 4.4 محلی برای نمودارها
│   ├── 📁 fonts/                  # فونت‌های محلی
│   │   ├── Vazirmatn-Regular.woff2  # فونت فارسی Vazirmatn
│   │   ├── Vazirmatn-Medium.woff2
│   │   └── Vazirmatn-Bold.woff2
│   ├── 📁 css/
│   │   └── site.css               # استایل‌های سفارشی
│   ├── 📁 js/
│   │   └── site.js                # اسکریپت‌های سفارشی
│   └── favicon.ico
│
├── 📁 Migrations/                 # Migration های EF Core
│   ├── 📁 Store/                  # Migration دیتابیس محصولات و سفارشات
│   │   ├── ..._InitialCreate.cs
│   │   └── ..._AddImageUrlToProduct.cs
│   └── 📁 Identity/               # Migration دیتابیس Identity
│       └── ..._InitialCreate.cs
│
└── 📁 Properties/
    └── launchSettings.json        # تنظیمات اجرا در Visual Studio
```

---

## 📚 توضیح فنی کدها

### 1. معماری کلی
برنامه از الگوی **Repository Pattern** استفاده می‌کند:
- `IStoreRepository` و `IOrderRepository` اینترفیس‌های Repository هستند
- `EFStoreRepository` و `EFOrderRepository` پیاده‌سازی واقعی با EF Core هستند
- این جداسازی امکان تست‌پذیری و تغییر دیتابیس را آسان می‌کند

### 2. نحوه کار Authorization
تمام صفحات با ویژگی `[Authorize]` مزین شده‌اند. صفحات ادمین با `[Authorize(Roles="Admin")]` محافظت می‌شوند. اگر کاربر وارد نشده باشد، به طور خودکار به صفحه `/Account/Login` هدایت می‌شود. اگر وارد شده اما نقش لازم را نداشته باشد، به `/Account/AccessDenied` هدایت می‌شود.

این کار در `Program.cs` پیکربندی شده:
```csharp
builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});
```

### 3. نقش‌ها (Roles)
دو نقش در `IdentitySeedData.cs` ساخته می‌شوند:
- **Admin**: دسترسی کامل
- **Customer**: دسترسی محدود

کاربران جدید از طریق صفحه `/Account/Register` به طور خودکار به نقش Customer اضافه می‌شوند:
```csharp
await _userManager.AddToRoleAsync(user, "Customer");
```

### 4. سیاست رمز عبور قوی
در `Program.cs` سیاست رمز قوی پیکربندی شده:
```csharp
options.Password.RequiredLength = 8;           // حداقل ۸ کاراکتر
options.Password.RequireNonAlphanumeric = true; // کاراکتر ویژه (!@#$%)
options.Password.RequireDigit = true;          // حداقل یک عدد
options.Password.RequireUppercase = true;      // حرف بزرگ
options.Password.RequireLowercase = true;      // حرف کوچک
options.Lockout.MaxFailedAccessAttempts = 5;   // قفل پس از ۵ تلاش
options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
```

### 5. سبد خرید مبتنی بر Session
کلاس `SessionCart` از `Cart` ارث‌بری می‌کند و متدها را طوری Override می‌کند که هر تغییری در سبد، در Session ذخیره شود. این کار با `JsonSerializer` انجام می‌شود.

### 6. صفحه‌بندی (Pagination)
صفحه‌بندی با ترکیب `Skip` و `Take` در LINQ انجام می‌شود که در SQL Server اجرا می‌شود:
```csharp
Products = query
    .OrderBy(p => p.ProductID)
    .Skip((currentPage - 1) * pageSize)  // رد کردن صفحات قبل
    .Take(pageSize)                       // گرفتن آیتم‌های صفحه فعلی
    .ToList();
```

### 7. Bootstrap، jQuery و Chart.js محلی
طبق درخواست شما، تمام فایل‌های JavaScript و CSS به صورت محلی در `wwwroot/lib/` قرار دارند:
- Bootstrap 5.3 (CSS + JS)
- jQuery 3.7
- jQuery Validation
- jQuery Validation Unobtrusive
- Chart.js 4.4 (برای نمودارهای داشبورد)

مزایا:
- ✅ کار بدون اینترنت
- ✅ سرعت بارگذاری بالاتر
- ✅ عدم وابستگی به در دسترس بودن CDN

### 8. فونت فارسی Vazirmatn
فونت Vazirmatn به صورت محلی در `wwwroot/fonts/` نصب شده و در `site.css` بارگذاری می‌شود:
```css
@font-face {
    font-family: 'Vazirmatn';
    src: url('/fonts/Vazirmatn-Regular.woff2') format('woff2');
    font-weight: 400;
    font-display: swap;
}
body {
    font-family: 'Vazirmatn', 'Tahoma', sans-serif;
}
```

### 9. تصاویر محصولات
مدل Product فیلد `ImageUrl` دارد که آدرس تصویر را نگه می‌دارد:
- اگر تصویر تنظیم شده باشد، نمایش داده می‌شود
- اگر نه، یک Placeholder با آیکون 📦 نمایش داده می‌شود
- در فرم Create، پیش‌نمایش زنده تصویر هنگام تایپ آدرس نشان داده می‌شود
- اگر تصویر بارگذاری نشود (URL خراب)، Placeholder جایگزین می‌شود

### 10. داشبورد گزارش‌ها
صفحه `/Reports` فقط برای Admin قابل دسترس است و شامل:
- **کارت‌های آماری**: تعداد محصولات، سفارشات، درآمد کل، سفارشات در انتظار
- **نمودار میله‌ای**: فروش بر اساس دسته‌بندی
- **نمودار دایره‌ای**: وضعیت سفارشات (ارسال شده vs در انتظار)
- **نمودار خطی**: روند سفارشات (۵ روز اخیر)
- **جدول پرفروش‌ترین محصولات**: ۱۰ محصول برتر بر اساس تعداد فروش

داده‌های نمودار به صورت JSON به View ارسال می‌شوند تا Chart.js بتواند بخواند.

### 11. Data Annotations و اعتبارسنجی
مدل‌ها از Data Annotation برای اعتبارسنجی استفاده می‌کنند. این اعتبارسنجی هم سمت کلاینت (با jQuery Validation) و هم سمت سرور انجام می‌شود.

### 12. امنیت (Anti-Forgery Token)
تمام فرم‌های POST از `@Html.AntiForgeryToken()` استفاده می‌کنند تا از حملات CSRF جلوگیری شود. Razor Pages به طور خودکار این توکن را تولید و بررسی می‌کند.

---

## 🩹 رفع اشکال

### 🚨 مشکل شماره ۱ (مهم): دیتابیس ساخته نمی‌شود!

اگر برنامه اجرا می‌شود اما دیتابیس‌ها در SQL Server ساخته نمی‌شوند، مراحل زیر را دنبال کنید:

#### مرحله ۱: بررسی Console خروجی
وقتی `dotnet run` را اجرا می‌کنید، در Console پیام‌های زیر را باید ببینید:

```
==============================================
  SportsStore - پیکربندی دیتابیس
==============================================
  Store    ConnectionString: Server=.\SQLEXPRESS;Database=SportsStore;...
  Identity ConnectionString: Server=.\SQLEXPRESS;Database=SportsStoreIdentity;...
==============================================
>> شروع مقداردهی اولیه دیتابیس Store...
info: SportsStore.Models.StoreDbContext[0]
      Store: شروع بررسی Migration های دیتابیس...
...
```

**اگر پیام "❌ خطا در ساخت دیتابیس!" دیدید:**
- پیام خطا را بخوانید (به فارسی نوشته شده)
- ConnectionString را مطابق با SQL Server خود تنظیم کنید
- SQL Server Service را بررسی کنید

#### مرحله ۲: بررسی SQL Server Service
1. در ویندوز: `Win+R` → `services.msc` → Enter
2. سرویس زیر را پیدا کنید: **SQL Server (SQLEXPRESS)**
3. مطمئن شوید **Status = Running** است
4. اگر نیست، راست‌کلیک → **Start**

همچنین سرویس **SQL Server Browser** را هم بررسی کنید (اگر از نام نمونه استفاده می‌کنید).

#### مرحله ۳: تست ConnectionString در SSMS
SQL Server Management Studio را باز کنید و سعی کنید با همان ConnectionString متصل شوید:
- Server name: `.\SQLEXPRESS`
- Authentication: `Windows Authentication`
- روی Connect کلیک کنید

اگر موفق شدید، پوشه `Databases` را باز کنید و ببینید آیا `SportsStore` و `SportsStoreIdentity` وجود دارند.

#### مرحله ۴: بررسی فایل appsettings.json
مطمئن شوید فایل `appsettings.json` درست است:

```json
{
  "Data": {
    "StoreProducts": {
      "ConnectionStrings": "Server=.\\SQLEXPRESS;Database=SportsStore;Trusted_Connection=True;TrustServerCertificate=True"
    },
    "Identity": {
      "ConnectionStrings": "Server=.\\SQLEXPRESS;Database=SportsStoreIdentity;Trusted_Connection=True;TrustServerCertificate=True"
    }
  }
}
```

> ⚠️ **توجه بسیار مهم:** در فایل JSON، کاراکتر `\` باید دو بار نوشته شود (`\\`). اگر فقط یک `\` بنویسید، JSON آن را به عنوان escape character تفسیر می‌کند.

#### مرحله ۵: استفاده از صفحه وضعیت دیتابیس
پس از ورود به عنوان Admin، در نوار ناوبری روی **"وضعیت DB"** کلیک کنید.
این صفحه به شما نشان می‌دهد:
- ✅/❌ آیا به دیتابیس Store متصل است؟
- ✅/❌ آیا به دیتابیس Identity متصل است؟
- لیست Migration های اعمال شده و معلق
- دکمه‌ای برای اعمال دستی Migration ها

#### مرحله ۶: ری‌استارت کامل برنامه
1. در Console که `dotnet run` اجرا می‌شود، `Ctrl+C` بزنید
2. صبر کنید تا برنامه کاملاً متوقف شود
3. دوباره `dotnet run` را اجرا کنید

> ⚠️ **توجه:** فقط `dotnet build` کافی نیست. باید برنامه را Stop و Start کنید.

#### مرحله ۷: اعمال دستی Migration
اگر Migration خودکار کار نکرد، دستی اعمال کنید:

```bash
# در پوشه پروژه
dotnet ef database update --context StoreDbContext
dotnet ef database update --context AppIdentityDbContext
```

اگر این دستورات خطا دادند، خطا را بخوانید - معمولاً مشکل از ConnectionString است.

#### مرحله ۸: حذف و ساخت مجدد دیتابیس
اگر هیچ راهی جواب نداد:

```bash
# حذف دیتابیس ها
dotnet ef database drop --context StoreDbContext --force
dotnet ef database drop --context AppIdentityDbContext --force

# ساخت مجدد
dotnet ef database update --context StoreDbContext
dotnet ef database update --context AppIdentityDbContext

# اجرای برنامه
dotnet run
```

---

### 📋 عیب‌یاب های دیگر

### مشکل ۲: خطای "Cannot connect to SQL Server"
```
Microsoft.Data.SqlClient.SqlException: Cannot connect to server...
```

**راه‌حل:**
1. بررسی کنید SQL Server Service در حال اجرا است
2. ConnectionString را مطابق با نصب SQL Server خود تنظیم کنید
3. اگر از Windows Authentication استفاده می‌کنید، `Trusted_Connection=True` باید تنظیم باشد
4. در صورت استفاده از SQL Authentication، `User Id` و `Password` را اضافه کنید

### مشکل ۳: خطای "Trust Server Certificate"
**راه‌حل:** `TrustServerCertificate=True` را به ConnectionString اضافه کنید.

### مشکل ۴: خطای "dotnet-ef not found"
```bash
dotnet tool install --global dotnet-ef --version 8.0.0
# یا برای به‌روزرسانی
dotnet tool update --global dotnet-ef
```

### مشکل ۵: خطای "Password too short" یا خطاهای رمز عبور
سیاست رمز قوی فعال است. رمز عبور باید:
- حداقل **۸ کاراکتر** باشد
- شامل حداقل **یک حرف بزرگ** (A-Z)
- شامل حداقل **یک حرف کوچک** (a-z)
- شامل حداقل **یک عدد** (0-9)
- شامل حداقل **یک کاراکتر ویژه** (!@#$%^&*)

مثال رمز معتبر: `Test1234$`

### مشکل ۶: قفل شدن اکانت پس از چند تلاش ناموفق
پس از ۵ تلاش ناموفق، اکانت ۱۵ دقیقه قفل می‌شود. برای باز کردن:
1. ۱۵ دقیقه صبر کنید
2. یا در SSMS این کوئری را اجرا کنید (روی دیتابیس Identity):
   ```sql
   DELETE FROM AspNetUserLockouts;
   ```

### مشکل ۷: صفحه Login در لوپ می‌افتد
اگر صفحه Login به طور مداوم ریدایرکت می‌شود:
1. Cookie های مرورگر را پاک کنید
2. برنامه را Stop و دوباره Start کنید
3. بررسی کنید `app.UseAuthentication()` قبل از `app.UseAuthorization()` در `Program.cs` باشد

### مشکل ۸: کاربر Admin ساخته نمی‌شود
اگر پس از اجرا نمی‌توانید با Admin وارد شوید:
1. بررسی کنید Migration های Identity اجرا شده‌اند
2. به صفحه **"وضعیت DB"** بروید و Migration ها را دستی اعمال کنید
3. دیتابیس `SportsStoreIdentity` را در SSMS بررسی کنید
4. برنامه را ری‌استارت کنید

### مشکل ۹: تصاویر محصولات نمایش داده نمی‌شوند
- تصاویر از URLs خارجی (مثل picsum.photos) استفاده می‌کنند
- اگر اینترنت ندارید، Placeholder نمایش داده می‌شود
- می‌توانید تصاویر محلی در `wwwroot/images/products/` قرار دهید

### مشکل ۱۰: پورت اشغال است
اگر پورت 5000 اشغال است، در `Properties/launchSettings.json` پورت را تغییر دهید:
```json
"applicationUrl": "http://localhost:5001"
```

### مشکل ۱۱: ConnectionString درست است ولی باز هم کار نمی‌کند
این مشکل معمولاً به یکی از دلایل زیر است:

1. **برنامه از فایل `bin/Debug/net8.0/appsettings.json` استفاده می‌کند**
   نه از فایل در ریشه پروژه. مطمئن شوید برنامه را Stop و دوباره Start کرده‌اید.

2. **SQL Server Browser Service در حال اجرا نیست**
   اگر از `.\SQLEXPRESS` استفاده می‌کنید، این سرویس باید در حال اجرا باشد.

3. **Firewall ویندوز جلوی اتصال را گرفته**
   پورت 1433 (یا پورت SQLEXPRESS) را در Firewall باز کنید.

4. **TCP/IP در SQL Server غیرفعال است**
   در SQL Server Configuration Manager → Protocols for SQLEXPRESS → TCP/IP را Enabled کنید.

5. **نسخه 32 بیتی vs 64 بیتی**
   مطمئن شوید SDK و SQL Server همگی 64 بیتی (یا همگی 32 بیتی) هستند.

---

## 📞 پشتیبانی و توسعه

### افزودن ویژگی جدید
برای افزودن ویژگی جدید:
1. اگر نیاز به مدل جدید است، آن را در `Models/` بسازید
2. Migration بسازید: `dotnet ef migrations add NameOfMigration --context StoreDbContext`
3. صفحه Razor در `Pages/` بسازید
4. در `Program.cs` سرویس‌های لازم را ثبت کنید
5. اگر صفحه ادمینی است، `[Authorize(Roles="Admin")]` را اضافه کنید

### تغییر دیتابیس
برای استفاده از دیتابیس دیگر (مثلاً SQLite یا PostgreSQL):
1. پکیج NuGet مربوطه را نصب کنید
2. در `Program.cs` متد `UseSqlServer` را با `UseSqlite` یا `UseNpgsql` جایگزین کنید
3. Migration های قبلی را حذف و Migration جدید بسازید

### افزودن نقش جدید
برای افزودن نقش جدید:
1. در `IdentitySeedData.cs` نقش جدید بسازید
2. در صفحات مورد نظر، `[Authorize(Roles="NewRole")]` را اضافه کنید

### افزودن گزارش جدید
برای افزودن گزارش به داشبورد:
1. در `Pages/Reports/Index.cshtml.cs` داده‌های جدید را محاسبه کنید
2. در `Pages/Reports/Index.cshtml` یک `<canvas>` اضافه کنید
3. اسکریپت Chart.js را برای آن بنویسید

---

## 📜 لایسنس

این پروژه برای اهداف آموزشی طراحی شده و آزادانه قابل استفاده، تغییر و توزیع است.

---

## 🎓 منابع یادگیری بیشتر

- [مستندات رسمی ASP.NET Core](https://learn.microsoft.com/aspnet/core/)
- [مستندات Entity Framework Core](https://learn.microsoft.com/ef/core/)
- [مستندات Razor Pages](https://learn.microsoft.com/aspnet/core/razor-pages/)
- [مستندات ASP.NET Core Identity](https://learn.microsoft.com/aspnet/core/security/authentication/identity)
- [آموزش Bootstrap 5](https://getbootstrap.com/docs/5.3/)
- [آموزش Chart.js](https://www.chartjs.org/docs/latest/)
- [فونت Vazirmatn](https://github.com/rastikerdar/vazirmatn)

---

## 🎉 خلاصه ویژگی‌های اضافه شده در نسخه 2

1. **فونت فارسی Vazirmatn** - نصب محلی، ۳ وزن (Regular، Medium، Bold)
2. **تصویر محصول** - فیلد `ImageUrl` با پیش‌نمایش زنده
3. **نقش‌های کاربری** - Admin و Customer با دسترسی متفاوت
4. **سیاست رمز قوی** - حداقل ۸ کاراکتر با ترکیب حروف، اعداد و کاراکتر ویژه
5. **محدودیت تلاش ورود** - قفل اکانت پس از ۵ تلاش ناموفق
6. **صفحه ثبت‌نام** - کاربران جدید می‌توانند به عنوان Customer ثبت‌نام کنند
7. **داشبورد گزارش‌ها** - ۴ کارت آماری + ۳ نمودار + جدول پرفروش‌ترین‌ها
8. **Chart.js محلی** - بدون نیاز به CDN
9. **منوی Dropdown کاربر** - در نوار ناوبری با نمایش نقش کاربر
10. **پیام‌های خطای فارسی** - خطاهای Identity به فارسی ترجمه شده‌اند

---

**موفق باشید! 🎉**
