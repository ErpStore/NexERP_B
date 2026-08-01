using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class prodandSubcon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ScreenManagements",
                columns: new[] { "Id", "Description", "Display", "Required", "ScreenName", "Topic" },
                values: new object[,]
                {
                    { 31, "When this setting is enabled, Production Issue WO Component will be processed PO-wise. After completing the Production Issue WO Component against a PO, the system will automatically close the respective PO and prevent further transactions.", "PO Wise Production Issue WO Component", false, "Production Issue WO Component", "PO Wise Production Issue WO Component & Auto Close" },
                    { 32, "When this setting is enabled, the Output Item in the Production Issue WO Component must be the same as the Input Item selected in the transaction. The system will restrict users from selecting a different Output Item. If disabled, users can select any item as the Output Item in the Production Issue GRN Component  process.", "Restrict Out Item Same As In Item", false, "Production Issue WO Component", "Ensure Output Item Matches Input Item in Production Issue WO Component" },
                    { 33, "When this setting is enabled, the Output Item in the Sub-Contract DC-GRN must be the same as the Input Item selected in the transaction. The system will restrict users from selecting a different Output Item. If disabled, users can select any item as the Output Item in the Sub-Contract DC GRN process.", "Restrict Out Item Same As In Item", false, "Sub-Contract DC-Out", "Ensure Output Item Matches Input Item in Sub-Contract DC-Out" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ScreenManagements",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "ScreenManagements",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "ScreenManagements",
                keyColumn: "Id",
                keyValue: 33);
        }
    }
}
