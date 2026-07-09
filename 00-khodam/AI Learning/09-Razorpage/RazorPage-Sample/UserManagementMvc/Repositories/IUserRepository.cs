using UserManagementMvc.ViewModels;

namespace UserManagementMvc.Repositories;

// این Interface قرارداد دسترسی به داده‌های کاربران را مشخص می‌کند.
// مزیت این کار این است که کنترلر فقط با قرارداد کار می‌کند، نه با پیاده‌سازی مستقیم دیتابیس.
public interface IUserRepository
{
    Task<UserListViewModel> GetPagedUsersAsync(UserFilterViewModel filter);
    Task<UserDetailsViewModel?> GetDetailsAsync(int id);
    Task<UserEditViewModel?> GetForEditAsync(int id);
    Task<UserDeleteViewModel?> GetForDeleteAsync(int id);
    Task CreateAsync(UserCreateViewModel model);
    Task<bool> UpdateAsync(UserEditViewModel model);
    Task<bool> DeleteAsync(int id);
}
