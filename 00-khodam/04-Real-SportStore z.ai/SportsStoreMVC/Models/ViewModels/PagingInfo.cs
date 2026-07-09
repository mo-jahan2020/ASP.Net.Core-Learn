namespace SportsStore.Models.ViewModels {

    /// <summary>
    /// اطلاعات صفحه‌بندی
    /// برای ساخت لینک‌های صفحه‌بندی در View استفاده می‌شود.
    /// </summary>
    public class PagingInfo {
        // تعداد کل آیتم‌ها
        public int TotalItems { get; set; }

        // تعداد آیتم در هر صفحه
        public int ItemsPerPage { get; set; }

        // شماره صفحه فعلی
        public int CurrentPage { get; set; }

        // محاسبه تعداد کل صفحات
        public int TotalPages =>
            (int)Math.Ceiling((decimal)TotalItems / ItemsPerPage);
    }
}
