using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddCancelDataAndCancelByAndOpenPoToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOpenPo",
                table: "SubConDcOutSub",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CancelBy",
                table: "SubConDcOut",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelDate",
                table: "SubConDcOut",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOpenPo",
                table: "SubConDcOutSub");

            migrationBuilder.DropColumn(
                name: "CancelBy",
                table: "SubConDcOut");

            migrationBuilder.DropColumn(
                name: "CancelDate",
                table: "SubConDcOut");
        }
    }
}
