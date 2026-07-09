using Microsoft.EntityFrameworkCore;

namespace SportsStore.Models {

    /// <summary>
    /// پیاده‌سازی IStoreRepository با استفاده از Entity Framework Core
    /// این کلاس واقعاً با دیتابیس کار می‌کند.
    /// </summary>
    public class EFStoreRepository : IStoreRepository {

        // زمینه دیتابیس تزریق شده از طریق DI
        // Scoped: یک نمونه به ازای هر HTTP Request
        private StoreDbContext context;

        public EFStoreRepository(StoreDbContext ctx) {
            context = ctx;
        }

        // دسترسی به محصولات به صورت Query (اجازه می‌دهد کوئری در دیتابیس اجرا شود)
        public IQueryable<Product> Products => context.Products;

        /// <summary>
        /// ایجاد محصول جدید در دیتابیس
        /// </summary>
        public void CreateProduct(Product p) {
            context.Add(p);
            context.SaveChanges();
        }

        /// <summary>
        /// حذف محصول از دیتابیس
        /// ابتدا محصول را پیدا می‌کنیم تا EF آن را Track کند، سپس حذف می‌کنیم.
        /// </summary>
        public void DeleteProduct(Product p) {
            // اگر محصول Track نشده، آن را Attach می‌کنیم
            context.Products.Attach(p);
            context.Products.Remove(p);
            context.SaveChanges();
        }

        /// <summary>
        /// ذخیره تغییرات یک محصول موجود
        ///
        /// نکته مهم: محصولی که از فرم می‌آید، در EF Track نشده است (Detached).
        /// برای ذخیره تغییرات دو راه وجود دارد:
        ///
        /// راه ۱ (ایمن‌تر): بارگذاری محصول از دیتابیس، کپی مقادیر، SaveChanges
        /// راه ۲: استفاده از Update که State را به Modified تغییر می‌دهد
        ///
        /// ما راه ۱ را انتخاب می‌کنیم چون اگر محصول وجود نداشت، خطا ندهیم.
        /// </summary>
        public void SaveProduct(Product p) {
            // اگر محصول شناسه ندارد، یعنی جدید است
            if (p.ProductID == null || p.ProductID == 0) {
                context.Products.Add(p);
            } else {
                // بارگذاری محصول موجود از دیتابیس
                Product? dbEntry = context.Products
                    .FirstOrDefault(x => x.ProductID == p.ProductID);

                if (dbEntry != null) {
                    // کپی مقادیر جدید از فرم به موجودیت بارگذاری‌شده
                    dbEntry.Name        = p.Name;
                    dbEntry.Description = p.Description;
                    dbEntry.Price       = p.Price;
                    dbEntry.Category    = p.Category;
                    dbEntry.ImageUrl    = p.ImageUrl;
                } else {
                    // اگر محصول در دیتابیس نبود، آن را اضافه می‌کنیم
                    context.Products.Add(p);
                }
            }

            context.SaveChanges();
        }
    }
}
