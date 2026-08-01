using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class addingprocessinflabinvwithpayDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProcessFormula",
                table: "Process",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessType",
                table: "Process",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ProcessId",
                table: "LabInv",
                type: "int",
                nullable: true);

            migrationBuilder.InsertData(
                table: "ScreenManagements",
                columns: new[] { "Id", "Description", "Display", "Required", "ScreenName", "Topic" },
                values: new object[,]
                {
                    { 26, "When enabled, the system will provide an option in Labour Invoice to cancel the invoice that has been submitted to the E-Invoice portal, after the E-Invoice is cancelled in the portal.", "Invoice Cancel", false, "Labour Invoice", "Need Invoice Cancel Option" },
                    { 27, "When enabled, the system will provide an option in Labour Invoice to select process for recording process information.", "Process Selection", false, "Labour Invoice", "Need Process Selection Option" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabInv_ProcessId",
                table: "LabInv",
                column: "ProcessId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabInv_Process_ProcessId",
                table: "LabInv",
                column: "ProcessId",
                principalTable: "Process",
                principalColumn: "ProcessId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabInv_Process_ProcessId",
                table: "LabInv");

            migrationBuilder.DropIndex(
                name: "IX_LabInv_ProcessId",
                table: "LabInv");

            migrationBuilder.DeleteData(
                table: "ScreenManagements",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "ScreenManagements",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DropColumn(
                name: "ProcessFormula",
                table: "Process");

            migrationBuilder.DropColumn(
                name: "ProcessType",
                table: "Process");

            migrationBuilder.DropColumn(
                name: "ProcessId",
                table: "LabInv");
        }
    }
}
