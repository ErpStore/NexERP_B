using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AdCostIdToRouteCardRelese : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CostId",
                table: "RouteCardReleaseSub",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RouteCardReleaseSub_CostId",
                table: "RouteCardReleaseSub",
                column: "CostId");

            migrationBuilder.AddForeignKey(
                name: "FK_RouteCardReleaseSub_CostCenter_CostId",
                table: "RouteCardReleaseSub",
                column: "CostId",
                principalTable: "CostCenter",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RouteCardReleaseSub_CostCenter_CostId",
                table: "RouteCardReleaseSub");

            migrationBuilder.DropIndex(
                name: "IX_RouteCardReleaseSub_CostId",
                table: "RouteCardReleaseSub");

            migrationBuilder.DropColumn(
                name: "CostId",
                table: "RouteCardReleaseSub");
        }
    }
}
