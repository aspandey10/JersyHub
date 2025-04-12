using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JersyHub.Migrations
{
    /// <inheritdoc />
    public partial class addedDistributer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Distributer",
                table: "Inventories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Distributer",
                table: "Inventories");
        }
    }
}
