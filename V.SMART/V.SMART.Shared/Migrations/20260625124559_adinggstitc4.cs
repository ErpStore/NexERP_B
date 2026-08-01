using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class adinggstitc4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemHistory",
                columns: table => new
                {
                    HistoryId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: true),
                    ItemCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Specification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DrawingNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Barcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QRCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BtOtorMfg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CategoryCode = table.Column<int>(type: "int", nullable: true),
                    MeasureUnit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PurchaseUnit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnitConvert = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    RackNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ROL = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    MOL = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    LeadTime = table.Column<int>(type: "int", nullable: true),
                    PartWeight = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    TotalArea = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    BinLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsiderAsRm = table.Column<bool>(type: "bit", nullable: true),
                    Make = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RmId = table.Column<int>(type: "int", nullable: true),
                    Shape = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Density = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    Length = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    Width = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    Height = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    Thickness = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    WallThickness = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    OuterDia = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    InnerDia = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    BatchNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rate = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    AltRate = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    HSNCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SACCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsServcS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsServcL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CGST = table.Column<decimal>(type: "decimal(5,3)", nullable: true),
                    SGST = table.Column<decimal>(type: "decimal(5,3)", nullable: true),
                    IGST = table.Column<decimal>(type: "decimal(5,3)", nullable: true),
                    Rev = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RevItemId = table.Column<int>(type: "int", nullable: true),
                    LabourPricePerHour = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    CycleTime = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    LabourTimeMinutes = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    LabourPrice = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    AssemblyPrice = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    IsLabourItem = table.Column<bool>(type: "bit", nullable: true),
                    HandlingCharges = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Hide = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemHistory", x => x.HistoryId);
                });

            migrationBuilder.InsertData(
                table: "Screens",
                columns: new[] { "Id", "IsPrintRequired", "ScreenCode", "ScreenName" },
                values: new object[] { 132, false, 132, "GSTITC04" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemHistory");

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 132);
        }
    }
}
