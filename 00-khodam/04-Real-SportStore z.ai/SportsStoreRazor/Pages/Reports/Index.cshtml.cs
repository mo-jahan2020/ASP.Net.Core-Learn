using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SportsStore.Models;
using System.Text.Json;

namespace SportsStore.Pages.Reports {

    /// <summary>
    /// PageModel صفحه گزارش‌ها و داشبورد (Index)
    /// این صفحه:
    /// - فقط برای کاربران با نقش Admin قابل دسترس است
    /// - کارت‌های آماری نشان می‌دهد (تعداد محصولات، سفارشات، درآمد)
    /// - نمودارهای میله‌ای، دایره‌ای و خطی با Chart.js نمایش می‌دهد
    /// - جدول پرفروش‌ترین محصولات را نشان می‌دهد
    /// داده‌های نمودار به صورت JSON به View ارسال می‌شوند تا Chart.js بتواند بخواند.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel {

        private readonly IStoreRepository  _storeRepo;
        private readonly IOrderRepository  _orderRepo;

        public IndexModel(IStoreRepository storeRepo, IOrderRepository orderRepo) {
            _storeRepo = storeRepo;
            _orderRepo = orderRepo;
        }

        // --- مقادیر کارت‌های آماری ---
        public int TotalProducts { get; set; }
        public int TotalOrders   { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingOrders { get; set; }
        public int ShippedOrders { get; set; }

        // --- داده‌های نمودار فروش بر اساس دسته‌بندی ---
        public string CategoryLabelsJson { get; set; } = "[]";
        public string CategoryDataJson   { get; set; } = "[]";

        // --- داده‌های نمودار روند روزانه ---
        public string DailyLabelsJson { get; set; } = "[]";
        public string DailyDataJson   { get; set; } = "[]";

        // --- لیست پرفروش‌ترین محصولات ---
        public List<TopProductItem> TopProducts { get; set; } = new();

        /// <summary>
        /// بارگذاری داده‌ها برای داشبورد
        /// </summary>
        public void OnGet() {
            // --- محاسبه مقادیر کارت‌های آماری ---
            TotalProducts = _storeRepo.Products.Count();

            var allOrders = _orderRepo.Orders.ToList();
            TotalOrders   = allOrders.Count;
            PendingOrders = allOrders.Count(o => !o.Shipped);
            ShippedOrders = allOrders.Count(o => o.Shipped);

            // محاسبه درآمد کل (مجموع قیمت همه آیتم‌های همه سفارشات)
            TotalRevenue = allOrders
                .SelectMany(o => o.Lines)
                .Sum(l => l.Product.Price * l.Quantity);

            // --- داده‌های نمودار دسته‌بندی ---
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

            // تبدیل به JSON برای ارسال به Chart.js
            CategoryLabelsJson = JsonSerializer.Serialize(
                categorySales.Select(x => x.Category).ToList());
            CategoryDataJson   = JsonSerializer.Serialize(
                categorySales.Select(x => x.Count).ToList());

            // --- داده‌های نمودار روند روزانه (۵ روز اخیر) ---
            var fiveDaysAgo = DateTime.Now.AddDays(-4).Date;
            var dailyOrders = allOrders
                .Where(o => true) // در آینده می‌توان تاریخ سفارش را اضافه کرد
                .ToList();

            // چون مدل Order فیلد تاریخ ندارد، نمودار روزانه را بر اساس
            // ۵ روز اخیر شبیه‌سازی می‌کنیم (در پروژه واقعی باید OrderDate اضافه شود)
            var dailyData = new List<int>();
            var dailyLabels = new List<string>();
            for (int i = 4; i >= 0; i--) {
                var day = DateTime.Now.AddDays(-i);
                dailyLabels.Add(day.ToString("MM/dd"));
                // برای نمونه: تعداد سفارشات در آن روز (در این مدل ساده، همه را توزیع می‌کنیم)
                dailyData.Add(0);
            }
            // توزیع سفارشات بین ۵ روز (چون تاریخ نداریم)
            for (int i = 0; i < allOrders.Count; i++) {
                dailyData[i % 5]++;
            }

            DailyLabelsJson = JsonSerializer.Serialize(dailyLabels);
            DailyDataJson   = JsonSerializer.Serialize(dailyData);

            // --- پرفروش‌ترین محصولات ---
            TopProducts = allOrders
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
        }
    }

    /// <summary>
    /// کلاس ViewModel برای نمایش پرفروش‌ترین محصولات
    /// </summary>
    public class TopProductItem {
        public string ProductName  { get; set; } = "";
        public string Category     { get; set; } = "";
        public int    TotalSold    { get; set; }
        public decimal UnitPrice   { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
