using System.ComponentModel.DataAnnotations;

namespace UserManagementMvc.ViewModels;

// ViewModel مخصوص فرم ساخت کاربر جدید
public class UserCreateViewModel
{
    [Required(ErrorMessage = "نام الزامی است")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "نام باید بین ۲ تا ۵۰ کاراکتر باشد")]
    [Display(Name = "نام")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "نام خانوادگی الزامی است")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "نام خانوادگی باید بین ۲ تا ۵۰ کاراکتر باشد")]
    [Display(Name = "نام خانوادگی")]
    public string Family { get; set; } = "";

    [Required(ErrorMessage = "تاریخ تولد الزامی است")]
    [DataType(DataType.Date)]
    [Display(Name = "تاریخ تولد")]
    public DateTime? BirthDate { get; set; }

    [Required(ErrorMessage = "شهر الزامی است")]
    [StringLength(100, ErrorMessage = "نام شهر نباید بیشتر از ۱۰۰ کاراکتر باشد")]
    [Display(Name = "شهر")]
    public string City { get; set; } = "";

    [Required(ErrorMessage = "آدرس الزامی است")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "آدرس باید بین ۱۰ تا ۵۰۰ کاراکتر باشد")]
    [Display(Name = "آدرس")]
    public string Address { get; set; } = "";

    [Required(ErrorMessage = "ایمیل الزامی است")]
    [EmailAddress(ErrorMessage = "فرمت ایمیل صحیح نیست")]
    [StringLength(100, ErrorMessage = "ایمیل نباید بیشتر از ۱۰۰ کاراکتر باشد")]
    [Display(Name = "ایمیل")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "شماره تلفن الزامی است")]
    [Phone(ErrorMessage = "فرمت شماره تلفن صحیح نیست")]
    [RegularExpression(@"^09[0-9]{9}$", ErrorMessage = "شماره موبایل باید با ۰۹ شروع شود و ۱۱ رقم باشد")]
    [Display(Name = "شماره تلفن")]
    public string Tel { get; set; } = "";
}
