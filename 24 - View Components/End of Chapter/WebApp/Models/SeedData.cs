// =====================================================================
// SeedData.cs - کلاس پر کردن پایگاه داده با داده‌های اولیه
// =====================================================================
// Seeding یعنی وارد کردن داده‌های پیش‌فرض به پایگاه داده در اولین اجرا.
// این کار معمولاً برای داده‌های مرجع (مثل دسته‌بندی‌ها) یا داده‌های نمونه
// (برای تست و توسعه) انجام می‌شود.
// =====================================================================

 using Microsoft.EntityFrameworkCore; // برای استفاده از متدهای Migration مانند Migrate()

// تعریف namespace
namespace WebApp.Models {
    // تعریف کلاس ایستا (static) SeedData
    // static یعنی تمام اعضای آن باید static باشند و نمی‌توان نمونه‌ای از آن ساخت
    public static class SeedData {

        // متد ایستا برای پر کردن پایگاه داده
        // این متد در Program.cs پس از ساخت app فراخوانی می‌شود
        public static void SeedDatabase(DataContext context) {
            // Migrate(): اعمال آخرین Migrationهای معلق بر روی پایگاه داده
            // اگر پایگاه داده وجود نداشته باشد، آن را ایجاد می‌کند
            context.Database.Migrate();

            // بررسی اینکه آیا جداول محصولات، تأمین‌کنندگان و دسته‌بندی‌ها خالی هستند یا نه
            // Count() تعداد رکوردها را برمی‌گرداند
            // اگر هر سه جدول خالی باشند (== 0)، عملیات seeding انجام می‌شود
            if (context.Products.Count() == 0 && context.Suppliers.Count() == 0
                    && context.Categories.Count() == 0) {

                // ----------------------------------------------------------------
                // ایجاد سه تأمین‌کننده (Supplier)
                // ----------------------------------------------------------------
                Supplier s1 = new Supplier { Name = "Splash Dudes", City = "San Jose" };
                Supplier s2 = new Supplier { Name = "Soccer Town", City = "Chicago" };
                Supplier s3 = new Supplier { Name = "Chess Co", City = "New York" };

                // ----------------------------------------------------------------
                // ایجاد سه دسته‌بندی (Category)
                // ----------------------------------------------------------------
                Category c1 = new Category { Name = "Watersports" };
                Category c2 = new Category { Name = "Soccer" };
                Category c3 = new Category { Name = "Chess" };

                // ----------------------------------------------------------------
                // اضافه کردن چندین محصول به صورت همزمان با AddRange
                // هر محصول به یک دسته‌بندی و یک تأمین‌کننده متصل است
                // ----------------------------------------------------------------
                context.Products.AddRange(
                    // محصول "Kayak" - قایق کوچک - متعلق به دسته ورزش‌های آبی
                    new Product {
                        Name = "Kayak", Price = 275,
                        Category = c1, Supplier = s1
                    },
                    // جلیقه نجات - متعلق به دسته ورزش‌های آبی
                    new Product {
                        Name = "Lifejacket", Price = 48.95m,
                        Category = c1, Supplier = s1
                    },
                    // توپ فوتبال - متعلق به دسته فوتبال
                    new Product {
                        Name = "Soccer Ball", Price = 19.50m,
                        Category = c2, Supplier = s2
                    },
                    // پرچم کرنر - متعلق به دسته فوتبال
                    new Product {
                        Name = "Corner Flags", Price = 34.95m,
                        Category = c2, Supplier = s2
                    },
                    // استادیوم - یک محصول گران‌قیمت
                    new Product {
                        Name = "Stadium", Price = 79500,
                        Category = c2, Supplier = s2
                    },
                    // کلاه فکر - متعلق به دسته شطرنج
                    new Product {
                        Name = "Thinking Cap", Price = 16,
                        Category = c3, Supplier = s3
                    },
                    // صندلی ناپایدار - متعلق به دسته شطرنج
                    new Product {
                        Name = "Unsteady Chair", Price = 29.95m,
                        Category = c3, Supplier = s3
                    },
                    // صفحه شطرنج انسانی - متعلق به دسته شطرنج
                    new Product {
                        Name = "Human Chess Board", Price = 75,
                        Category = c3, Supplier = s3
                    },
                    // شاهی پرزرق و برق - محصول بسیار گران‌قیمت شطرنج
                    new Product {
                        Name = "Bling-Bling King", Price = 1200,
                        Category = c3, Supplier = s3
                    }
                );

                // ذخیره‌ی تغییرات در پایگاه داده
                // این متد تراکنشی (transactional) است؛ یا همه چیز ذخیره می‌شود یا هیچ‌چیز
                context.SaveChanges();
            }
        }
    }
}
