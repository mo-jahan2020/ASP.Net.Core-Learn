using System.ComponentModel.DataAnnotations;

namespace UserManagementMvc.Models;

// این مدل Entity اصلی جدول Users است.
// در این نسخه برای فرم‌ها مستقیماً از این کلاس استفاده نمی‌کنیم،
// بلکه ViewModelهای جداگانه داریم تا لایه نمایش از لایه دیتابیس جدا بماند.
public class UserInputModel
{
    // کلید اصلی جدول
    public int Id { get; set; }

    // Required یعنی این فیلد اجباری است.
    // StringLength طول مجاز متن را مشخص می‌کند.
    // Display نام فارسی فیلد را در فرم‌ها و پیام‌ها نمایش می‌دهد.
    [Required(ErrorMessage = "نام الزامی است")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "نام باید بین ۲ تا ۵۰ کاراکتر باشد")]
    [Display(Name = "نام")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "نام خانوادگی الزامی است")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "نام خانوادگی باید بین ۲ تا ۵۰ کاراکتر باشد")]
    [Display(Name = "نام خانوادگی")]
    public string Family { get; set; } = "";

    // DateTime? یعنی این فیلد می‌تواند null هم باشد.
    // DataType.Date باعث نمایش کنترل تاریخ در فرم می‌شود.
    [Required(ErrorMessage = "تاریخ تولد الزامی است")]
    [DataType(DataType.Date)]
    [Display(Name = "تاریخ تولد")]
    public DateTime? BirthDate { get; set; }

    // نام شهر
    [Required(ErrorMessage = "شهر الزامی است")]
    [StringLength(100, ErrorMessage = "نام شهر نباید بیشتر از ۱۰۰ کاراکتر باشد")]
    [Display(Name = "شهر")]
    public string City { get; set; } = "";

    // آدرس کامل کاربر
    [Required(ErrorMessage = "آدرس الزامی است")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "آدرس باید بین ۱۰ تا ۵۰۰ کاراکتر باشد")]
    [Display(Name = "آدرس")]
    public string Address { get; set; } = "";

    // EmailAddress فرمت کلی ایمیل را اعتبارسنجی می‌کند.
    [Required(ErrorMessage = "ایمیل الزامی است")]
    [EmailAddress(ErrorMessage = "فرمت ایمیل صحیح نیست")]
    [StringLength(100, ErrorMessage = "ایمیل نباید بیشتر از ۱۰۰ کاراکتر باشد")]
    [Display(Name = "ایمیل")]
    public string Email { get; set; } = "";

    // Phone یک بررسی عمومی انجام می‌دهد.
    // RegularExpression بررسی دقیق‌تر برای شماره موبایل ایران انجام می‌دهد.
    [Required(ErrorMessage = "شماره تلفن الزامی است")]
    [Phone(ErrorMessage = "فرمت شماره تلفن صحیح نیست")]
    [RegularExpression(@"^09[0-9]{9}$", ErrorMessage = "شماره موبایل باید با ۰۹ شروع شود و ۱۱ رقم باشد")]
    [Display(Name = "شماره تلفن")]
    public string Tel { get; set; } = "";
}
