using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class Estimation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Estimate",
                columns: table => new
                {
                    EstiamateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustId = table.Column<int>(type: "int", nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CostCenterId = table.Column<int>(type: "int", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    EstiamateNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstimateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TypeofWork = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnquiryNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Overp = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Marp = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Riskp = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Packingper = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Logesticper = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Over = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Mar = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Risk = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Packing = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Logestic = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MtlSQR = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FnsizeSQR = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    finsqrlen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RmthSQR = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RmwdSQR = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RmlenSQR = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DenSQR = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RmkgSQR = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RmrateSQR = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RmcostSQR = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MtlRND = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FnsizeRND = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RmdiaRND = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RmlenRND = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DensityRND = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RmkgRND = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RmrateRND = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RmcostRND = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Castwt = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CastAmt = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CutSize = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Conv = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    totalRmcost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Totalcost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Grand = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsManualEstimate = table.Column<bool>(type: "bit", nullable: false),
                    RefEnqSubId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estimate", x => x.EstiamateId);
                    table.ForeignKey(
                        name: "FK_Estimate_CostCenter_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Estimate_Customer_CustId",
                        column: x => x.CustId,
                        principalTable: "Customer",
                        principalColumn: "CustId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Estimate_EnquirySalesSub_RefEnqSubId",
                        column: x => x.RefEnqSubId,
                        principalTable: "EnquirySalesSub",
                        principalColumn: "EnquirySubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Estimate_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EstimateSub",
                columns: table => new
                {
                    EstiamateSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstiamateId = table.Column<int>(type: "int", nullable: false),
                    ProcessId = table.Column<int>(type: "int", nullable: false),
                    HrlyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Hrs = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RefEnqSubId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstimateSub", x => x.EstiamateSubId);
                    table.ForeignKey(
                        name: "FK_EstimateSub_EnquirySalesSub_RefEnqSubId",
                        column: x => x.RefEnqSubId,
                        principalTable: "EnquirySalesSub",
                        principalColumn: "EnquirySubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EstimateSub_Estimate_EstiamateId",
                        column: x => x.EstiamateId,
                        principalTable: "Estimate",
                        principalColumn: "EstiamateId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EstimateSub_Process_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Process",
                        principalColumn: "ProcessId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Screens",
                columns: new[] { "Id", "IsPrintRequired", "ScreenCode", "ScreenName" },
                values: new object[] { 142, false, 142, "Estimation" });

            migrationBuilder.CreateIndex(
                name: "IX_Estimate_CostCenterId",
                table: "Estimate",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_Estimate_CustId",
                table: "Estimate",
                column: "CustId");

            migrationBuilder.CreateIndex(
                name: "IX_Estimate_ItemId",
                table: "Estimate",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Estimate_RefEnqSubId",
                table: "Estimate",
                column: "RefEnqSubId");

            migrationBuilder.CreateIndex(
                name: "IX_EstimateSub_EstiamateId",
                table: "EstimateSub",
                column: "EstiamateId");

            migrationBuilder.CreateIndex(
                name: "IX_EstimateSub_ProcessId",
                table: "EstimateSub",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_EstimateSub_RefEnqSubId",
                table: "EstimateSub",
                column: "RefEnqSubId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EstimateSub");

            migrationBuilder.DropTable(
                name: "Estimate");

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 142);
        }
    }
}
