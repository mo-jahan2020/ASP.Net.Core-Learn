// ════════════════════════════════════════════════════════════════
// فایل: Models/UserInputModel.cs
// هدف: تعریف مدل داده‌ای کاربر به همراه قوانین اعتبارسنجی
//
// 📌 مفهوم Model در MVC:
//    Model نماینده داده‌های برنامه است. هر Property (خاصیت) در این
//    کلاس معادل یک ستون در جدول بانک اطلاعاتی خواهد بود.
//
// 📌 Data Annotations چیست؟
//    ویژگی‌هایی (Attributes) هستند که بالای هر Property نوشته می‌شوند
//    و قوانین اعتبارسنجی را تعریف می‌کنند. این قوانین هم در سمت
//    سرور (C#) و هم در سمت مرورگر (JavaScript) بررسی می‌شوند.
// ════════════════════════════════════════════════════════════════

// وارد کردن فضای نام مربوط به Data Annotations
using System.ComponentModel.DataAnnotations;

// تعریف Namespace — نامه‌ای منحصربه‌فرد برای گروه‌بندی کلاس‌ها
namespace RazorSample.Models
{
    // تعریف کلاس مدل کاربر
    // این کلاس توسط Entity Framework Core به یک جدول در SQL Server تبدیل می‌شود
    public class UserInputModel
    {
        // ── کلید اصلی (Primary Key) ─────────────────────────────────
        // EF Core به‌طور خودکار فیلدی با نام Id یا {ClassName}Id را
        // به عنوان کلید اصلی جدول می‌شناسد و مقدار آن را auto-increment می‌کند
        public int Id { get; set; }

        // ── نام ─────────────────────────────────────────────────────
        // [Required] → این فیلد اجباری است؛ خالی نمی‌تواند باشد
        [Required(ErrorMessage = "نام الزامی است")]

        // [StringLength] → حداکثر و حداقل تعداد کاراکتر مجاز
        [StringLength(50, MinimumLength = 2, ErrorMessage = "نام باید بین ۲ تا ۵۰ کاراکتر باشد")]

        // [Display] → نام نمایشی که در Label های فرم نشان داده می‌شود
        [Display(Name = "نام")]
        public string Name { get; set; } = ""; // مقدار پیش‌فرض: رشته خالی (برای جلوگیری از null)

        // ── نام خانوادگی ────────────────────────────────────────────
        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "نام خانوادگی باید بین ۲ تا ۵۰ کاراکتر باشد")]
        [Display(Name = "نام خانوادگی")]
        public string Family { get; set; } = "";

        // ── تاریخ تولد ──────────────────────────────────────────────
        [Required(ErrorMessage = "تاریخ تولد الزامی است")]

        // [DataType] → نوع منطقی داده را مشخص می‌کند (برای نمایش صحیح در فرم)
        // DataType.Date باعث می‌شود مرورگر یک date-picker نشان دهد
        [DataType(DataType.Date)]
        [Display(Name = "تاریخ تولد")]

        // DateTime? یعنی این فیلد می‌تواند null باشد (علامت ? = Nullable)
        public DateTime? BirthDate { get; set; }

        // ── شهر ─────────────────────────────────────────────────────
        [Required(ErrorMessage = "شهر الزامی است")]
        // اینجا فقط MaxLength تعریف شده (MinimumLength نداریم)
        [StringLength(100, ErrorMessage = "نام شهر نباید بیشتر از ۱۰۰ کاراکتر باشد")]
        [Display(Name = "شهر")]
        public string City { get; set; } = "";

        // ── آدرس ────────────────────────────────────────────────────
        [Required(ErrorMessage = "آدرس الزامی است")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "آدرس باید بین ۱۰ تا ۵۰۰ کاراکتر باشد")]
        [Display(Name = "آدرس")]
        public string Address { get; set; } = "";

        // ── ایمیل ───────────────────────────────────────────────────
        [Required(ErrorMessage = "ایمیل الزامی است")]

        // [EmailAddress] → فرمت ایمیل را بررسی می‌کند (باید @ و . داشته باشد)
        [EmailAddress(ErrorMessage = "فرمت ایمیل صحیح نیست")]
        [StringLength(100, ErrorMessage = "ایمیل نباید بیشتر از ۱۰۰ کاراکتر باشد")]
        [Display(Name = "ایمیل")]
        public string Email { get; set; } = "";

        // ── شماره تلفن ──────────────────────────────────────────────
        [Required(ErrorMessage = "شماره تلفن الزامی است")]

        // [Phone] → اعتبارسنجی پایه برای شماره تلفن
        [Phone(ErrorMessage = "فرمت شماره تلفن صحیح نیست")]

        // [RegularExpression] → الگوی دقیق‌تر با Regex
        // ^ = شروع رشته
        // 09 = باید با ۰۹ شروع شود
        // [0-9]{9} = بعد از آن دقیقاً ۹ رقم
        // $ = پایان رشته
        // نتیجه: دقیقاً ۱۱ رقم که با ۰۹ شروع می‌شود
        [RegularExpression(@"^09[0-9]{9}$", ErrorMessage = "شماره موبایل باید با ۰۹ شروع شود و ۱۱ رقم باشد")]
        [Display(Name = "شماره تلفن")]
        public string Tel { get; set; } = "";
    }
}
