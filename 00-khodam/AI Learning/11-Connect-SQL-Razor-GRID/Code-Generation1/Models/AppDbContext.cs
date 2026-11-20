using Code_Generation1.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Code_Generation.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // DbSet برای Product
        public DbSet<Product> Products { get; set; }

        // اگر مدل‌های دیگری دارید
         public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // تنظیمات اضافی مدل Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductID);//یعنی این یک کلید خارجی است

                entity.Property(e => e.ProductID).UseIdentityColumn();//یعنی مقدار آن افزایشی اتوماتیک است

                entity.Property(e => e.Name).IsRequired().HasMaxLength(25);//یعنی مقدار این فیلد اجباری و حداکثر طول آن 25 تا است

                entity.Property(e => e.Price).IsRequired();//اجباری است

                // Configure foreign keys
                //این کد در اصل یک Foreign Key با شرط Restrict در دیتابیس می‌سازد. اما در زمانِ گرفتنِ گزارش و کوئری زدن، اگر فیلد شما اجباری باشد معادل INNER JOIN و اگر اختیاری باشد معادل LEFT JOIN رفتار خواهد کرد.
                //ALTER TABLE Products  
                //ADD CONSTRAINT FK_Products_Categories_CategoryId
                //FOREIGN KEY(CategoryId) REFERENCES Categories(CategoryId)
                //ON DELETE RESTICT;
                entity.HasOne(e => e.Category)
                      .WithMany()
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Supplier)
                      .WithMany()
                      .HasForeignKey(e => e.SupplierId)
                      .OnDelete(DeleteBehavior.Restrict);

            });
            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(e => e.CategoryId).IsRequired();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(25); ;
            });

            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.Property(e => e.SupplierId).IsRequired();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(25); ;
                entity.Property(e => e.City).IsRequired().HasMaxLength(25); ;
            });
        }
    }
}