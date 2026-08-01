using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class RemovetheNewlyAddedField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabourDcReturnCompTrack_LabourDcOutgoing_RefDcId",
                table: "LabourDcReturnCompTrack");

            migrationBuilder.RenameColumn(
                name: "RefDcId",
                table: "LabourDcReturnCompTrack",
                newName: "LabourDcOutgoingDcId");

            migrationBuilder.RenameIndex(
                name: "IX_LabourDcReturnCompTrack_RefDcId",
                table: "LabourDcReturnCompTrack",
                newName: "IX_LabourDcReturnCompTrack_LabourDcOutgoingDcId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabourDcReturnCompTrack_LabourDcOutgoing_LabourDcOutgoingDcId",
                table: "LabourDcReturnCompTrack",
                column: "LabourDcOutgoingDcId",
                principalTable: "LabourDcOutgoing",
                principalColumn: "DcId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabourDcReturnCompTrack_LabourDcOutgoing_LabourDcOutgoingDcId",
                table: "LabourDcReturnCompTrack");

            migrationBuilder.RenameColumn(
                name: "LabourDcOutgoingDcId",
                table: "LabourDcReturnCompTrack",
                newName: "RefDcId");

            migrationBuilder.RenameIndex(
                name: "IX_LabourDcReturnCompTrack_LabourDcOutgoingDcId",
                table: "LabourDcReturnCompTrack",
                newName: "IX_LabourDcReturnCompTrack_RefDcId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabourDcReturnCompTrack_LabourDcOutgoing_RefDcId",
                table: "LabourDcReturnCompTrack",
                column: "RefDcId",
                principalTable: "LabourDcOutgoing",
                principalColumn: "DcId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
