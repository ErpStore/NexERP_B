using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class DashboardNameChage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 110,
                column: "ScreenName",
                value: "Dashboard");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 110,
                column: "ScreenName",
                value: "Dashbord");
        }
    }
}
