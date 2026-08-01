using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class addingbillpending : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 114,
                column: "ScreenName",
                value: "Bill Pending List");

            migrationBuilder.UpdateData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 115,
                column: "ScreenName",
                value: "Bill Paid List");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 114,
                column: "ScreenName",
                value: "Bills Pending List");

            migrationBuilder.UpdateData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 115,
                column: "ScreenName",
                value: "Bills Paid List");
        }
    }
}
