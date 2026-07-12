// =====================================================================
// Product.cs - مدل مربوط به محصول
// =====================================================================
// این کلاس نمایانگر یک محصول در فروشگاه است.
// محصولات به دسته‌بندی‌ها (Category) و تأمین‌کنندگان (Supplier) مرتبط هستند.
// این مدل هم برای پایگاه داده (EF Core) و هم برای JSON (API) استفاده می‌شود.
// =====================================================================

 using System.ComponentModel.DataAnnotations.Schema; // برای استفاده از ویژگی‌هایی مثل [Column]
 using System.Text.Json.Serialization; // برای کنترل سریال‌سازی JSON

// تعریف namespace
namespace WebApp.Models {
    // تعریف کلاس Product
    public class Product {

        // کلید اصلی (Primary Key) جدول Products
        public long ProductId { get; set; }

        // نام محصول
        // string.Empty مقدار پیش‌فرض را به جای null قرار می‌دهد
        public string Name { get; set; } = string.Empty;

        // قیمت محصول
        // [Column(TypeName = "decimal(8, 2)")]: مشخص می‌کند در پایگاه داده، این ستون
        //   از نوع decimal با 8 رقم کل و 2 رقم اعشار باشد (مثلاً 999999.99)
        //   این کار از خطاهای گرد کردن اعداد در محاسبات مالی جلوگیری می‌کند
        [Column(TypeName = "decimal(8, 2)")]
        public decimal Price { get; set; }

        // کلید خارجی (Foreign Key) به جدول Categories
        // هر محصول متعلق به یک دسته‌بندی است (رابطه‌ی چند-به-یک)
        public long CategoryId { get; set; }

        // Navigation Property به کلاس Category
        // با استفاده از این ویژگی می‌توانیم به اطلاعات دسته‌بندی محصول دسترسی داشته باشیم
        // Category? یعنی می‌تواند null باشد (مثلاً اگر محصول هنوز به دسته‌ای متصل نشده)
        public Category? Category { get; set; }

        // کلید خارجی به جدول Suppliers
        public long SupplierId { get; set; }

        // Navigation Property به کلاس Supplier
        // [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]:
        //   اگر Supplier در حین سریال‌سازی JSON برابر null بود، این ویژگی در خروجی JSON
        //   نمایش داده نمی‌شود. این کار از ارسال فیلدهای خالی به کلاینت جلوگیری می‌کند.
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Supplier? Supplier { get; set; }
    }
}
