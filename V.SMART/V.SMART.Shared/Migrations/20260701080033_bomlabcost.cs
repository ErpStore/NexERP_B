using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class bomlabcost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "M1",
                table: "BiometricExcelSetting");

            migrationBuilder.RenameColumn(
                name: "W1",
                table: "BiometricExcelSetting",
                newName: "W");

            migrationBuilder.RenameColumn(
                name: "P2",
                table: "BiometricExcelSetting",
                newName: "P");

            migrationBuilder.RenameColumn(
                name: "P1",
                table: "BiometricExcelSetting",
                newName: "O");

            migrationBuilder.RenameColumn(
                name: "M2",
                table: "BiometricExcelSetting",
                newName: "M");

            migrationBuilder.AddColumn<string>(
                name: "AttendanceCode",
                table: "LeaveType",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "DeductSalary",
                table: "LeaveType",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PaidLeave",
                table: "LeaveType",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AssmblyDefLabour",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssmblyID = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UtilQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Cancel = table.Column<bool>(type: "bit", nullable: false),
                    IsComponent = table.Column<bool>(type: "bit", nullable: false),
                    ProcessId = table.Column<int>(type: "int", nullable: true),
                    StockConvertedQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PRConvertedQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Completed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    MaterialCost = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    LaborCost = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    TotalCost = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    AssemblyCost = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    IsLabourMaterial = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssmblyDefLabour", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssmblyDefLabour_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssmblyDefLabour_Item_AssmblyID",
                        column: x => x.AssmblyID,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssmblyDefLabour_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssmblyDefLabour_Process_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Process",
                        principalColumn: "ProcessId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Screens",
                columns: new[] { "Id", "IsPrintRequired", "ScreenCode", "ScreenName" },
                values: new object[] { 139, false, 139, "BOM Labour" });

            migrationBuilder.CreateIndex(
                name: "IX_AssmblyDefLabour_AssmblyID_ItemId",
                table: "AssmblyDefLabour",
                columns: new[] { "AssmblyID", "ItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssmblyDefLabour_CostId",
                table: "AssmblyDefLabour",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_AssmblyDefLabour_ItemId",
                table: "AssmblyDefLabour",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AssmblyDefLabour_ProcessId",
                table: "AssmblyDefLabour",
                column: "ProcessId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssmblyDefLabour");

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 139);

            migrationBuilder.DropColumn(
                name: "AttendanceCode",
                table: "LeaveType");

            migrationBuilder.DropColumn(
                name: "DeductSalary",
                table: "LeaveType");

            migrationBuilder.DropColumn(
                name: "PaidLeave",
                table: "LeaveType");

            migrationBuilder.RenameColumn(
                name: "W",
                table: "BiometricExcelSetting",
                newName: "W1");

            migrationBuilder.RenameColumn(
                name: "P",
                table: "BiometricExcelSetting",
                newName: "P2");

            migrationBuilder.RenameColumn(
                name: "O",
                table: "BiometricExcelSetting",
                newName: "P1");

            migrationBuilder.RenameColumn(
                name: "M",
                table: "BiometricExcelSetting",
                newName: "M2");

            migrationBuilder.AddColumn<string>(
                name: "M1",
                table: "BiometricExcelSetting",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
