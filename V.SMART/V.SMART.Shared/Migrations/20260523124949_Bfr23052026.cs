using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class Bfr23052026 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsServiceBill",
                table: "PurchPo",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsServiceBill",
                table: "MfgPo",
                type: "bit",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ServiceBills",
                columns: table => new
                {
                    ServiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncomingServics = table.Column<bool>(type: "bit", nullable: false),
                    InvNo = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InvDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustVendorId = table.Column<int>(type: "int", nullable: false),
                    NoOfItems = table.Column<int>(type: "int", nullable: true),
                    MainRemark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DcNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DcDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModeofTrasnport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VehicleNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrId = table.Column<int>(type: "int", nullable: false),
                    TodayVal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsCancel = table.Column<bool>(type: "bit", nullable: false),
                    ShortClose = table.Column<bool>(type: "bit", nullable: false),
                    CancelReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PayTerms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalGrossAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IsItemWiseDiscAmtOrPerReq = table.Column<bool>(type: "bit", nullable: false),
                    DiscAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalBasicAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    FreightCharges = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PackingAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    PackingPercent = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PackingCharges = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    InsuranceAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    InsurancePercent = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    InsuranceCharges = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CGstRate = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    SGstRate = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    IGstRate = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    TotalCGSTAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalSGSTAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalIGSTAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    OtherCharges = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TCSAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    TCSPercent = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TCSAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalTaxable = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DebitAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DebitRem = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Debit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IsRoundOffEnabled = table.Column<bool>(type: "bit", nullable: false),
                    RoundOff = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ServiceTally = table.Column<bool>(type: "bit", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceBills", x => x.ServiceId);
                    table.ForeignKey(
                        name: "FK_ServiceBills_Currency_CurrId",
                        column: x => x.CurrId,
                        principalTable: "Currency",
                        principalColumn: "CurrId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceBillsSub",
                columns: table => new
                {
                    ServiceBillSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    LineId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    LineGross = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    LineDiscountPercent = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    LineDiscountAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    LineBasicAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    LineCGSTRate = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    LineSGSTRate = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    LineIGSTRate = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    RefMfgPoSubId = table.Column<int>(type: "int", nullable: true),
                    RefPurchPoSubId = table.Column<int>(type: "int", nullable: true),
                    RefPoNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefPoDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PoDueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TypeOfPo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    ItemCancel = table.Column<bool>(type: "bit", nullable: false),
                    ItemCancelReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceBillsSub", x => x.ServiceBillSubId);
                    table.ForeignKey(
                        name: "FK_ServiceBillsSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceBillsSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceBillsSub_MfgPoSub_RefMfgPoSubId",
                        column: x => x.RefMfgPoSubId,
                        principalTable: "MfgPoSub",
                        principalColumn: "PoSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceBillsSub_PurchPoSub_RefPurchPoSubId",
                        column: x => x.RefPurchPoSubId,
                        principalTable: "PurchPoSub",
                        principalColumn: "PoSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceBillsSub_ServiceBills_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "ServiceBills",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Screens",
                columns: new[] { "Id", "IsPrintRequired", "ScreenCode", "ScreenName" },
                values: new object[] { 116, false, 116, "Service Bills" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBills_CurrId",
                table: "ServiceBills",
                column: "CurrId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBillsSub_CostId",
                table: "ServiceBillsSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBillsSub_ItemId",
                table: "ServiceBillsSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBillsSub_RefMfgPoSubId",
                table: "ServiceBillsSub",
                column: "RefMfgPoSubId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBillsSub_RefPurchPoSubId",
                table: "ServiceBillsSub",
                column: "RefPurchPoSubId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBillsSub_ServiceId",
                table: "ServiceBillsSub",
                column: "ServiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceBillsSub");

            migrationBuilder.DropTable(
                name: "ServiceBills");

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 116);

            migrationBuilder.DropColumn(
                name: "IsServiceBill",
                table: "PurchPo");

            migrationBuilder.DropColumn(
                name: "IsServiceBill",
                table: "MfgPo");
        }
    }
}
