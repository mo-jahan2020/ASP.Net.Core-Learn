using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagementMvc.Models;

namespace UserManagementMvc.Data;

// این کلاس کارهای اولیه دیتابیس را انجام می‌دهد:
// 1) ساخت دیتابیس در اولین اجرا
// 2) ساخت Stored Procedure
// 3) ثبت کاربر پیش‌فرض برای ورود
// 4) ثبت چند داده نمونه برای آموزش و تست
public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();

        await dbContext.Database.EnsureCreatedAsync();
        await CreateStoredProcedureAsync(dbContext);
        await SeedAppUserAsync(dbContext, passwordHasher);
        await SeedUsersAsync(dbContext);
    }

    private static async Task CreateStoredProcedureAsync(ApplicationDbContext dbContext)
    {
        var sql = """
                  CREATE OR ALTER PROCEDURE dbo.sp_GetUsersPaged
                      @SearchTerm NVARCHAR(100) = NULL,
                      @City NVARCHAR(100) = NULL,
                      @SortField NVARCHAR(50) = N'Id',
                      @SortDirection NVARCHAR(4) = N'asc',
                      @Page INT = 1,
                      @PageSize INT = 5
                  AS
                  BEGIN
                      SET NOCOUNT ON;

                      IF (@Page < 1) SET @Page = 1;
                      IF (@PageSize < 1) SET @PageSize = 5;

                      ;WITH FilteredUsers AS
                      (
                          SELECT
                              Id,
                              Name,
                              Family,
                              BirthDate,
                              City,
                              Address,
                              Email,
                              Tel
                          FROM Users
                          WHERE
                              (ISNULL(@SearchTerm, N'') = N''
                               OR Name LIKE N'%' + @SearchTerm + N'%'
                               OR Family LIKE N'%' + @SearchTerm + N'%'
                               OR Email LIKE N'%' + @SearchTerm + N'%'
                               OR Tel LIKE N'%' + @SearchTerm + N'%')
                              AND
                              (ISNULL(@City, N'') = N'' OR City = @City)
                      )
                      SELECT
                          Id,
                          Name,
                          Family,
                          BirthDate,
                          City,
                          Email,
                          Tel,
                          COUNT(1) OVER() AS TotalCount
                      FROM FilteredUsers
                      ORDER BY
                          CASE WHEN @SortField = N'Id' AND @SortDirection = N'asc' THEN Id END ASC,
                          CASE WHEN @SortField = N'Id' AND @SortDirection = N'desc' THEN Id END DESC,
                          CASE WHEN @SortField = N'Name' AND @SortDirection = N'asc' THEN Name END ASC,
                          CASE WHEN @SortField = N'Name' AND @SortDirection = N'desc' THEN Name END DESC,
                          CASE WHEN @SortField = N'Family' AND @SortDirection = N'asc' THEN Family END ASC,
                          CASE WHEN @SortField = N'Family' AND @SortDirection = N'desc' THEN Family END DESC,
                          CASE WHEN @SortField = N'City' AND @SortDirection = N'asc' THEN City END ASC,
                          CASE WHEN @SortField = N'City' AND @SortDirection = N'desc' THEN City END DESC,
                          CASE WHEN @SortField = N'Email' AND @SortDirection = N'asc' THEN Email END ASC,
                          CASE WHEN @SortField = N'Email' AND @SortDirection = N'desc' THEN Email END DESC,
                          CASE WHEN @SortField = N'BirthDate' AND @SortDirection = N'asc' THEN BirthDate END ASC,
                          CASE WHEN @SortField = N'BirthDate' AND @SortDirection = N'desc' THEN BirthDate END DESC,
                          Id ASC
                      OFFSET (@Page - 1) * @PageSize ROWS
                      FETCH NEXT @PageSize ROWS ONLY;
                  END
                  """;

        await dbContext.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task SeedAppUserAsync(
        ApplicationDbContext dbContext,
        IPasswordHasher<AppUser> passwordHasher)
    {
        if (await dbContext.AppUsers.AnyAsync())
        {
            return;
        }

        var adminUser = new AppUser
        {
            Username = "admin",
            NormalizedUsername = "ADMIN",
            FullName = "مدیر سیستم"
        };

        // رمز عبور نمونه برای آموزش:
        // Username: admin
        // Password: 123456
        adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "123456");

        dbContext.AppUsers.Add(adminUser);
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(ApplicationDbContext dbContext)
    {
        if (await dbContext.Users.AnyAsync())
        {
            return;
        }

        var users = new List<UserInputModel>
        {
            new() { Name = "علی", Family = "محمدی", BirthDate = new DateTime(1993, 5, 10), City = "تهران", Address = "تهران، خیابان آزادی، پلاک ۱۲", Email = "ali@example.com", Tel = "09123456789" },
            new() { Name = "سارا", Family = "کریمی", BirthDate = new DateTime(1995, 8, 14), City = "شیراز", Address = "شیراز، بلوار چمران، کوچه ۵", Email = "sara@example.com", Tel = "09121234567" },
            new() { Name = "رضا", Family = "احمدی", BirthDate = new DateTime(1991, 11, 21), City = "اصفهان", Address = "اصفهان، خیابان چهارباغ، مجتمع گل‌ها", Email = "reza@example.com", Tel = "09129876543" },
            new() { Name = "مریم", Family = "صادقی", BirthDate = new DateTime(1997, 2, 2), City = "تبریز", Address = "تبریز، ولیعصر، ساختمان بهار", Email = "maryam@example.com", Tel = "09125554444" },
            new() { Name = "حسین", Family = "نصیری", BirthDate = new DateTime(1989, 1, 18), City = "تهران", Address = "تهران، نارمک، خیابان ۴۶", Email = "hossein@example.com", Tel = "09127778888" },
            new() { Name = "نگار", Family = "جعفری", BirthDate = new DateTime(1998, 9, 30), City = "مشهد", Address = "مشهد، احمدآباد، کوچه لاله", Email = "negar@example.com", Tel = "09123334444" },
            new() { Name = "امیر", Family = "اکبری", BirthDate = new DateTime(1992, 7, 9), City = "کرج", Address = "کرج، عظیمیه، بلوک ۷", Email = "amir@example.com", Tel = "09126667777" }
        };

        dbContext.Users.AddRange(users);
        await dbContext.SaveChangesAsync();
    }
}
