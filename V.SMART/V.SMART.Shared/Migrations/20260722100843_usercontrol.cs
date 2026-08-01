using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class usercontrol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsColumnsRow",
                table: "BiometricExcelSetting");

            migrationBuilder.DropColumn(
                name: "IsMonthRow",
                table: "BiometricExcelSetting");

            migrationBuilder.AddColumn<string>(
                name: "DesktopDeviceId",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DesktopName",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "Users",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDesktop",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMobile",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MobileDeviceId",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobileIpAddress",
                table: "Users",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobileName",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MonthDateColumn",
                table: "BiometricExcelSetting",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MonthDateRow",
                table: "BiometricExcelSetting",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "DesktopDeviceId", "DesktopName", "IpAddress", "IsDesktop", "IsMobile", "MobileDeviceId", "MobileIpAddress", "MobileName" },
                values: new object[] { null, null, null, false, false, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DesktopDeviceId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DesktopName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsDesktop",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsMobile",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MobileDeviceId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MobileIpAddress",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MobileName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MonthDateColumn",
                table: "BiometricExcelSetting");

            migrationBuilder.DropColumn(
                name: "MonthDateRow",
                table: "BiometricExcelSetting");

            migrationBuilder.AddColumn<string>(
                name: "IsColumnsRow",
                table: "BiometricExcelSetting",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IsMonthRow",
                table: "BiometricExcelSetting",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
