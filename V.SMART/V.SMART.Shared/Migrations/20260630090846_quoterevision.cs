using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class quoterevision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRevisionNo",
                table: "MfgQuote",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RefRevisionQuoteId",
                table: "MfgQuote",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RevisionQuoteNo",
                table: "MfgQuote",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRevisionNo",
                table: "MfgQuote");

            migrationBuilder.DropColumn(
                name: "RefRevisionQuoteId",
                table: "MfgQuote");

            migrationBuilder.DropColumn(
                name: "RevisionQuoteNo",
                table: "MfgQuote");
        }
    }
}
