using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class initialdatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefDcDate",
                table: "LabInvSub");

            migrationBuilder.DropColumn(
                name: "RefDcNo",
                table: "LabInvSub");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RefDcDate",
                table: "LabInvSub",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefDcNo",
                table: "LabInvSub",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
