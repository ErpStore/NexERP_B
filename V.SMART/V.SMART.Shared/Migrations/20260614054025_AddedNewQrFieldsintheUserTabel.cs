using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddedNewQrFieldsintheUserTabel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsQrEnabled",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastQrLogin",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "QrCreatedDate",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "QrExpiryDate",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "QrToken",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "IsQrEnabled", "LastQrLogin", "QrCreatedDate", "QrExpiryDate", "QrToken" },
                values: new object[] { false, null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsQrEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastQrLogin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "QrCreatedDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "QrExpiryDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "QrToken",
                table: "Users");
        }
    }
}
