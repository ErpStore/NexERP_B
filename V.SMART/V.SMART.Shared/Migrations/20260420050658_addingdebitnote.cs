using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class addingdebitnote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DebitNote",
                columns: table => new
                {
                    DbId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Prefix = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    DebitNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    DebitDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DebitDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VendorCode = table.Column<int>(type: "int", nullable: false),
                    KindOfAttention = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    IsSameAsShipping = table.Column<bool>(type: "bit", nullable: false),
                    ShipAddrsId = table.Column<int>(type: "int", nullable: true),
                    NoOfItems = table.Column<int>(type: "int", nullable: true),
                    MainRemark = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CurrId = table.Column<int>(type: "int", nullable: false),
                    TodayVal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsManualInvNo = table.Column<bool>(type: "bit", nullable: true),
                    Rejection = table.Column<bool>(type: "bit", nullable: true),
                    RateDifference = table.Column<bool>(type: "bit", nullable: true),
                    Purchase = table.Column<bool>(type: "bit", nullable: true),
                    SubContract = table.Column<bool>(type: "bit", nullable: true),
                    ACKNO = table.Column<string>(type: "nvarchar(53)", maxLength: 53, nullable: true),
                    IRNNo = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    SignedInvoiceQrCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ACKNODate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvType = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    TotalGrossAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsItemWiseDiscAmtOrPerReq = table.Column<bool>(type: "bit", nullable: false),
                    DiscAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalBasicAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FreightCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PackingAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    PackingPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PackingCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InsuranceAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    InsurancePercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InsuranceCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    SGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    IGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TotalCGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalSGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalIGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OtherCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TCSAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    TCSPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TCSAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalTaxable = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RoundOff = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsRoundOffEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DebitNote", x => x.DbId);
                    table.ForeignKey(
                        name: "FK_DebitNote_Currency_CurrId",
                        column: x => x.CurrId,
                        principalTable: "Currency",
                        principalColumn: "CurrId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DebitNote_CustomerIndirect_ShipAddrsId",
                        column: x => x.ShipAddrsId,
                        principalTable: "CustomerIndirect",
                        principalColumn: "AltCustId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DebitNote_Vendor_VendorCode",
                        column: x => x.VendorCode,
                        principalTable: "Vendor",
                        principalColumn: "VendorCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DebitNoteSub",
                columns: table => new
                {
                    DbSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DbId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RejQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RewQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    CrDrQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    CrDrUnitPrice = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    LineGross = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineDiscountPercent = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    LineDiscountAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LineBasicAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineCGSTRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LineSGSTRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LineIGSTRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RefPurchInvSubId = table.Column<int>(type: "int", nullable: true),
                    RefSubConInvSubId = table.Column<int>(type: "int", nullable: true),
                    RefPoSubId = table.Column<int>(type: "int", nullable: true),
                    CostId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DebitNoteSub", x => x.DbSubId);
                    table.ForeignKey(
                        name: "FK_DebitNoteSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DebitNoteSub_DebitNote_DbId",
                        column: x => x.DbId,
                        principalTable: "DebitNote",
                        principalColumn: "DbId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DebitNoteSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DebitNoteSub_PurchaseInvoiceSub_RefPurchInvSubId",
                        column: x => x.RefPurchInvSubId,
                        principalTable: "PurchaseInvoiceSub",
                        principalColumn: "InvSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DebitNoteSub_SubConInvSub_RefSubConInvSubId",
                        column: x => x.RefSubConInvSubId,
                        principalTable: "SubConInvSub",
                        principalColumn: "InvSubId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Screens",
                columns: new[] { "Id", "IsPrintRequired", "ScreenCode", "ScreenName" },
                values: new object[] { 103, false, 103, "Debit Note" });

            migrationBuilder.CreateIndex(
                name: "IX_DebitNote_CurrId",
                table: "DebitNote",
                column: "CurrId");

            migrationBuilder.CreateIndex(
                name: "IX_DebitNote_ShipAddrsId",
                table: "DebitNote",
                column: "ShipAddrsId");

            migrationBuilder.CreateIndex(
                name: "IX_DebitNote_VendorCode",
                table: "DebitNote",
                column: "VendorCode");

            migrationBuilder.CreateIndex(
                name: "IX_DebitNoteSub_CostId",
                table: "DebitNoteSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_DebitNoteSub_DbId",
                table: "DebitNoteSub",
                column: "DbId");

            migrationBuilder.CreateIndex(
                name: "IX_DebitNoteSub_ItemId",
                table: "DebitNoteSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DebitNoteSub_RefPurchInvSubId",
                table: "DebitNoteSub",
                column: "RefPurchInvSubId");

            migrationBuilder.CreateIndex(
                name: "IX_DebitNoteSub_RefSubConInvSubId",
                table: "DebitNoteSub",
                column: "RefSubConInvSubId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DebitNoteSub");

            migrationBuilder.DropTable(
                name: "DebitNote");

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 103);
        }
    }
}
