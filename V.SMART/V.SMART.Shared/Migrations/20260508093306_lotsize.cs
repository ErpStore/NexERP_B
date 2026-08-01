using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class lotsize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeliveryDays",
                table: "Item",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LotSize",
                table: "Item",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryDays",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "LotSize",
                table: "Item");
        }
    }
}
