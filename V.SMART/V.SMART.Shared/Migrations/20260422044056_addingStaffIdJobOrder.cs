using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class addingStaffIdJobOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StaffID",
                table: "JobOrder",
                type: "int",
                nullable: true);

            migrationBuilder.InsertData(
                table: "ScreenManagements",
                columns: new[] { "Id", "Description", "Display", "Required", "ScreenName", "Topic" },
                values: new object[,]
                {
                    { 22, "Enables automatic generation of Assembly and Sub-Assembly items during Job Order processing.", "JobOrder Generate Assembly and SubAssembly", false, "Job Order", "Assembly & Sub-Assembly Generation" },
                    { 23, "Allows assigning and managing Job Order operations across different departments based on process.", "Assign Job Order Department-wise", false, "Job Order", "Department-wise Job Order Assignment" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobOrder_StaffID",
                table: "JobOrder",
                column: "StaffID");

            migrationBuilder.AddForeignKey(
                name: "FK_JobOrder_Staff_StaffID",
                table: "JobOrder",
                column: "StaffID",
                principalTable: "Staff",
                principalColumn: "StaffID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobOrder_Staff_StaffID",
                table: "JobOrder");

            migrationBuilder.DropIndex(
                name: "IX_JobOrder_StaffID",
                table: "JobOrder");

            migrationBuilder.DeleteData(
                table: "ScreenManagements",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "ScreenManagements",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DropColumn(
                name: "StaffID",
                table: "JobOrder");
        }
    }
}
