using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SportsStore.Models {

    /// <summary>
    /// کلاس Seed برای Identity
    /// این کلاس:
    /// 1) دو نقش (Role) پیش‌فرض می‌سازد: Admin و Customer
    /// 2) یک کاربر Admin پیش‌فرض می‌سازد و آن را به نقش Admin اضافه می‌کند
    /// نقش‌ها برای اعمال [Authorize(Roles="...")] در صفحات استفاده می‌شوند.
    ///
    /// ⚠️ نکته مهم: در نسخه قبلی این متد `async void` بود که باعث می‌شد
    /// هر خطای دیتابیس به صورت خاموش نادیده گرفته شود. این یکی از
    /// بدترین الگوهای C# است. الان به `async Task` تغییر یافته تا
    /// exception ها به درستی گزارش شوند.
    /// </summary>
    public static class IdentitySeedData {

        // نام کاربری و رمز عبور پیش‌فرض Admin
        private const string adminUser  = "Admin";
        private const string adminPassword = "Secret123$";

        // نام نقش‌ها
        public const string AdminRole   = "Admin";
        public const string CustomerRole = "Customer";

        /// <summary>
        /// این متد در زمان Startup برنامه فراخوانی می‌شود
        /// و دیتابیس Identity را آماده استفاده می‌کند.
        ///
        /// تغییر مهم: از `async Task` به جای `async void` استفاده شده
        /// تا exception ها به درستی propagate شوند.
        /// </summary>
        public static async Task EnsurePopulatedAsync(IApplicationBuilder app) {
            // ایجاد Scope برای دریافت Service از DI Container
            // (چون این متد در زمان Startup اجرا می‌شود و DbContext Scoped است)
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;

            AppIdentityDbContext context = services
                .GetRequiredService<AppIdentityDbContext>();

            // دریافت Logger برای گزارش خطاها و اطلاعات
            // نکته: چون IdentitySeedData کلاس static است، از ILogger<AppIdentityDbContext> استفاده می‌کنیم
            var logger = services.GetRequiredService<ILogger<AppIdentityDbContext>>();

            try {
                logger.LogInformation("Identity: شروع بررسی Migration های دیتابیس...");

                // بررسی اتصال به دیتابیس قبل از هر کار
                if (!await context.Database.CanConnectAsync()) {
                    logger.LogWarning(
                        "Identity: امکان اتصال به دیتابیس Identity وجود ندارد. " +
                        "سعی در ساخت دیتابیس از طریق Migration...");
                }

                // اجرای Migration های معلق Identity
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any()) {
                    logger.LogInformation(
                        "Identity: اعمال {Count} Migration معلق: {Migrations}",
                        pendingMigrations.Count(),
                        string.Join(", ", pendingMigrations));
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Identity: Migration ها با موفقیت اعمال شدند.");
                } else {
                    logger.LogInformation("Identity: Migration معقلی وجود ندارد.");
                }

                // دریافت UserManager و RoleManager از DI
                UserManager<IdentityUser> userManager =
                    services.GetRequiredService<UserManager<IdentityUser>>();
                RoleManager<IdentityRole> roleManager =
                    services.GetRequiredService<RoleManager<IdentityRole>>();

                // --- ساخت نقش Admin (اگر وجود ندارد) ---
                if (!await roleManager.RoleExistsAsync(AdminRole)) {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(AdminRole));
                    if (roleResult.Succeeded) {
                        logger.LogInformation("Identity: نقش {Role} ساخته شد.", AdminRole);
                    } else {
                        logger.LogError(
                            "Identity: خطا در ساخت نقش {Role}: {Errors}",
                            AdminRole,
                            string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    }
                }

                // --- ساخت نقش Customer (اگر وجود ندارد) ---
                if (!await roleManager.RoleExistsAsync(CustomerRole)) {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(CustomerRole));
                    if (roleResult.Succeeded) {
                        logger.LogInformation("Identity: نقش {Role} ساخته شد.", CustomerRole);
                    }
                }

                // --- ساخت کاربر Admin پیش‌فرض (اگر وجود ندارد) ---
                IdentityUser? user = await userManager.FindByNameAsync(adminUser);
                if (user == null) {
                    user = new IdentityUser(adminUser) {
                        Email = "admin@example.com",
                        EmailConfirmed = true,
                        PhoneNumber = "555-1234"
                    };
                    // ساخت کاربر با رمز عبور
                    IdentityResult createResult =
                        await userManager.CreateAsync(user, adminPassword);

                    if (createResult.Succeeded) {
                        logger.LogInformation(
                            "Identity: کاربر {User} با موفقیت ساخته شد.", adminUser);

                        // اضافه کردن کاربر به نقش Admin
                        await userManager.AddToRoleAsync(user, AdminRole);
                        logger.LogInformation(
                            "Identity: کاربر {User} به نقش {Role} اضافه شد.",
                            adminUser, AdminRole);
                    } else {
                        logger.LogError(
                            "Identity: خطا در ساخت کاربر {User}: {Errors}",
                            adminUser,
                            string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    }
                } else {
                    logger.LogInformation(
                        "Identity: کاربر {User} قبلاً وجود دارد.", adminUser);

                    // اطمینان از اینکه کاربر Admin در نقش Admin قرار دارد
                    if (!await userManager.IsInRoleAsync(user, AdminRole)) {
                        await userManager.AddToRoleAsync(user, AdminRole);
                        logger.LogInformation(
                            "Identity: کاربر {User} به نقش {Role} اضافه شد.",
                            adminUser, AdminRole);
                    }
                }

                logger.LogInformation("Identity: مقداردهی اولیه با موفقیت کامل شد.");
            }
            catch (Exception ex) {
                // ثبت خطا با جزئیات کامل
                logger.LogError(ex,
                    "Identity: خطا در مقداردهی اولیه دیتابیس Identity. " +
                    "Connection string ممکن است اشتباه باشد یا SQL Server در دسترس نباشد. " +
                    "Inner exception: {InnerMessage}",
                    ex.InnerException?.Message ?? "ندارد");
                // دوباره throw می‌کنیم تا Program.cs هم بتواند آن را مدیریت کند
                throw;
            }
        }
    }
}
