using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SportsStore.Models;

namespace SportsStore.Pages.Order {

    /// <summary>
    /// PageModel صفحه جزئیات سفارش (Details)
    /// بر اساس id سفارش، آن را به همراه خطوط و محصولات نمایش می‌دهد.
    /// </summary>
    [Authorize]
    public class DetailsModel : PageModel {

        private readonly IOrderRepository _repository;
        private readonly StoreDbContext _dbContext;

        public DetailsModel(IOrderRepository repo, StoreDbContext dbContext) {
            _repository = repo;
            _dbContext = dbContext;
        }

        // سفارشی که قرار است نمایش داده شود
        public SportsStore.Models.Order? Order { get; set; }

        /// <summary>
        /// بارگذاری سفارش بر اساس id
        /// </summary>
        public IActionResult OnGet(int id) {
            // جستجوی سفارش (به همراه خطوط و محصولات - Eager Loading)
            Order = _repository.Orders.FirstOrDefault(o => o.OrderID == id);

            if (Order == null) {
                return NotFound();
            }

            return Page();
        }

        /// <summary>
        /// علامت‌گذاری سفارش به عنوان ارسال شده
        ///
        /// نکته امنیتی (Defense in Depth):
        /// این handler فقط برای کاربران با نقش Admin قابل دسترسی است.
        /// در Razor Pages نمی‌توان [Authorize] را روی یک متد گذاشت، پس
        /// با User.IsInRole("Admin") بررسی دستی انجام می‌دهیم.
        /// هم در UI دکمه پنهان می‌شود و هم در backend دسترسی محدود می‌شود.
        /// </summary>
        public IActionResult OnPostMarkShipped(int id) {
            // --- بررسی امنیتی: فقط Admin می‌تواند وضعیت را تغییر دهد ---
            if (!User.IsInRole("Admin")) {
                TempData["ErrorMessage"] = "شما اجازه انجام این عملیات را ندارید.";
                return RedirectToPage("/Account/AccessDenied");
            }

            // بارگذاری سفارش از دیتابیس (بدون Lines)
            var order = _dbContext.Orders.FirstOrDefault(o => o.OrderID == id);
            if (order != null) {
                order.Shipped = true;
                _dbContext.SaveChanges();
                TempData["SuccessMessage"] =
                    $"سفارش شماره {id} به عنوان ارسال شده علامت‌گذاری شد.";
            }
            return RedirectToPage("Details", new { id });
        }
    }
}
