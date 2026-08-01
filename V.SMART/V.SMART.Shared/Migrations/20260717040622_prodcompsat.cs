using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class prodcompsat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InspectBy",
                table: "ProductionSCNComp",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "EndTime",
                table: "ProductionReturnCompSub",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "PlatedTime",
                table: "ProductionReturnCompSub",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "StartTime",
                table: "ProductionReturnCompSub",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProcessId",
                table: "ProductionReturnComp",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShiftId",
                table: "ProductionReturnComp",
                type: "int",
                nullable: true);

            migrationBuilder.InsertData(
                table: "ScreenManagements",
                columns: new[] { "Id", "Description", "Display", "Required", "ScreenName", "Topic" },
                values: new object[] { 35, "Enables Sharadha-specific Production Log functionality, including customized screen layout, shift selection, vat selection, and other production-related fields as per business requirements.", "Production Log Configuration", false, "Production Return Component", "Sharadha Production Log Customization" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnComp_ProcessId",
                table: "ProductionReturnComp",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnComp_ShiftId",
                table: "ProductionReturnComp",
                column: "ShiftId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionReturnComp_Process_ProcessId",
                table: "ProductionReturnComp",
                column: "ProcessId",
                principalTable: "Process",
                principalColumn: "ProcessId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionReturnComp_ShiftAllocation_ShiftId",
                table: "ProductionReturnComp",
                column: "ShiftId",
                principalTable: "ShiftAllocation",
                principalColumn: "ShiftId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionReturnComp_Process_ProcessId",
                table: "ProductionReturnComp");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionReturnComp_ShiftAllocation_ShiftId",
                table: "ProductionReturnComp");

            migrationBuilder.DropIndex(
                name: "IX_ProductionReturnComp_ProcessId",
                table: "ProductionReturnComp");

            migrationBuilder.DropIndex(
                name: "IX_ProductionReturnComp_ShiftId",
                table: "ProductionReturnComp");

            migrationBuilder.DeleteData(
                table: "ScreenManagements",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DropColumn(
                name: "InspectBy",
                table: "ProductionSCNComp");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "ProductionReturnCompSub");

            migrationBuilder.DropColumn(
                name: "PlatedTime",
                table: "ProductionReturnCompSub");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "ProductionReturnCompSub");

            migrationBuilder.DropColumn(
                name: "ProcessId",
                table: "ProductionReturnComp");

            migrationBuilder.DropColumn(
                name: "ShiftId",
                table: "ProductionReturnComp");
        }
    }
}
