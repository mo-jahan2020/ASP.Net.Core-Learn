// =============================================================================
// فایل Models/SeedData.cs — مقداردهی اولیه دیتابیس (Database Seeding)
// =============================================================================
// Seeding: پر کردن دیتابیس با داده‌های نمونه در اولین اجرای برنامه.
// فقط اگر هر سه جدول خالی باشند، داده‌ها وارد می‌شوند (جلوگیری از تکرار).
// =============================================================================

using Microsoft.EntityFrameworkCore;

namespace WebApp.Models {
    // کلاس static — نیازی به نمونه‌سازی (new) نیست
    public static class SeedData {

        public static void SeedDatabase(DataContext context) {
            // Database.Migrate(): اعمال Migration های منتظر و ساخت دیتابیس
            context.Database.Migrate();

            // بررسی خالی بودن دیتابیس — فقط یک بار داده‌ها وارد می‌شوند
            // ⚠️ بهتر است از Any() به جای Count() استفاده کنید (سریع‌تر)
            if (context.Products.Count() == 0 && context.Suppliers.Count() == 0
                    && context.Categories.Count() == 0) {

                // ایجاد داده‌های نمونه — تأمین‌کنندگان (SupplierId خودکار تولید می‌شود)
                Supplier s1 = new Supplier { Name = "Splash Dudes", City = "San Jose" };
                Supplier s2 = new Supplier { Name = "Soccer Town", City = "Chicago" };
                Supplier s3 = new Supplier { Name = "Chess Co", City = "New York" };

                // ایجاد داده‌های نمونه — دسته‌بندی‌ها
                Category c1 = new Category { Name = "Watersports" };
                Category c2 = new Category { Name = "Soccer" };
                Category c3 = new Category { Name = "Chess" };

                // ایجاد ۹ محصول نمونه با ارتباط به دسته‌بندی و تأمین‌کننده
                // EF Core با Change Tracking و Fix-up کلیدهای خارجی را تنظیم می‌کند
                // پسوند m (مثل 48.95m): مشخص‌کننده نوع decimal
                context.Products.AddRange(
                    new Product {
                        Name = "Kayak", Price = 275,
                        Category = c1, Supplier = s1
                    },
                    new Product {
                        Name = "Lifejacket", Price = 48.95m,
                        Category = c1, Supplier = s1
                    },
                    new Product {
                        Name = "Soccer Ball", Price = 19.50m,
                        Category = c2, Supplier = s2
                    },
                    new Product {
                        Name = "Corner Flags", Price = 34.95m,
                        Category = c2, Supplier = s2
                    },
                    new Product {
                        Name = "Stadium", Price = 79500,
                        Category = c2, Supplier = s2
                    },
                    new Product {
                        Name = "Thinking Cap", Price = 16,
                        Category = c3, Supplier = s3
                    },
                    new Product {
                        Name = "Unsteady Chair", Price = 29.95m,
                        Category = c3, Supplier = s3
                    },
                    new Product {
                        Name = "Human Chess Board", Price = 75,
                        Category = c3, Supplier = s3
                    },
                    new Product {
                        Name = "Bling-Bling King", Price = 1200,
                        Category = c3, Supplier = s3
                    }
                );
                // ذخیره در قالب یک تراکنش (Transaction)
                context.SaveChanges();
            }
        }
    }
}
