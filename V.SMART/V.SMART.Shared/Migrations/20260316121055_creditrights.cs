using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class creditrights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 98,
                column: "ScreenCode",
                value: 98);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 98,
                column: "ScreenCode",
                value: 97);
        }
    }
}
