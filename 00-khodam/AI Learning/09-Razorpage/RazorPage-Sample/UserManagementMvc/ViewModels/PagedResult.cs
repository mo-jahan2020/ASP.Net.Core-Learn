namespace UserManagementMvc.ViewModels;

// این ViewModel مخصوص صفحه‌بندی است.
// یعنی علاوه بر لیست داده‌ها، اطلاعات لازم برای ساختن Pagination را هم نگه می‌دارد.
public class PagedResult<T>
{
    // آیتم‌های همان صفحه جاری
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

    // شماره صفحه فعلی
    public int CurrentPage { get; set; }

    // تعداد آیتم در هر صفحه
    public int PageSize { get; set; }

    // تعداد کل رکوردها
    public int TotalItems { get; set; }

    // محاسبه تعداد کل صفحه‌ها
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);

    // آیا صفحه قبلی وجود دارد؟
    public bool HasPreviousPage => CurrentPage > 1;

    // آیا صفحه بعدی وجود دارد؟
    public bool HasNextPage => CurrentPage < TotalPages;
}
