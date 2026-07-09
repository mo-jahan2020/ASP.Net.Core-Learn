using Microsoft.EntityFrameworkCore;

namespace SportsStore.Models {

    /// <summary>
    /// پیاده‌سازی IOrderRepository با EF Core
    /// از Include و ThenInclude برای Eager Loading استفاده می‌کنیم
    /// تا سفارش به همراه خطوط و محصولات هر خط در یک کوئری بارگذاری شود.
    /// </summary>
    public class EFOrderRepository : IOrderRepository {
        private StoreDbContext context;

        public EFOrderRepository(StoreDbContext ctx) {
            context = ctx;
        }

        // بارگذاری سفارشات با خطوط و محصولات مرتبط
        public IQueryable<Order> Orders => context.Orders
                            .Include(o => o.Lines)
                            .ThenInclude(l => l.Product);

        /// <summary>
        /// ذخیره یک سفارش
        ///
        /// مراحل:
        /// 1) AttachRange: محصولات را Track می‌کند بدون اینکه مجدداً Insert شوند
        ///    (جلوگیری از تکرار محصولات در دیتابیس)
        /// 2) اگر OrderID == 0 یعنی سفارش جدید است، آن را Add می‌کنیم
        /// 3) اگر OrderID != 0 یعنی سفارش موجود است (مثلاً فقط Shipped را تغییر داده‌ایم)
        ///    در این حالت باید موجودیت را Update کنیم
        /// </summary>
        public void SaveOrder(Order order) {
            // Attach کردن محصولات تا EF آن‌ها را در دیتابیس جستجو کند، نه Insert
            // این کار از تکرار شدن محصولات جلوگیری می‌کند
            context.AttachRange(order.Lines.Select(l => l.Product));

            if (order.OrderID == 0) {
                // سفارش جدید - Add می‌کنیم
                context.Orders.Add(order);
            } else {
                // سفارش موجود - Update می‌کنیم
                // این کار برای زمانی است که می‌خواهیم Shipped را تغییر دهیم
                context.Orders.Update(order);
            }

            context.SaveChanges();
        }
    }
}
