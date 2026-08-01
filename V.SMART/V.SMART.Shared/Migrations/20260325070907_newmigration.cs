using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class newmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AssemblyPrice",
                table: "Item",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CycleTime",
                table: "Item",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HandlingCharges",
                table: "Item",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsLabourItem",
                table: "Item",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "LabourPrice",
                table: "Item",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LabourPricePerHour",
                table: "Item",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LabourTimeMinutes",
                table: "Item",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AssemblyCost",
                table: "AssmblyDef",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLabourMaterial",
                table: "AssmblyDef",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "LaborCost",
                table: "AssmblyDef",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaterialCost",
                table: "AssmblyDef",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCost",
                table: "AssmblyDef",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "AssmblyDef",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AssemblyCharge",
                columns: table => new
                {
                    AssemblyExtraChargeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssmblyID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAmount = table.Column<bool>(type: "bit", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    Percent = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    IsLabour = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CalculatedAmount = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssemblyCharge", x => x.AssemblyExtraChargeId);
                });

            migrationBuilder.InsertData(
                table: "ScreenManagements",
                columns: new[] { "Id", "Description", "Display", "Required", "ScreenName", "Topic" },
                values: new object[,]
                {
                    { 15, "Displays the Labour Cost section in all applicable screens when enabled.", "Labour Cost", false, "BOMLabourCost", "Enable Labour Cost Display in Each Screen" },
                    { 16, "When enabled, allows users to add and manage labour-related additional charges within the BOM costing screen.", "Add Additional Charges", false, "BOMLabourCost", "Enable Add Additional Charges Button in BOM Screen" },
                    { 17, "When enabled, the system validates during Receive Material entry that the item exists in the BOM and is marked as Customer Material. Only such items will be allowed for receipt.", "Customer Material Validation", false, "BOM", "BOM Customer Material Check in Receive Material" }
                });

            migrationBuilder.InsertData(
                table: "Screens",
                columns: new[] { "Id", "IsPrintRequired", "ScreenCode", "ScreenName" },
                values: new object[,]
                {
                    { 100, false, 100, "LabourCostManagement" },
                    { 101, false, 101, "BOMLabourCost" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssemblyCharge");

            migrationBuilder.DeleteData(
                table: "ScreenManagements",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "ScreenManagements",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "ScreenManagements",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 100);

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DropColumn(
                name: "AssemblyPrice",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "CycleTime",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "HandlingCharges",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "IsLabourItem",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "LabourPrice",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "LabourPricePerHour",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "LabourTimeMinutes",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "AssemblyCost",
                table: "AssmblyDef");

            migrationBuilder.DropColumn(
                name: "IsLabourMaterial",
                table: "AssmblyDef");

            migrationBuilder.DropColumn(
                name: "LaborCost",
                table: "AssmblyDef");

            migrationBuilder.DropColumn(
                name: "MaterialCost",
                table: "AssmblyDef");

            migrationBuilder.DropColumn(
                name: "TotalCost",
                table: "AssmblyDef");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "AssmblyDef");
        }
    }
}
