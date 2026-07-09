using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SportsStore.Models;

namespace SportsStore.Pages.Products {

    /// <summary>
    /// PageModel صفحه جزئیات محصول (Details)
    /// بر اساس id محصول، آن را از دیتابیس بارگذاری و نمایش می‌دهد.
    /// </summary>
    [Authorize]
    public class DetailsModel : PageModel {

        private readonly IStoreRepository _repository;

        public DetailsModel(IStoreRepository repo) {
            _repository = repo;
        }

        // محصولی که قرار است نمایش داده شود
        public Product? Product { get; set; }

        /// <summary>
        /// بارگذاری محصول بر اساس id
        /// </summary>
        /// <param name="id">شناسه محصول</param>
        public IActionResult OnGet(long? id) {
            if (id == null) {
                return NotFound();
            }

            // جستجوی محصول در دیتابیس
            Product = _repository.Products
                .FirstOrDefault(p => p.ProductID == id);

            if (Product == null) {
                return NotFound();
            }

            return Page();
        }
    }
}
