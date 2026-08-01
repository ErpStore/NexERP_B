using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class prodComp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SCNBalQty",
                table: "SubConSCNSub",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SCNBalQty",
                table: "ProductionSCNCompSub",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "RefPoSubId",
                table: "ProductionReturnCompTrack",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Cancel",
                table: "ProductionReturnComp",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CancelBy",
                table: "ProductionReturnComp",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelDate",
                table: "ProductionReturnComp",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelReason",
                table: "ProductionReturnComp",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsManual",
                table: "ProductionReturnComp",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWithoutPoDc",
                table: "ProductionReturnComp",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShortClose",
                table: "ProductionReturnComp",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Cancel",
                table: "ProductionIssueComp",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CancelBy",
                table: "ProductionIssueComp",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelDate",
                table: "ProductionIssueComp",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelReason",
                table: "ProductionIssueComp",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOutItemSameAsInItem",
                table: "ProductionIssueComp",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWithoutPoDc",
                table: "ProductionIssueComp",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShortClose",
                table: "ProductionIssueComp",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnCompTrack_RefPoSubId",
                table: "ProductionReturnCompTrack",
                column: "RefPoSubId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionReturnCompTrack_MfgPoSub_RefPoSubId",
                table: "ProductionReturnCompTrack",
                column: "RefPoSubId",
                principalTable: "MfgPoSub",
                principalColumn: "PoSubId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionReturnCompTrack_MfgPoSub_RefPoSubId",
                table: "ProductionReturnCompTrack");

            migrationBuilder.DropIndex(
                name: "IX_ProductionReturnCompTrack_RefPoSubId",
                table: "ProductionReturnCompTrack");

            migrationBuilder.DropColumn(
                name: "SCNBalQty",
                table: "SubConSCNSub");

            migrationBuilder.DropColumn(
                name: "SCNBalQty",
                table: "ProductionSCNCompSub");

            migrationBuilder.DropColumn(
                name: "RefPoSubId",
                table: "ProductionReturnCompTrack");

            migrationBuilder.DropColumn(
                name: "Cancel",
                table: "ProductionReturnComp");

            migrationBuilder.DropColumn(
                name: "CancelBy",
                table: "ProductionReturnComp");

            migrationBuilder.DropColumn(
                name: "CancelDate",
                table: "ProductionReturnComp");

            migrationBuilder.DropColumn(
                name: "CancelReason",
                table: "ProductionReturnComp");

            migrationBuilder.DropColumn(
                name: "IsManual",
                table: "ProductionReturnComp");

            migrationBuilder.DropColumn(
                name: "IsWithoutPoDc",
                table: "ProductionReturnComp");

            migrationBuilder.DropColumn(
                name: "ShortClose",
                table: "ProductionReturnComp");

            migrationBuilder.DropColumn(
                name: "Cancel",
                table: "ProductionIssueComp");

            migrationBuilder.DropColumn(
                name: "CancelBy",
                table: "ProductionIssueComp");

            migrationBuilder.DropColumn(
                name: "CancelDate",
                table: "ProductionIssueComp");

            migrationBuilder.DropColumn(
                name: "CancelReason",
                table: "ProductionIssueComp");

            migrationBuilder.DropColumn(
                name: "IsOutItemSameAsInItem",
                table: "ProductionIssueComp");

            migrationBuilder.DropColumn(
                name: "IsWithoutPoDc",
                table: "ProductionIssueComp");

            migrationBuilder.DropColumn(
                name: "ShortClose",
                table: "ProductionIssueComp");
        }
    }
}
