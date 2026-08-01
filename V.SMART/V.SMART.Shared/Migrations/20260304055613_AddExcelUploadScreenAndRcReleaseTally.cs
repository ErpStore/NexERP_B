using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddExcelUploadScreenAndRcReleaseTally : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RcReleaseTally",
                table: "RouteCardRelease",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "Screens",
                columns: new[] { "Id", "IsPrintRequired", "ScreenCode", "ScreenName" },
                values: new object[] { 97, false, 97, "Excel Upload" });

            migrationBuilder.InsertData(
                table: "UserRights",
                columns: new[] { "Id", "CanCreate", "CanDelete", "CanEdit", "CanView", "CreatedBy", "CreatedDate", "IsHide", "Remarks", "ScreenCode", "UpdatedBy", "UpdatedDate", "UserId" },
                values: new object[] { 97, true, true, true, true, null, null, false, null, 97, null, null, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 97);

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 97);

            migrationBuilder.DropColumn(
                name: "RcReleaseTally",
                table: "RouteCardRelease");
        }
    }
}
