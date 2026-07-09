using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SportsStore.Models {

    /// <summary>
    /// کلاس مدل سفارش (Order)
    /// هر سفارش شامل چند خط سبد خرید (CartLine) و اطلاعات گیرنده است.
    /// [BindNever] به Model Binder می‌گوید که این فیلد را از فرم پر نکند
    /// (چون مقادیر آن توسط سرور تنظیم می‌شوند، مثلاً OrderID توسط دیتابیس).
    /// </summary>
    public class Order {

        // شناسه یکتای سفارش - توسط دیتابیس تولید می‌شود
        [BindNever]
        public int OrderID { get; set; }

        // لیست خطوط (آیتم‌های) سفارش
        // نکته: از List به جای ICollection استفاده می‌کنیم چون:
        // 1) امکان دسترسی با index را می‌دهد
        // 2) EF Core به طور خودکار آن را به عنوان Navigation Property مدیریت می‌کند
        // 3) متدهای Add/Remove/Count راحت‌تر در دسترس هستند
        [BindNever]
        public List<CartLine> Lines { get; set; } = new List<CartLine>();

        // --- شناسه کاربر (UserId) که سفارش را ثبت کرده ---
        // این فیلد برای پیش‌پر کردن فرم سفارش در خریدهای بعدی استفاده می‌شود.
        // نکته: این فیلد با "Name" (نام گیرنده) متفاوت است.
        // - UserId: شناسه یکتای کاربر login شده (مثلاً GUID در Identity)
        // - Name: نام گیرنده سفارش (مثلاً "علی رضایی") - ممکن است متفاوت باشد
        // با این فیلد، می‌توانیم آخرین سفارش کاربر را پیدا کنیم حتی اگر
        // نام گیرنده در سفارش‌های مختلف متفاوت باشد.
        [BindNever]
        public string? UserId { get; set; }

        // نام گیرنده - اجباری
        [Required(ErrorMessage = "لطفاً نام را وارد کنید")]
        public string? Name { get; set; }

        // خط اول آدرس - اجباری
        [Required(ErrorMessage = "لطفاً خط اول آدرس را وارد کنید")]
        public string? Line1 { get; set; }
        public string? Line2 { get; set; }
        public string? Line3 { get; set; }

        // شهر - اجباری
        [Required(ErrorMessage = "لطفاً نام شهر را وارد کنید")]
        public string? City { get; set; }

        // استان/ایالت - اجباری
        [Required(ErrorMessage = "لطفاً نام استان را وارد کنید")]
        public string? State { get; set; }

        // کد پستی - اختیاری
        public string? Zip { get; set; }

        // کشور - اجباری
        [Required(ErrorMessage = "لطفاً نام کشور را وارد کنید")]
        public string? Country { get; set; }

        // آیا بسته‌بندی هدیه انجام شود؟
        public bool GiftWrap { get; set; }

        // آیا سفارش ارسال شده است؟
        [BindNever]
        public bool Shipped { get; set; }
    }
}
