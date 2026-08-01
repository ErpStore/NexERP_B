using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddLabourGrnSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ScreenManagements",
                columns: new[] { "Id", "Description", "Display", "Required", "ScreenName", "Topic" },
                values: new object[] { 9, "When this setting is enabled, the Output Item in the Labour GRN must be the same as the Input Item selected in the transaction. The system will restrict users from selecting a different Output Item. If disabled, users can select any item as the Output Item in the Labour GRN process.", "Restrict Out Item Same As In Item", false, "Labour GRN", "Ensure Output Item Matches Input Item in Labour GRN" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ScreenManagements",
                keyColumn: "Id",
                keyValue: 9);
        }
    }
}
