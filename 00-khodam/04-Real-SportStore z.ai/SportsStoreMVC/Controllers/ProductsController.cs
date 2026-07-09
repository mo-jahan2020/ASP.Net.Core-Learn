using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsStore.Models;
using SportsStore.Models.ViewModels;

namespace SportsStore.Controllers {

    /// <summary>
    /// Controller محصولات (Products)
    /// مسئولیت: نمایش لیست محصولات، جزئیات، ایجاد، ویرایش و حذف
    ///
    /// مقایسه با Razor Pages:
    /// - در Razor Pages: پوشه Products/ با ۵ صفحه Index, Details, Create, Edit, Delete
    /// - در MVC: یک ProductsController با ۵ اکشن (هر کدام GET و POST جداگانه)
    ///
    /// مسیرها (با Route پیش‌فرض {controller=Home}/{action=Index}/{id?}):
    /// - GET /Products            → Index()       لیست محصولات با Pagination
    /// - GET /Products/Details/5  → Details(5)    جزئیات محصول
    /// - GET /Products/Create     → Create()      فرم ایجاد (فقط Admin)
    /// - POST /Products/Create    → Create(Product) ذخیره محصول جدید
    /// - GET /Products/Edit/5     → Edit(5)       فرم ویرایش (فقط Admin)
    /// - POST /Products/Edit/5    → Edit(5, Product) ذخیره تغییرات
    /// - GET /Products/Delete/5   → Delete(5)     تأیید حذف (فقط Admin)
    /// - POST /Products/Delete/5  → DeleteConfirmed(5) حذف واقعی
    /// </summary>
    [Authorize]  // تمام اکشن‌های این Controller نیاز به ورود دارند
    public class ProductsController : Controller {

        private readonly IStoreRepository _repository;

        public ProductsController(IStoreRepository repo) {
            _repository = repo;
        }

        /// <summary>
        /// نمایش لیست محصولات با Pagination و فیلتر دسته‌بندی
        /// (GET /Products یا /Products?productPage=2&category=Soccer)
        /// </summary>
        public IActionResult Index(int? productPage, string? category) {
            int pageSize = 6;

            // محاسبه دسته‌بندی‌های موجود برای دکمه‌های فیلتر
            var categories = _repository.Products
                .Select(p => p.Category ?? "")
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            // فیلتر بر اساس دسته‌بندی
            IQueryable<Product> query = _repository.Products;
            if (!string.IsNullOrEmpty(category)) {
                query = query.Where(p => p.Category == category);
            }

            int totalItems = query.Count();
            int currentPage = productPage ?? 1;
            if (currentPage < 1) currentPage = 1;

            // اعمال Pagination (Skip + Take در SQL اجرا می‌شود)
            var products = query
                .OrderBy(p => p.ProductID)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // ساخت ViewModel برای View
            var viewModel = new ProductsListViewModel {
                Products = products,
                PagingInfo = new PagingInfo {
                    CurrentPage = currentPage,
                    ItemsPerPage = pageSize,
                    TotalItems = totalItems
                },
                CurrentCategory = category
            };

            ViewBag.Categories = categories;
            return View(viewModel);
        }

        /// <summary>
        /// نمایش جزئیات یک محصول (GET /Products/Details/5)
        /// </summary>
        /// <param name="id">شناسه محصول</param>
        public IActionResult Details(long? id) {
            if (id == null) return NotFound();

            Product? product = _repository.Products
                .FirstOrDefault(p => p.ProductID == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // --------------------------------------------------------------------
        // CREATE (ایجاد محصول - فقط Admin)
        // --------------------------------------------------------------------

        /// <summary>
        /// نمایش فرم ایجاد محصول (GET /Products/Create)
        /// فقط کاربران با نقش Admin می‌توانند این صفحه را ببینند
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create() {
            // ارسال لیست دسته‌بندی‌های موجود به View (برای datalist)
            ViewBag.ExistingCategories = _repository.Products
                .Select(p => p.Category ?? "")
                .Distinct()
                .OrderBy(c => c)
                .ToList();
            return View();
        }

        /// <summary>
        /// پردازش فرم ایجاد محصول (POST /Products/Create)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product) {
            if (!ModelState.IsValid) {
                ViewBag.ExistingCategories = _repository.Products
                    .Select(p => p.Category ?? "")
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();
                return View(product);
            }

            _repository.CreateProduct(product);
            TempData["SuccessMessage"] = $"محصول «{product.Name}» با موفقیت ایجاد شد.";
            return RedirectToAction(nameof(Index));
        }

        // --------------------------------------------------------------------
        // EDIT (ویرایش محصول - فقط Admin)
        // --------------------------------------------------------------------

        /// <summary>
        /// نمایش فرم ویرایش محصول (GET /Products/Edit/5)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Edit(long? id) {
            if (id == null) return NotFound();

            Product? product = _repository.Products.FirstOrDefault(p => p.ProductID == id);
            if (product == null) return NotFound();

            ViewBag.ExistingCategories = _repository.Products
                .Select(p => p.Category ?? "")
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            return View(product);
        }

        /// <summary>
        /// پردازش فرم ویرایش محصول (POST /Products/Edit/5)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(long id, Product product) {
            if (id != product.ProductID) return NotFound();

            if (!ModelState.IsValid) {
                ViewBag.ExistingCategories = _repository.Products
                    .Select(p => p.Category ?? "")
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();
                return View(product);
            }

            _repository.SaveProduct(product);
            TempData["SuccessMessage"] = $"محصول «{product.Name}» با موفقیت به‌روزرسانی شد.";
            return RedirectToAction(nameof(Index));
        }

        // --------------------------------------------------------------------
        // DELETE (حذف محصول - فقط Admin)
        // --------------------------------------------------------------------

        /// <summary>
        /// نمایش صفحه تأیید حذف (GET /Products/Delete/5)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Delete(long? id) {
            if (id == null) return NotFound();

            Product? product = _repository.Products.FirstOrDefault(p => p.ProductID == id);
            if (product == null) return NotFound();

            return View(product);
        }

        /// <summary>
        /// حذف واقعی محصول پس از تأیید کاربر (POST /Products/Delete/5)
        /// نام متد DeleteConfirmed است تا با اکشن GET Delete تداخل نداشته باشد
        /// (در MVC دو متد هم‌نام با پارامترهای متفاوت مجاز است، اما برای وضوح
        /// از ActionName استفاده می‌کنیم تا URL همچنان /Delete باشد)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(long id) {
            Product? product = _repository.Products.FirstOrDefault(p => p.ProductID == id);
            if (product != null) {
                _repository.DeleteProduct(product);
                TempData["SuccessMessage"] = $"محصول «{product.Name}» با موفقیت حذف شد.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
