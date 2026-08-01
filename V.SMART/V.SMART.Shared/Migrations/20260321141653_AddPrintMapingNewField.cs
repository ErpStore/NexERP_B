using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddPrintMapingNewField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "PrintingMap",
                columns: new[] { "Id", "IsDefault", "ReportName", "ScreenCode", "ScreenName", "SlNo", "TemplateName" },
                values: new object[] { 5, true, "LabGRNPrintingStickerLabel.frx", 92, "Labour GRN", 5, "Standard" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PrintingMap",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
