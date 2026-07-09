namespace SportsStore.Models.ViewModels {

    /// <summary>
    /// ViewModel برای لیست محصولات
    /// شامل لیست محصولات، اطلاعات صفحه‌بندی و دسته‌بندی فعلی
    /// </summary>
    public class ProductsListViewModel {
        // لیست محصولات در صفحه فعلی
        public IEnumerable<Product> Products { get; set; }
            = Enumerable.Empty<Product>();

        // اطلاعات صفحه‌بندی
        public PagingInfo PagingInfo { get; set; } = new();

        // دسته‌بندی انتخاب شده (null یعنی همه دسته‌ها)
        public string? CurrentCategory { get; set; }
    }
}
