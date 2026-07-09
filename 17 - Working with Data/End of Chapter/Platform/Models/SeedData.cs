using Microsoft.EntityFrameworkCore;

namespace Platform.Models {
    // کلاسی که مسئول مقداردهی اولیه (Seed) دیتابیس محاسبات با داده‌های نمونه است
    public class SeedData {
        private CalculationContext context;
        private ILogger<SeedData> logger;

        // مجموعه‌ای از داده‌های نمونه از پیش محاسبه‌شده (Count -> Result)
        // مثلاً برای Count=5، نتیجه مجموع اعداد 1 تا 5 برابر 15 است
        private static Dictionary<int, long> data
            = new Dictionary<int, long>() {
                {1, 1}, {2, 3}, {3, 6}, {4, 10}, {5, 15},
                {6, 21}, {7, 28}, {8, 36}, {9, 45}, {10, 55}
            };

        // سازنده‌ای که CalculationContext و Logger را از طریق تزریق وابستگی دریافت می‌کند
        public SeedData(CalculationContext dataContext, ILogger<SeedData> log) {
            context = dataContext;
            logger = log;
        }

        // متد اصلی مقداردهی اولیه دیتابیس
        public void SeedDatabase() {
            // اعمال Migration‌های در انتظار روی دیتابیس (ساخت یا به‌روزرسانی جداول)
            context.Database.Migrate();
            // اگر جدول محاسبات خالی باشد، داده‌های نمونه را وارد کن
            if (context.Calculations?.Count() == 0) {
                logger.LogInformation("Preparing to seed database");
                context.Calculations.AddRange(
                        data.Select(kvp => new Calculation() {
                            Count = kvp.Key, Result = kvp.Value
                        }));
                context.SaveChanges();
                logger.LogInformation("Database seeded");
            } else {
                // در غیر این صورت (داده‌ای از قبل وجود دارد)، کاری انجام نده
                logger.LogInformation("Database not seeded");
            }
        }
    }
}
