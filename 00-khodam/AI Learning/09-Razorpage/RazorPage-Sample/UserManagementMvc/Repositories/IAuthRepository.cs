using UserManagementMvc.Models;

namespace UserManagementMvc.Repositories;

// قرارداد مربوط به احراز هویت
public interface IAuthRepository
{
    Task<AppUser?> ValidateUserAsync(string username, string password);
}
