using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddAthorizationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSalesPo",
                table: "UserAuthority",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IsSalesPoLevel",
                table: "UserAuthority",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovalDate",
                table: "MfgQuote",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "MfgQuote",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                table: "MfgQuote",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAuthorized",
                table: "MfgQuote",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLevel1",
                table: "MfgQuote",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLevel2",
                table: "MfgQuote",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLevel3",
                table: "MfgQuote",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRejected",
                table: "MfgQuote",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastApproved",
                table: "MfgQuote",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Level1Sign",
                table: "MfgQuote",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Level2Sign",
                table: "MfgQuote",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Level3Sign",
                table: "MfgQuote",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectReason",
                table: "MfgQuote",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovalDate",
                table: "MfgPo",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "MfgPo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                table: "MfgPo",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAuthorized",
                table: "MfgPo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLevel1",
                table: "MfgPo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLevel2",
                table: "MfgPo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLevel3",
                table: "MfgPo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRejected",
                table: "MfgPo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastApproved",
                table: "MfgPo",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Level1Sign",
                table: "MfgPo",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Level2Sign",
                table: "MfgPo",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Level3Sign",
                table: "MfgPo",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectReason",
                table: "MfgPo",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSalesPo",
                table: "UserAuthority");

            migrationBuilder.DropColumn(
                name: "IsSalesPoLevel",
                table: "UserAuthority");

            migrationBuilder.DropColumn(
                name: "ApprovalDate",
                table: "MfgQuote");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "MfgQuote");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                table: "MfgQuote");

            migrationBuilder.DropColumn(
                name: "IsAuthorized",
                table: "MfgQuote");

            migrationBuilder.DropColumn(
                name: "IsLevel1",
                table: "MfgQuote");

            migrationBuilder.DropColumn(
                name: "IsLevel2",
                table: "MfgQuote");

            migrationBuilder.DropColumn(
                name: "IsLevel3",
                table: "MfgQuote");

            migrationBuilder.DropColumn(
                name: "IsRejected",
                table: "MfgQuote");

            migrationBuilder.DropColumn(
                name: "LastApproved",
                table: "MfgQuote");

            migrationBuilder.DropColumn(
                name: "Level1Sign",
                table: "MfgQuote");

            migrationBuilder.DropColumn(
                name: "Level2Sign",
                table: "MfgQuote");

            migrationBuilder.DropColumn(
                name: "Level3Sign",
                table: "MfgQuote");

            migrationBuilder.DropColumn(
                name: "RejectReason",
                table: "MfgQuote");

            migrationBuilder.DropColumn(
                name: "ApprovalDate",
                table: "MfgPo");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "MfgPo");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                table: "MfgPo");

            migrationBuilder.DropColumn(
                name: "IsAuthorized",
                table: "MfgPo");

            migrationBuilder.DropColumn(
                name: "IsLevel1",
                table: "MfgPo");

            migrationBuilder.DropColumn(
                name: "IsLevel2",
                table: "MfgPo");

            migrationBuilder.DropColumn(
                name: "IsLevel3",
                table: "MfgPo");

            migrationBuilder.DropColumn(
                name: "IsRejected",
                table: "MfgPo");

            migrationBuilder.DropColumn(
                name: "LastApproved",
                table: "MfgPo");

            migrationBuilder.DropColumn(
                name: "Level1Sign",
                table: "MfgPo");

            migrationBuilder.DropColumn(
                name: "Level2Sign",
                table: "MfgPo");

            migrationBuilder.DropColumn(
                name: "Level3Sign",
                table: "MfgPo");

            migrationBuilder.DropColumn(
                name: "RejectReason",
                table: "MfgPo");
        }
    }
}
