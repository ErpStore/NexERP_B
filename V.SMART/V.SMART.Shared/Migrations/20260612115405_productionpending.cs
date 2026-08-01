using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class productionpending : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Screens",
                columns: new[] { "Id", "IsPrintRequired", "ScreenCode", "ScreenName" },
                values: new object[] { 130, false, 130, "Production Pending Summary" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 130);
        }
    }
}
