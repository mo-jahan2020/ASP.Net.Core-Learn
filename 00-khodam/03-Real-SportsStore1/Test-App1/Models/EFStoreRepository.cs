using Microsoft.EntityFrameworkCore;

namespace Test_App1.Models
{
    public class EFStoreRepository : IStoreRepository
    {
        private StoreDbContext _context;
        public EFStoreRepository(StoreDbContext ctx)
        {
            this._context = ctx;
        }
        public IQueryable<Product> Products => _context.Products;

        public void AddProduct(Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
        }

        public void SaveProduct(Product product)
        {
            _context.Products.Update(product);
            _context.SaveChanges();
        }

        public void DeleteProduct(int productId)
        {
            var product = _context.Products.Find(productId);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
        }
        public async Task<int> GetTotalProducesCountAsync()
        {
            return await _context.Products.CountAsync();
        }
        public async Task<IEnumerable<Product>> GetPagedProductsAsync(int pageNumber, int pageSize)
        {
            return await _context.Products
                .OrderBy(p => p.ProductID) // مرتب‌سازی بر اساس یک فیلد (مثلاً ProductID)
                .Skip((pageNumber - 1) * pageSize) // محاسبه رکوردهایی که باید رد شوند
                .Take(pageSize) // تعداد رکوردهای مورد نیاز برای صفحه فعلی
                .ToListAsync(); // تبدیل به لیست
        }
    }
}
