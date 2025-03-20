using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JersyHub.Migrations
{
    /// <inheritdoc />
    public partial class addDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DicountPercent",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DicountPercent",
                table: "Products");
        }
    }
}
