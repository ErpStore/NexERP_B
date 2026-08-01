using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class InUserRightsAddForeignKeyReferenceAsScreenId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRights_Screens_ScreenCode",
                table: "UserRights");

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 66);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 68);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 69);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 70);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 72);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 73);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 74);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 75);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 76);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 77);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 78);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 79);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 80);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 81);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 82);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 83);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 84);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 85);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 86);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 87);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 88);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 89);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 90);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 91);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 92);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 93);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 94);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 95);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 96);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 97);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 98);

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 99);

            migrationBuilder.RenameColumn(
                name: "ScreenCode",
                table: "UserRights",
                newName: "ScreenId");

            migrationBuilder.RenameIndex(
                name: "IX_UserRights_ScreenCode",
                table: "UserRights",
                newName: "IX_UserRights_ScreenId");

            migrationBuilder.UpdateData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 98,
                column: "ScreenCode",
                value: 98);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRights_Screens_ScreenId",
                table: "UserRights",
                column: "ScreenId",
                principalTable: "Screens",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRights_Screens_ScreenId",
                table: "UserRights");

            migrationBuilder.RenameColumn(
                name: "ScreenId",
                table: "UserRights",
                newName: "ScreenCode");

            migrationBuilder.RenameIndex(
                name: "IX_UserRights_ScreenId",
                table: "UserRights",
                newName: "IX_UserRights_ScreenCode");

            migrationBuilder.UpdateData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 98,
                column: "ScreenCode",
                value: 96);

            migrationBuilder.InsertData(
                table: "UserRights",
                columns: new[] { "Id", "CanCreate", "CanDelete", "CanEdit", "CanView", "CreatedBy", "CreatedDate", "IsHide", "Remarks", "ScreenCode", "UpdatedBy", "UpdatedDate", "UserId" },
                values: new object[,]
                {
                    { 1, true, true, true, true, null, null, false, null, 1, null, null, 1 },
                    { 2, true, true, true, true, null, null, false, null, 2, null, null, 1 },
                    { 3, true, true, true, true, null, null, false, null, 3, null, null, 1 },
                    { 4, true, true, true, true, null, null, false, null, 4, null, null, 1 },
                    { 5, true, true, true, true, null, null, false, null, 5, null, null, 1 },
                    { 6, true, true, true, true, null, null, false, null, 6, null, null, 1 },
                    { 7, true, true, true, true, null, null, false, null, 7, null, null, 1 },
                    { 8, true, true, true, true, null, null, false, null, 8, null, null, 1 },
                    { 9, true, true, true, true, null, null, false, null, 9, null, null, 1 },
                    { 10, true, true, true, true, null, null, false, null, 10, null, null, 1 },
                    { 11, true, true, true, true, null, null, false, null, 11, null, null, 1 },
                    { 12, true, true, true, true, null, null, false, null, 12, null, null, 1 },
                    { 13, true, true, true, true, null, null, false, null, 13, null, null, 1 },
                    { 14, true, true, true, true, null, null, false, null, 14, null, null, 1 },
                    { 15, true, true, true, true, null, null, false, null, 15, null, null, 1 },
                    { 16, true, true, true, true, null, null, false, null, 16, null, null, 1 },
                    { 17, true, true, true, true, null, null, false, null, 17, null, null, 1 },
                    { 18, true, true, true, true, null, null, false, null, 18, null, null, 1 },
                    { 19, true, true, true, true, null, null, false, null, 19, null, null, 1 },
                    { 20, true, true, true, true, null, null, false, null, 20, null, null, 1 },
                    { 21, true, true, true, true, null, null, false, null, 21, null, null, 1 },
                    { 22, true, true, true, true, null, null, false, null, 22, null, null, 1 },
                    { 23, true, true, true, true, null, null, false, null, 23, null, null, 1 },
                    { 24, true, true, true, true, null, null, false, null, 24, null, null, 1 },
                    { 25, true, true, true, true, null, null, false, null, 25, null, null, 1 },
                    { 26, true, true, true, true, null, null, false, null, 26, null, null, 1 },
                    { 27, true, true, true, true, null, null, false, null, 27, null, null, 1 },
                    { 28, true, true, true, true, null, null, false, null, 28, null, null, 1 },
                    { 29, true, true, true, true, null, null, false, null, 29, null, null, 1 },
                    { 30, true, true, true, true, null, null, false, null, 30, null, null, 1 },
                    { 31, true, true, true, true, null, null, false, null, 31, null, null, 1 },
                    { 32, true, true, true, true, null, null, false, null, 32, null, null, 1 },
                    { 33, true, true, true, true, null, null, false, null, 33, null, null, 1 },
                    { 34, true, true, true, true, null, null, false, null, 34, null, null, 1 },
                    { 35, true, true, true, true, null, null, false, null, 35, null, null, 1 },
                    { 36, true, true, true, true, null, null, false, null, 36, null, null, 1 },
                    { 37, true, true, true, true, null, null, false, null, 37, null, null, 1 },
                    { 38, true, true, true, true, null, null, false, null, 38, null, null, 1 },
                    { 39, true, true, true, true, null, null, false, null, 39, null, null, 1 },
                    { 40, true, true, true, true, null, null, false, null, 40, null, null, 1 },
                    { 41, true, true, true, true, null, null, false, null, 41, null, null, 1 },
                    { 42, true, true, true, true, null, null, false, null, 42, null, null, 1 },
                    { 43, true, true, true, true, null, null, false, null, 43, null, null, 1 },
                    { 44, true, true, true, true, null, null, false, null, 44, null, null, 1 },
                    { 45, true, true, true, true, null, null, false, null, 45, null, null, 1 },
                    { 46, true, true, true, true, null, null, false, null, 46, null, null, 1 },
                    { 47, true, true, true, true, null, null, false, null, 47, null, null, 1 },
                    { 48, true, true, true, true, null, null, false, null, 48, null, null, 1 },
                    { 49, true, true, true, true, null, null, false, null, 49, null, null, 1 },
                    { 50, true, true, true, true, null, null, false, null, 50, null, null, 1 },
                    { 51, true, true, true, true, null, null, false, null, 51, null, null, 1 },
                    { 52, true, true, true, true, null, null, false, null, 52, null, null, 1 },
                    { 53, true, true, true, true, null, null, false, null, 53, null, null, 1 },
                    { 54, true, true, true, true, null, null, false, null, 54, null, null, 1 },
                    { 55, true, true, true, true, null, null, false, null, 55, null, null, 1 },
                    { 56, true, true, true, true, null, null, false, null, 56, null, null, 1 },
                    { 57, true, true, true, true, null, null, false, null, 57, null, null, 1 },
                    { 58, true, true, true, true, null, null, false, null, 58, null, null, 1 },
                    { 59, true, true, true, true, null, null, false, null, 59, null, null, 1 },
                    { 60, true, true, true, true, null, null, false, null, 60, null, null, 1 },
                    { 61, true, true, true, true, null, null, false, null, 61, null, null, 1 },
                    { 62, true, true, true, true, null, null, false, null, 62, null, null, 1 },
                    { 63, true, true, true, true, null, null, false, null, 63, null, null, 1 },
                    { 64, true, true, true, true, null, null, false, null, 64, null, null, 1 },
                    { 65, true, true, true, true, null, null, false, null, 65, null, null, 1 },
                    { 66, true, true, true, true, null, null, false, null, 66, null, null, 1 },
                    { 67, true, true, true, true, null, null, false, null, 67, null, null, 1 },
                    { 68, true, true, true, true, null, null, false, null, 68, null, null, 1 },
                    { 69, true, true, true, true, null, null, false, null, 69, null, null, 1 },
                    { 70, true, true, true, true, null, null, false, null, 70, null, null, 1 },
                    { 71, true, true, true, true, null, null, false, null, 71, null, null, 1 },
                    { 72, true, true, true, true, null, null, false, null, 72, null, null, 1 },
                    { 73, true, true, true, true, null, null, false, null, 73, null, null, 1 },
                    { 74, true, true, true, true, null, null, false, null, 74, null, null, 1 },
                    { 75, true, true, true, true, null, null, false, null, 75, null, null, 1 },
                    { 76, true, true, true, true, null, null, false, null, 76, null, null, 1 },
                    { 77, true, true, true, true, null, null, false, null, 77, null, null, 1 },
                    { 78, true, true, true, true, null, null, false, null, 78, null, null, 1 },
                    { 79, true, true, true, true, null, null, false, null, 79, null, null, 1 },
                    { 80, true, true, true, true, null, null, false, null, 80, null, null, 1 },
                    { 81, true, true, true, true, null, null, false, null, 81, null, null, 1 },
                    { 82, true, true, true, true, null, null, false, null, 82, null, null, 1 },
                    { 83, true, true, true, true, null, null, false, null, 83, null, null, 1 },
                    { 84, true, true, true, true, null, null, false, null, 84, null, null, 1 },
                    { 85, true, true, true, true, null, null, false, null, 85, null, null, 1 },
                    { 86, true, true, true, true, null, null, false, null, 86, null, null, 1 },
                    { 87, true, true, true, true, null, null, false, null, 87, null, null, 1 },
                    { 88, true, true, true, true, null, null, false, null, 88, null, null, 1 },
                    { 89, true, true, true, true, null, null, false, null, 89, null, null, 1 },
                    { 90, true, true, true, true, null, null, false, null, 90, null, null, 1 },
                    { 91, true, true, true, true, null, null, false, null, 91, null, null, 1 },
                    { 92, true, true, true, true, null, null, false, null, 92, null, null, 1 },
                    { 93, true, true, true, true, null, null, false, null, 93, null, null, 1 },
                    { 94, true, true, true, true, null, null, false, null, 94, null, null, 1 },
                    { 95, true, true, true, true, null, null, false, null, 95, null, null, 1 },
                    { 96, true, true, true, true, null, null, false, null, 96, null, null, 1 },
                    { 97, true, true, true, true, null, null, false, null, 97, null, null, 1 },
                    { 98, true, true, true, true, null, null, false, null, 98, null, null, 1 },
                    { 99, true, true, true, true, null, null, false, null, 99, null, null, 1 }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_UserRights_Screens_ScreenCode",
                table: "UserRights",
                column: "ScreenCode",
                principalTable: "Screens",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
