using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SportsStore.Models;

namespace SportsStore.Pages.Products {

    /// <summary>
    /// PageModel صفحه ایجاد محصول جدید (Create)
    /// فقط کاربران با نقش "Admin" اجازه دسترسی به این صفحه را دارند.
    /// این محدودیت با ویژگی [Authorize(Roles="Admin")] اعمال می‌شود.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel {

        private readonly IStoreRepository _repository;

        public CreateModel(IStoreRepository repo) {
            _repository = repo;
        }

        // محصول جدیدی که از فرم پر می‌شود
        [BindProperty]
        public Product Product { get; set; } = new();

        // لیست دسته‌بندی‌های موجود (برای پیشنهاد در Input)
        public IEnumerable<string> ExistingCategories { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// نمایش فرم ایجاد محصول
        /// </summary>
        public void OnGet() {
            // بارگذاری دسته‌بندی‌های موجود برایdatalist
            ExistingCategories = _repository.Products
                .Select(p => p.Category ?? "")
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }

        /// <summary>
        /// پردازش فرم ایجاد محصول (پس از Submit)
        /// </summary>
        public IActionResult OnPost() {
            // اعتبارسنجی مدل
            if (!ModelState.IsValid) {
                // در صورت خطا، لیست دسته‌بندی‌ها را دوباره پر کن
                ExistingCategories = _repository.Products
                    .Select(p => p.Category ?? "")
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();
                return Page();
            }

            // ایجاد محصول در دیتابیس
            _repository.CreateProduct(Product);

            // نمایش پیام موفقیت
            TempData["SuccessMessage"] = $"محصول «{Product.Name}» با موفقیت ایجاد شد.";

            // هدایت به صفحه لیست محصولات
            return RedirectToPage("Index");
        }
    }
}
