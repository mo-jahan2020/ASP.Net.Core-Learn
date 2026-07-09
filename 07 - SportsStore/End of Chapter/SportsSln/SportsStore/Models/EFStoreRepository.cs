using Microsoft.EntityFrameworkCore;

namespace SportsStore.Models {
    public class EFStoreRepository : IStoreRepository {
        private StoreDbContext context;

        // سازنده که DbContext را به عنوان پارامتر می‌گیرد
        public EFStoreRepository(StoreDbContext ctx) {
            context = ctx;
        }

        // پیاده‌سازی IQueryable<Product> برای دسترسی به محصولات
        public IQueryable<Product> Products => context.Products;

        public void AddNewProduct(Product newProduct)
        {
            context.Products.Add(newProduct); // اضافه کردن محصول به DbSet
            context.SaveChanges(); // ذخیره تغییرات در پایگاه داده
        }

        public void SaveProduct(Product newProduct)
        {
            context.Products.Update(newProduct); // به روز رسانی محصول
            context.SaveChanges(); // ذخیره تغییرات در پایگاه داده
        }
        public Product GetProductById(int id)
        {
            return context.Products.Find(id); // پیدا کردن محصول بر اساس شناسه
        }
        public void DeleteProductById(int id)
        {
            var Product = context.Products.Find(id);//  یافتن محصول بر اساس شناسه
            if (Product!=null && context.Products.Find(id) != null) // اگر محصول پیدا شد، آن را حذف می‌کنیم
                context.Products.Remove(Product); // حذف محصول از DbSet    
        }
    }
}
