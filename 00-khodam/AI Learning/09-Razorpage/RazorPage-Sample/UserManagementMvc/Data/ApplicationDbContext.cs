using Microsoft.EntityFrameworkCore;
using UserManagementMvc.Models;

namespace UserManagementMvc.Data;

// DbContext قلب ارتباط Entity Framework Core با دیتابیس است.
// هر DbSet معمولاً نماینده یک جدول در بانک اطلاعاتی است.
public class ApplicationDbContext : DbContext
{
    // options شامل تنظیمات اتصال به دیتابیس است که از Program.cs ارسال می‌شود.
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // جدول Users بر اساس مدل UserInputModel
    public DbSet<UserInputModel> Users => Set<UserInputModel>();

    // جدول کاربران ورود به سیستم
    public DbSet<AppUser> AppUsers => Set<AppUser>();

    // این متد برای شخصی‌سازی مدل دیتابیسی استفاده می‌شود.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserInputModel>(entity =>
        {
            // مشخص می‌کنیم نام جدول در SQL Server برابر Users باشد.
            entity.ToTable("Users");
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("AppUsers");

            // نام کاربری نرمال‌شده باید یکتا باشد تا یک کاربر دوبار ثبت نشود.
            entity.HasIndex(x => x.NormalizedUsername).IsUnique();
        });
    }
}
