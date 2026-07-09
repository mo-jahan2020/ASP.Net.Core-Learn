using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SportsStore.Models {

    /// <summary>
    /// کلاس مدل محصول (Product)
    /// این کلاس نمایانگر یک محصول در فروشگاه است.
    /// هر فیلد دارای Data Annotation است که اعتبارسنجی و رفتار EF را تعیین می‌کند.
    /// </summary>
    public class Product {

        // کلید اصلی (Primary Key). nullable چون موقع ساخت، هنوز مقدار ندارد.
        public long? ProductID { get; set; }

        // نام محصول - اجباری با پیام خطای فارسی
        [Required(ErrorMessage = "لطفاً نام محصول را وارد کنید")]
        public string Name { get; set; } = String.Empty;

        // توضیحات محصول - اجباری
        [Required(ErrorMessage = "لطفاً توضیحات را وارد کنید")]
        public string Description { get; set; } = String.Empty;

        // قیمت محصول - باید بین 0.01 و حداکثر مقدار باشد
        // Column(TypeName="decimal(8,2)") باعث می‌شود در SQL Server ستون از نوع
        // decimal با ۸ رقم کل و ۲ رقم اعشار ساخته شود
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "لطفاً یک قیمت مثبت وارد کنید")]
        [Column(TypeName = "decimal(8, 2)")]
        public decimal Price { get; set; }

        // دسته‌بندی محصول - اجباری
        [Required(ErrorMessage = "لطفاً دسته‌بندی را مشخص کنید")]
        public string Category { get; set; } = String.Empty;

        // ----------------------------------------------------------------------------
        // فیلد جدید: آدرس تصویر محصول (اختیاری)
        // اگر تصویری تنظیم نشود، یک Placeholder نمایش داده می‌شود.
        // می‌تواند URL نسبی مثل "/images/products/kayak.jpg" یا URL کامل باشد.
        // ----------------------------------------------------------------------------
        [Display(Name = "آدرس تصویر محصول")]
        [Url(ErrorMessage = "لطفاً یک آدرس URL معتبر وارد کنید")]
        [StringLength(500, ErrorMessage = "آدرس تصویر نمی‌تواند بیشتر از ۵۰۰ کاراکتر باشد")]
        public string? ImageUrl { get; set; }
    }
}
