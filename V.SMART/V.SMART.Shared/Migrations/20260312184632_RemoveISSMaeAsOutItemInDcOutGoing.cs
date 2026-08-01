using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class RemoveISSMaeAsOutItemInDcOutGoing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOutItemSameAsInItem",
                table: "LabourDcOutgoing");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOutItemSameAsInItem",
                table: "LabourDcOutgoing",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
