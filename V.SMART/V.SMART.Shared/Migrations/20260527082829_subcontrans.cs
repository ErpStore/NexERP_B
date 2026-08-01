using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class subcontrans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreditNoteSub_ExpInvSub_ExpInvSubId1",
                table: "CreditNoteSub");

            migrationBuilder.DropForeignKey(
                name: "FK_CreditNoteSub_LabInvSub_LabInvSubId1",
                table: "CreditNoteSub");

            migrationBuilder.DropForeignKey(
                name: "FK_CreditNoteSub_MfgInvSub_MfgInvSubInvSubId",
                table: "CreditNoteSub");

            migrationBuilder.DropForeignKey(
                name: "FK_LabInvSub_LabourDcOutgoingSub_LabourDcOutgoingSubDcSubId",
                table: "LabInvSub");

            migrationBuilder.DropIndex(
                name: "IX_LabInvSub_LabourDcOutgoingSubDcSubId",
                table: "LabInvSub");

            migrationBuilder.DropIndex(
                name: "IX_CreditNoteSub_ExpInvSubId1",
                table: "CreditNoteSub");

            migrationBuilder.DropIndex(
                name: "IX_CreditNoteSub_LabInvSubId1",
                table: "CreditNoteSub");

            migrationBuilder.DropIndex(
                name: "IX_CreditNoteSub_MfgInvSubInvSubId",
                table: "CreditNoteSub");

            migrationBuilder.DropColumn(
                name: "LabourDcOutgoingSubDcSubId",
                table: "LabInvSub");

            migrationBuilder.DropColumn(
                name: "ExpInvSubId1",
                table: "CreditNoteSub");

            migrationBuilder.DropColumn(
                name: "LabInvSubId1",
                table: "CreditNoteSub");

            migrationBuilder.DropColumn(
                name: "MfgInvSubInvSubId",
                table: "CreditNoteSub");

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelDate",
                table: "SubConSCN",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CanceledBy",
                table: "SubConSCN",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShortClose",
                table: "SubConSCN",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RCId",
                table: "PurchPoSub",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RefRcSubId",
                table: "PurchPoSub",
                type: "int",
                nullable: true);

            migrationBuilder.InsertData(
                table: "ScreenManagements",
                columns: new[] { "Id", "Description", "Display", "Required", "ScreenName", "Topic" },
                values: new object[] { 29, "When this setting is enabled, Sub-Contract DC Outgoing will be processed PO-wise. After completing the DC Outgoing against a PO, the system will automatically close the respective PO and prevent further transactions.", "PO Wise Sub-Contract DC Outgoing", false, "Sub-Contract DC-Out", "PO WiseSub-Contract DC Outgoing & Auto Close" });

            migrationBuilder.UpdateData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 102,
                column: "ScreenName",
                value: "Sales Track Report");

            migrationBuilder.InsertData(
                table: "Screens",
                columns: new[] { "Id", "IsPrintRequired", "ScreenCode", "ScreenName" },
                values: new object[,]
                {
                    { 117, false, 117, "Po Pendings" },
                    { 118, false, 118, "Pending Statements" },
                    { 119, false, 119, "ToolCrib Issue Summary" },
                    { 120, false, 120, "Item History" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchPoSub_RCId",
                table: "PurchPoSub",
                column: "RCId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchPoSub_RefRcSubId",
                table: "PurchPoSub",
                column: "RefRcSubId");

            migrationBuilder.CreateIndex(
                name: "IX_LabInvSub_RefDcSubId",
                table: "LabInvSub",
                column: "RefDcSubId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabInvSub_LabourDcOutgoingSub_RefDcSubId",
                table: "LabInvSub",
                column: "RefDcSubId",
                principalTable: "LabourDcOutgoingSub",
                principalColumn: "DcSubId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchPoSub_RouteCardSub_RefRcSubId",
                table: "PurchPoSub",
                column: "RefRcSubId",
                principalTable: "RouteCardSub",
                principalColumn: "RCSubId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchPoSub_RouteCard_RCId",
                table: "PurchPoSub",
                column: "RCId",
                principalTable: "RouteCard",
                principalColumn: "RCId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabInvSub_LabourDcOutgoingSub_RefDcSubId",
                table: "LabInvSub");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchPoSub_RouteCardSub_RefRcSubId",
                table: "PurchPoSub");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchPoSub_RouteCard_RCId",
                table: "PurchPoSub");

            migrationBuilder.DropIndex(
                name: "IX_PurchPoSub_RCId",
                table: "PurchPoSub");

            migrationBuilder.DropIndex(
                name: "IX_PurchPoSub_RefRcSubId",
                table: "PurchPoSub");

            migrationBuilder.DropIndex(
                name: "IX_LabInvSub_RefDcSubId",
                table: "LabInvSub");

            migrationBuilder.DeleteData(
                table: "ScreenManagements",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 117);

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 118);

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 119);

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 120);

            migrationBuilder.DropColumn(
                name: "CancelDate",
                table: "SubConSCN");

            migrationBuilder.DropColumn(
                name: "CanceledBy",
                table: "SubConSCN");

            migrationBuilder.DropColumn(
                name: "ShortClose",
                table: "SubConSCN");

            migrationBuilder.DropColumn(
                name: "RCId",
                table: "PurchPoSub");

            migrationBuilder.DropColumn(
                name: "RefRcSubId",
                table: "PurchPoSub");

            migrationBuilder.AddColumn<int>(
                name: "LabourDcOutgoingSubDcSubId",
                table: "LabInvSub",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExpInvSubId1",
                table: "CreditNoteSub",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LabInvSubId1",
                table: "CreditNoteSub",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MfgInvSubInvSubId",
                table: "CreditNoteSub",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 102,
                column: "ScreenName",
                value: "Labour Track Report");

            migrationBuilder.CreateIndex(
                name: "IX_LabInvSub_LabourDcOutgoingSubDcSubId",
                table: "LabInvSub",
                column: "LabourDcOutgoingSubDcSubId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNoteSub_ExpInvSubId1",
                table: "CreditNoteSub",
                column: "ExpInvSubId1");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNoteSub_LabInvSubId1",
                table: "CreditNoteSub",
                column: "LabInvSubId1");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNoteSub_MfgInvSubInvSubId",
                table: "CreditNoteSub",
                column: "MfgInvSubInvSubId");

            migrationBuilder.AddForeignKey(
                name: "FK_CreditNoteSub_ExpInvSub_ExpInvSubId1",
                table: "CreditNoteSub",
                column: "ExpInvSubId1",
                principalTable: "ExpInvSub",
                principalColumn: "ExpInvSubId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CreditNoteSub_LabInvSub_LabInvSubId1",
                table: "CreditNoteSub",
                column: "LabInvSubId1",
                principalTable: "LabInvSub",
                principalColumn: "LabInvSubId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CreditNoteSub_MfgInvSub_MfgInvSubInvSubId",
                table: "CreditNoteSub",
                column: "MfgInvSubInvSubId",
                principalTable: "MfgInvSub",
                principalColumn: "InvSubId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LabInvSub_LabourDcOutgoingSub_LabourDcOutgoingSubDcSubId",
                table: "LabInvSub",
                column: "LabourDcOutgoingSubDcSubId",
                principalTable: "LabourDcOutgoingSub",
                principalColumn: "DcSubId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
