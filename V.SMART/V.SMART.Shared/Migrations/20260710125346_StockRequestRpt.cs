using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class StockRequestRpt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsStockReq",
                table: "UserAuthority",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IsStockReqLevel",
                table: "UserAuthority",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RefRequestSubId",
                table: "MaterialIssNoteSub",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StockIssueRequest",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToWhom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NoOfItems = table.Column<int>(type: "int", nullable: true),
                    MainRemark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsWithBom = table.Column<bool>(type: "bit", nullable: false),
                    AssyId = table.Column<int>(type: "int", nullable: true),
                    SubAssyId = table.Column<int>(type: "int", nullable: true),
                    SubAssyId2 = table.Column<int>(type: "int", nullable: true),
                    SubAssyId3 = table.Column<int>(type: "int", nullable: true),
                    ReqQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    StoreIssId = table.Column<int>(type: "int", nullable: false),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    ReqTally = table.Column<bool>(type: "bit", nullable: false),
                    IsLevel1 = table.Column<bool>(type: "bit", nullable: false),
                    IsLevel2 = table.Column<bool>(type: "bit", nullable: false),
                    IsLevel3 = table.Column<bool>(type: "bit", nullable: false),
                    IsAuthorized = table.Column<bool>(type: "bit", nullable: false),
                    Level1Sign = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Level2Sign = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Level3Sign = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsRejected = table.Column<bool>(type: "bit", nullable: false),
                    RejectReason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    LastApproved = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockIssueRequest", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_StockIssueRequest_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockIssueRequest_Item_AssyId",
                        column: x => x.AssyId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockIssueRequest_Item_SubAssyId",
                        column: x => x.SubAssyId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockIssueRequest_Item_SubAssyId2",
                        column: x => x.SubAssyId2,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockIssueRequest_Item_SubAssyId3",
                        column: x => x.SubAssyId3,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockIssueRequest_Stores_StoreIssId",
                        column: x => x.StoreIssId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockIssueRequestSub",
                columns: table => new
                {
                    RequestSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UtlQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ReqQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ReqBalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BatchNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Make = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RackNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CostId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockIssueRequestSub", x => x.RequestSubId);
                    table.ForeignKey(
                        name: "FK_StockIssueRequestSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockIssueRequestSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockIssueRequestSub_StockIssueRequest_RequestId",
                        column: x => x.RequestId,
                        principalTable: "StockIssueRequest",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Screens",
                columns: new[] { "Id", "IsPrintRequired", "ScreenCode", "ScreenName" },
                values: new object[,]
                {
                    { 143, false, 143, "Route Card Analysis" },
                    { 144, false, 144, "Stock Issue-Request" },
                    { 145, false, 145, "CreditDebit Summary Report" },
                    { 146, false, 146, "TDSummary Report" },
                    { 147, false, 147, "HSNSummary Report" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssNoteSub_RefRequestSubId",
                table: "MaterialIssNoteSub",
                column: "RefRequestSubId");

            migrationBuilder.CreateIndex(
                name: "IX_StockIssueRequest_AssyId",
                table: "StockIssueRequest",
                column: "AssyId");

            migrationBuilder.CreateIndex(
                name: "IX_StockIssueRequest_CostId",
                table: "StockIssueRequest",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_StockIssueRequest_StoreIssId",
                table: "StockIssueRequest",
                column: "StoreIssId");

            migrationBuilder.CreateIndex(
                name: "IX_StockIssueRequest_SubAssyId",
                table: "StockIssueRequest",
                column: "SubAssyId");

            migrationBuilder.CreateIndex(
                name: "IX_StockIssueRequest_SubAssyId2",
                table: "StockIssueRequest",
                column: "SubAssyId2");

            migrationBuilder.CreateIndex(
                name: "IX_StockIssueRequest_SubAssyId3",
                table: "StockIssueRequest",
                column: "SubAssyId3");

            migrationBuilder.CreateIndex(
                name: "IX_StockIssueRequestSub_CostId",
                table: "StockIssueRequestSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_StockIssueRequestSub_ItemId",
                table: "StockIssueRequestSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StockIssueRequestSub_RequestId",
                table: "StockIssueRequestSub",
                column: "RequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialIssNoteSub_StockIssueRequestSub_RefRequestSubId",
                table: "MaterialIssNoteSub",
                column: "RefRequestSubId",
                principalTable: "StockIssueRequestSub",
                principalColumn: "RequestSubId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaterialIssNoteSub_StockIssueRequestSub_RefRequestSubId",
                table: "MaterialIssNoteSub");

            migrationBuilder.DropTable(
                name: "StockIssueRequestSub");

            migrationBuilder.DropTable(
                name: "StockIssueRequest");

            migrationBuilder.DropIndex(
                name: "IX_MaterialIssNoteSub_RefRequestSubId",
                table: "MaterialIssNoteSub");

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 143);

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 144);

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 145);

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 146);

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 147);

            migrationBuilder.DropColumn(
                name: "IsStockReq",
                table: "UserAuthority");

            migrationBuilder.DropColumn(
                name: "IsStockReqLevel",
                table: "UserAuthority");

            migrationBuilder.DropColumn(
                name: "RefRequestSubId",
                table: "MaterialIssNoteSub");
        }
    }
}
