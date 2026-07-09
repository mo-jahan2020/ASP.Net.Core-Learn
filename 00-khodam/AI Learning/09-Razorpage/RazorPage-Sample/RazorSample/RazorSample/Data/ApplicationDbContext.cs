// ════════════════════════════════════════════════════════════════
// فایل: Data/ApplicationDbContext.cs
// هدف: تعریف پل ارتباطی بین برنامه C# و بانک اطلاعاتی SQL Server
//
// 📌 Entity Framework Core (EF Core) چیست؟
//    یک ORM (Object-Relational Mapper) است. ORM یعنی ابزاری که
//    کلاس‌های C# را به جداول SQL تبدیل می‌کند و برعکس.
//    با EF Core دیگر نیازی به نوشتن دستورات SQL ندارید؛
//    همه‌چیز با کد C# انجام می‌شود.
//
// 📌 DbContext چیست؟
//    کلاس اصلی EF Core است که:
//    ۱. اتصال به بانک اطلاعاتی را مدیریت می‌کند
//    ۲. جداول را به صورت DbSet نشان می‌دهد
//    ۳. عملیات CRUD را انجام می‌دهد
// ════════════════════════════════════════════════════════════════

// وارد کردن فضای نام EF Core
using Microsoft.EntityFrameworkCore;
using RazorSample.Models;

namespace RazorSample.Data
{
    // کلاس ApplicationDbContext از DbContext ارث‌بری می‌کند
    // DbContext کلاس پایه EF Core است که همه امکانات بانک اطلاعاتی را دارد
    public class ApplicationDbContext : DbContext
    {
        // ── سازنده (Constructor) ─────────────────────────────────────
        // options شامل اطلاعات اتصال به بانک اطلاعاتی است (مثل Connection String)
        // این اطلاعات از فایل appsettings.json می‌آید و توسط DI تزریق می‌شود
        //
        // 📌 Dependency Injection (DI) چیست؟
        //    یک الگوی طراحی که وابستگی‌ها را از بیرون به کلاس می‌دهد
        //    به جای اینکه کلاس خودش آنها را بسازد. این کار باعث می‌شود
        //    کد قابل تست‌تر و نگهداری‌پذیرتر باشد.
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) // ارسال options به سازنده کلاس پایه DbContext
        {
        }

        // ── جدول کاربران ────────────────────────────────────────────
        // DbSet<T> نماینده یک جدول در بانک اطلاعاتی است
        // EF Core این Property را به جدول "Users" در SQL Server تبدیل می‌کند
        // نام جدول از نام Property گرفته می‌شود (Users)
        public DbSet<UserInputModel> Users { get; set; }

        // ── پیکربندی جداول ──────────────────────────────────────────
        // این متد توسط EF Core فراخوانی می‌شود تا جزئیات جداول را تنظیم کنیم
        // اینجا می‌توانیم قوانینی تعریف کنیم که در کلاس Model تعریف نشده‌اند
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // فراخوانی متد والد — همیشه باید صدا زده شود
            base.OnModelCreating(modelBuilder);

            // پیکربندی Entity (جدول) مربوط به UserInputModel
            // این بخش "Fluent API" نام دارد — روش دیگری برای تنظیم جداول
            modelBuilder.Entity<UserInputModel>(entity =>
            {
                // تعریف کلید اصلی — اگرچه EF Core خودش Id را می‌شناسد،
                // اما تعریف صریح آن بهترین عملکرد است
                entity.HasKey(e => e.Id);

                // تعریف محدودیت‌های هر ستون در سطح بانک اطلاعاتی
                // IsRequired() = NOT NULL در SQL
                // HasMaxLength() = VARCHAR(n) در SQL

                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Family).IsRequired().HasMaxLength(50);
                entity.Property(e => e.City).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Tel).IsRequired().HasMaxLength(11);

                // 📌 نکته: BirthDate اینجا تعریف نشده چون DateTime? (Nullable) است
                //    و EF Core خودش آن را به ستون NULL-able تبدیل می‌کند
            });
        }
    }
}
