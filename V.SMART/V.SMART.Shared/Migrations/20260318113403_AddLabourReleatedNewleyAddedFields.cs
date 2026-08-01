using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddLabourReleatedNewleyAddedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customer_GSTNo",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_PANNo",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "ReleaseBalQty",
                table: "RouteCardRelease");

            migrationBuilder.AddColumn<string>(
                name: "CancelBy",
                table: "RouteCardRelease",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelDate",
                table: "RouteCardRelease",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelReason",
                table: "RouteCardRelease",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCancel",
                table: "RouteCardRelease",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShortClose",
                table: "RouteCardRelease",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RefRcReleseId",
                table: "RouteCard",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isOpenPo",
                table: "LabourGRNSub",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_RouteCard_RefRcReleseId",
                table: "RouteCard",
                column: "RefRcReleseId");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_GSTNo",
                table: "Customer",
                column: "GSTNo");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_PANNo",
                table: "Customer",
                column: "PANNo");

            migrationBuilder.AddForeignKey(
                name: "FK_RouteCard_RouteCardRelease_RefRcReleseId",
                table: "RouteCard",
                column: "RefRcReleseId",
                principalTable: "RouteCardRelease",
                principalColumn: "RouteCardReleaseId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RouteCard_RouteCardRelease_RefRcReleseId",
                table: "RouteCard");

            migrationBuilder.DropIndex(
                name: "IX_RouteCard_RefRcReleseId",
                table: "RouteCard");

            migrationBuilder.DropIndex(
                name: "IX_Customer_GSTNo",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_PANNo",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "CancelBy",
                table: "RouteCardRelease");

            migrationBuilder.DropColumn(
                name: "CancelDate",
                table: "RouteCardRelease");

            migrationBuilder.DropColumn(
                name: "CancelReason",
                table: "RouteCardRelease");

            migrationBuilder.DropColumn(
                name: "IsCancel",
                table: "RouteCardRelease");

            migrationBuilder.DropColumn(
                name: "ShortClose",
                table: "RouteCardRelease");

            migrationBuilder.DropColumn(
                name: "RefRcReleseId",
                table: "RouteCard");

            migrationBuilder.DropColumn(
                name: "isOpenPo",
                table: "LabourGRNSub");

            migrationBuilder.AddColumn<decimal>(
                name: "ReleaseBalQty",
                table: "RouteCardRelease",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Customer_GSTNo",
                table: "Customer",
                column: "GSTNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customer_PANNo",
                table: "Customer",
                column: "PANNo",
                unique: true,
                filter: "[PANNo] IS NOT NULL");
        }
    }
}
