using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class snscustomized : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DepartmentCode",
                table: "Staff",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepartmentCode",
                table: "ProductionIssueAssy",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MonthCode",
                table: "ProductionIssueAssy",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.InsertData(
                table: "ScreenManagements",
                columns: new[] { "Id", "Description", "Display", "Required", "ScreenName", "Topic" },
                values: new object[,]
                {
                    { 19, "When enabled, the Reorder Level (ROL) column will be displayed in the Purchase Order screen to help users view item reorder levels during purchase entry. And system will use MAX(ROL, Required Qty) during auto Material Requisition In AutoIndent Creation", "ROL", false, "Purchase Order", "Purchase Order - ROL Column Visibility" },
                    { 20, "When enabled, the system will generate Production Issue numbers based on Department, Month, and Financial Year. The running number will automatically reset every month for each department, ensuring unique and structured numbering (e.g., R-03-000001-2026-27).", "Month-wise Number Generation", false, "Production Issue WO Assembly", "Department-wise and Month-wise Issue Number Generation" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ScreenManagements",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "ScreenManagements",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DropColumn(
                name: "DepartmentCode",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "DepartmentCode",
                table: "ProductionIssueAssy");

            migrationBuilder.DropColumn(
                name: "MonthCode",
                table: "ProductionIssueAssy");
        }
    }
}
