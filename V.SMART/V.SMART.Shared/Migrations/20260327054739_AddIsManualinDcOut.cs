using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddIsManualinDcOut : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsManualDcNo",
                table: "LabourDcOutgoing",
                type: "bit",
                nullable: true);

            migrationBuilder.InsertData(
                table: "ScreenManagements",
                columns: new[] { "Id", "Description", "Display", "Required", "ScreenName", "Topic" },
                values: new object[] { 18, "When enabled, the Labour button in the Sales Order screen will be set to true by default for all new entries, ensuring the Labour option is readily available without manual activation.", "Enable Labour Button by Default", false, "Sales-Order", "Automatically enable Labour button in Sales PO" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ScreenManagements",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DropColumn(
                name: "IsManualDcNo",
                table: "LabourDcOutgoing");
        }
    }
}
