using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SportsStore.Models;

namespace SportsStore.Pages {

    /// <summary>
    /// PageModel صفحه سبد خرید (Cart)
    ///
    /// این PageModel دو نوع Handler دارد:
    ///
    /// 1) Handlerهای GET:
    ///    - OnGet: نمایش سبد خرید
    ///    - OnGetAdd: افزودن محصول به سبد (از لینک در صفحه محصولات فراخوانی می‌شود)
    ///    - OnGetRemove: حذف محصول از سبد (از لینک در صفحه سبد فراخوانی می‌شود)
    ///
    /// 2) Handlerهای POST:
    ///    - OnPostClear: خالی کردن کامل سبد (از فرم فراخوانی می‌شود)
    ///
    /// نکته فنی: چرا برای Add و Remove از GET استفاده می‌کنیم نه POST؟
    /// چون فرم POST از صفحه محصولات به صفحه سبد (Cross-page POST) به دلیل
    /// Antiforgery Token در Razor Pages پیچیده است. GET ساده‌تر و امن است
    /// (چون فقط Session را تغییر می‌دهد، نه دیتابیس را).
    /// </summary>
    public class CartModel : PageModel {

        private readonly IStoreRepository _repository;

        // سبد خرید از DI تزریق می‌شود (به ازای هر کاربر یک نمونه)
        public CartModel(IStoreRepository repo, Cart cartService) {
            _repository = repo;
            Cart = cartService;
        }

        // سبد خرید فعلی کاربر
        public Cart Cart { get; set; }

        // آدرس بازگشت (برای دکمه "ادامه خرید")
        public string ReturnUrl { get; set; } = "/";

        /// <summary>
        /// نمایش سبد خرید (GET /Cart)
        /// </summary>
        public void OnGet(string? returnUrl) {
            ReturnUrl = returnUrl ?? "/Products";
        }

        /// <summary>
        /// افزودن محصول به سبد (GET /Cart?handler=Add&id=...&returnUrl=...)
        ///
        /// از لینک در صفحه Products/Index یا Products/Details فراخوانی می‌شود.
        /// استفاده از GET به جای POST باعث می‌شود نیازی به Antiforgery Token نباشد
        /// و کاربر بتواند با Back/Forward مرورگر به راحتی کار کند.
        /// </summary>
        /// <param name="id">شناسه محصول (همان productId)</param>
        /// <param name="returnUrl">آدرس بازگشت (اختیاری)</param>
        public IActionResult OnGetAdd(long id, string? returnUrl) {
            // جستجوی محصول در دیتابیس
            Product? product = _repository.Products
                .FirstOrDefault(p => p.ProductID == id);

            if (product != null) {
                // افزودن به سبد (تعداد 1)
                // Cart به صورت Scoped ثبت شده، پس در Session ذخیره می‌شود
                Cart.AddItem(product, 1);
            }

            // هدایت به صفحه سبد خرید
            return RedirectToPage("/Cart", new { returnUrl = returnUrl ?? "/Products" });
        }

        /// <summary>
        /// حذف محصول از سبد (GET /Cart?handler=Remove&id=...)
        ///
        /// از لینک در صفحه Cart خودش فراخوانی می‌شود.
        /// </summary>
        public IActionResult OnGetRemove(long id, string? returnUrl) {
            Product? product = _repository.Products
                .FirstOrDefault(p => p.ProductID == id);

            if (product != null) {
                Cart.RemoveLine(product);
            }

            return RedirectToPage("/Cart", new { returnUrl = returnUrl ?? "/Products" });
        }

        /// <summary>
        /// خالی کردن کامل سبد (POST /Cart?handler=Clear)
        ///
        /// از فرم در همان صفحه Cart فراخوانی می‌شود.
        /// چون در همان صفحه است، Antiforgery Token به درستی کار می‌کند.
        /// </summary>
        public IActionResult OnPostClear() {
            Cart.Clear();
            return RedirectToPage("/Cart");
        }
    }
}
