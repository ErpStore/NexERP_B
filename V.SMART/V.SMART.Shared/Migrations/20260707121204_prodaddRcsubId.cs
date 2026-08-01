using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class prodaddRcsubId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RCPSubId",
                table: "ProductionLog",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLog_RCPSubId",
                table: "ProductionLog",
                column: "RCPSubId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLog_RouteCardSub_RCPSubId",
                table: "ProductionLog",
                column: "RCPSubId",
                principalTable: "RouteCardSub",
                principalColumn: "RCSubId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLog_RouteCardSub_RCPSubId",
                table: "ProductionLog");

            migrationBuilder.DropIndex(
                name: "IX_ProductionLog_RCPSubId",
                table: "ProductionLog");

            migrationBuilder.DropColumn(
                name: "RCPSubId",
                table: "ProductionLog");
        }
    }
}
