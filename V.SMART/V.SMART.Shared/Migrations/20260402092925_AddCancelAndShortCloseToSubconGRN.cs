using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddCancelAndShortCloseToSubconGRN : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Cancel",
                table: "SubConGRN",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CancelBy",
                table: "SubConGRN",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelDate",
                table: "SubConGRN",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelReason",
                table: "SubConGRN",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShortClose",
                table: "SubConGRN",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cancel",
                table: "SubConGRN");

            migrationBuilder.DropColumn(
                name: "CancelBy",
                table: "SubConGRN");

            migrationBuilder.DropColumn(
                name: "CancelDate",
                table: "SubConGRN");

            migrationBuilder.DropColumn(
                name: "CancelReason",
                table: "SubConGRN");

            migrationBuilder.DropColumn(
                name: "ShortClose",
                table: "SubConGRN");
        }
    }
}
