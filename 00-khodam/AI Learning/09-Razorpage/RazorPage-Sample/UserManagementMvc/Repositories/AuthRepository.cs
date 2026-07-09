using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagementMvc.Data;
using UserManagementMvc.Models;

namespace UserManagementMvc.Repositories;

// این Repository عملیات ورود را انجام می‌دهد.
public class AuthRepository : IAuthRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<AppUser> _passwordHasher;

    public AuthRepository(ApplicationDbContext context, IPasswordHasher<AppUser> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<AppUser?> ValidateUserAsync(string username, string password)
    {
        var normalizedUsername = username.Trim().ToUpperInvariant();

        var user = await _context.AppUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.NormalizedUsername == normalizedUsername);

        if (user == null)
        {
            return null;
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        return result == PasswordVerificationResult.Success ? user : null;
    }
}
