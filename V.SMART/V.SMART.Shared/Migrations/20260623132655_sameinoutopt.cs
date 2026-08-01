using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class sameinoutopt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "SubConDcOutSub",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOutItemSameAsInItem",
                table: "SubConDcOut",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RcSubId",
                table: "ProductionIssueAssySub",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionIssueAssySub_RcSubId",
                table: "ProductionIssueAssySub",
                column: "RcSubId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionIssueAssySub_RouteCardSub_RcSubId",
                table: "ProductionIssueAssySub",
                column: "RcSubId",
                principalTable: "RouteCardSub",
                principalColumn: "RCSubId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionIssueAssySub_RouteCardSub_RcSubId",
                table: "ProductionIssueAssySub");

            migrationBuilder.DropIndex(
                name: "IX_ProductionIssueAssySub_RcSubId",
                table: "ProductionIssueAssySub");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "SubConDcOutSub");

            migrationBuilder.DropColumn(
                name: "IsOutItemSameAsInItem",
                table: "SubConDcOut");

            migrationBuilder.DropColumn(
                name: "RcSubId",
                table: "ProductionIssueAssySub");
        }
    }
}
