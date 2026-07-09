using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsStore.Models;
using SportsStore.Models.ViewModels;
using System.Text.Json;

namespace SportsStore.Controllers {

    /// <summary>
    /// Controller گزارش‌ها (Reports)
    /// مسئولیت: نمایش داشبورد آماری و نمودارها
    ///
    /// فقط کاربران با نقش Admin می‌توانند به این Controller دسترسی داشته باشند.
    /// با قرار دادن [Authorize(Roles = "Admin")] در سطح Controller، تمام اکشن‌ها
    /// محافظت می‌شوند.
    ///
    /// مسیرها:
    /// - GET /Reports        → Index()   نمایش داشبورد
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller {

        private readonly IStoreRepository _storeRepo;
        private readonly IOrderRepository _orderRepo;

        public ReportsController(IStoreRepository storeRepo,
                                 IOrderRepository orderRepo) {
            _storeRepo = storeRepo;
            _orderRepo = orderRepo;
        }

        /// <summary>
        /// نمایش داشبورد گزارش‌ها (GET /Reports)
        /// شامل: کارت‌های آماری، نمودارها، جدول پرفروش‌ترین محصولات
        /// </summary>
        public IActionResult Index() {
            // --- محاسبه مقادیر کارت‌های آماری ---
            var allOrders = _orderRepo.Orders.ToList();

            var viewModel = new ReportsViewModel {
                TotalProducts = _storeRepo.Products.Count(),
                TotalOrders   = allOrders.Count,
                PendingOrders = allOrders.Count(o => !o.Shipped),
                ShippedOrders = allOrders.Count(o => o.Shipped),

                // محاسبه درآمد کل (مجموع قیمت همه آیتم‌های همه سفارشات)
                TotalRevenue = allOrders
                    .SelectMany(o => o.Lines)
                    .Sum(l => l.Product.Price * l.Quantity)
            };

            // --- داده‌های نمودار فروش بر اساس دسته‌بندی ---
            // گروه‌بندی محصولات فروخته شده بر اساس دسته‌بندی
            var categorySales = allOrders
                .SelectMany(o => o.Lines)
                .GroupBy(l => l.Product.Category ?? "بدون دسته")
                .Select(g => new {
                    Category = g.Key,
                    Count    = g.Sum(l => l.Quantity)
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            // تبدیل به JSON برای ارسال به Chart.js در View
            viewModel.CategoryLabelsJson = JsonSerializer.Serialize(
                categorySales.Select(x => x.Category).ToList());
            viewModel.CategoryDataJson = JsonSerializer.Serialize(
                categorySales.Select(x => x.Count).ToList());

            // --- داده‌های نمودار روند روزانه (۵ روز اخیر) ---
            // چون مدل Order فیلد تاریخ ثبت سفارش ندارد، نمودار روزانه را
            // به صورت نمونه می‌سازیم (در پروژه واقعی باید OrderDate اضافه شود)
            var dailyData = new List<int>();
            var dailyLabels = new List<string>();
            for (int i = 4; i >= 0; i--) {
                var day = DateTime.Now.AddDays(-i);
                dailyLabels.Add(day.ToString("MM/dd"));
                dailyData.Add(0);
            }
            // توزیع سفارشات بین ۵ روز (چون تاریخ نداریم)
            for (int i = 0; i < allOrders.Count; i++) {
                dailyData[i % 5]++;
            }

            viewModel.DailyLabelsJson = JsonSerializer.Serialize(dailyLabels);
            viewModel.DailyDataJson   = JsonSerializer.Serialize(dailyData);

            // --- پرفروش‌ترین محصولات ---
            viewModel.TopProducts = allOrders
                .SelectMany(o => o.Lines)
                .GroupBy(l => new {
                    l.Product.ProductID,
                    l.Product.Name,
                    l.Product.Category,
                    l.Product.Price
                })
                .Select(g => new TopProductItem {
                    ProductName  = g.Key.Name ?? "",
                    Category     = g.Key.Category ?? "",
                    TotalSold    = g.Sum(l => l.Quantity),
                    UnitPrice    = g.Key.Price,
                    TotalRevenue = g.Sum(l => l.Product.Price * l.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(10)
                .ToList();

            return View(viewModel);
        }
    }
}
