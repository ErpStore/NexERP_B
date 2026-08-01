using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddInDexingToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                table: "Grouping",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FRCNo",
                table: "Grouping",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Grouping",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Item_RevItemId",
                table: "Item",
                column: "RevItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_CustId",
                table: "Customer",
                column: "CustId");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_CustName",
                table: "Customer",
                column: "CustName")
                .Annotation("SqlServer:Include", new[] { "GSTNo", "ContactNo", "StateId", "CurrId", "Inactive" });

            migrationBuilder.CreateIndex(
                name: "IX_Customer_GSTNo",
                table: "Customer",
                column: "GSTNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customer_Inactive",
                table: "Customer",
                column: "Inactive");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_Inactive_CustName",
                table: "Customer",
                columns: new[] { "Inactive", "CustName" });

            migrationBuilder.CreateIndex(
                name: "IX_Customer_PANNo",
                table: "Customer",
                column: "PANNo",
                unique: true,
                filter: "[PANNo] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Item_Item_RevItemId",
                table: "Item",
                column: "RevItemId",
                principalTable: "Item",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Item_Item_RevItemId",
                table: "Item");

            migrationBuilder.DropIndex(
                name: "IX_Item_RevItemId",
                table: "Item");

            migrationBuilder.DropIndex(
                name: "IX_Customer_CustId",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_CustName",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_GSTNo",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_Inactive",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_Inactive_CustName",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_PANNo",
                table: "Customer");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                table: "Grouping",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FRCNo",
                table: "Grouping",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Grouping",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
