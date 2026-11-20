using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SsoServer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIsAdUserFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdUser",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAdUser",
                table: "AspNetUsers");
        }
    }
}
