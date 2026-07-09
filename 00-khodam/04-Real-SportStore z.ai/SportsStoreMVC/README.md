# 🏪 پروژه آموزشی فروشگاه ورزشی (SportsStore) - نسخه MVC

> یک پروژه کامل ASP.NET Core 8 با **MVC (Model-View-Controller)** و Entity Framework Core
> که نمونه‌ای از یک فروشگاه اینترنتی ساده با امکانات احراز هویت، مدیریت
> محصولات، ثبت سفارش، نقش‌های کاربری و داشبورد گزارش‌گیری است.

---

## 📋 فهرست مطالب

1. [معرفی پروژه](#-معرفی-پروژه)
2. [تفاوت با نسخه Razor Pages](#-تفاوت-با-نسخه-razor-pages)
3. [ویژگی‌های اصلی](#-ویژگی‌های-اصلی)
4. [پیش‌نیازها](#-پیش‌نیازها)
5. [نصب و راه‌اندازی گام‌به‌گام](#-نصب-و-راه‌اندازی-گام-به-گام)
6. [اطلاعات ورود پیش‌فرض](#-اطلاعات-ورود-پیش‌فرض)
7. [نقش‌های کاربری](#-نقش‌های-کاربری)
8. [ساختار پروژه](#-ساختار-پروژه)
9. [توضیح فنی معماری MVC](#-توضیح-فنی-معماری-mvc)
10. [رفع اشکال](#-رفع-اشکال)

---

## 🎯 معرفی پروژه

این پروژه یک فروشگاه اینترنتی ورزشی است که با **ASP.NET Core 8** و **MVC** ساخته شده است. تمام کدها دارای **کامنت‌های آموزشی فارسی** هستند تا برای یادگیری الگوی MVC در ASP.NET Core مناسب باشد. برنامه از **Entity Framework Core** برای کار با دیتابیس و از **SQL Server Express** به عنوان دیتابیس استفاده می‌کند.

این نسخه معادل نسخه Razor Pages است، اما از الگوی **MVC کلاسیک** با Controller و Viewها استفاده می‌کند.

---

## 🔄 تفاوت با نسخه Razor Pages

| جنبه | نسخه Razor Pages | نسخه MVC (این پروژه) |
|---|---|---|
| **ساختار** | `Pages/` با فایل‌های `.cshtml` + `.cshtml.cs` | `Controllers/` + `Views/` جداگانه |
| **مسیریابی** | بر اساس ساختار پوشه‌ها (Convention) | `{controller=Home}/{action=Index}/{id?}` |
| **مدیریت منطق** | PageModel (هر صفحه یک PageModel) | Controller (هر Controller چندین Action) |
| **لینک‌ها در View** | `asp-page="/Products"` | `asp-controller="Products" asp-action="Index"` |
| **Authorization روی اکشن** | فقط روی PageModel (نه روی Handler) | روی هر Action جداگانه ✅ |
| **مناسب برای** | برنامه‌های مبتنی بر صفحه | برنامه‌های بزرگتر با منطق مشترک |

### مقایسه یک عملیات: افزودن محصول

**در Razor Pages:**
```
Pages/Products/Create.cshtml       ← View
Pages/Products/Create.cshtml.cs    ← PageModel (هر دو در یک پوشه)
```

**در MVC:**
```
Controllers/ProductsController.cs  ← Controller (شامل Create، Edit، Delete و...)
Views/Products/Create.cshtml       ← View جداگانه
```

---

## ✨ ویژگی‌های اصلی

| ویژگی | توضیح |
|---|---|
| 🏗️ **الگوی MVC** | Controller + View + Model (کلاسیک) |
| 🔐 احراز هویت | تمام صفحات نیاز به ورود دارند (ASP.NET Identity) |
| 👥 نقش‌های کاربری | Admin و Customer با دسترسی متفاوت |
| 📦 مدیریت محصولات | افزودن، ویرایش، حذف و مشاهده جزئیات (فقط Admin) |
| 🖼️ تصویر محصول | هر محصول می‌تواند تصویر داشته باشد |
| 🗂️ فیلتر دسته‌بندی | فیلتر محصولات بر اساس دسته‌بندی |
| 📄 صفحه‌بندی | لیست محصولات و سفارشات دارای Pagination |
| 🛒 سبد خرید | سبد خرید مبتنی بر Session |
| 📝 ثبت سفارش | با پیش‌پر کردن خودکار آدرس از سفارش قبلی |
| 📊 داشبورد گزارش‌ها | نمودارها با Chart.js (فقط Admin) |
| 🔒 سیاست رمز قوی | حداقل ۸ کاراکتر با ترکیب کامل |
| 🚫 محدودیت تلاش ورود | قفل اکانت پس از ۵ تلاش ناموفق |
| 📝 ثبت‌نام کاربر | کاربران جدید به نقش Customer اضافه می‌شوند |
| 🎨 Bootstrap محلی | بدون نیاز به اینترنت (Bootstrap 5.3) |
| 🔤 فونت Vazirmatn | فونت فارسی زیبا، نصب محلی |
| 🌐 RTL & فارسی | رابط کاربری کاملاً فارسی و راست‌چین |

---

## 🛠️ پیش‌نیازها

### 1. .NET 8 SDK
دانلود از: https://dotnet.microsoft.com/download/dotnet/8.0

```bash
dotnet --version
# باید 8.0.x یا بالاتر باشد
```

### 2. SQL Server Express
دانلود از: https://www.microsoft.com/sql-server/sql-server-downloads (نسخه Express رایگان)

### 3. (اختیاری) Visual Studio 2022 یا VS Code

### 4. (اختیاری) SQL Server Management Studio (SSMS)

---

## 🚀 نصب و راه‌اندازی گام‌به‌گام

### مرحله ۱: استخراج فایل ZIP
```bash
# ویندوز (PowerShell)
Expand-Archive SportsStoreMVC.zip -DestinationPath C:\Projects\SportsStoreMVC

# لینوکس/مک
unzip SportsStoreMVC.zip -d ~/Projects/SportsStoreMVC
```

### مرحله ۲: ورود به پوشه پروژه
```bash
cd C:\Projects\SportsStoreMVC\SportsStore
# یا در لینوکس/مک
cd ~/Projects/SportsStoreMVC/SportsStore
```

### مرحله ۳: تنظیم ConnectionString
فایل `appsettings.json` را باز کنید و `ConnectionString` را مطابق با SQL Server خود تنظیم کنید:

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

> ⚠️ **توجه مهم:** اگر در محیط Development کار می‌کنید، فایل `appsettings.Development.json` را هم به‌روزرسانی کنید. این فایل روی `appsettings.json` اولویت دارد!

#### تنظیمات رایج ConnectionString:

| نوع نصب SQL Server | ConnectionString نمونه |
|---|---|
| LocalDB | `Server=(localdb)\MSSQLLocalDB;Database=SportsStore;Trusted_Connection=True;TrustServerCertificate=True` |
| SQLEXPRESS | `Server=.\SQLEXPRESS;Database=SportsStore;Trusted_Connection=True;TrustServerCertificate=True` |
| SQL Server کامل | `Server=.;Database=SportsStore;Trusted_Connection=True;TrustServerCertificate=True` |
| احراز هویت SQL | `Server=.;Database=SportsStore;User Id=sa;Password=YourPassword;TrustServerCertificate=True` |

### مرحله ۴: بازیابی پکیج‌های NuGet
```bash
dotnet restore
```

### مرحله ۵: ساخت دیتابیس (Migration)
```bash
dotnet ef database update --context StoreDbContext
dotnet ef database update --context AppIdentityDbContext
```

اگر `dotnet-ef` نصب نیست:
```bash
dotnet tool install --global dotnet-ef --version 8.0.0
```

### مرحله ۶: اجرای برنامه
```bash
dotnet run
```

### مرحله ۷: باز کردن در مرورگر
برنامه در آدرس زیر در دسترس است:
```
http://localhost:5000
```

مرورگر به‌طور خودکار به صفحه ورود هدایت می‌شود.

---

## 🔑 اطلاعات ورود پیش‌فرض

| فیلد | مقدار |
|---|---|
| **نام کاربری** | `Admin` |
| **رمز عبور** | `Secret123$` |
| **ایمیل** | admin@example.com |
| **نقش** | Admin |

> ⚠️ **هشدار امنیتی:** این اطلاعات فقط برای محیط توسعه هستند.

---

## 👥 نقش‌های کاربری

### ۱. نقش Admin (مدیر)
- دسترسی کامل به همه بخش‌ها
- افزودن/ویرایش/حذف محصولات
- دسترسی به داشبورد گزارش‌ها
- تغییر وضعیت سفارش به "ارسال شده"

### ۲. نقش Customer (مشتری)
- مشاهده محصولات و افزودن به سبد
- ثبت سفارش
- مشاهده سفارشات خود
- **نمی‌تواند** محصول بسازد/ویرایش/حذف کند
- **نمی‌تواند** به داشبورد گزارش‌ها دسترسی داشته باشد

### جدول دسترسی صفحات (URL)

| URL | Admin | Customer |
|---|---|---|
| `/Products` | ✅ با دکمه ویرایش/حذف | ✅ بدون دکمه ویرایش/حذف |
| `/Products/Create` | ✅ | ❌ (AccessDenied) |
| `/Products/Edit/5` | ✅ | ❌ (AccessDenied) |
| `/Products/Delete/5` | ✅ | ❌ (AccessDenied) |
| `/Products/Details/5` | ✅ | ✅ |
| `/Cart` | ✅ | ✅ |
| `/Order` | ✅ | ✅ |
| `/Order/Create` | ✅ | ✅ |
| `/Order/MarkShipped/5` | ✅ | ❌ (Authorize روی Action) |
| `/Reports` | ✅ | ❌ (AccessDenied) |
| `/Account/Register` | ✅ | ✅ |

---

## 📁 ساختار پروژه

```
SportsStore/
│
├── 📄 SportsStore.csproj          # فایل پروژه و پکیج‌ها
├── 📄 Program.cs                  # نقطه شروع، AddControllersWithViews، مسیریابی
├── 📄 appsettings.json            # ConnectionString (SQL Server)
├── 📄 appsettings.Development.json # ConnectionString برای محیط Development
├── 📄 global.json                 # نسخه SDK
├── 📄 README.md                   # این فایل
│
├── 📁 Controllers/                # ★ Controller ها (تفاوت اصلی با Razor Pages)
│   ├── HomeController.cs          # هدایت از / به /Products
│   ├── AccountController.cs       # Login, Logout, Register, AccessDenied
│   ├── ProductsController.cs      # Index, Details, Create, Edit, Delete
│   ├── CartController.cs          # Index, Add, Remove, Clear
│   ├── OrderController.cs         # Index, Create, Details, MarkShipped
│   └── ReportsController.cs       # Index (داشبورد)
│
├── 📁 Models/                     # مدل‌های دامنه و Repository ها
│   ├── Product.cs                 # مدل محصول (با ImageUrl)
│   ├── Order.cs                   # مدل سفارش (با UserId)
│   ├── Cart.cs                    # سبد خرید پایه
│   ├── SessionCart.cs             # سبد خرید مبتنی بر Session
│   ├── StoreDbContext.cs          # DbContext اصلی
│   ├── AppIdentityDbContext.cs    # DbContext احراز هویت
│   ├── IStoreRepository.cs        # اینترفیس Repository محصولات
│   ├── EFStoreRepository.cs       # پیاده‌سازی EF Repository محصولات
│   ├── IOrderRepository.cs        # اینترفیس Repository سفارشات
│   ├── EFOrderRepository.cs       # پیاده‌سازی EF Repository سفارشات
│   ├── SeedData.cs                # داده اولیه محصولات
│   ├── IdentitySeedData.cs        # کاربر Admin و نقش‌ها
│   └── 📁 ViewModels/            # View Model ها
│       ├── LoginModel.cs
│       ├── RegisterViewModel.cs
│       ├── PagingInfo.cs
│       ├── ProductsListViewModel.cs
│       ├── OrdersListViewModel.cs
│       └── ReportsViewModel.cs
│
├── 📁 Infrastructure/            # کلاس‌های کمکی
│   ├── SessionExtensions.cs       # Extension Method برای Session
│   └── PaginationTagHelper.cs     # Helper ساخت لینک‌های صفحه‌بندی
│
├── 📁 Views/                      # ★ View ها (تفاوت اصلی با Razor Pages)
│   ├── _ViewImports.cshtml        # فضاهای نام مشترک
│   ├── _ViewStart.cshtml          # تنظیم Layout پیش‌فرض
│   ├── 📁 Shared/                # Layout مشترک
│   │   ├── _Layout.cshtml         # قالب اصلی (Bootstrap و Vazirmatn محلی)
│   │   ├── _ValidationScriptsPartial.cshtml
│   │   └── Error.cshtml
│   ├── 📁 Account/               # View های احراز هویت
│   │   ├── Login.cshtml
│   │   ├── Register.cshtml
│   │   └── AccessDenied.cshtml
│   ├── 📁 Products/              # View های محصولات
│   │   ├── Index.cshtml           # لیست با Pagination
│   │   ├── Details.cshtml
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   └── Delete.cshtml
│   ├── 📁 Cart/
│   │   └── Index.cshtml           # سبد خرید
│   ├── 📁 Order/
│   │   ├── Index.cshtml           # لیست سفارشات با Pagination
│   │   ├── Create.cshtml          # ثبت سفارش (با پیش‌پر آدرس)
│   │   └── Details.cshtml
│   └── 📁 Reports/
│       └── Index.cshtml           # داشبورد
│
├── 📁 wwwroot/                    # فایل‌های استاتیک
│   ├── 📁 lib/                    # کتابخانه‌های محلی (نه CDN)
│   │   ├── 📁 bootstrap/         # Bootstrap 5.3 محلی
│   │   ├── 📁 jquery/            # jQuery 3.7 محلی
│   │   ├── 📁 jquery-validate/
│   │   ├── 📁 jquery-validation-unobtrusive/
│   │   └── 📁 chartjs/           # Chart.js 4.4 محلی
│   ├── 📁 fonts/                  # فونت Vazirmatn محلی
│   ├── 📁 css/                    # site.css
│   ├── 📁 js/                     # site.js
│   └── favicon.ico
│
├── 📁 Migrations/                 # Migration های EF Core
│   ├── 📁 Store/                  # Migration دیتابیس محصولات
│   └── 📁 Identity/               # Migration دیتابیس Identity
│
└── 📁 Properties/
    └── launchSettings.json
```

---

## 📚 توضیح فنی معماری MVC

### ۱) الگوی MVC (Model-View-Controller)

```
کاربر (Browser)
    │
    │  HTTP Request: GET /Products/Details/5
    ▼
┌──────────────────────────────────────────┐
│  Routing (در Program.cs)                 │
│  {controller=Home}/{action=Index}/{id?}  │
│  → ProductsController.Details(5)         │
└──────────────────────────────────────────┘
    │
    ▼
┌──────────────────────────────────────────┐
│  Controller (ProductsController)         │
│  - دریافت داده از Repository (Model)     │
│  - پردازش منطق Business                  │
│  - return View(product)                  │
└──────────────────────────────────────────┘
    │
    ▼
┌──────────────────────────────────────────┐
│  View (Views/Products/Details.cshtml)    │
│  - نمایش داده‌ها با HTML/Razor           │
│  - ارسال به مرورگر                       │
└──────────────────────────────────────────┘
    │
    ▼
کاربر (Browser) HTML را می‌بیند
```

### ۲) Controller

Controller کلاسی است که از `Controller` ارث می‌برد. هر متد public یک **Action** است:

```csharp
public class ProductsController : Controller {
    [Authorize]  // این Controller نیاز به ورود دارد
    public class ProductsController : Controller {

        private readonly IStoreRepository _repository;

        // Constructor Injection
        public ProductsController(IStoreRepository repo) {
            _repository = repo;
        }

        // GET /Products
        public IActionResult Index(int? productPage, string? category) {
            var products = _repository.Products.ToList();
            return View(products);  // View را با Model برمی‌گرداند
        }

        // GET /Products/Details/5
        public IActionResult Details(long? id) {
            var product = _repository.Products.FirstOrDefault(p => p.ProductID == id);
            return View(product);
        }

        // GET /Products/Create (فقط Admin)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create() {
            return View();  // View خالی
        }

        // POST /Products/Create (فقط Admin)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]  // جلوگیری از CSRF
        public IActionResult Create(Product product) {
            if (!ModelState.IsValid) return View(product);
            _repository.CreateProduct(product);
            return RedirectToAction(nameof(Index));
        }
    }
}
```

### ۳) View

View در مسیر `Views/{ControllerName}/{ActionName}.cshtml` قرار می‌گیرد:

```razor
@* Views/Products/Details.cshtml *@
@model Product  @* نوع مدل *@@

@{
    ViewData["Title"] = "جزئیات محصول";
}

<h1>@Model.Name</h1>
<p>قیمت: @Model.Price.ToString("C")</p>

<a asp-controller="Products" asp-action="Index">بازگشت</a>
```

### ۴) مسیریابی (Routing)

در `Program.cs`:

```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

این یعنی:
- `/` → `HomeController.Index()`
- `/Products` → `ProductsController.Index()`
- `/Products/Details/5` → `ProductsController.Details(5)`
- `/Account/Login` → `AccountController.Login()`

### ۵) ویژگی‌های MVC (Attributes)

| Attribute | کاربرد |
|---|---|
| `[HttpGet]` | اکشن فقط به GET پاسخ می‌دهد |
| `[HttpPost]` | اکشن فقط به POST پاسخ می‌دهد |
| `[Authorize]` | فقط کاربران وارد شده |
| `[Authorize(Roles = "Admin")]` | فقط کاربران با نقش Admin |
| `[ValidateAntiForgeryToken]` | جلوگیری از CSRF در فرم‌های POST |

### ۶) مزیت MVC نسبت به Razor Pages

| مزیت | توضیح |
|---|---|
| ✅ **Authorization روی Action** | می‌توان `[Authorize]` را روی یک Action خاص گذاشت (در Razor Pages فقط روی کل PageModel) |
| ✅ **منطق مشترک** | چندین Action می‌توانند از Constructor و فیلدهای مشترک استفاده کنند |
| ✅ **استاندارد آشنا** | برای کسانی که از ASP.NET MVC کلاسیک یا Spring MVC می‌آیند، آشناست |
| ✅ **مناسب API** | می‌توان Controller را به API تبدیل کرد (`[ApiController]`) |

### ۷) پیش‌پر کردن فرم سفارش

وقتی کاربر قبلاً سفارشی ثبت کرده، در خرید بعدی فرم به‌طور خودکار با اطلاعات آدرس قبلی پر می‌شود. این کار با جستجوی آخرین سفارش کاربر (بر اساس `UserId`) انجام می‌شود:

```csharp
public async Task<IActionResult> Create() {
    IdentityUser? user = await _userManager.GetUserAsync(User);

    if (user != null) {
        Order? lastOrder = _repository.Orders
            .Where(o => o.UserId == user.Id)  // تطابق با UserId
            .OrderByDescending(o => o.OrderID)
            .FirstOrDefault();

        if (lastOrder != null) {
            // کپی اطلاعات آدرس از سفارش قبلی
            return View(new Order {
                Name = lastOrder.Name,
                Line1 = lastOrder.Line1,
                // ...
            });
        }
    }

    return View(new Order());
}
```

---

## 🩹 رفع اشکال

### مشکل ۱: دیتابیس ساخته نمی‌شود

1. در فایل `appsettings.json` ConnectionString را بررسی کنید
2. فایل `appsettings.Development.json` را هم بررسی کنید (در محیط Development)
3. مطمئن شوید SQL Server Service در حال اجرا است (`services.msc`)
4. در SSMS به `.\SQLEXPRESS` (یا همان سرور ConnectionString) وصل شوید

### مشکل ۲: خطای Migration
```bash
dotnet ef database drop --context StoreDbContext --force
dotnet ef database drop --context AppIdentityDbContext --force
dotnet ef database update --context StoreDbContext
dotnet ef database update --context AppIdentityDbContext
dotnet run
```

### مشکل ۳: خطای "dotnet-ef not found"
```bash
dotnet tool install --global dotnet-ef --version 8.0.0
```

### مشکل ۴: سیاست رمز عبور
رمز باید:
- حداقل **۸ کاراکتر**
- شامل **حرف بزرگ** (A-Z)
- شامل **حرف کوچک** (a-z)
- شامل **عدد** (0-9)
- شامل **کاراکتر ویژه** (!@#$%^&*)

مثال: `Test1234$`

### مشکل ۵: قفل شدن اکانت
پس از ۵ تلاش ناموفق، اکانت ۱۵ دقیقه قفل می‌شود. برای باز کردن:
```sql
USE SportsStoreIdentity;
DELETE FROM AspNetUserLockouts;
```

### مشکل ۶: کاربر Admin ساخته نمی‌شود
1. Migration های Identity را بررسی کنید
2. دیتابیس `SportsStoreIdentity` را در SSMS بررسی کنید
3. برنامه را ری‌استارت کنید

---

## 📜 لایسنس

این پروژه برای اهداف آموزشی طراحی شده و آزادانه قابل استفاده، تغییر و توزیع است.

---

## 🎓 منابع یادگیری بیشتر

- [مستندات ASP.NET Core MVC](https://learn.microsoft.com/aspnet/core/mvc/)
- [مستندات Entity Framework Core](https://learn.microsoft.com/ef/core/)
- [مستندات ASP.NET Core Identity](https://learn.microsoft.com/aspnet/core/security/authentication/identity)
- [آموزش Bootstrap 5](https://getbootstrap.com/docs/5.3/)
- [آموزش Chart.js](https://www.chartjs.org/docs/latest/)

---

**موفق باشید! 🎉**
