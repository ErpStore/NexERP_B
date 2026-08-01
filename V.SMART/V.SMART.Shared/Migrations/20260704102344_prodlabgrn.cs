using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class prodlabgrn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SplCharAfterID",
                table: "BiometricExcelSetting",
                newName: "IsMonthRow");

            migrationBuilder.RenameColumn(
                name: "PrefixBfrID",
                table: "BiometricExcelSetting",
                newName: "IsColumnsRow");

            migrationBuilder.AddColumn<int>(
                name: "RefGRNSubId",
                table: "ProductionIssueCompSub",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProdCompBalQty",
                table: "LabourGRNSub",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.InsertData(
                table: "ScreenManagements",
                columns: new[] { "Id", "Description", "Display", "Required", "ScreenName", "Topic" },
                values: new object[] { 34, "When this setting is enabled, the system will link Labour GRN transactions with the corresponding Production Issue WO Component records. Users will only be allowed to receive Labour GRN quantities against the linked Production Issue Component, ensuring proper quantity tracking and preventing unrelated Labour GRN entries. If disabled, Labour GRN can be created without enforcing a link to the Production Issue WO Component.", "Labour GRN Link With Production Component", false, "Production Issue WO Component", "Link Labour GRN With Production Issue WO Component" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionIssueCompSub_RefGRNSubId",
                table: "ProductionIssueCompSub",
                column: "RefGRNSubId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionIssueCompSub_LabourGRNSub_RefGRNSubId",
                table: "ProductionIssueCompSub",
                column: "RefGRNSubId",
                principalTable: "LabourGRNSub",
                principalColumn: "GRNSubId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionIssueCompSub_LabourGRNSub_RefGRNSubId",
                table: "ProductionIssueCompSub");

            migrationBuilder.DropIndex(
                name: "IX_ProductionIssueCompSub_RefGRNSubId",
                table: "ProductionIssueCompSub");

            migrationBuilder.DeleteData(
                table: "ScreenManagements",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DropColumn(
                name: "RefGRNSubId",
                table: "ProductionIssueCompSub");

            migrationBuilder.DropColumn(
                name: "ProdCompBalQty",
                table: "LabourGRNSub");

            migrationBuilder.RenameColumn(
                name: "IsMonthRow",
                table: "BiometricExcelSetting",
                newName: "SplCharAfterID");

            migrationBuilder.RenameColumn(
                name: "IsColumnsRow",
                table: "BiometricExcelSetting",
                newName: "PrefixBfrID");
        }
    }
}
