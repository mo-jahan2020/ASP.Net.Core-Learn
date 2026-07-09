// =============================================================================
// فایل Models/Supplier.cs — مدل موجودیت تأمین‌کننده (Supplier)
// =============================================================================
// موجودیت ساده با سه خصوصیت. رابطه One-to-Many با Product.
// =============================================================================

namespace WebApp.Models {
    public class Supplier {

        // کلید اصلی — bigint Identity (خودکار افزایشی: 1, 2, 3, ...)
        public long SupplierId { get; set; }

        // نام تأمین‌کننده — nvarchar(max)
        public string Name { get; set; } = string.Empty;

        // شهر تأمین‌کننده — nvarchar(max)
        public string City { get; set; } = string.Empty;

        // خاصیت ناوبری — لیست محصولات این تأمین‌کننده
        // با Include بارگذاری می‌شود: context.Suppliers.Include(s => s.Products)
        public IEnumerable<Product>? Products { get; set; }
    }
}
