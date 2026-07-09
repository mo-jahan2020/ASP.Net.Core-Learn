namespace SportsStore.Models {

    /// <summary>
    /// اینترفیس Repository محصولات
    /// الگوی Repository: لایه‌ای بین Business Logic و Data Access
    /// مزیت: تست‌پذیری و جداسازی منطق دسترسی به داده
    /// </summary>
    public interface IStoreRepository {

        // لیست محصولات به صورت IQueryable (اجازه می‌دهد Query در دیتابیس اجرا شود)
        IQueryable<Product> Products { get; }

        // ذخیره تغییرات یک محصول موجود
        void SaveProduct(Product p);

        // ایجاد محصول جدید
        void CreateProduct(Product p);

        // حذف محصول
        void DeleteProduct(Product p);
    }
}
