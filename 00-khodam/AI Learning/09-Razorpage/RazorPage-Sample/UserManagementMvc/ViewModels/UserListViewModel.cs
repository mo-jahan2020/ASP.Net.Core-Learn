namespace UserManagementMvc.ViewModels;

// این ViewModel تمام داده‌های لازم برای صفحه لیست را نگه می‌دارد.
public class UserListViewModel
{
    public IReadOnlyList<UserListItemViewModel> Users { get; set; } = Array.Empty<UserListItemViewModel>();

    public UserFilterViewModel Filter { get; set; } = new();

    public IReadOnlyList<string> AvailableCities { get; set; } = Array.Empty<string>();

    public int TotalItems { get; set; }

    public int TotalPages => TotalItems == 0
        ? 1
        : (int)Math.Ceiling(TotalItems / (double)Filter.PageSize);

    public bool HasPreviousPage => Filter.Page > 1;

    public bool HasNextPage => Filter.Page < TotalPages;
}
