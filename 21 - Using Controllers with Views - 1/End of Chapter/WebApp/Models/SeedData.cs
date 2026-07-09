// =============================================================================
// فایل Models/SeedData.cs — کلاس مقداردهی اولیه دیتابیس (Database Seeding)
// =============================================================================
// این کلاس داده‌های اولیه (Seed Data) را در دیتابیس وارد می‌کند.
// "Seeding" یعنی پر کردن دیتابیس با داده‌های نمونه در اولین اجرای برنامه.
// این کار برای محیط توسعه و تست بسیار مفید است.
// =============================================================================

using Microsoft.EntityFrameworkCore;

namespace WebApp.Models {
    // -------------------------------------------------------------------------
    // کلاس SeedData — کلاس استاتیک برای مقداردهی اولیه
    // -------------------------------------------------------------------------
    // کلاس static: نیازی به نمونه‌سازی (new SeedData()) نیست.
    // متد SeedDatabase مستقیماً از طریق نام کلاس قابل فراخوانی است.
    public static class SeedData {

        // ---------------------------------------------------------------------
        // متد SeedDatabase — مقداردهی اولیه دیتابیس
        // ---------------------------------------------------------------------
        // پارامتر DataContext: برای دسترسی به جداول دیتابیس.
        // این متد در Program.cs پس از ساخت برنامه فراخوانی می‌شود.
        public static void SeedDatabase(DataContext context) {

            // -----------------------------------------------------------------
            // Database.Migrate(): اعمال Migration های منتظر
            // -----------------------------------------------------------------
            // این متد بررسی می‌کند آیا Migration جدیدی وجود دارد که هنوز
            // روی دیتابیس اعمال نشده باشد. اگر بله، آن‌ها را اعمال می‌کند.
            // همچنین اگر دیتابیس وجود نداشته باشد، آن را می‌سازد.
            // این جایگزین دستور dotnet ef database update است.
            context.Database.Migrate();

            // -----------------------------------------------------------------
            // بررسی خالی بودن دیتابیس
            // -----------------------------------------------------------------
            // فقط اگر هر سه جدول خالی باشند، داده‌ها وارد می‌شوند.
            // این جلوی وارد شدن داده‌های تکراری در اجراهای بعدی می‌شود.
            // Count(): تعداد رکوردهای هر جدول را می‌شمارد.
            // ⚠️ نکته: در برنامه‌های واقعی با حجم داده زیاد، بهتر است
            // از Any() به جای Count() استفاده کنید زیرا Any() سریع‌تر است
            // (فقط بررسی وجود یک رکورد کافی است).
            if (context.Products.Count() == 0 && context.Suppliers.Count() == 0
                    && context.Categories.Count() == 0) {

                // ==============================================================
                // ایجاد داده‌های نمونه — تأمین‌کنندگان (Suppliers)
                // ==============================================================
                // هر Supplier یک Name و City دارد. EF Core به صورت خودکار
                // SupplierId را تولید می‌کند (Identity Column).
                // این اشیاء هنوز در دیتابیس ذخیره نشده‌اند (فقط در حافظه هستند).
                Supplier s1 = new Supplier { Name = "Splash Dudes", City = "San Jose" };
                Supplier s2 = new Supplier { Name = "Soccer Town", City = "Chicago" };
                Supplier s3 = new Supplier { Name = "Chess Co", City = "New York" };

                // ==============================================================
                // ایجاد داده‌های نمونه — دسته‌بندی‌ها (Categories)
                // ==============================================================
                // هر Category فقط یک Name دارد. محصولات بعداً به این دسته‌ها
                // متصل می‌شوند (از طریق CategoryId).
                Category c1 = new Category { Name = "Watersports" };
                Category c2 = new Category { Name = "Soccer" };
                Category c3 = new Category { Name = "Chess" };

                // ==============================================================
                // ایجاد داده‌های نمونه — محصولات (Products)
                // ==============================================================
                // AddRange: چندین موجودیت را همزمان به DbContext اضافه می‌کند.
                // هر Product از طریق خصوصیات ناوبری (Category و Supplier)
                // به دسته‌بندی و تأمین‌کننده متصل می‌شود. EF Core به صورت
                // خودکار کلیدهای خارجی (CategoryId, SupplierId) را تنظیم می‌کند.
                //
                // نکته مهم: وقتی Category = c1 تنظیم می‌شود، EF Core
                // ۱. ابتدا c1 را در جدول Categories درج می‌کند
                // ۲. سپس Product را با CategoryId مناسب درج می‌کند
                // (این به دلیل "Change Tracking" و "Fix-up" در EF Core است).
                //
                // فرمت قیمت: می‌توانید از پسوند m (مثل 48.95m) برای decimal
                // استفاده کنید تا کامپایلر عدد را به درستی تشخیص دهد.
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

                // ==============================================================
                // ذخیره همه تغییرات در دیتابیس
                // ==============================================================
                // SaveChanges(): تمام تغییرات (Insert, Update, Delete) را
                // در قالب یک تراکنش (Transaction) در دیتابیس ذخیره می‌کند.
                // اگر هر کدام خطا دهد، همه تغییرات برگردانده می‌شود (Rollback).
                // ⚠️ در اینجا از SaveChangesAsync استفاده نشده، اما در کنترلرها
                // از نسخه Async استفاده شده. بهتر است اینجا هم Async باشد.
                context.SaveChanges();
            }
        }
    }
}