// =============================================================================
// فایل Models/Category.cs — مدل موجودیت دسته‌بندی (Category)
// =============================================================================
// این کلاس یک Entity است که به جدول Categories در دیتابیس نگاشت می‌شود.
// EF Core با Convention نام کلاس را به نام جدول نگاشت می‌کند.
// =============================================================================

namespace WebApp.Models {
    public class Category {

        // کلید اصلی (Primary Key) — Convention: الگوی <ClassName>Id
        // نوع long → bigint در SQL Server با Identity (خودکار افزایشی)
        public long CategoryId { get; set; }

        // نام دسته‌بندی — nvarchar(max) در دیتابیس
        public string Name { get; set; } = string.Empty;

        // خاصیت ناوبری (Navigation Property) — رابطه One-to-Many با Product
        // ⚠️ ستون در دیتابیس ندارد! فقط برای ناوبری بین موجودیت‌هاست.
        // با Include می‌توانید محصولات را همراه دسته بارگذاری کنید:
        //   context.Categories.Include(c => c.Products)
        public IEnumerable<Product>? Products { get; set; }
    }
}
