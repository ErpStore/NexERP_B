using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddExistingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "LabourGRNSub");

            migrationBuilder.InsertData(
                table: "ScreenManagements",
                columns: new[] { "Id", "Description", "Display", "Required", "ScreenName", "Topic" },
                values: new object[,]
                {
                    { 5, "When this setting is enabled, the Rate field in Labour GRN becomes non-editable. The system will automatically take the rate from the Reference PO, and the rate cannot be modified.", "Rate Control", false, "Labour GRN", "Rate Lock Setting" },
                    { 6, "When this setting is enabled, the Rate field in Labour Delivery Challan becomes non-editable. The system will automatically take the rate from the Reference PO, and the rate cannot be modified.", "Rate Control", false, "Labour Delivery Challan", "Rate Lock Setting" },
                    { 7, "When this setting is enabled, the Rate field in Labour Invoice becomes non-editable. The system will automatically take the rate from the Reference PO, and the rate cannot be modified.", "Rate Control", false, "Labour Invoice", "Rate Lock Setting" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ScreenManagements",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ScreenManagements",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ScreenManagements",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "LabourGRNSub",
                type: "int",
                nullable: true);
        }
    }
}
