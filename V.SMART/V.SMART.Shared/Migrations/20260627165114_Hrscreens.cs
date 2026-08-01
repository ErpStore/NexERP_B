using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class Hrscreens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Attendance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    MonthId = table.Column<int>(type: "int", nullable: false),
                    Ldetail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OtDetail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OutTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BioMetricId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttendanceSource = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reporting = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attendance_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BiometricExcelSetting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InTimeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OutTimeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalWorkedHrs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InTimeNameStartRow = table.Column<int>(type: "int", nullable: false),
                    InTimeDataStartCol = table.Column<int>(type: "int", nullable: false),
                    EmpCodeIndexFromInTime = table.Column<int>(type: "int", nullable: false),
                    StatusNameIndexFromInTime = table.Column<int>(type: "int", nullable: false),
                    OutTimeIndexFromInTime = table.Column<int>(type: "int", nullable: false),
                    TotalWorkHrsIndexFromInTime = table.Column<int>(type: "int", nullable: false),
                    EmpIdColIndex = table.Column<int>(type: "int", nullable: false),
                    HeaderInTimeStartCol = table.Column<int>(type: "int", nullable: false),
                    P1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    P2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    H = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    W1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    M1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    M2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    N = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    A = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    L = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrefixBfrID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SplCharAfterID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OTName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OTNameIndexFromInTime = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BiometricExcelSetting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HRMasterSetting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmplrESICode = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ESISealing = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EmpESI = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EmplrESI = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EmplrCodeNo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EmprAcGroupNo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EmpPF = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PFSealing = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EmplrPFAcNo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EmplrPFAcNo21 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdminChrAcNo2 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdminChrAcNo22 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PFBasic = table.Column<bool>(type: "bit", nullable: false),
                    PFDA = table.Column<bool>(type: "bit", nullable: false),
                    PFHRA = table.Column<bool>(type: "bit", nullable: false),
                    PFConveyance = table.Column<bool>(type: "bit", nullable: false),
                    PFOTAmount = table.Column<bool>(type: "bit", nullable: false),
                    PFSplAllow = table.Column<bool>(type: "bit", nullable: false),
                    PFMedicalAllow = table.Column<bool>(type: "bit", nullable: false),
                    PFLTA = table.Column<bool>(type: "bit", nullable: false),
                    PFEA = table.Column<bool>(type: "bit", nullable: false),
                    PFMiscEarning = table.Column<bool>(type: "bit", nullable: false),
                    PFOtherAllow = table.Column<bool>(type: "bit", nullable: false),
                    PFAttendBonus = table.Column<bool>(type: "bit", nullable: false),
                    ESIBasic = table.Column<bool>(type: "bit", nullable: false),
                    ESIDA = table.Column<bool>(type: "bit", nullable: false),
                    ESIHRA = table.Column<bool>(type: "bit", nullable: false),
                    ESIConveyance = table.Column<bool>(type: "bit", nullable: false),
                    ESIOTAmt = table.Column<bool>(type: "bit", nullable: false),
                    ESISplAllow = table.Column<bool>(type: "bit", nullable: false),
                    ESIMedicalAllow = table.Column<bool>(type: "bit", nullable: false),
                    ESILTA = table.Column<bool>(type: "bit", nullable: false),
                    ESIEA = table.Column<bool>(type: "bit", nullable: false),
                    ESIMiscEarning = table.Column<bool>(type: "bit", nullable: false),
                    ESIOtherAllow = table.Column<bool>(type: "bit", nullable: false),
                    ESIAttBonus = table.Column<bool>(type: "bit", nullable: false),
                    OTBasic = table.Column<bool>(type: "bit", nullable: false),
                    OTDA = table.Column<bool>(type: "bit", nullable: false),
                    OTHRA = table.Column<bool>(type: "bit", nullable: false),
                    OTEA = table.Column<bool>(type: "bit", nullable: false),
                    OTLTA = table.Column<bool>(type: "bit", nullable: false),
                    OTSplAllow = table.Column<bool>(type: "bit", nullable: false),
                    OTConveyance = table.Column<bool>(type: "bit", nullable: false),
                    OTMedicalAllow = table.Column<bool>(type: "bit", nullable: false),
                    OTMiscEarning = table.Column<bool>(type: "bit", nullable: false),
                    OTOtherAllow = table.Column<bool>(type: "bit", nullable: false),
                    OTAttendBonus = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HRMasterSetting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Salary",
                columns: table => new
                {
                    RowId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalYear = table.Column<int>(type: "int", nullable: false),
                    SalMonth = table.Column<int>(type: "int", nullable: false),
                    SalDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    Dept = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkingDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PayableDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LeaveDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PresentDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AbsentDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Holidays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalEarnings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Basic = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EarnedBasic = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EarnedDA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsOTunderBasic = table.Column<bool>(type: "bit", nullable: false),
                    IsOTPerHour = table.Column<bool>(type: "bit", nullable: false),
                    OT_times_now = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EarnedOT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsAttBonus = table.Column<bool>(type: "bit", nullable: false),
                    EarnedAttBonus = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsHRAPerDay = table.Column<bool>(type: "bit", nullable: false),
                    IsHRAPerMonth = table.Column<bool>(type: "bit", nullable: false),
                    HRA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EarnedHRA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsTAPerDay = table.Column<bool>(type: "bit", nullable: false),
                    IsTAPerMonth = table.Column<bool>(type: "bit", nullable: false),
                    TA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EarnedTA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsSplAllowPerDay = table.Column<bool>(type: "bit", nullable: false),
                    IsSplAllowPerMonth = table.Column<bool>(type: "bit", nullable: false),
                    SplAllow = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EarnedSplAllow = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Miscellaneous = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Others = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsLTAPerDay = table.Column<bool>(type: "bit", nullable: false),
                    IsLTAPerMonth = table.Column<bool>(type: "bit", nullable: false),
                    LTA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EarnedLTA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsMRPerDay = table.Column<bool>(type: "bit", nullable: false),
                    IsMRPerMonth = table.Column<bool>(type: "bit", nullable: false),
                    MR = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EarnedMR = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsEAPerDay = table.Column<bool>(type: "bit", nullable: false),
                    IsEAPerMonth = table.Column<bool>(type: "bit", nullable: false),
                    EA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EarnedEA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Society = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Insurance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalAdvance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Fines = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DamageLoss = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherDed = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsPF = table.Column<bool>(type: "bit", nullable: false),
                    EarnedPF = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsESI = table.Column<bool>(type: "bit", nullable: false),
                    EarnedESI = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsPT = table.Column<bool>(type: "bit", nullable: false),
                    PT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TDS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    chckOT = table.Column<bool>(type: "bit", nullable: false),
                    LoanId = table.Column<int>(type: "int", nullable: true),
                    LoanAmtDed = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LoanBalB4 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDed = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OldBal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    paidAmt = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AttBonus = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LoanDedId = table.Column<int>(type: "int", nullable: true),
                    IsDAtoPF = table.Column<bool>(type: "bit", nullable: false),
                    ESIBankCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ESIBankName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ESICQDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ESICQNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ESIDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ESIPytMode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PFBankName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PFCQDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PFCQNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PFDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PFDepositor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PFPytMode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Createdby = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Createddate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Modifiedby = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modifieddate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PH = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    W = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LOP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    P = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    O = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Salary", x => x.RowId);
                });

            migrationBuilder.CreateTable(
                name: "SalaryHeadPrintsSetting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrintName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPrint = table.Column<bool>(type: "bit", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    StrRow = table.Column<int>(type: "int", nullable: true),
                    StrCol = table.Column<int>(type: "int", nullable: true),
                    VertCol = table.Column<int>(type: "int", nullable: true),
                    VertRow = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryHeadPrintsSetting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StaffLoan",
                columns: table => new
                {
                    LoanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StaffId = table.Column<int>(type: "int", maxLength: 50, nullable: false),
                    DateOfIssue = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NumberOfMonthsRecoveries = table.Column<int>(type: "int", nullable: false),
                    LoanType = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsFinished = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BalanceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffLoan", x => x.LoanId);
                });

            migrationBuilder.InsertData(
                table: "Screens",
                columns: new[] { "Id", "IsPrintRequired", "ScreenCode", "ScreenName" },
                values: new object[,]
                {
                    { 133, false, 133, "HRMaster" },
                    { 134, false, 134, "Biometric Excel Set" },
                    { 135, false, 135, "Salary Head Print Setting" },
                    { 136, true, 136, "Salary" },
                    { 137, false, 137, "Attendance" },
                    { 138, false, 138, "StaffLoan" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_StaffId",
                table: "Attendance",
                column: "StaffId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attendance");

            migrationBuilder.DropTable(
                name: "BiometricExcelSetting");

            migrationBuilder.DropTable(
                name: "HRMasterSetting");

            migrationBuilder.DropTable(
                name: "Salary");

            migrationBuilder.DropTable(
                name: "SalaryHeadPrintsSetting");

            migrationBuilder.DropTable(
                name: "StaffLoan");

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 133);

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 134);

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 135);

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 136);

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 137);

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 138);
        }
    }
}
