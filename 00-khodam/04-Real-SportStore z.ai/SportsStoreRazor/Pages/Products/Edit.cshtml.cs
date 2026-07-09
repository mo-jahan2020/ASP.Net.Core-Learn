using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SportsStore.Models;

namespace SportsStore.Pages.Products {

    /// <summary>
    /// PageModel صفحه ویرایش محصول (Edit)
    /// فقط کاربران با نقش Admin اجازه دسترسی دارند.
    /// - OnGet: بارگذاری اطلاعات محصول در فرم
    /// - OnPost: ذخیره تغییرات
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel {

        private readonly IStoreRepository _repository;

        public EditModel(IStoreRepository repo) {
            _repository = repo;
        }

        [BindProperty]
        public Product Product { get; set; } = new();

        public IEnumerable<string> ExistingCategories { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// بارگذاری اطلاعات محصول در فرم
        /// </summary>
        public IActionResult OnGet(long? id) {
            if (id == null) {
                return NotFound();
            }

            // جستجوی محصول
            Product = _repository.Products.FirstOrDefault(p => p.ProductID == id)
                      ?? new Product();

            if (Product.ProductID == null) {
                return NotFound();
            }

            // بارگذاری دسته‌بندی‌ها
            ExistingCategories = _repository.Products
                .Select(p => p.Category ?? "")
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            return Page();
        }

        /// <summary>
        /// ذخیره تغییرات محصول
        /// </summary>
        public IActionResult OnPost() {
            if (!ModelState.IsValid) {
                ExistingCategories = _repository.Products
                    .Select(p => p.Category ?? "")
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();
                return Page();
            }

            // به‌روزرسانی محصول در دیتابیس
            // EF تغییرات را Track می‌کند، فقط SaveChanges لازم است
            _repository.SaveProduct(Product);

            TempData["SuccessMessage"] = $"محصول «{Product.Name}» با موفقیت به‌روزرسانی شد.";

            return RedirectToPage("Index");
        }
    }
}
