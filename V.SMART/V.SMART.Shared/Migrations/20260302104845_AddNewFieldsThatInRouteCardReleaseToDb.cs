using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddNewFieldsThatInRouteCardReleaseToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustId",
                table: "RouteCardRelease",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "RcReleaseDate",
                table: "RouteCardRelease",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "RcReleaseNo",
                table: "RouteCardRelease",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Suffix",
                table: "RouteCardRelease",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_RouteCardRelease_CustId",
                table: "RouteCardRelease",
                column: "CustId");

            migrationBuilder.AddForeignKey(
                name: "FK_RouteCardRelease_Customer_CustId",
                table: "RouteCardRelease",
                column: "CustId",
                principalTable: "Customer",
                principalColumn: "CustId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RouteCardRelease_Customer_CustId",
                table: "RouteCardRelease");

            migrationBuilder.DropIndex(
                name: "IX_RouteCardRelease_CustId",
                table: "RouteCardRelease");

            migrationBuilder.DropColumn(
                name: "CustId",
                table: "RouteCardRelease");

            migrationBuilder.DropColumn(
                name: "RcReleaseDate",
                table: "RouteCardRelease");

            migrationBuilder.DropColumn(
                name: "RcReleaseNo",
                table: "RouteCardRelease");

            migrationBuilder.DropColumn(
                name: "Suffix",
                table: "RouteCardRelease");
        }
    }
}
