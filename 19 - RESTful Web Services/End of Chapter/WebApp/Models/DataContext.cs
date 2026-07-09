// =============================================================================
// فایل Models/DataContext.cs — کلاس Context دیتابیس (DbContext)
// =============================================================================
// DataContext کلاس اصلی Entity Framework Core است که به عنوان پل بین کد C#
// و دیتابیس SQL Server عمل می‌کند. EF Core از این کلاس برای:
// - ترجمه کوئری‌های LINQ به SQL
// - ردیابی تغییرات (Change Tracking)
// - ذخیره تغییرات در دیتابیس (SaveChanges)
// استفاده می‌کند.
// =============================================================================

using Microsoft.EntityFrameworkCore;

namespace WebApp.Models {
    // -------------------------------------------------------------------------
    // کلاس DataContext — ارث‌بری از DbContext
    // -------------------------------------------------------------------------
    // DbContext کلاس پایه EF Core است. هر پروژه معمولاً یک DbContext دارد
    // که تمام موجودیت‌ها (Entities) را از طریق DbSet ها در دسترس قرار می‌دهد.
    public class DataContext : DbContext {

        // ---------------------------------------------------------------------
        // Constructor — دریافت تنظیمات DbContext
        // ---------------------------------------------------------------------
        // DbContextOptions<DataContext>: شامل تنظیماتی مثل Provider دیتابیس
        // و رشته اتصال است. این تنظیمات در Program.cs با AddDbContext مشخص شده‌اند.
        // base(opts): تنظیمات را به کلاس پایه DbContext منتقل می‌کند.
        public DataContext(DbContextOptions<DataContext> opts)
            : base(opts) { }

        // ---------------------------------------------------------------------
        // DbSet<Product> Products — دسترسی به جدول محصولات
        // ---------------------------------------------------------------------
        // DbSet: هر DbSet نماینده یک جدول در دیتابیس است.
        // عملیات CRUD روی جدول Products از طریق این خصوصیت انجام می‌شود:
        // - Products.ToList(): SELECT * FROM Products
        // - Products.Add(p): INSERT INTO Products
        // - Products.Remove(p): DELETE FROM Products
        // - Products.Update(p): UPDATE Products
        // Set<Product>(): روش جدید در EF Core 7+ برای تعریف DbSet
        // (جایگزین قدیمی: public DbSet<Product> Products { get; set; })
        public DbSet<Product> Products => Set<Product>();

        // ---------------------------------------------------------------------
        // DbSet<Category> Categories — دسترسی به جدول دسته‌بندی‌ها
        // ---------------------------------------------------------------------
        // هر دسته‌بندی می‌تواند چند محصول داشته باشد (رابطه One-to-Many).
        // EF Core این رابطه را از طریق ForeignKey تشخیص می‌دهد.
        public DbSet<Category> Categories => Set<Category>();

        // ---------------------------------------------------------------------
        // DbSet<Supplier> Suppliers — دسترسی به جدول تأمین‌کنندگان
        // ---------------------------------------------------------------------
        // هر تأمین‌کننده می‌تواند چند محصول داشته باشد (رابطه One-to-Many).
        public DbSet<Supplier> Suppliers => Set<Supplier>();
    }
}