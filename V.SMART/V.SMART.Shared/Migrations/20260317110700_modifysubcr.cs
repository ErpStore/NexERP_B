using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class modifysubcr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "ExpInvSubId1",
                table: "CreditNoteSub");

            migrationBuilder.DropColumn(
                name: "LabInvSubId1",
                table: "CreditNoteSub");

            migrationBuilder.DropColumn(
                name: "MfgInvSubInvSubId",
                table: "CreditNoteSub");
        }
    }
}
