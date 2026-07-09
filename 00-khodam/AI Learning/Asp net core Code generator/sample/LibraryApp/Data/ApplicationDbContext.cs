// ============================================================
// ApplicationDbContext
// پل ارتباطی بین برنامه و دیتابیس (Entity Framework Core)
// از IdentityDbContext ارث‌بری می‌کند تا جداول کاربران را هم بسازد
// ============================================================

using LibraryApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // جدول کتاب‌ها در دیتابیس
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // فراخوانی پایه برای ساخت جداول Identity
        base.OnModelCreating(builder);

        // می‌توانید تنظیمات Fluent API را اینجا اضافه کنید
        builder.Entity<Book>().HasIndex(b => b.Title);
    }
}
