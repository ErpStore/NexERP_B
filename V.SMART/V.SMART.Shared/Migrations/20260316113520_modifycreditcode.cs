using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class modifycreditcode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExpInvSubId",
                table: "CreditNoteSub",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LabInvSubId",
                table: "CreditNoteSub",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditNoteSub_ExpInvSubId",
                table: "CreditNoteSub",
                column: "ExpInvSubId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNoteSub_LabInvSubId",
                table: "CreditNoteSub",
                column: "LabInvSubId");

            migrationBuilder.AddForeignKey(
                name: "FK_CreditNoteSub_ExpInvSub_ExpInvSubId",
                table: "CreditNoteSub",
                column: "ExpInvSubId",
                principalTable: "ExpInvSub",
                principalColumn: "ExpInvSubId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CreditNoteSub_LabInvSub_LabInvSubId",
                table: "CreditNoteSub",
                column: "LabInvSubId",
                principalTable: "LabInvSub",
                principalColumn: "LabInvSubId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreditNoteSub_ExpInvSub_ExpInvSubId",
                table: "CreditNoteSub");

            migrationBuilder.DropForeignKey(
                name: "FK_CreditNoteSub_LabInvSub_LabInvSubId",
                table: "CreditNoteSub");

            migrationBuilder.DropIndex(
                name: "IX_CreditNoteSub_ExpInvSubId",
                table: "CreditNoteSub");

            migrationBuilder.DropIndex(
                name: "IX_CreditNoteSub_LabInvSubId",
                table: "CreditNoteSub");

            migrationBuilder.DropColumn(
                name: "ExpInvSubId",
                table: "CreditNoteSub");

            migrationBuilder.DropColumn(
                name: "LabInvSubId",
                table: "CreditNoteSub");
        }
    }
}
