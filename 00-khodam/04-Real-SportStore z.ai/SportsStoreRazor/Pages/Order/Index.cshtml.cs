using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SportsStore.Models;

namespace SportsStore.Pages.Order {

    /// <summary>
    /// PageModel صفحه لیست سفارشات (Index)
    /// این صفحه:
    /// - نیاز به ورود کاربر دارد ([Authorize])
    /// - لیست سفارشات را با Pagination نمایش می‌دهد
    /// - امکان علامت‌گذاری سفارش به عنوان "ارسال شده" را دارد
    /// </summary>
    [Authorize]
    public class IndexModel : PageModel {

        private readonly IOrderRepository _repository;
        // برای عملیات MarkShipped به DbContext مستقیم نیاز داریم
        private readonly StoreDbContext _dbContext;

        public IndexModel(IOrderRepository repo, StoreDbContext dbContext) {
            _repository = repo;
            _dbContext = dbContext;
        }

        // لیست سفارشات در صفحه فعلی
        // نکته: نام کلاس Order با namespace SportsStore.Pages.Order تداخل دارد
        // بنابراین از نام کامل SportsStore.Models.Order استفاده می‌کنیم
        public IEnumerable<SportsStore.Models.Order> Orders { get; set; }
            = Enumerable.Empty<SportsStore.Models.Order>();

        // اطلاعات صفحه‌بندی
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalItems { get; set; } = 0;

        // تعداد آیتم در هر صفحه
        private const int PageSize = 5;

        /// <summary>
        /// نمایش لیست سفارشات با Pagination
        /// </summary>
        /// <param name="pageNumber">شماره صفحه فعلی</param>
        public void OnGet(int? pageNumber) {
            // محاسبه شماره صفحه فعلی (حداقل 1)
            CurrentPage = pageNumber ?? 1;
            if (CurrentPage < 1) CurrentPage = 1;

            // محاسبه تعداد کل سفارشات
            TotalItems = _repository.Orders.Count();

            // محاسبه تعداد کل صفحات
            TotalPages = (int)Math.Ceiling((decimal)TotalItems / PageSize);
            if (CurrentPage > TotalPages && TotalPages > 0) {
                CurrentPage = TotalPages;
            }

            // بارگذاری سفارشات صفحه فعلی با Pagination
            // Skip: رد کردن صفحات قبل
            // Take: گرفتن آیتم‌های صفحه فعلی
            Orders = _repository.Orders
                .OrderByDescending(o => o.OrderID) // جدیدترین اول
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        /// <summary>
        /// علامت‌گذاری سفارش به عنوان "ارسال شده"
        ///
        /// نکته فنی ۱: به جای SaveOrder، از DbContext مستقیم استفاده می‌کنیم.
        /// دلیل: SaveOrder برای سفارش موجود Update کامل را انجام می‌دهد که
        /// ممکن است باعث خطا شود. ما فقط فیلد Shipped را آپدیت می‌کنیم.
        ///
        /// نکته فنی ۲ (امنیت - Defense in Depth):
        /// در Razor Pages، ویژگی [Authorize] نمی‌تواند روی یک متد Handler اعمال شود
        /// (فقط روی کل PageModel یا Global). به جای آن، در ابتدای متد با
        /// User.IsInRole("Admin") بررسی می‌کنیم. اگر کاربر Admin نبود،
        /// به صفحه "دسترسی ممنوع" هدایت می‌شود.
        /// این الگو "Defense in Depth" نامیده می‌شود - هم در UI دکمه را پنهان
        /// می‌کنیم و هم در backend دسترسی را محدود می‌کنیم.
        /// </summary>
        public IActionResult OnPostMarkShipped(int id) {
            // --- بررسی امنیتی: فقط Admin می‌تواند سفارش را ارسال شده علامت بزند ---
            if (!User.IsInRole("Admin")) {
                TempData["ErrorMessage"] = "شما اجازه انجام این عملیات را ندارید.";
                return RedirectToPage("/Account/AccessDenied");
            }

            // بارگذاری سفارش از دیتابیس (بدون Lines - چون فقط Shipped را تغییر می‌دهیم)
            var order = _dbContext.Orders.FirstOrDefault(o => o.OrderID == id);
            if (order != null) {
                order.Shipped = true;
                _dbContext.SaveChanges();
                TempData["SuccessMessage"] =
                    $"سفارش شماره {id} به عنوان ارسال شده علامت‌گذاری شد.";
            }
            return RedirectToPage();
        }
    }
}
