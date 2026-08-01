using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class addingcredit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CrBalQty",
                table: "MfgInvSub",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ACKNO",
                table: "MfgInv",
                type: "nvarchar(53)",
                maxLength: 53,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ACKNODate",
                table: "MfgInv",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IRNNo",
                table: "MfgInv",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvType",
                table: "MfgInv",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsManualInvNo",
                table: "MfgInv",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignedInvoiceQrCode",
                table: "MfgInv",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsManualDcNo",
                table: "MfgDc",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubSupplyDesc",
                table: "MfgDc",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CrBalQty",
                table: "LabInvSub",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ACKNO",
                table: "LabInv",
                type: "nvarchar(53)",
                maxLength: 53,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ACKNODate",
                table: "LabInv",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IRNNo",
                table: "LabInv",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvType",
                table: "LabInv",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsManualInvNo",
                table: "LabInv",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignedInvoiceQrCode",
                table: "LabInv",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CrBalQty",
                table: "ExpInvSub",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ACKNO",
                table: "ExpInv",
                type: "nvarchar(53)",
                maxLength: 53,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ACKNODate",
                table: "ExpInv",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IRNNo",
                table: "ExpInv",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvType",
                table: "ExpInv",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsManualInvNo",
                table: "ExpInv",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignedInvoiceQrCode",
                table: "ExpInv",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "APIEinvoiceLicenseKey",
                table: "CompanyDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EwayCommodityCode",
                table: "CompanyDetails",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EwayPassword",
                table: "CompanyDetails",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EwayURL",
                table: "CompanyDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EwayUser",
                table: "CompanyDetails",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GSTAddress",
                table: "CompanyDetails",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalName",
                table: "CompanyDetails",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CreditNote",
                columns: table => new
                {
                    CrId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Prefix = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    CreditNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    CreditDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreditDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustId = table.Column<int>(type: "int", nullable: false),
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
                    Sales = table.Column<bool>(type: "bit", nullable: true),
                    Labour = table.Column<bool>(type: "bit", nullable: true),
                    Export = table.Column<bool>(type: "bit", nullable: true),
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
                    table.PrimaryKey("PK_CreditNote", x => x.CrId);
                    table.ForeignKey(
                        name: "FK_CreditNote_Currency_CurrId",
                        column: x => x.CurrId,
                        principalTable: "Currency",
                        principalColumn: "CurrId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CreditNote_CustomerIndirect_ShipAddrsId",
                        column: x => x.ShipAddrsId,
                        principalTable: "CustomerIndirect",
                        principalColumn: "AltCustId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CreditNote_Customer_CustId",
                        column: x => x.CustId,
                        principalTable: "Customer",
                        principalColumn: "CustId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DcRunningNumber",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DcType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LastNumber = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DcRunningNumber", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceAutoRunningNumber",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LastNumber = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceAutoRunningNumber", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CreditNoteSub",
                columns: table => new
                {
                    CrSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CrId = table.Column<int>(type: "int", nullable: false),
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
                    RefInvSubId = table.Column<int>(type: "int", nullable: true),
                    RefPoSubId = table.Column<int>(type: "int", nullable: true),
                    CostId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditNoteSub", x => x.CrSubId);
                    table.ForeignKey(
                        name: "FK_CreditNoteSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CreditNoteSub_CreditNote_CrId",
                        column: x => x.CrId,
                        principalTable: "CreditNote",
                        principalColumn: "CrId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CreditNoteSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CreditNoteSub_MfgInvSub_RefInvSubId",
                        column: x => x.RefInvSubId,
                        principalTable: "MfgInvSub",
                        principalColumn: "InvSubId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ScreenManagements",
                columns: new[] { "Id", "Description", "Display", "Required", "ScreenName", "Topic" },
                values: new object[,]
                {
                    { 10, "When enabled, the E-Way Bill will be generated automatically after successful generation of the E-Invoice, eliminating the need for manual E-Way Bill creation.", "E-Way Bill", false, "Manufacturing Invoice", "E-Way Bill Auto-Generation After E-Invoice" },
                    { 11, "When enabled, a prefix must be defined for the document number. The system will not allow saving or generating the invoice without a valid prefix.", "Prefix", false, "Manufacturing Invoice", "Document Number Prefix Requirement" },
                    { 12, "When enabled, the E-Way Bill will be generated automatically after successful generation of the E-Invoice, eliminating the need for manual E-Way Bill creation.", "E-Way Bill", false, "Export Invoice", "E-Way Bill Auto-Generation After E-Invoice" },
                    { 13, "When enabled, a prefix must be defined for the document number. The system will not allow saving or generating the invoice without a valid prefix.", "Prefix", false, "Export Invoice", "Document Number Prefix Requirement" },
                    { 14, "When enabled, a prefix must be defined for the document number. The system will not allow saving or generating the invoice without a valid prefix.", "Prefix", false, "Labour Invoice", "Document Number Prefix Requirement" }
                });

            migrationBuilder.InsertData(
                table: "Screens",
                columns: new[] { "Id", "IsPrintRequired", "ScreenCode", "ScreenName" },
                values: new object[] { 98, false, 96, "Credit Note" });

            migrationBuilder.InsertData(
                table: "UserRights",
                columns: new[] { "Id", "CanCreate", "CanDelete", "CanEdit", "CanView", "CreatedBy", "CreatedDate", "IsHide", "Remarks", "ScreenCode", "UpdatedBy", "UpdatedDate", "UserId" },
                values: new object[] { 98, true, true, true, true, null, null, false, null, 97, null, null, 1 });

            migrationBuilder.CreateIndex(
                name: "IX_CreditNote_CurrId",
                table: "CreditNote",
                column: "CurrId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNote_CustId",
                table: "CreditNote",
                column: "CustId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNote_ShipAddrsId",
                table: "CreditNote",
                column: "ShipAddrsId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNoteSub_CostId",
                table: "CreditNoteSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNoteSub_CrId",
                table: "CreditNoteSub",
                column: "CrId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNoteSub_ItemId",
                table: "CreditNoteSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNoteSub_RefInvSubId",
                table: "CreditNoteSub",
                column: "RefInvSubId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreditNoteSub");

            migrationBuilder.DropTable(
                name: "DcRunningNumber");

            migrationBuilder.DropTable(
                name: "InvoiceAutoRunningNumber");

            migrationBuilder.DropTable(
                name: "CreditNote");

            migrationBuilder.DeleteData(
                table: "ScreenManagements",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "ScreenManagements",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "ScreenManagements",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "ScreenManagements",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "ScreenManagements",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 98);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 98);

            migrationBuilder.DropColumn(
                name: "CrBalQty",
                table: "MfgInvSub");

            migrationBuilder.DropColumn(
                name: "ACKNO",
                table: "MfgInv");

            migrationBuilder.DropColumn(
                name: "ACKNODate",
                table: "MfgInv");

            migrationBuilder.DropColumn(
                name: "IRNNo",
                table: "MfgInv");

            migrationBuilder.DropColumn(
                name: "InvType",
                table: "MfgInv");

            migrationBuilder.DropColumn(
                name: "IsManualInvNo",
                table: "MfgInv");

            migrationBuilder.DropColumn(
                name: "SignedInvoiceQrCode",
                table: "MfgInv");

            migrationBuilder.DropColumn(
                name: "IsManualDcNo",
                table: "MfgDc");

            migrationBuilder.DropColumn(
                name: "SubSupplyDesc",
                table: "MfgDc");

            migrationBuilder.DropColumn(
                name: "CrBalQty",
                table: "LabInvSub");

            migrationBuilder.DropColumn(
                name: "ACKNO",
                table: "LabInv");

            migrationBuilder.DropColumn(
                name: "ACKNODate",
                table: "LabInv");

            migrationBuilder.DropColumn(
                name: "IRNNo",
                table: "LabInv");

            migrationBuilder.DropColumn(
                name: "InvType",
                table: "LabInv");

            migrationBuilder.DropColumn(
                name: "IsManualInvNo",
                table: "LabInv");

            migrationBuilder.DropColumn(
                name: "SignedInvoiceQrCode",
                table: "LabInv");

            migrationBuilder.DropColumn(
                name: "CrBalQty",
                table: "ExpInvSub");

            migrationBuilder.DropColumn(
                name: "ACKNO",
                table: "ExpInv");

            migrationBuilder.DropColumn(
                name: "ACKNODate",
                table: "ExpInv");

            migrationBuilder.DropColumn(
                name: "IRNNo",
                table: "ExpInv");

            migrationBuilder.DropColumn(
                name: "InvType",
                table: "ExpInv");

            migrationBuilder.DropColumn(
                name: "IsManualInvNo",
                table: "ExpInv");

            migrationBuilder.DropColumn(
                name: "SignedInvoiceQrCode",
                table: "ExpInv");

            migrationBuilder.DropColumn(
                name: "APIEinvoiceLicenseKey",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "EwayCommodityCode",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "EwayPassword",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "EwayURL",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "EwayUser",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "GSTAddress",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "LegalName",
                table: "CompanyDetails");
        }
    }
}
