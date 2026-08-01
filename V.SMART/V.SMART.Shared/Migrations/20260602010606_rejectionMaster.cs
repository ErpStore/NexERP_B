using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class rejectionMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RejectionId",
                table: "SubConSCNSub",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RejectionId",
                table: "PurchaseSCNSub",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RejectionId",
                table: "ProductionSCNCompSub",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RejectionId",
                table: "ProductionSCNAssySub",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RejectionId",
                table: "LabourSCNSub",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RejectionMaster",
                columns: table => new
                {
                    RejectionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RejectionCode = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    RejectionDescription = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RejectionMaster", x => x.RejectionId);
                });

            migrationBuilder.InsertData(
                table: "ScreenManagements",
                columns: new[] { "Id", "Description", "Display", "Required", "ScreenName", "Topic" },
                values: new object[] { 30, "When enabled, users are required to select a Rejection Reason from the Rejection Master while entering rejection details.", "Enable Rejection Reason", false, "RejectionMaster", "Rejection Reason Selection" });

            migrationBuilder.InsertData(
                table: "Screens",
                columns: new[] { "Id", "IsPrintRequired", "ScreenCode", "ScreenName" },
                values: new object[] { 126, false, 126, "RejectionMaster" });

            migrationBuilder.CreateIndex(
                name: "IX_SubConSCNSub_RejectionId",
                table: "SubConSCNSub",
                column: "RejectionId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseSCNSub_RejectionId",
                table: "PurchaseSCNSub",
                column: "RejectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSCNCompSub_RejectionId",
                table: "ProductionSCNCompSub",
                column: "RejectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSCNAssySub_RejectionId",
                table: "ProductionSCNAssySub",
                column: "RejectionId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourSCNSub_RejectionId",
                table: "LabourSCNSub",
                column: "RejectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabourSCNSub_RejectionMaster_RejectionId",
                table: "LabourSCNSub",
                column: "RejectionId",
                principalTable: "RejectionMaster",
                principalColumn: "RejectionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionSCNAssySub_RejectionMaster_RejectionId",
                table: "ProductionSCNAssySub",
                column: "RejectionId",
                principalTable: "RejectionMaster",
                principalColumn: "RejectionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionSCNCompSub_RejectionMaster_RejectionId",
                table: "ProductionSCNCompSub",
                column: "RejectionId",
                principalTable: "RejectionMaster",
                principalColumn: "RejectionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseSCNSub_RejectionMaster_RejectionId",
                table: "PurchaseSCNSub",
                column: "RejectionId",
                principalTable: "RejectionMaster",
                principalColumn: "RejectionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubConSCNSub_RejectionMaster_RejectionId",
                table: "SubConSCNSub",
                column: "RejectionId",
                principalTable: "RejectionMaster",
                principalColumn: "RejectionId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabourSCNSub_RejectionMaster_RejectionId",
                table: "LabourSCNSub");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionSCNAssySub_RejectionMaster_RejectionId",
                table: "ProductionSCNAssySub");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionSCNCompSub_RejectionMaster_RejectionId",
                table: "ProductionSCNCompSub");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseSCNSub_RejectionMaster_RejectionId",
                table: "PurchaseSCNSub");

            migrationBuilder.DropForeignKey(
                name: "FK_SubConSCNSub_RejectionMaster_RejectionId",
                table: "SubConSCNSub");

            migrationBuilder.DropTable(
                name: "RejectionMaster");

            migrationBuilder.DropIndex(
                name: "IX_SubConSCNSub_RejectionId",
                table: "SubConSCNSub");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseSCNSub_RejectionId",
                table: "PurchaseSCNSub");

            migrationBuilder.DropIndex(
                name: "IX_ProductionSCNCompSub_RejectionId",
                table: "ProductionSCNCompSub");

            migrationBuilder.DropIndex(
                name: "IX_ProductionSCNAssySub_RejectionId",
                table: "ProductionSCNAssySub");

            migrationBuilder.DropIndex(
                name: "IX_LabourSCNSub_RejectionId",
                table: "LabourSCNSub");

            migrationBuilder.DeleteData(
                table: "ScreenManagements",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 126);

            migrationBuilder.DropColumn(
                name: "RejectionId",
                table: "SubConSCNSub");

            migrationBuilder.DropColumn(
                name: "RejectionId",
                table: "PurchaseSCNSub");

            migrationBuilder.DropColumn(
                name: "RejectionId",
                table: "ProductionSCNCompSub");

            migrationBuilder.DropColumn(
                name: "RejectionId",
                table: "ProductionSCNAssySub");

            migrationBuilder.DropColumn(
                name: "RejectionId",
                table: "LabourSCNSub");
        }
    }
}
