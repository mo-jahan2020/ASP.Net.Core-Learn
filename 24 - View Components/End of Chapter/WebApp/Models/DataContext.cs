// =====================================================================
// DataContext.cs - DbContext اصلی برنامه
// =====================================================================
// DbContext مهم‌ترین کلاس Entity Framework Core است که:
//   1. اتصال به پایگاه داده را مدیریت می‌کند
//   2. جداول را با استفاده از DbSet نمایش می‌دهد
//   3. عملیات CRUD (Create, Read, Update, Delete) را انجام می‌دهد
//   4. تغییرات را ردیابی (Track) می‌کند
// =====================================================================

 using Microsoft.EntityFrameworkCore; // فضای نام EF Core برای استفاده از DbContext

// تعریف namespace
namespace WebApp.Models {
    // تعریف کلاس DataContext که از DbContext ارث‌بری می‌کند
    // DbContext یک کلاس پایه‌ی EF Core است
    public class DataContext : DbContext {

        // سازنده‌ی کلاس
        // opts: شامل تنظیمات DbContext است (مثلاً رشته اتصال)
        // : base(opts) یعنی تنظیمات را به سازنده‌ی کلاس پایه (DbContext) پاس می‌دهیم
        public DataContext(DbContextOptions<DataContext> opts)
            : base(opts) { }

        // DbSet<Product>: نمایش‌دهنده‌ی جدول Products در پایگاه داده
        // با استفاده از این ویژگی می‌توانیم کوئری‌هایی روی جدول محصولات بنویسیم
        // Set<Product>(): معادل اضافه کردن { get; set; } به یک ویژگی DbSet
        public DbSet<Product> Products => Set<Product>();

        // DbSet<Category>: نمایش‌دهنده‌ی جدول Categories
        public DbSet<Category> Categories => Set<Category>();

        // DbSet<Supplier>: نمایش‌دهنده‌ی جدول Suppliers
        public DbSet<Supplier> Suppliers => Set<Supplier>();
    }
}
