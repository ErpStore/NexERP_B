using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupIdAndIsSamItemInAndOutToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "LabourGRNSub",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOutItemSameAsInItem",
                table: "LabourGRN",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "LabourGRNSub");

            migrationBuilder.DropColumn(
                name: "IsOutItemSameAsInItem",
                table: "LabourGRN");
        }
    }
}
