using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using UserManagementMvc.Data;
using UserManagementMvc.Models;
using UserManagementMvc.ViewModels;

namespace UserManagementMvc.Repositories;

// این Repository تمام عملیات مربوط به کاربران را انجام می‌دهد.
// نکته مهم:
// - برای نمایش لیست از Stored Procedure استفاده می‌کنیم.
// - برای CRUD معمولی از Entity Framework Core استفاده می‌کنیم.
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserListViewModel> GetPagedUsersAsync(UserFilterViewModel filter)
    {
        filter = NormalizeFilter(filter);

        var users = new List<UserListItemViewModel>();
        var filteredQuery = _context.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            filteredQuery = filteredQuery.Where(x =>
                x.Name.Contains(filter.SearchTerm) ||
                x.Family.Contains(filter.SearchTerm) ||
                x.Email.Contains(filter.SearchTerm) ||
                x.Tel.Contains(filter.SearchTerm));
        }

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            filteredQuery = filteredQuery.Where(x => x.City == filter.City);
        }

        // برای داشتن Pagination دقیق، تعداد کل رکوردهای فیلترشده را محاسبه می‌کنیم.
        var totalItems = await filteredQuery.CountAsync();
        var totalPages = totalItems == 0 ? 1 : (int)Math.Ceiling(totalItems / (double)filter.PageSize);

        if (filter.Page > totalPages)
        {
            filter.Page = totalPages;
        }

        var connection = _context.Database.GetDbConnection();
        var shouldCloseConnection = false;
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
            shouldCloseConnection = true;
        }

        using var command = connection.CreateCommand();
        command.CommandText = "dbo.sp_GetUsersPaged";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@SearchTerm", (object?)filter.SearchTerm ?? DBNull.Value));
        command.Parameters.Add(new SqlParameter("@City", (object?)filter.City ?? DBNull.Value));
        command.Parameters.Add(new SqlParameter("@SortField", filter.SortField));
        command.Parameters.Add(new SqlParameter("@SortDirection", filter.SortDirection));
        command.Parameters.Add(new SqlParameter("@Page", filter.Page));
        command.Parameters.Add(new SqlParameter("@PageSize", filter.PageSize));

        using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                users.Add(new UserListItemViewModel
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Family = reader.GetString(reader.GetOrdinal("Family")),
                    BirthDate = reader.IsDBNull(reader.GetOrdinal("BirthDate"))
                        ? null
                        : reader.GetDateTime(reader.GetOrdinal("BirthDate")),
                    City = reader.GetString(reader.GetOrdinal("City")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    Tel = reader.GetString(reader.GetOrdinal("Tel"))
                });
            }
        }

        if (shouldCloseConnection)
        {
            await connection.CloseAsync();
        }

        var cities = await _context.Users
            .AsNoTracking()
            .Select(x => x.City)
            .Where(x => x != "")
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        return new UserListViewModel
        {
            Users = users,
            Filter = filter,
            AvailableCities = cities,
            TotalItems = totalItems
        };
    }

    public async Task<UserDetailsViewModel?> GetDetailsAsync(int id)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new UserDetailsViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Family = x.Family,
                BirthDate = x.BirthDate,
                City = x.City,
                Address = x.Address,
                Email = x.Email,
                Tel = x.Tel
            })
            .FirstOrDefaultAsync();
    }

    public async Task<UserEditViewModel?> GetForEditAsync(int id)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new UserEditViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Family = x.Family,
                BirthDate = x.BirthDate,
                City = x.City,
                Address = x.Address,
                Email = x.Email,
                Tel = x.Tel
            })
            .FirstOrDefaultAsync();
    }

    public async Task<UserDeleteViewModel?> GetForDeleteAsync(int id)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new UserDeleteViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Family = x.Family,
                Email = x.Email,
                Tel = x.Tel
            })
            .FirstOrDefaultAsync();
    }

    public async Task CreateAsync(UserCreateViewModel model)
    {
        var user = new UserInputModel
        {
            Name = model.Name,
            Family = model.Family,
            BirthDate = model.BirthDate,
            City = model.City,
            Address = model.Address,
            Email = model.Email,
            Tel = model.Tel
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(UserEditViewModel model)
    {
        var user = await _context.Users.FindAsync(model.Id);
        if (user == null)
        {
            return false;
        }

        user.Name = model.Name;
        user.Family = model.Family;
        user.BirthDate = model.BirthDate;
        user.City = model.City;
        user.Address = model.Address;
        user.Email = model.Email;
        user.Tel = model.Tel;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return false;
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    private static UserFilterViewModel NormalizeFilter(UserFilterViewModel filter)
    {
        filter.SearchTerm = string.IsNullOrWhiteSpace(filter.SearchTerm)
            ? null
            : filter.SearchTerm.Trim();

        filter.City = string.IsNullOrWhiteSpace(filter.City)
            ? null
            : filter.City.Trim();

        if (filter.Page < 1)
        {
            filter.Page = 1;
        }

        if (filter.PageSize < 1)
        {
            filter.PageSize = 5;
        }

        var validSortFields = new[] { "Id", "Name", "Family", "City", "Email", "BirthDate" };
        if (!validSortFields.Contains(filter.SortField))
        {
            filter.SortField = "Id";
        }

        filter.SortDirection = filter.SortDirection?.ToLowerInvariant() == "desc" ? "desc" : "asc";

        return filter;
    }
}
