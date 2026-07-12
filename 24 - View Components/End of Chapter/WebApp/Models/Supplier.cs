// =====================================================================
// Supplier.cs - مدل مربوط به تأمین‌کننده
// =====================================================================
// تأمین‌کننده شرکت یا فردی است که محصولات را برای فروشگاه تأمین می‌کند.
// هر تأمین‌کننده می‌تواند چندین محصول داشته باشد (رابطه‌ی یک-به-چند).
// =====================================================================

// تعریف namespace
namespace WebApp.Models {
    // تعریف کلاس Supplier
    public class Supplier {

        // کلید اصلی جدول Suppliers
        public long SupplierId { get; set; }

        // نام تأمین‌کننده (مثلاً "Splash Dudes")
        public string Name { get; set; } = string.Empty;

        // شهری که تأمین‌کننده در آن قرار دارد (مثلاً "San Jose")
        public string City { get; set; } = string.Empty;

        // Navigation Property: لیست محصولاتی که این تأمین‌کننده تأمین می‌کند
        // IEnumerable<Product>? یعنی اختیاری است و می‌تواند null باشد
        public IEnumerable<Product>? Products { get; set; }
    }
}
