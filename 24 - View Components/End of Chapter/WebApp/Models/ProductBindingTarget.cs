// =====================================================================
// ProductBindingTarget.cs - کلاس هدف (DTO) برای Binding داده‌ها
// =====================================================================
// DTO (Data Transfer Object) کلاسی است که برای انتقال داده‌ها بین لایه‌ها
// استفاده می‌شود. در اینجا، این کلاس برای دریافت اطلاعات محصول از طریق
// API (مثلاً درخواست POST) استفاده می‌شود.
// ویژگی‌های Data Annotations (مثل [Required] و [Range]) قوانین اعتبارسنجی هستند.
// =====================================================================

 using System.ComponentModel.DataAnnotations; // فضای نام ویژگی‌های اعتبارسنجی

// تعریف namespace
namespace WebApp.Models {
    // تعریف کلاس ProductBindingTarget
    public class ProductBindingTarget {

        // [Required]: این ویژگی اعتبارسنجی، نام محصول را اجباری می‌کند
        // اگر مقدار Name خالی یا null باشد، ModelState نامعتبر می‌شود
        [Required]
        public string Name { get; set; } = "";

        // [Range(1, 1000)]: قیمت محصول باید بین 1 تا 1000 باشد
        // اگر خارج از این محدوده باشد، اعتبارسنجی ناموفق می‌شود
        [Range(1, 1000)]
        public decimal Price { get; set; }

        // [Range(1, long.MaxValue)]: شناسه‌ی دسته‌بندی باید حداقل 1 باشد
        [Range(1, long.MaxValue)]
        public long CategoryId { get; set; }

        // [Range(1, long.MaxValue)]: شناسه‌ی تأمین‌کننده نیز باید حداقل 1 باشد
        [Range(1, long.MaxValue)]
        public long SupplierId { get; set; }

        // متد ToProduct: تبدیل این DTO به یک شیء Product
        // این متد زمانی استفاده می‌شود که بخواهیم داده‌های دریافتی از کلاینت
        // را به یک شیء دامنه (Domain Object) تبدیل کنیم تا در پایگاه داده ذخیره شود
        // استفاده از Expression-Bodied Member (=>) برای خلاصه‌نویسی
        public Product ToProduct() => new Product() {
            // انتقال مقادیر از این شیء (this) به شیء Product جدید
            Name = this.Name,
            Price = this.Price,
            CategoryId = this.CategoryId,
            SupplierId = this.SupplierId
        };
    }
}
