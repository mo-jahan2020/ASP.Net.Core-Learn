namespace SportsStore.Models.ViewModels {

    /// <summary>
    /// ViewModel برای داشبورد گزارش‌ها
    /// شامل کارت‌های آماری، داده‌های نمودار و لیست پرفروش‌ترین محصولات
    /// </summary>
    public class ReportsViewModel {

        // --- مقادیر کارت‌های آماری ---
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingOrders { get; set; }
        public int ShippedOrders { get; set; }

        // --- داده‌های نمودار فروش بر اساس دسته‌بندی ---
        public string CategoryLabelsJson { get; set; } = "[]";
        public string CategoryDataJson { get; set; } = "[]";

        // --- داده‌های نمودار روند روزانه ---
        public string DailyLabelsJson { get; set; } = "[]";
        public string DailyDataJson { get; set; } = "[]";

        // --- لیست پرفروش‌ترین محصولات ---
        public List<TopProductItem> TopProducts { get; set; } = new();
    }

    /// <summary>
    /// آیتم پرفروش‌ترین محصول
    /// </summary>
    public class TopProductItem {
        public string ProductName { get; set; } = "";
        public string Category { get; set; } = "";
        public int TotalSold { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
