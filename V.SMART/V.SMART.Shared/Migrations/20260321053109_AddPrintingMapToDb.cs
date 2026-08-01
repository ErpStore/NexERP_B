using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddPrintingMapToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PrintingMap",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    TemplateName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScreenCode = table.Column<int>(type: "int", nullable: true),
                    ScreenName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrintingMap", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrintingMap_Screens_ScreenCode",
                        column: x => x.ScreenCode,
                        principalTable: "Screens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "PrintingMap",
                columns: new[] { "Id", "IsDefault", "ReportName", "ScreenCode", "ScreenName", "SlNo", "TemplateName" },
                values: new object[,]
                {
                    { 1, true, "LabourGRN.frx", 92, "Labour GRN", 1, "Standard" },
                    { 2, true, "LabourSCN.frx", 93, "Labour SCN", 2, "Standard" },
                    { 3, true, "LabDC.frx", 94, "Labour Delivery Challan", 3, "Standard" },
                    { 4, true, "LabInvoice.frx", 95, "Labour Invoice", 4, "Standard" }
                });

            migrationBuilder.InsertData(
                table: "Screens",
                columns: new[] { "Id", "IsPrintRequired", "ScreenCode", "ScreenName" },
                values: new object[] { 99, false, 99, "Printing Map" });

            migrationBuilder.InsertData(
                table: "UserRights",
                columns: new[] { "Id", "CanCreate", "CanDelete", "CanEdit", "CanView", "CreatedBy", "CreatedDate", "IsHide", "Remarks", "ScreenCode", "UpdatedBy", "UpdatedDate", "UserId" },
                values: new object[] { 99, true, true, true, true, null, null, false, null, 99, null, null, 1 });

            migrationBuilder.CreateIndex(
                name: "IX_PrintingMap_ScreenCode",
                table: "PrintingMap",
                column: "ScreenCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrintingMap");

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 99);

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 99);
        }
    }
}
