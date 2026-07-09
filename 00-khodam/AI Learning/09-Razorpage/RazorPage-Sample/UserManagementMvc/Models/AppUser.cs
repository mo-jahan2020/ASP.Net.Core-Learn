using System.ComponentModel.DataAnnotations;

namespace UserManagementMvc.Models;

// این مدل برای کاربرانِ ورود به سیستم استفاده می‌شود.
// دقت کن که این جدول با جدول Users فرق دارد:
// AppUsers برای احراز هویت است و Users برای اطلاعات کاربران برنامه.
public class AppUser
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; } = "";

    [Required]
    [StringLength(50)]
    public string NormalizedUsername { get; set; } = "";

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = "";

    [Required]
    public string PasswordHash { get; set; } = "";
}
