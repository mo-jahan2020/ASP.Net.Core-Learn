// =============================================================================
// فایل Models/DataContext.cs — کلاس DbContext دیتابیس
// =============================================================================
// DataContext پل بین کد C# و دیتابیس SQL Server است.
// مسئولیت‌ها: ترجمه LINQ به SQL، Change Tracking، SaveChanges.
// =============================================================================

using Microsoft.EntityFrameworkCore;

namespace WebApp.Models {
    // ارث‌بری از DbContext — کلاس پایه EF Core
    public class DataContext : DbContext {

        // Constructor — دریافت تنظیمات (Provider, Connection String)
        // این تنظیمات در Program.cs با AddDbContext مشخص شده‌اند.
        public DataContext(DbContextOptions<DataContext> opts)
            : base(opts) { }

        // DbSet<Product>: دسترسی به جدول Products — عملیات CRUD
        // Set<T>(): روش جدید EF Core 7+ (جایگزین { get; set; })
        public DbSet<Product> Products => Set<Product>();

        // DbSet<Category>: دسترسی به جدول Categories
        public DbSet<Category> Categories => Set<Category>();

        // DbSet<Supplier>: دسترسی به جدول Suppliers
        public DbSet<Supplier> Suppliers => Set<Supplier>();
    }
}
