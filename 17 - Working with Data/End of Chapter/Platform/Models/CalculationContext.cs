using Microsoft.EntityFrameworkCore;

namespace Platform.Models {

    // کلاس Context مربوط به Entity Framework Core که ارتباط با دیتابیس محاسبات را مدیریت می‌کند
    public class CalculationContext : DbContext {

        // سازنده‌ای که تنظیمات DbContext (مانند رشته اتصال) را از طریق تزریق وابستگی دریافت می‌کند
        public CalculationContext(DbContextOptions<CalculationContext> opts)
            : base(opts) { }

        // مجموعه (جدول) محاسبات در دیتابیس
        public DbSet<Calculation> Calculations => Set<Calculation>();
    }
}
