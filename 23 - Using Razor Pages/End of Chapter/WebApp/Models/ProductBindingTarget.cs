// =============================================================================
// فایل Models/ProductBindingTarget.cs — کلاس DTO (Data Transfer Object)
// =============================================================================
// چرا DTO؟ برای جلوگیری از Over-posting. کاربر نتواند ProductId را دستکاری کند.
// فقط فیلدهای مجاز: Name, Price, CategoryId, SupplierId
// =============================================================================

using System.ComponentModel.DataAnnotations; // برای [Required], [Range]

namespace WebApp.Models {
    public class ProductBindingTarget {

        // [Required]: فیلد الزامی — اگر نباشد → 400 Bad Request خودکار
        [Required]
        public string Name { get; set; } = "";

        // [Range(1, 1000)]: اعتبارسنجی محدوده — قیمت بین ۱ تا ۱۰۰۰
        [Range(1, 1000)]
        public decimal Price { get; set; }

        // [Range(1, long.MaxValue)]: شناسه مثبت الزامی
        [Range(1, long.MaxValue)]
        public long CategoryId { get; set; }

        // [Range(1, long.MaxValue)]: شناسه مثبت الزامی
        [Range(1, long.MaxValue)]
        public long SupplierId { get; set; }

        // تبدیل DTO به Entity — فقط فیلدهای مجاز کپی می‌شوند
        // Expression-bodied method (=>) روش کوتاه‌نویسی تک‌خطی
        public Product ToProduct() => new Product() {
            Name = this.Name, Price = this.Price,
            CategoryId = this.CategoryId, SupplierId = this.SupplierId
        };
    }
}
