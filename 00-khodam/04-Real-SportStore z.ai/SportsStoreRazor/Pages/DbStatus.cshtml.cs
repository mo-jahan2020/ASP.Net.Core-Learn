using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SportsStore.Models;

namespace SportsStore.Pages {

    /// <summary>
    /// PageModel صفحه وضعیت دیتابیس و عیب‌یابی (DbStatus)
    ///
    /// این صفحه برای کمک به کاربر در صورت بروز مشکل در دیتابیس طراحی شده است.
    /// کاربر می‌تواند:
    /// - ConnectionString های فعلی را ببیند
    /// - وضعیت اتصال به هر دیتابیس را بررسی کند
    /// - لیست Migration های اعمال شده و معلق را ببیند
    /// - در صورت نیاز Migration ها را دستی اعمال کند
    ///
    /// نکته امنیتی: فقط کاربران Admin می‌توانند به این صفحه دسترسی داشته باشند.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class DbStatusModel : PageModel {

        private readonly StoreDbContext       _storeContext;
        private readonly AppIdentityDbContext _identityContext;
        private readonly IConfiguration       _configuration;
        private readonly IHostEnvironment     _env;

        public DbStatusModel(StoreDbContext storeContext,
                             AppIdentityDbContext identityContext,
                             IConfiguration configuration,
                             IHostEnvironment env) {
            _storeContext    = storeContext;
            _identityContext = identityContext;
            _configuration   = configuration;
            _env             = env;
        }

        // --- ConnectionString های فعلی ---
        public string StoreConnectionString    { get; set; } = "";
        public string IdentityConnectionString { get; set; } = "";

        // --- وضعیت اتصال ---
        public bool StoreCanConnect    { get; set; }
        public bool IdentityCanConnect { get; set; }

        // --- Migration ها ---
        public IEnumerable<string> StoreAppliedMigrations    { get; set; } = new List<string>();
        public IEnumerable<string> StorePendingMigrations    { get; set; } = new List<string>();
        public IEnumerable<string> IdentityAppliedMigrations { get; set; } = new List<string>();
        public IEnumerable<string> IdentityPendingMigrations { get; set; } = new List<string>();

        // --- نتیجه اعمال دستی Migration ---
        public string? MigrationResult { get; set; }

        // --- اطلاعات اضافی برای دیباگ ---
        public string? StoreProviderName    { get; set; }
        public string? IdentityProviderName { get; set; }
        public int StoreProductCount        { get; set; }
        public int IdentityUserCount        { get; set; }

        // --- آیا در محیط Development هستیم؟ ---
        public bool IsDevelopment => _env.IsDevelopment();

        /// <summary>
        /// نمایش صفحه وضعیت دیتابیس
        /// </summary>
        public void OnGet() {
            // خواندن ConnectionString ها از Configuration
            StoreConnectionString    = _configuration["Data:StoreProducts:ConnectionStrings"]
                                       ?? "(تنظیم نشده)";
            IdentityConnectionString = _configuration["Data:Identity:ConnectionStrings"]
                                       ?? "(تنظیم نشده)";

            // بررسی وضعیت اتصال - در try/catch چون ممکن است خطا بدهد
            try {
                StoreCanConnect = _storeContext.Database.CanConnect();
                StoreProviderName = _storeContext.Database.ProviderName;
                if (StoreCanConnect) {
                    StoreProductCount = _storeContext.Products.Count();
                }
            } catch {
                StoreCanConnect = false;
            }

            try {
                IdentityCanConnect = _identityContext.Database.CanConnect();
                IdentityProviderName = _identityContext.Database.ProviderName;
                if (IdentityCanConnect) {
                    // شمارش کاربران از طریق جدول AspNetUsers
                    IdentityUserCount = _identityContext.Users.Count();
                }
            } catch {
                IdentityCanConnect = false;
            }

            // دریافت لیست Migration ها
            try {
                StoreAppliedMigrations = _storeContext.Database.GetAppliedMigrations().ToList();
                StorePendingMigrations = _storeContext.Database.GetPendingMigrations().ToList();
            } catch {
                StoreAppliedMigrations = new List<string>();
                StorePendingMigrations = new List<string>();
            }

            try {
                IdentityAppliedMigrations = _identityContext.Database.GetAppliedMigrations().ToList();
                IdentityPendingMigrations = _identityContext.Database.GetPendingMigrations().ToList();
            } catch {
                IdentityAppliedMigrations = new List<string>();
                IdentityPendingMigrations = new List<string>();
            }
        }

        /// <summary>
        /// اعمال دستی Migration های معلق (Handler: ApplyMigrations)
        /// </summary>
        public async Task<IActionResult> OnPostApplyMigrationsAsync() {
            try {
                var messages = new List<string>();

                // اعمال Migration های Store
                var storePending = _storeContext.Database.GetPendingMigrations().ToList();
                if (storePending.Any()) {
                    await _storeContext.Database.MigrateAsync();
                    messages.Add($"Store: {storePending.Count} Migration اعمال شد.");
                } else {
                    messages.Add("Store: Migration معقلی وجود ندارد.");
                }

                // اعمال Migration های Identity
                var identityPending = _identityContext.Database.GetPendingMigrations().ToList();
                if (identityPending.Any()) {
                    await _identityContext.Database.MigrateAsync();
                    messages.Add($"Identity: {identityPending.Count} Migration اعمال شد.");
                } else {
                    messages.Add("Identity: Migration معقلی وجود ندارد.");
                }

                MigrationResult = string.Join(" ", messages);
                TempData["SuccessMessage"] = "Migration ها با موفقیت اعمال شدند.";
            }
            catch (Exception ex) {
                MigrationResult = $"خطا: {ex.Message}";
                if (ex.InnerException != null) {
                    MigrationResult += $" | Inner: {ex.InnerException.Message}";
                }
                TempData["ErrorMessage"] = "خطا در اعمال Migration. جزئیات در صفحه.";
            }

            return RedirectToPage();
        }
    }
}
