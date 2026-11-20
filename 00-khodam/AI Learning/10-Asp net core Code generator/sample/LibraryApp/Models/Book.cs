// ============================================================
// مدل کتاب (Book Entity)
// این کلاس یک جدول در دیتابیس را نمایندگی می‌کند
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Models;

public class Book
{
    // کلید اصلی - به‌صورت خودکار توسط EF Core شناسایی می‌شود
    public int Id { get; set; }

    [Required(ErrorMessage = "عنوان کتاب الزامی است")]
    [StringLength(200)]
    [Display(Name = "عنوان")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "نام نویسنده الزامی است")]
    [StringLength(150)]
    [Display(Name = "نویسنده")]
    public string Author { get; set; } = string.Empty;

    [StringLength(50)]
    [Display(Name = "شابک (ISBN)")]
    public string? ISBN { get; set; }

    [Display(Name = "سال انتشار")]
    [Range(1000, 2100, ErrorMessage = "سال معتبر وارد کنید")]
    public int? PublishYear { get; set; }

    [StringLength(1000)]
    [Display(Name = "توضیحات")]
    public string? Description { get; set; }

    [Display(Name = "تاریخ ثبت")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
