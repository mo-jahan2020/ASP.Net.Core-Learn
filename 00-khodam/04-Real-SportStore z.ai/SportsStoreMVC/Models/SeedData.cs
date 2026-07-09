using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SportsStore.Models {

    /// <summary>
    /// کلاس استاتیک SeedData
    /// وظیفه: پر کردن دیتابیس با داده‌های اولیه (اگر خالی باشد)
    /// و اجرای Migration های معلق به صورت خودکار
    ///
    /// در این نسخه، گزارش‌گیری (Logging) اضافه شده تا اگر خطایی در
    /// ارتباط با دیتابیس رخ داد، به طور واضح در Console نمایش داده شود.
    /// </summary>
    public static class SeedData {

        /// <summary>
        /// این متد در زمان Startup برنامه فراخوانی می‌شود
        /// و دیتابیس را آماده استفاده می‌کند.
        /// </summary>
        public static void EnsurePopulated(IApplicationBuilder app) {
            // ایجاد Scope برای دریافت Service از DI Container
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;

            StoreDbContext context = services.GetRequiredService<StoreDbContext>();
            // نکته: چون SeedData کلاس static است، نمی‌توان از ILogger<SeedData> استفاده کرد
            // به جای آن از ILogger غیر جنریک استفاده می‌کنیم
            var logger = services.GetRequiredService<ILogger<StoreDbContext>>();

            try {
                logger.LogInformation("Store: شروع بررسی Migration های دیتابیس...");

                // بررسی اتصال به دیتابیس قبل از Migration
                if (!context.Database.CanConnect()) {
                    logger.LogWarning(
                        "Store: امکان اتصال به دیتابیس Store وجود ندارد. " +
                        "سعی در ساخت دیتابیس از طریق Migration...");
                }

                // اجرای Migration های معلق (آماده‌سازی جداول)
                var pendingMigrations = context.Database.GetPendingMigrations().ToList();
                if (pendingMigrations.Any()) {
                    logger.LogInformation(
                        "Store: اعمال {Count} Migration معلق: {Migrations}",
                        pendingMigrations.Count,
                        string.Join(", ", pendingMigrations));
                    context.Database.Migrate();
                    logger.LogInformation("Store: Migration ها با موفقیت اعمال شدند.");
                } else {
                    logger.LogInformation("Store: Migration معقلی وجود ندارد.");
                }

                // اگر هیچ محصولی وجود نداشت، داده‌های اولیه را اضافه کن
                if (!context.Products.Any()) {
                    logger.LogInformation("Store: افزودن داده‌های اولیه محصولات...");
                    context.Products.AddRange(
                        new Product {
                            Name = "Kayak", Description = "A boat for one person",
                            Category = "Watersports", Price = 275,
                            ImageUrl = "https://picsum.photos/seed/kayak/400/300"
                        },
                        new Product {
                            Name = "Lifejacket",
                            Description = "Protective and fashionable",
                            Category = "Watersports", Price = 48.95m,
                            ImageUrl = "https://picsum.photos/seed/lifejacket/400/300"
                        },
                        new Product {
                            Name = "Soccer Ball",
                            Description = "FIFA-approved size and weight",
                            Category = "Soccer", Price = 19.50m,
                            ImageUrl = "https://picsum.photos/seed/soccer/400/300"
                        },
                        new Product {
                            Name = "Corner Flags",
                            Description = "Give your playing field a professional touch",
                            Category = "Soccer", Price = 34.95m,
                            ImageUrl = "https://picsum.photos/seed/flags/400/300"
                        },
                        new Product {
                            Name = "Stadium",
                            Description = "Flat-packed 35,000-seat stadium",
                            Category = "Soccer", Price = 79500,
                            ImageUrl = "https://picsum.photos/seed/stadium/400/300"
                        },
                        new Product {
                            Name = "Thinking Cap",
                            Description = "Improve brain efficiency by 75%",
                            Category = "Chess", Price = 16,
                            ImageUrl = "https://picsum.photos/seed/cap/400/300"
                        },
                        new Product {
                            Name = "Unsteady Chair",
                            Description = "Secretly give your opponent a disadvantage",
                            Category = "Chess", Price = 29.95m,
                            ImageUrl = "https://picsum.photos/seed/chair/400/300"
                        },
                        new Product {
                            Name = "Human Chess Board",
                            Description = "A fun game for the family",
                            Category = "Chess", Price = 75,
                            ImageUrl = "https://picsum.photos/seed/board/400/300"
                        },
                        new Product {
                            Name = "Bling-Bling King",
                            Description = "Gold-plated, diamond-studded King",
                            Category = "Chess", Price = 1200,
                            ImageUrl = "https://picsum.photos/seed/king/400/300"
                        }
                    );
                    context.SaveChanges();
                    logger.LogInformation("Store: ۹ محصول اولیه با موفقیت اضافه شد.");
                } else {
                    logger.LogInformation(
                        "Store: {Count} محصول از قبل موجود است، داده اولیه نیاز نیست.",
                        context.Products.Count());
                }

                logger.LogInformation("Store: مقداردهی اولیه با موفقیت کامل شد.");
            }
            catch (Exception ex) {
                // ثبت خطا با جزئیات کامل
                logger.LogError(ex,
                    "Store: خطا در مقداردهی اولیه دیتابیس Store. " +
                    "Connection string ممکن است اشتباه باشد یا SQL Server در دسترس نباشد. " +
                    "Inner exception: {InnerMessage}",
                    ex.InnerException?.Message ?? "ندارد");
                // دوباره throw می‌کنیم تا Program.cs هم بتواند آن را ببیند
                throw;
            }
        }
    }
}
