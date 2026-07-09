// =============================================================================
// فایل Models/Product.cs — مدل موجودیت محصول (Product)
// =============================================================================
// موجودیت اصلی برنامه. هر نمونه = یک رکورد در جدول Products.
// روابط: Product → Category (چند به یک)، Product → Supplier (چند به یک)
// =============================================================================

using System.ComponentModel.DataAnnotations.Schema; // برای [Column]
using System.Text.Json.Serialization; // برای [JsonIgnore]

namespace WebApp.Models {
    public class Product {

        // کلید اصلی — Convention: <ClassName>Id → bigint Identity
        public long ProductId { get; set; }

        // نام محصول — nvarchar(max)
        public string Name { get; set; } = string.Empty;

        // قیمت — [Column(TypeName = "decimal(8, 2)")]: دقت ۸ رقم کل، ۲ اعشار
        // بدون این ویژگی، EF Core از decimal(18,2) استفاده می‌کرد (فضای بیشتر).
        // نوع decimal: مناسب‌ترین نوع C# برای مقادیر مالی.
        [Column(TypeName = "decimal(8, 2)")]
        public decimal Price { get; set; }

        // کلید خارجی به Categories — Convention: <NavigationProperty>Id
        public long CategoryId { get; set; }

        // خاصیت ناوبری به دسته‌بندی — با Include بارگذاری می‌شود
        public Category? Category { get; set; }

        // کلید خارجی به Suppliers
        public long SupplierId { get; set; }

        // خاصیت ناوبری به تأمین‌کننده
        // [JsonIgnore(WhenWritingNull)]: اگر null باشد در JSON نیاید.
        // مزایا: ۱) کاهش حجم پاسخ  ۲) جلوگیری از Circular Reference
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Supplier? Supplier { get; set; }
    }
}
