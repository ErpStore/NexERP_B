using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class Insert_ScrenManagemnet_CustomerBasedItemRestriction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ScreenManagements",
                columns: new[] { "Id", "Description", "Display", "Required", "ScreenName", "Topic" },
                values: new object[] { 8, "When this setting is enabled, transaction screens will load only those items that are allocated to the selected customer in the Item Master (Item Customer Assign). If disabled, all active items will be available for selection regardless of customer allocation.", "Customer-Based Item Restriction", false, "Items", "Restrict Item Loading Based on Customer Allocation" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ScreenManagements",
                keyColumn: "Id",
                keyValue: 8);
        }
    }
}
