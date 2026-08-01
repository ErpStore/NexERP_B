using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class expters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeliveryTems",
                table: "ExpInv",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayTerms",
                table: "ExpInv",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "EstimateSub",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Density",
                table: "Estimate",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Height",
                table: "Estimate",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InnerDia",
                table: "Estimate",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Length",
                table: "Estimate",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OuterDia",
                table: "Estimate",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RmPrice",
                table: "Estimate",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RmTotAmount",
                table: "Estimate",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Shape",
                table: "Estimate",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Thickness",
                table: "Estimate",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WallThickness",
                table: "Estimate",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                table: "Estimate",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Width",
                table: "Estimate",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEstiItemTally",
                table: "EnquirySalesSub",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "Screens",
                columns: new[] { "Id", "IsPrintRequired", "ScreenCode", "ScreenName" },
                values: new object[] { 148, false, 148, "PR PO Rating Report" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 148);

            migrationBuilder.DropColumn(
                name: "DeliveryTems",
                table: "ExpInv");

            migrationBuilder.DropColumn(
                name: "PayTerms",
                table: "ExpInv");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "EstimateSub");

            migrationBuilder.DropColumn(
                name: "Density",
                table: "Estimate");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "Estimate");

            migrationBuilder.DropColumn(
                name: "InnerDia",
                table: "Estimate");

            migrationBuilder.DropColumn(
                name: "Length",
                table: "Estimate");

            migrationBuilder.DropColumn(
                name: "OuterDia",
                table: "Estimate");

            migrationBuilder.DropColumn(
                name: "RmPrice",
                table: "Estimate");

            migrationBuilder.DropColumn(
                name: "RmTotAmount",
                table: "Estimate");

            migrationBuilder.DropColumn(
                name: "Shape",
                table: "Estimate");

            migrationBuilder.DropColumn(
                name: "Thickness",
                table: "Estimate");

            migrationBuilder.DropColumn(
                name: "WallThickness",
                table: "Estimate");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Estimate");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "Estimate");

            migrationBuilder.DropColumn(
                name: "IsEstiItemTally",
                table: "EnquirySalesSub");
        }
    }
}
