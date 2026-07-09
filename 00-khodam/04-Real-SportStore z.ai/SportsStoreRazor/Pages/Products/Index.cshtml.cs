using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SportsStore.Models;
using SportsStore.Models.ViewModels;

namespace SportsStore.Pages.Products {

    /// <summary>
    /// PageModel صفحه لیست محصولات (Index)
    /// این صفحه:
    /// - نیاز به ورود کاربر دارد ([Authorize])
    /// - لیست محصولات را با Pagination نمایش می‌دهد
    /// - امکان فیلتر بر اساس دسته‌بندی را دارد
    /// </summary>
    [Authorize] // فقط کاربران وارد شده می‌توانند این صفحه را ببینند
    public class IndexModel : PageModel {

        // Repository محصولات (تزریق شده از طریق DI)
        private readonly IStoreRepository _repository;

        public IndexModel(IStoreRepository repo) {
            _repository = repo;
        }

        // لیست محصولات در صفحه فعلی
        public IEnumerable<Product> Products { get; set; } = Enumerable.Empty<Product>();

        // اطلاعات صفحه‌بندی
        public PagingInfo PagingInfo { get; set; } = new();

        // دسته‌بندی انتخاب شده (null یعنی همه دسته‌ها)
        public string? CurrentCategory { get; set; }

        // لیست همه دسته‌بندی‌ها (برای دکمه‌های فیلتر)
        public IEnumerable<string> Categories { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// نمایش لیست محصولات
        /// </summary>
        /// <param name="productPage">شماره صفحه فعلی</param>
        /// <param name="category">دسته‌بندی انتخاب شده (اختیاری)</param>
        public void OnGet(int? productPage, string? category) {
            // تنظیم تعداد محصولات در هر صفحه
            int pageSize = 4;

            // محاسبه دسته‌بندی‌های موجود (Distinct و مرتب شده)
            Categories = _repository.Products
                .Select(p => p.Category ?? "")
                .Distinct()
                .OrderBy(c => c);

            CurrentCategory = category;

            // نحوه کار:
            // 1) اگر category مشخص بود، فقط محصولات آن دسته را فیلتر می‌کنیم
            // 2) در غیر این صورت، همه محصولات را نشان می‌دهیم
            IQueryable<Product> query = _repository.Products;
            if (!string.IsNullOrEmpty(category)) {
                query = query.Where(p => p.Category == category);
            }

            // محاسبه تعداد کل آیتم‌ها (قبل از Pagination)
            int totalItems = query.Count();

            // محاسبه شماره صفحه فعلی (حداقل 1)
            int currentPage = productPage ?? 1;
            if (currentPage < 1) currentPage = 1;

            // اعمال Pagination روی کوئری
            // Skip: رد کردن آیتم‌های صفحات قبل
            // Take: گرفتن آیتم‌های صفحه فعلی
            // (این عملیات در SQL Server اجرا می‌شود - کارآمد)
            Products = query
                .OrderBy(p => p.ProductID)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // ساخت اطلاعات صفحه‌بندی
            PagingInfo = new PagingInfo {
                CurrentPage = currentPage,
                ItemsPerPage = pageSize,
                TotalItems = totalItems
            };
        }
    }
}
