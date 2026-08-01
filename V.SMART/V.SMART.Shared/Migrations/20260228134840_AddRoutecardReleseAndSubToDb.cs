using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddRoutecardReleseAndSubToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RouteCardRelease",
                columns: table => new
                {
                    RouteCardReleaseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PoId = table.Column<int>(type: "int", nullable: true),
                    PoSubId = table.Column<int>(type: "int", nullable: true),
                    AssemblyItemId = table.Column<int>(type: "int", nullable: false),
                    PoQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ReleaseQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ReleaseBalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ReleasedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReleasedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteCardRelease", x => x.RouteCardReleaseId);
                    table.ForeignKey(
                        name: "FK_RouteCardRelease_Item_AssemblyItemId",
                        column: x => x.AssemblyItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RouteCardRelease_MfgPoSub_PoSubId",
                        column: x => x.PoSubId,
                        principalTable: "MfgPoSub",
                        principalColumn: "PoSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteCardRelease_MfgPo_PoId",
                        column: x => x.PoId,
                        principalTable: "MfgPo",
                        principalColumn: "PoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RouteCardReleaseSub",
                columns: table => new
                {
                    RouteCardReleaseSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RouteCardReleaseId = table.Column<int>(type: "int", nullable: false),
                    ComponentItemId = table.Column<int>(type: "int", nullable: false),
                    CumulativeUtilQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RequiredQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    IssuedQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RemainingQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteCardReleaseSub", x => x.RouteCardReleaseSubId);
                    table.ForeignKey(
                        name: "FK_RouteCardReleaseSub_Item_ComponentItemId",
                        column: x => x.ComponentItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RouteCardReleaseSub_RouteCardRelease_RouteCardReleaseId",
                        column: x => x.RouteCardReleaseId,
                        principalTable: "RouteCardRelease",
                        principalColumn: "RouteCardReleaseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Screens",
                columns: new[] { "Id", "IsPrintRequired", "ScreenCode", "ScreenName" },
                values: new object[] { 96, false, 96, "Route Card Release" });

            migrationBuilder.InsertData(
                table: "UserRights",
                columns: new[] { "Id", "CanCreate", "CanDelete", "CanEdit", "CanView", "CreatedBy", "CreatedDate", "IsHide", "Remarks", "ScreenCode", "UpdatedBy", "UpdatedDate", "UserId" },
                values: new object[] { 96, true, true, true, true, null, null, false, null, 96, null, null, 1 });

            migrationBuilder.CreateIndex(
                name: "IX_RouteCardRelease_AssemblyItemId",
                table: "RouteCardRelease",
                column: "AssemblyItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteCardRelease_PoId",
                table: "RouteCardRelease",
                column: "PoId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteCardRelease_PoSubId",
                table: "RouteCardRelease",
                column: "PoSubId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteCardReleaseSub_ComponentItemId",
                table: "RouteCardReleaseSub",
                column: "ComponentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteCardReleaseSub_RouteCardReleaseId",
                table: "RouteCardReleaseSub",
                column: "RouteCardReleaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RouteCardReleaseSub");

            migrationBuilder.DropTable(
                name: "RouteCardRelease");

            migrationBuilder.DeleteData(
                table: "UserRights",
                keyColumn: "Id",
                keyValue: 96);

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 96);
        }
    }
}
