// =============================================================================
// فایل Migrations/20260601205447_initial.cs — فایل Migration اولیه
// =============================================================================
// Migration ها در Entity Framework Core برای مدیریت تغییرات ساختار دیتابیس
// (Schema) به صورت نسخه‌ای استفاده می‌شوند.
//
// هر Migration دارای دو متد اصلی است:
// - Up(): تغییراتی که باید اعمال شوند (ایجاد جداول، ستون‌ها و...)
// - Down(): تغییراتی که باید برگردانده شوند (حذف جداول، ستون‌ها و...)
//
// دستور ایجاد Migration: dotnet ef migrations add initial
// دستور اعمال Migration: dotnet ef database update
// یا از طریق کد: context.Database.Migrate()
//
// نام فایل شامل تاریخ و زمان ایجاد Migration است (20260601205447).
// =============================================================================

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Migrations
{
    /// <inheritdoc />
    // -------------------------------------------------------------------------
    // کلاس initial — اولین Migration پروژه
    // -------------------------------------------------------------------------
    // این Migration سه جدول اصلی را ایجاد می‌کند: Categories, Suppliers, Products.
    // نام "initial" در دستور "dotnet ef migrations add initial" انتخاب شده است.
    public partial class initial : Migration
    {
        /// <inheritdoc />
        // -----------------------------------------------------------------
        // متد Up — ایجاد ساختار دیتابیس
        // -----------------------------------------------------------------
        // این متد هنگام اعمال Migration اجرا می‌شود (forward).
        // MigrationBuilder ابزاری است برای ساخت دستورات SQL بدون نوشتن مستقیم SQL.
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ==============================================================
            // ایجاد جدول Categories
            // ==============================================================
            // CreateTable: یک جدول جدید در دیتابیس ایجاد می‌کند.
            // name: نام جدول در دیتابیس.
            // columns: تعریف ستون‌های جدول به صورت Lambda Expression.
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    // CategoryId: ستون کلید اصلی از نوع bigint.
                    // Annotation("SqlServer:Identity", "1, 1"): این ستون Identity است
                    // (خودکار افزایشی، شروع از ۱ با گام ۱).
                    // nullable: false یعنی این ستون نمی‌تواند null باشد.
                    CategoryId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    // Name: ستون نام از نوع nvarchar(max).
                    // nullable: false یعنی الزامی است.
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    // PrimaryKey: تعریف کلید اصلی جدول.
                    // "PK_Categories": نام محدودیت (Constraint) کلید اصلی.
                    // x => x.CategoryId: ستون کلید اصلی.
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            // ==============================================================
            // ایجاد جدول Suppliers
            // ==============================================================
            // مشابه Categories اما با ستون اضافی City.
            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    SupplierId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    // City: ستون شهر از نوع nvarchar(max).
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.SupplierId);
                });

            // ==============================================================
            // ایجاد جدول Products
            // ==============================================================
            // این جدول پیچیده‌تر است زیرا دارای کلیدهای خارجی (Foreign Keys) است.
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    // Price: ستون قیمت از نوع decimal(8,2).
                    // این نوع دقیقاً مطابق [Column(TypeName = "decimal(8, 2)")] در
                    // کلاس Product است.
                    Price = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    // CategoryId: کلید خارجی به جدول Categories.
                    CategoryId = table.Column<long>(type: "bigint", nullable: false),
                    // SupplierId: کلید خارجی به جدول Suppliers.
                    SupplierId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                    // ======================================================
                    // ForeignKey: کلید خارجی به جدول Categories
                    // ======================================================
                    // name: نام محدودیت کلید خارجی.
                    // column: ستون فعلی (در جدول Products).
                    // principalTable: جدول مرجع (Categories).
                    // principalColumn: ستون مرجع (CategoryId در Categories).
                    // onDelete: ReferentialAction.Cascade: وقتی یک دسته‌بندی
                    // حذف شود، تمام محصولات آن نیز حذف می‌شوند.
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                    // ======================================================
                    // ForeignKey: کلید خارجی به جدول Suppliers
                    // ======================================================
                    // مشابه Category اما به جدول Suppliers.
                    table.ForeignKey(
                        name: "FK_Products_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierId",
                        onDelete: ReferentialAction.Cascade);
                });

            // ==============================================================
            // ایجاد Index ها (نمایه‌ها)
            // ==============================================================
            // CreateIndex: یک Index روی ستون مشخص ایجاد می‌کند.
            // Index ها سرعت جستجو را افزایش می‌دهند.
            // بدون این Index ها، هر جستجو بر اساس CategoryId یا SupplierId
            // نیازمند Scan کامل جدول (Full Table Scan) خواهد بود.

            // Index روی CategoryId: برای جستجوی سریع محصولات بر اساس دسته‌بندی.
            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            // Index روی SupplierId: برای جستجوی سریع محصولات بر اساس تأمین‌کننده.
            migrationBuilder.CreateIndex(
                name: "IX_Products_SupplierId",
                table: "Products",
                column: "SupplierId");
        }

        /// <inheritdoc />
        // -----------------------------------------------------------------
        // متد Down — برگرداندن تغییرات Migration
        // -----------------------------------------------------------------
        // این متد هنگام بازگشت (Rollback) Migration اجرا می‌شود.
        // ترتیب حذف مهم است: ابتدا جدول Products (که دارای FK است) حذف می‌شود،
        // سپس جداول Categories و Suppliers.
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // DropTable: کل جدول را حذف می‌کند.
            // Products اول حذف می‌شود زیرا دارای کلید خارجی به جداول دیگر است.
            migrationBuilder.DropTable(
                name: "Products");

            // Categories و Suppliers می‌توانند به هر ترتیبی حذف شوند
            // زیرا وابستگی مشترکی ندارند.
            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Suppliers");
        }
    }
}