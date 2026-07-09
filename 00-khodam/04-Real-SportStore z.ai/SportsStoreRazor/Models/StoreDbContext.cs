using Microsoft.EntityFrameworkCore;

namespace SportsStore.Models {

    /// <summary>
    /// DbContext اصلی برنامه
    /// این کلاس پل ارتباطی بین مدل‌های دامنه و دیتابیس است.
    /// DbSet ها نمایانگر جداول دیتابیس هستند.
    /// </summary>
    public class StoreDbContext : DbContext {

        // سازنده: گزینه‌های کانفیگ (مثل ConnectionString) را از Dependency Injection دریافت می‌کند
        public StoreDbContext(DbContextOptions<StoreDbContext> options)
            : base(options) { }

        // جدول محصولات
        public DbSet<Product> Products => Set<Product>();

        // جدول سفارشات
        public DbSet<Order> Orders => Set<Order>();
    }
}
