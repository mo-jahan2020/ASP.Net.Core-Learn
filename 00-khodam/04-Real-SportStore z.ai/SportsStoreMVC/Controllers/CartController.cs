using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsStore.Models;

namespace SportsStore.Controllers {

    /// <summary>
    /// Controller سبد خرید (Cart)
    /// مسئولیت: مدیریت سبد خرید مبتنی بر Session
    ///
    /// مقایسه با Razor Pages:
    /// - در Razor Pages: Cart.cshtml.cs با Handler های OnGet، OnGetAdd، OnGetRemove، OnPostClear
    /// - در MVC: CartController با اکشن‌های Index، Add، Remove، Clear
    ///
    /// مسیرها:
    /// - GET /Cart                   → Index()       نمایش سبد خرید
    /// - GET /Cart/Add?id=1&returnUrl=/Products  → Add(1, "/Products")  افزودن محصول
    /// - GET /Cart/Remove?id=1       → Remove(1)     حذف محصول از سبد
    /// - POST /Cart/Clear            → Clear()       خالی کردن کامل سبد
    ///
    /// نکته: Add و Remove از GET استفاده می‌کنند (نه POST) چون:
    /// 1) فقط Session را تغییر می‌دهند، نه دیتابیس را
    /// 2) در MVC با Razor Views، فرم POST به یک Controller Action با Antiforgery Token
    ///    به درستی کار می‌کند (برخلاف Razor Pages که Cross-page POST مشکل دارد)
    /// 3) اما برای سادگی و URL‌های تمیزتر، از GET استفاده می‌کنیم
    /// </summary>
    [Authorize]
    public class CartController : Controller {

        private readonly IStoreRepository _repository;
        private readonly Cart _cart;

        // Cart از DI تزریق می‌شود (Scoped - یک نمونه به ازای هر کاربر)
        public CartController(IStoreRepository repo, Cart cartService) {
            _repository = repo;
            _cart = cartService;
        }

        /// <summary>
        /// نمایش محتوای سبد خرید (GET /Cart)
        /// </summary>
        /// <param name="returnUrl">آدرس بازگشت برای دکمه "ادامه خرید"</param>
        public IActionResult Index(string? returnUrl = "/") {
            ViewBag.ReturnUrl = returnUrl ?? "/Products";
            return View(_cart);
        }

        /// <summary>
        /// افزودن محصول به سبد (GET /Cart/Add?id=1&returnUrl=/Products)
        ///
        /// نکته: در MVC می‌توانستیم از [HttpPost] و فرم هم استفاده کنیم،
        /// اما GET ساده‌تر است و برای کاربر امکان Bookmark کردن را فراهم می‌کند.
        /// </summary>
        public IActionResult Add(long id, string? returnUrl = "/") {
            Product? product = _repository.Products
                .FirstOrDefault(p => p.ProductID == id);

            if (product != null) {
                _cart.AddItem(product, 1);
            }

            return RedirectToAction(nameof(Index), new { returnUrl });
        }

        /// <summary>
        /// حذف محصول از سبد (GET /Cart/Remove?id=1)
        /// </summary>
        public IActionResult Remove(long id, string? returnUrl = "/") {
            Product? product = _repository.Products
                .FirstOrDefault(p => p.ProductID == id);

            if (product != null) {
                _cart.RemoveLine(product);
            }

            return RedirectToAction(nameof(Index), new { returnUrl });
        }

        /// <summary>
        /// خالی کردن کامل سبد (POST /Cart/Clear)
        /// از POST استفاده می‌کنیم چون تغییر قابل توجهی است
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear() {
            _cart.Clear();
            return RedirectToAction(nameof(Index));
        }
    }
}
