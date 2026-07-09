using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SportsStore.Models;

namespace SportsStore.Pages.Products {

    /// <summary>
    /// PageModel صفحه حذف محصول (Delete)
    /// فقط کاربران با نقش Admin اجازه دسترسی دارند.
    /// - OnGet: نمایش اطلاعات محصول برای تأیید حذف
    /// - OnPost: حذف محصول از دیتابیس
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel {

        private readonly IStoreRepository _repository;

        public DeleteModel(IStoreRepository repo) {
            _repository = repo;
        }

        [BindProperty]
        public Product? Product { get; set; }

        /// <summary>
        /// نمایش صفحه تأیید حذف
        /// </summary>
        public IActionResult OnGet(long? id) {
            if (id == null) {
                return NotFound();
            }

            Product = _repository.Products.FirstOrDefault(p => p.ProductID == id);
            if (Product == null) {
                return NotFound();
            }

            return Page();
        }

        /// <summary>
        /// حذف محصول پس از تأیید کاربر
        /// </summary>
        public IActionResult OnPost() {
            if (Product?.ProductID == null) {
                return NotFound();
            }

            // پیدا کردن محصول واقعی از دیتابیس
            var productToDelete = _repository.Products
                .FirstOrDefault(p => p.ProductID == Product.ProductID);

            if (productToDelete != null) {
                _repository.DeleteProduct(productToDelete);
                TempData["SuccessMessage"] =
                    $"محصول «{productToDelete.Name}» با موفقیت حذف شد.";
            }

            return RedirectToPage("Index");
        }
    }
}
