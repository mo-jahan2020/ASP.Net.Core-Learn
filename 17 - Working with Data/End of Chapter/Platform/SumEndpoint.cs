using Microsoft.Extensions.Caching.Distributed;
using Platform.Services;
using Platform.Models;

namespace Platform {

    // کلاسی که Endpoint محاسبه مجموع اعداد از 1 تا count را پیاده‌سازی می‌کند
    // و نتیجه را برای استفاده مجدد در دیتابیس ذخیره (کش) می‌کند
    public class SumEndpoint {

        // متد Endpoint که با استفاده از تزریق وابستگی به CalculationContext (دیتابیس) دسترسی دارد
        public async Task Endpoint(HttpContext context,
                CalculationContext dataContext) {
            int count;
            // تلاش برای تبدیل پارامتر مسیر "count" به عدد صحیح
            int.TryParse((string?)context.Request.RouteValues["count"],
                out count);
            // جستجو در دیتابیس برای یافتن نتیجه محاسبه‌شده قبلی برای همین مقدار count
            long total = dataContext.Calculations?
                .FirstOrDefault(c => c.Count == count)?.Result ?? 0;
            // اگر نتیجه‌ای در دیتابیس یافت نشد (یا برابر صفر بود)، محاسبه را از ابتدا انجام بده
            if (total == 0) {
                for (int i = 1; i <= count; i++) {
                    total += i;
                }
                // ذخیره نتیجه جدید محاسبه در دیتابیس برای استفاده‌های بعدی
                dataContext.Calculations?.Add(new() {
                    Count = count, Result = total
                });
                await dataContext.SaveChangesAsync();
            }
            // ساخت رشته نتیجه به همراه زمان جاری
            string totalString = $"({ DateTime.Now.ToLongTimeString() }) {total}";
            // نوشتن نتیجه نهایی در پاسخ
            await context.Response.WriteAsync(
                $"({DateTime.Now.ToLongTimeString()}) Total for {count}"
                + $" values:\n{totalString}\n");
        }
    }
}
