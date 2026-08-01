using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApprovalHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecordId = table.Column<int>(type: "int", nullable: false),
                    ApprovalType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bank",
                columns: table => new
                {
                    BankId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    AccountNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IFSCCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BranchName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BankAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    BankState = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BankOPBal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsDebit = table.Column<bool>(type: "bit", nullable: false),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bank", x => x.BankId);
                });

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    CategoryCode = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CategoryDesc = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsSystemDefined = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.CategoryCode);
                });

            migrationBuilder.CreateTable(
                name: "ContractReviewMaster",
                columns: table => new
                {
                    SlNo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractReviewMaster", x => x.SlNo);
                });

            migrationBuilder.CreateTable(
                name: "Correspondences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UploadedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferenceType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferenceTypeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferenceId = table.Column<int>(type: "int", nullable: true),
                    DocumentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Modifiedby = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Correspondences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Currency",
                columns: table => new
                {
                    CurrId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrSub = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsSystemDefined = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currency", x => x.CurrId);
                });

            migrationBuilder.CreateTable(
                name: "DefectInfo",
                columns: table => new
                {
                    DefectCode = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DefectName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DefectDesc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefectNo = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefectInfo", x => x.DefectCode);
                });

            migrationBuilder.CreateTable(
                name: "EnquiryPurchase",
                columns: table => new
                {
                    EnquiryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchORSubCon = table.Column<bool>(type: "bit", nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    EnquiryNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    EnquiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EnquiryDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EnquiryTally = table.Column<bool>(type: "bit", nullable: false),
                    NoOfItems = table.Column<int>(type: "int", nullable: false),
                    MainRemarks = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    KindOfAttention = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    EnquiryShortClose = table.Column<bool>(type: "bit", nullable: false),
                    Cancel = table.Column<bool>(type: "bit", nullable: false),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CancellationRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    POCName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    POCContactNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    ExpactedReplayDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnquiryPurchase", x => x.EnquiryId);
                });

            migrationBuilder.CreateTable(
                name: "Expense",
                columns: table => new
                {
                    ExpenseCode = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExpenseName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ExpenseDesc = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDirect = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expense", x => x.ExpenseCode);
                });

            migrationBuilder.CreateTable(
                name: "Factors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Factors = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Factors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Grouping",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FRCNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LeadTime = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grouping", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "HolidayList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HolidayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HolidayDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HolidayDay = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HolidayList", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HSNMaster",
                columns: table => new
                {
                    Slno = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HSNCategoryCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HSNcode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Required = table.Column<bool>(type: "bit", nullable: false),
                    HSNCategory = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HSNMaster", x => x.Slno);
                });

            migrationBuilder.CreateTable(
                name: "Income",
                columns: table => new
                {
                    IncomeCode = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncomeName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    IncomeDesc = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsDirect = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Income", x => x.IncomeCode);
                });

            migrationBuilder.CreateTable(
                name: "InspectionSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SettingName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SamplesPerSheet = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeaveType",
                columns: table => new
                {
                    LeaveTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeaveName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LeaveDescription = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    MaxAllowedPerYear = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsCarryForward = table.Column<bool>(type: "bit", nullable: false),
                    IsCashable = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveType", x => x.LeaveTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Machine",
                columns: table => new
                {
                    MachineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    SerialNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Identification = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Make = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MachineCapacity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MachineModel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MachineAccuracy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OtherDetails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MachineHrRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Machine", x => x.MachineId);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceProcess",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleId = table.Column<int>(type: "int", nullable: false),
                    MaintenanceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MachineId = table.Column<int>(type: "int", nullable: true),
                    MaintenanceId = table.Column<int>(type: "int", nullable: true),
                    MaintenanceStatus = table.Column<bool>(type: "bit", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Preparedby = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceProcess", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaterialReq",
                columns: table => new
                {
                    MreqId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsPurchOrSubCon = table.Column<bool>(type: "bit", nullable: false),
                    IsManual = table.Column<bool>(type: "bit", nullable: false),
                    MReqNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MreqDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MreqDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NoOfItems = table.Column<int>(type: "int", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MreqCancel = table.Column<bool>(type: "bit", nullable: false),
                    CancelReason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShortClose = table.Column<bool>(type: "bit", nullable: false),
                    IsLevel1 = table.Column<bool>(type: "bit", nullable: false),
                    IsLevel2 = table.Column<bool>(type: "bit", nullable: false),
                    IsLevel3 = table.Column<bool>(type: "bit", nullable: false),
                    IsAuthorized = table.Column<bool>(type: "bit", nullable: false),
                    IsRejected = table.Column<bool>(type: "bit", nullable: false),
                    RejectReason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Level1Sign = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Level2Sign = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Level3Sign = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    KindAttn = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    LastApproved = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MainRemark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PRTally = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialReq", x => x.MreqId);
                });

            migrationBuilder.CreateTable(
                name: "PoType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeriesNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrintSetting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScreenName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CopyName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NoOfCopies = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    WatermarkRequired = table.Column<bool>(type: "bit", nullable: false),
                    LogoRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsoLogoRequired = table.Column<bool>(type: "bit", nullable: false),
                    WatermarkLable = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrintSetting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Process",
                columns: table => new
                {
                    ProcessId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProcessName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    HrlyRate = table.Column<float>(type: "real", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Process", x => x.ProcessId);
                });

            migrationBuilder.CreateTable(
                name: "ProductionLogSetting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProdStopType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EfficiencyPerc = table.Column<int>(type: "int", nullable: false),
                    Definition = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionLogSetting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectTypeMaster",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeOfProject = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TypeOfCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    StartSeries = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTypeMaster", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RawMaterial",
                columns: table => new
                {
                    RMId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RMName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Specification = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Density = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawMaterial", x => x.RMId);
                });

            migrationBuilder.CreateTable(
                name: "ScreenManagements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScreenName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Display = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Topic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Required = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreenManagements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Screens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScreenCode = table.Column<int>(type: "int", nullable: false),
                    ScreenName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPrintRequired = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Screens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShiftAllocation",
                columns: table => new
                {
                    ShiftId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShiftCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ShiftName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShiftStartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    ShiftEndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    BreakStartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    BreakEndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    TotalWorkingHours = table.Column<TimeSpan>(type: "time", nullable: true),
                    OverTimeAllowed = table.Column<bool>(type: "bit", nullable: false),
                    LateMarkAllowed = table.Column<int>(type: "int", nullable: true),
                    EarlyLeaveAllowed = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftAllocation", x => x.ShiftId);
                });

            migrationBuilder.CreateTable(
                name: "State",
                columns: table => new
                {
                    StateCode = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StateName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsSystemDefined = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_State", x => x.StateCode);
                });

            migrationBuilder.CreateTable(
                name: "Stores",
                columns: table => new
                {
                    StoreId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoreName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    StoreDesc = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CanIssue = table.Column<bool>(type: "bit", nullable: false),
                    CanAdd = table.Column<bool>(type: "bit", nullable: false),
                    IsSystemDefined = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stores", x => x.StoreId);
                });

            migrationBuilder.CreateTable(
                name: "TermsAndConditions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermsAndConditions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UOM",
                columns: table => new
                {
                    UnitCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UnitDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSystemDefined = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UOM", x => x.UnitCode);
                });

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScreenName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreferenceJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CurrencyToday",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrId = table.Column<int>(type: "int", nullable: false),
                    CurrValueInRs = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrValueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyToday", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurrencyToday_Currency_CurrId",
                        column: x => x.CurrId,
                        principalTable: "Currency",
                        principalColumn: "CurrId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BreakdownMaintenance",
                columns: table => new
                {
                    BreakdownId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    BreakdownDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BreakdownDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ResolutionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolutionDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Idlecost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BreakdownMaintenance", x => x.BreakdownId);
                    table.ForeignKey(
                        name: "FK_BreakdownMaintenance_Machine_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machine",
                        principalColumn: "MachineId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceSchedule",
                columns: table => new
                {
                    ScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    MaintenanceId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ScheduleDay = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RevisionNo = table.Column<int>(type: "int", nullable: false),
                    RevisionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceSchedule", x => x.ScheduleId);
                    table.ForeignKey(
                        name: "FK_MaintenanceSchedule_Machine_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machine",
                        principalColumn: "MachineId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupingSub",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupingId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: true),
                    Factors = table.Column<int>(type: "int", nullable: true),
                    Processid = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupingSub", x => x.ID);
                    table.ForeignKey(
                        name: "FK_GroupingSub_Factors_Factors",
                        column: x => x.Factors,
                        principalTable: "Factors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupingSub_Grouping_GroupingId",
                        column: x => x.GroupingId,
                        principalTable: "Grouping",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupingSub_Process_Processid",
                        column: x => x.Processid,
                        principalTable: "Process",
                        principalColumn: "ProcessId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanyDetails",
                columns: table => new
                {
                    CompanyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CompanyCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BusinessType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IndustryType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RegisteredAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    BillingAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ShippingAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StateName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StateCode = table.Column<int>(type: "int", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ZipCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MobileNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    EmailId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GSTIN = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    PAN = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IECNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    IsEOURegistered = table.Column<bool>(type: "bit", nullable: false),
                    EOUType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LUTNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BankAuthorizedDealerCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    EOURegistrationNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EOURegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EOUStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EOUValidityTill = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinancialYearType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OpeningBalanceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsoLogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsoLogoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrencyCode = table.Column<int>(type: "int", nullable: false),
                    TimeZone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CIN = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: true),
                    TIN = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BankBranch = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AccountType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IFSCCode = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    BankAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    BankCity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MICRCode = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: true),
                    SWIFTCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UdyamNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PFRegistrationNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    ESIRegistrationNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    ShopEstablishmentNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ExciseRegistrationNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    SEZRegistrationNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CustomsRegistrationNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    AlternateEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContactPersonName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CustomFinancialYearType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomBusinessType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomIndustryType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DecimalPlaces = table.Column<int>(type: "int", nullable: false),
                    BookTypeDc = table.Column<byte>(type: "tinyint", nullable: false),
                    BookTypeInvoice = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyDetails", x => x.CompanyId);
                    table.ForeignKey(
                        name: "FK_CompanyDetails_Currency_CurrencyCode",
                        column: x => x.CurrencyCode,
                        principalTable: "Currency",
                        principalColumn: "CurrId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyDetails_State_StateCode",
                        column: x => x.StateCode,
                        principalTable: "State",
                        principalColumn: "StateCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Customer",
                columns: table => new
                {
                    CustId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CustAddr = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    GSTNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    PANNo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    VendorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContactNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    StateId = table.Column<int>(type: "int", nullable: false),
                    StateName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OpenBal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    OpenBalPndg = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    OpenBalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Inactive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    PinCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    Scope = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BusiType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TanNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CINNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RejectRetrac = table.Column<bool>(type: "bit", nullable: false),
                    CurrId = table.Column<int>(type: "int", nullable: false),
                    BankDetails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Distance = table.Column<double>(type: "float", nullable: false),
                    TDS = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TDSExmptdAmt = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TDSBillval = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TDSDeductper = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TdsDeduct = table.Column<bool>(type: "bit", nullable: false),
                    SupTyp = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TdsDeductInSales = table.Column<bool>(type: "bit", nullable: false),
                    LineId = table.Column<bool>(type: "bit", nullable: false),
                    IsWoNoReq = table.Column<bool>(type: "bit", nullable: false),
                    TaxValidate = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.CustId);
                    table.ForeignKey(
                        name: "FK_Customer_Currency_CurrId",
                        column: x => x.CurrId,
                        principalTable: "Currency",
                        principalColumn: "CurrId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Customer_State_StateId",
                        column: x => x.StateId,
                        principalTable: "State",
                        principalColumn: "StateCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Leads",
                columns: table => new
                {
                    LeadId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeadName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Designation = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    PhoneNo = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    LeadOwner = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    LeadSource = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LeadStatus = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    StateId = table.Column<int>(type: "int", nullable: false),
                    StateName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    City = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PinCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: true),
                    WebSite = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BusiType = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    SupType = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    CurrId = table.Column<int>(type: "int", nullable: true),
                    GSTNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    PANNo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LeadTally = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leads", x => x.LeadId);
                    table.ForeignKey(
                        name: "FK_Leads_Currency_CurrId",
                        column: x => x.CurrId,
                        principalTable: "Currency",
                        principalColumn: "CurrId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Leads_State_StateId",
                        column: x => x.StateId,
                        principalTable: "State",
                        principalColumn: "StateCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vendor",
                columns: table => new
                {
                    VendorCode = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    VendorAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    PANNo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ContactNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Scope = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    GSTNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    CustomerCode = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    OpenBal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    OpenBalPndg = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    OpenBalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BusiType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Inactive = table.Column<bool>(type: "bit", nullable: false),
                    SSI = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BankAccountNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BankIFSCCode = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    BankBranchName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StateName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StateCode = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PinCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    TDS = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TDSExmptdAmt = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TDSBillval = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TDSDeductper = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TdsDeduct = table.Column<bool>(type: "bit", nullable: false),
                    IsitPurch = table.Column<bool>(type: "bit", nullable: false),
                    IsitSubCon = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TANNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CINNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RejectionRetract = table.Column<bool>(type: "bit", nullable: false),
                    IsAllowAccRejRew = table.Column<bool>(type: "bit", nullable: false),
                    Distance = table.Column<double>(type: "float", nullable: false),
                    TaxValidate = table.Column<bool>(type: "bit", nullable: false),
                    TermsAndCondition = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    YearOfEstd = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PreviousYearTurnover = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NoofEmployee = table.Column<int>(type: "int", nullable: false),
                    ProdMfg = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MajorCust = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MfgFacility = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InspectionFacility = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnyQualSysFoll = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnyProdApprvl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CouldMeetQuantityReq = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    CompnyType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ManintngDocs = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    FAIMaintained = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    FODMaintained = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    CurrId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendor", x => x.VendorCode);
                    table.ForeignKey(
                        name: "FK_Vendor_Currency_CurrId",
                        column: x => x.CurrId,
                        principalTable: "Currency",
                        principalColumn: "CurrId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vendor_State_StateCode",
                        column: x => x.StateCode,
                        principalTable: "State",
                        principalColumn: "StateCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionIssueAssy",
                columns: table => new
                {
                    IssueId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IssueNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IssueDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IssueToWhom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StoreIssId = table.Column<int>(type: "int", nullable: false),
                    NoOfItems = table.Column<int>(type: "int", nullable: false),
                    MainRemark = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IssueTally = table.Column<bool>(type: "bit", nullable: false),
                    IssueQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionIssueAssy", x => x.IssueId);
                    table.ForeignKey(
                        name: "FK_ProductionIssueAssy_Stores_StoreIssId",
                        column: x => x.StoreIssId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionReturnAssy",
                columns: table => new
                {
                    ReturnId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsNoInspection = table.Column<bool>(type: "bit", nullable: false),
                    Rejection = table.Column<bool>(type: "bit", nullable: false),
                    Return = table.Column<bool>(type: "bit", nullable: false),
                    ReturnNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MainRemark = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ReturnBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddStoreId = table.Column<int>(type: "int", nullable: true),
                    ReturnTally = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionReturnAssy", x => x.ReturnId);
                    table.ForeignKey(
                        name: "FK_ProductionReturnAssy_Stores_AddStoreId",
                        column: x => x.AddStoreId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionSCNAssy",
                columns: table => new
                {
                    SCNId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SCNNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SCNDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SCNDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MainRemark = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    AddStoreId = table.Column<int>(type: "int", nullable: true),
                    IssueStoreId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionSCNAssy", x => x.SCNId);
                    table.ForeignKey(
                        name: "FK_ProductionSCNAssy_Stores_AddStoreId",
                        column: x => x.AddStoreId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionSCNAssy_Stores_IssueStoreId",
                        column: x => x.IssueStoreId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionSCNComp",
                columns: table => new
                {
                    SCNId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SCNNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SCNDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SCNDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MainRemark = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    AddStoreId = table.Column<int>(type: "int", nullable: true),
                    IssueStoreId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionSCNComp", x => x.SCNId);
                    table.ForeignKey(
                        name: "FK_ProductionSCNComp_Stores_AddStoreId",
                        column: x => x.AddStoreId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionSCNComp_Stores_IssueStoreId",
                        column: x => x.IssueStoreId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StoreMap",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    ScreenName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FormName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoreId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreMap", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoreMap_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ToolCribIssue",
                columns: table => new
                {
                    TCIssueId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TCIssueNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TCIssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TCIssueDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TCIssueTally = table.Column<bool>(type: "bit", nullable: false),
                    CategoryCode = table.Column<int>(type: "int", nullable: true),
                    StoreId = table.Column<int>(type: "int", nullable: true),
                    IssuedToWhom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsReturn = table.Column<bool>(type: "bit", nullable: false),
                    MainRemarks = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    NoOfItems = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolCribIssue", x => x.TCIssueId);
                    table.ForeignKey(
                        name: "FK_ToolCribIssue_Category_CategoryCode",
                        column: x => x.CategoryCode,
                        principalTable: "Category",
                        principalColumn: "CategoryCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ToolCribIssue_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ToolCribReturns",
                columns: table => new
                {
                    TCReturnId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TCReturnNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TCReturnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TCReturnDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromWhom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NoOfItems = table.Column<int>(type: "int", nullable: false),
                    MainRemarks = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    BreakageRemarks = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    StoreId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolCribReturns", x => x.TCReturnId);
                    table.ForeignKey(
                        name: "FK_ToolCribReturns_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Item",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemCode = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Specification = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DrawingNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Barcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QRCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BtOtorMfg = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CategoryCode = table.Column<int>(type: "int", nullable: false),
                    Hide = table.Column<bool>(type: "bit", nullable: false),
                    MeasureUnit = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PurchaseUnit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnitConvert = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    RackNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ROL = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    MOL = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    leadTime = table.Column<int>(type: "int", nullable: true),
                    PartWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BinLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsiderAsRm = table.Column<bool>(type: "bit", nullable: false),
                    Make = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RmId = table.Column<int>(type: "int", nullable: true),
                    Shape = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Density = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Length = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Width = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Height = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Thickness = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    WallThickness = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    OuterDia = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    InnerDia = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BatchNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    AltRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    HSNCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SACCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsServcS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsServcL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CGST = table.Column<decimal>(type: "decimal(5,3)", precision: 5, scale: 3, nullable: false),
                    SGST = table.Column<decimal>(type: "decimal(5,3)", precision: 5, scale: 3, nullable: false),
                    IGST = table.Column<decimal>(type: "decimal(5,3)", precision: 5, scale: 3, nullable: false),
                    Rev = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RevItemId = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_Item_Category_CategoryCode",
                        column: x => x.CategoryCode,
                        principalTable: "Category",
                        principalColumn: "CategoryCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Item_RawMaterial_RmId",
                        column: x => x.RmId,
                        principalTable: "RawMaterial",
                        principalColumn: "RMId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Item_UOM_MeasureUnit",
                        column: x => x.MeasureUnit,
                        principalTable: "UOM",
                        principalColumn: "UnitCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CostCenter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectTypeId = table.Column<int>(type: "int", nullable: false),
                    ProjectNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    CostCenterName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CustId = table.Column<int>(type: "int", nullable: false),
                    AllocatedBudget = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalBudget = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AvailableBudget = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    IsDispatch = table.Column<bool>(type: "bit", nullable: false),
                    ProjectStatus = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AssToEnq = table.Column<bool>(type: "bit", nullable: false),
                    AssToQuote = table.Column<bool>(type: "bit", nullable: false),
                    AssToMfgPO = table.Column<bool>(type: "bit", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BroughtOutMechanical = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MechanicalFabricated = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CEFieldElement = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CePanelElement = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MechanicalDesign = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ElectricalDesign = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PlcofflineProgram = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SoftwareDesign = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MechanicalAssembly = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ElectricalWiring = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Commision = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PLCOnlineProgram = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProjectManager = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    RawMaterial = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ToolingCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Fabrication = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Machining = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Finishing = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DesignCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AssemblyFAT = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Installation = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Documentation = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Logistics = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FinancialCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AdminOverHead = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Others = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ConsumableAmt = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaintenanceAmt = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SafetyAmt = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ServiceDabaspetAmt = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ServiceHalolAmt = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CapexAmt = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostCenter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CostCenter_Customer_CustId",
                        column: x => x.CustId,
                        principalTable: "Customer",
                        principalColumn: "CustId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CostCenter_ProjectTypeMaster_ProjectTypeId",
                        column: x => x.ProjectTypeId,
                        principalTable: "ProjectTypeMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerIndirect",
                columns: table => new
                {
                    AltCustId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustId = table.Column<int>(type: "int", nullable: false),
                    AltCustName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    AltCustAddr1 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    AltCustAddr2 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GSTNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    State = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Pincode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContactNo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PANNo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerIndirect", x => x.AltCustId);
                    table.ForeignKey(
                        name: "FK_CustomerIndirect_Customer_CustId",
                        column: x => x.CustId,
                        principalTable: "Customer",
                        principalColumn: "CustId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EnquirySales",
                columns: table => new
                {
                    EnquiryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Prefix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    EnquiryNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnquiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EnquiryDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustId = table.Column<int>(type: "int", nullable: false),
                    NoOfItems = table.Column<int>(type: "int", nullable: false),
                    MainRemarks = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    KindOfAttention = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    TotalValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    EnquiryTally = table.Column<bool>(type: "bit", nullable: false),
                    MfgORLab = table.Column<bool>(type: "bit", nullable: false),
                    Cancel = table.Column<bool>(type: "bit", nullable: false),
                    CancelReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShortClose = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    POCName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    POCContactNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpectedReplyDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnquirySales", x => x.EnquiryId);
                    table.ForeignKey(
                        name: "FK_EnquirySales_Customer_CustId",
                        column: x => x.CustId,
                        principalTable: "Customer",
                        principalColumn: "CustId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LabourGRN",
                columns: table => new
                {
                    GRNId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustId = table.Column<int>(type: "int", nullable: false),
                    GRNNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    GRNDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GRNDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RefDcNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefDcDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DcTally = table.Column<bool>(type: "bit", nullable: false),
                    DcCancel = table.Column<bool>(type: "bit", nullable: false),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ShortClose = table.Column<bool>(type: "bit", nullable: false),
                    AddStoreId = table.Column<int>(type: "int", nullable: false),
                    NoOfItems = table.Column<int>(type: "int", nullable: true),
                    TransportMode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    VehicleNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TransFrom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TransTo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    KindofAttention = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CancelReason = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WithoutPoGRN = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabourGRN", x => x.GRNId);
                    table.ForeignKey(
                        name: "FK_LabourGRN_Customer_CustId",
                        column: x => x.CustId,
                        principalTable: "Customer",
                        principalColumn: "CustId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabourGRN_Stores_AddStoreId",
                        column: x => x.AddStoreId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LabourSCN",
                columns: table => new
                {
                    SCNId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Suffix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SCNNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    SCNDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SCNDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustId = table.Column<int>(type: "int", nullable: false),
                    StoreIssId = table.Column<int>(type: "int", nullable: false),
                    StoreIssueStoreId = table.Column<int>(type: "int", nullable: true),
                    AddStoreId = table.Column<int>(type: "int", nullable: false),
                    SCNCancel = table.Column<bool>(type: "bit", nullable: false),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ShortClose = table.Column<bool>(type: "bit", nullable: false),
                    SCNTally = table.Column<bool>(type: "bit", nullable: false),
                    MainRemarks = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    KindofAttention = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NoOfItems = table.Column<int>(type: "int", nullable: false),
                    CancelReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CanceledBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsLevel1 = table.Column<bool>(type: "bit", nullable: false),
                    IsLevel2 = table.Column<bool>(type: "bit", nullable: false),
                    IsLevel3 = table.Column<bool>(type: "bit", nullable: false),
                    IsLevel4 = table.Column<bool>(type: "bit", nullable: false),
                    Level1Sign = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Level2Sign = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Level3Sign = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Level4Sign = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Authorized = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsRejected = table.Column<bool>(type: "bit", nullable: false),
                    RejectReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabourSCN", x => x.SCNId);
                    table.ForeignKey(
                        name: "FK_LabourSCN_Customer_CustId",
                        column: x => x.CustId,
                        principalTable: "Customer",
                        principalColumn: "CustId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabourSCN_Stores_StoreIssId",
                        column: x => x.StoreIssId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabourSCN_Stores_StoreIssueStoreId",
                        column: x => x.StoreIssueStoreId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionIssueComp",
                columns: table => new
                {
                    IssueId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IssueNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IssueDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IssueToWhom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StoreIssId = table.Column<int>(type: "int", nullable: false),
                    CustId = table.Column<int>(type: "int", nullable: true),
                    NoOfItems = table.Column<int>(type: "int", nullable: false),
                    IssueQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MainRemark = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IssueTally = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionIssueComp", x => x.IssueId);
                    table.ForeignKey(
                        name: "FK_ProductionIssueComp_Customer_CustId",
                        column: x => x.CustId,
                        principalTable: "Customer",
                        principalColumn: "CustId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionIssueComp_Stores_StoreIssId",
                        column: x => x.StoreIssId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionReturnComp",
                columns: table => new
                {
                    ReturnId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsNoInspection = table.Column<bool>(type: "bit", nullable: false),
                    Rejection = table.Column<bool>(type: "bit", nullable: false),
                    Return = table.Column<bool>(type: "bit", nullable: false),
                    ReturnNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustId = table.Column<int>(type: "int", nullable: true),
                    MainRemark = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ReturnBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddStoreId = table.Column<int>(type: "int", nullable: true),
                    ReturnTally = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionReturnComp", x => x.ReturnId);
                    table.ForeignKey(
                        name: "FK_ProductionReturnComp_Customer_CustId",
                        column: x => x.CustId,
                        principalTable: "Customer",
                        principalColumn: "CustId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReturnComp_Stores_AddStoreId",
                        column: x => x.AddStoreId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContactPerson",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustId = table.Column<int>(type: "int", nullable: true),
                    ContactPersonName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PhoneNo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Designation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LeadId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactPerson", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactPerson_Customer_CustId",
                        column: x => x.CustId,
                        principalTable: "Customer",
                        principalColumn: "CustId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContactPerson_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "LeadId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseGRN",
                columns: table => new
                {
                    GRNId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Suffix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    GRNNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    GRNDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GRNDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VendorCode = table.Column<int>(type: "int", nullable: true),
                    RefDcNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RefDcDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefInvNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RefInvDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MainRemarks = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    AddStoreId = table.Column<int>(type: "int", nullable: false),
                    NoOfItems = table.Column<int>(type: "int", nullable: true),
                    GRNCancel = table.Column<bool>(type: "bit", nullable: false),
                    ShortClose = table.Column<bool>(type: "bit", nullable: false),
                    GRNTally = table.Column<bool>(type: "bit", nullable: false),
                    IsWithoutPoGRN = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsWithoutInspection = table.Column<bool>(type: "bit", nullable: false),
                    KindofAttention = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CancelReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseGRN", x => x.GRNId);
                    table.ForeignKey(
                        name: "FK_PurchaseGRN_Stores_AddStoreId",
                        column: x => x.AddStoreId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseGRN_Vendor_VendorCode",
                        column: x => x.VendorCode,
                        principalTable: "Vendor",
                        principalColumn: "VendorCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseSCN",
                columns: table => new
                {
                    SCNId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Suffix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SCNNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    SCNDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SCNDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VendorCode = table.Column<int>(type: "int", nullable: false),
                    AddStoreId = table.Column<int>(type: "int", nullable: false),
                    StoreIssId = table.Column<int>(type: "int", nullable: false),
                    SCNCancel = table.Column<bool>(type: "bit", nullable: false),
                    SCNTally = table.Column<bool>(type: "bit", nullable: false),
                    ShortClose = table.Column<bool>(type: "bit", nullable: false),
                    MainRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    KindofAttention = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    NoOfItems = table.Column<int>(type: "int", nullable: false),
                    CancelReason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CancelBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsLevel1 = table.Column<bool>(type: "bit", nullable: false),
                    IsLevel2 = table.Column<bool>(type: "bit", nullable: false),
                    IsLevel3 = table.Column<bool>(type: "bit", nullable: false),
                    IsLevel4 = table.Column<bool>(type: "bit", nullable: false),
                    Level1Sign = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Level2Sign = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Level3Sign = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Level4Sign = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Authorized = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsRejected = table.Column<bool>(type: "bit", nullable: false),
                    RejectReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseSCN", x => x.SCNId);
                    table.ForeignKey(
                        name: "FK_PurchaseSCN_Stores_AddStoreId",
                        column: x => x.AddStoreId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseSCN_Stores_StoreIssId",
                        column: x => x.StoreIssId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseSCN_Vendor_VendorCode",
                        column: x => x.VendorCode,
                        principalTable: "Vendor",
                        principalColumn: "VendorCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubConGRN",
                columns: table => new
                {
                    GRNId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsWithoutPoDc = table.Column<bool>(type: "bit", nullable: false),
                    IsNoInspection = table.Column<bool>(type: "bit", nullable: false),
                    IsManual = table.Column<bool>(type: "bit", nullable: false),
                    Rejection = table.Column<bool>(type: "bit", nullable: false),
                    Return = table.Column<bool>(type: "bit", nullable: false),
                    GRNNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    GRNDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GRNDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VendorCode = table.Column<int>(type: "int", nullable: false),
                    NoOfItems = table.Column<int>(type: "int", nullable: false),
                    MainRemark = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    PartyDC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartyDCDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AddStoreId = table.Column<int>(type: "int", nullable: true),
                    GRNTally = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubConGRN", x => x.GRNId);
                    table.ForeignKey(
                        name: "FK_SubConGRN_Stores_AddStoreId",
                        column: x => x.AddStoreId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubConGRN_Vendor_VendorCode",
                        column: x => x.VendorCode,
                        principalTable: "Vendor",
                        principalColumn: "VendorCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubConSCN",
                columns: table => new
                {
                    SCNId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SCNNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SCNDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SCNDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MainRemark = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    VendorCode = table.Column<int>(type: "int", nullable: false),
                    AddStoreId = table.Column<int>(type: "int", nullable: true),
                    IssueStoreId = table.Column<int>(type: "int", nullable: true),
                    SCNCancel = table.Column<bool>(type: "bit", nullable: false),
                    SCNTally = table.Column<bool>(type: "bit", nullable: false),
                    MainRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancelReason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubConSCN", x => x.SCNId);
                    table.ForeignKey(
                        name: "FK_SubConSCN_Stores_AddStoreId",
                        column: x => x.AddStoreId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubConSCN_Stores_IssueStoreId",
                        column: x => x.IssueStoreId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubConSCN_Vendor_VendorCode",
                        column: x => x.VendorCode,
                        principalTable: "Vendor",
                        principalColumn: "VendorCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VendorContact",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorCode = table.Column<int>(type: "int", nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContactNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorContact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorContact_Vendor_VendorCode",
                        column: x => x.VendorCode,
                        principalTable: "Vendor",
                        principalColumn: "VendorCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VendorInDirect",
                columns: table => new
                {
                    AltVendorCode = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorCode = table.Column<int>(type: "int", nullable: true),
                    AltVendorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AltVendorAddr1 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    AltVendorAddr2 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    City = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GSTNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    StateName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    PinCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: true),
                    PANNo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContactNo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorInDirect", x => x.AltVendorCode);
                    table.ForeignKey(
                        name: "FK_VendorInDirect_Vendor_VendorCode",
                        column: x => x.VendorCode,
                        principalTable: "Vendor",
                        principalColumn: "VendorCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssemblyModify",
                columns: table => new
                {
                    AssyModifyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecNo = table.Column<int>(type: "int", nullable: false),
                    AssyId = table.Column<int>(type: "int", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: true),
                    UtilityQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cancel = table.Column<bool>(type: "bit", nullable: false),
                    ProcessId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssemblyModify", x => x.AssyModifyId);
                    table.ForeignKey(
                        name: "FK_AssemblyModify_Item_AssyId",
                        column: x => x.AssyId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssemblyModify_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssemblyModify_Process_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Process",
                        principalColumn: "ProcessId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompMaster",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompItemId = table.Column<int>(type: "int", nullable: true),
                    RMId = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    OtherDet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LabWORM = table.Column<bool>(type: "bit", nullable: false),
                    IsDefaultRM = table.Column<bool>(type: "bit", nullable: false),
                    EBLength = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    EBWidth = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompMaster_Item_CompItemId",
                        column: x => x.CompItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompMaster_Item_RMId",
                        column: x => x.RMId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinalInspectionRef",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemID = table.Column<int>(type: "int", nullable: false),
                    InsRefData = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    TotRows = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinalInspectionRef", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinalInspectionRef_Item_ItemID",
                        column: x => x.ItemID,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncomingInspectionRef",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemID = table.Column<int>(type: "int", nullable: false),
                    ProcessId = table.Column<int>(type: "int", nullable: false),
                    InsRefData = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    TotRows = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingInspectionRef", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingInspectionRef_Item_ItemID",
                        column: x => x.ItemID,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InspectionRef",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemID = table.Column<int>(type: "int", nullable: false),
                    ProcessId = table.Column<int>(type: "int", nullable: false),
                    InsRefData = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    TotRows = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionRef", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionRef_Item_ItemID",
                        column: x => x.ItemID,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InstrumentDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UniqueId = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Range = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    LeastCount = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    OtherDetails = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    NextCalibrate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstrumentDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstrumentDetails_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemProductAssign",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    CustId = table.Column<int>(type: "int", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HSNCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemProductAssign", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemProductAssign_Customer_CustId",
                        column: x => x.CustId,
                        principalTable: "Customer",
                        principalColumn: "CustId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemProductAssign_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemSub",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: true),
                    VendorId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemSub", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemSub_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "CustId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemSub_Vendor_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendor",
                        principalColumn: "VendorCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MasterInspection",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectNo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InspectDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Qty = table.Column<float>(type: "real", nullable: false),
                    ProcessId = table.Column<int>(type: "int", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    StaffName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    InspectData = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    TotRows = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterInspection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MasterInspection_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MasterInspection_Process_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Process",
                        principalColumn: "ProcessId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SCNGen",
                columns: table => new
                {
                    SCNGenId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SCNGenNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SCNGenDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StoreId = table.Column<int>(type: "int", nullable: false),
                    IsWithBom = table.Column<bool>(type: "bit", nullable: false),
                    AssyId = table.Column<int>(type: "int", nullable: true),
                    SubAssyId = table.Column<int>(type: "int", nullable: true),
                    SubAssyId2 = table.Column<int>(type: "int", nullable: true),
                    SubAssyId3 = table.Column<int>(type: "int", nullable: true),
                    ReqQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MainRemark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NoOfItems = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SCNGen", x => x.SCNGenId);
                    table.ForeignKey(
                        name: "FK_SCNGen_Item_AssyId",
                        column: x => x.AssyId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SCNGen_Item_SubAssyId",
                        column: x => x.SubAssyId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SCNGen_Item_SubAssyId2",
                        column: x => x.SubAssyId2,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SCNGen_Item_SubAssyId3",
                        column: x => x.SubAssyId3,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SCNGen_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockAdd",
                columns: table => new
                {
                    AddId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AddDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    StoreId = table.Column<int>(type: "int", nullable: false),
                    AddQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    BatchNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ScreenCode = table.Column<int>(type: "int", nullable: false),
                    SubItemRefID = table.Column<int>(type: "int", nullable: false),
                    RefNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RcSubID = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockAdd", x => x.AddId);
                    table.ForeignKey(
                        name: "FK_StockAdd_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockAdd_Screens_ScreenCode",
                        column: x => x.ScreenCode,
                        principalTable: "Screens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockAdd_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockIssue",
                columns: table => new
                {
                    IssueId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    StoreId = table.Column<int>(type: "int", nullable: false),
                    IssueQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    BatchNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ScreenCode = table.Column<int>(type: "int", nullable: false),
                    SubItemRefID = table.Column<int>(type: "int", nullable: true),
                    RefNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RefDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IssuedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockIssue", x => x.IssueId);
                    table.ForeignKey(
                        name: "FK_StockIssue_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockIssue_Screens_ScreenCode",
                        column: x => x.ScreenCode,
                        principalTable: "Screens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockIssue_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ToolCribIssueSub",
                columns: table => new
                {
                    TCIssueSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TCIssueId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    QtyOut = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolCribIssueSub", x => x.TCIssueSubId);
                    table.ForeignKey(
                        name: "FK_ToolCribIssueSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ToolCribIssueSub_ToolCribIssue_TCIssueId",
                        column: x => x.TCIssueId,
                        principalTable: "ToolCribIssue",
                        principalColumn: "TCIssueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssmblyDef",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssmblyID = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UtilQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Cancel = table.Column<bool>(type: "bit", nullable: false),
                    IsComponent = table.Column<bool>(type: "bit", nullable: false),
                    ProcessId = table.Column<int>(type: "int", nullable: true),
                    StockConvertedQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PRConvertedQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Completed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssmblyDef", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssmblyDef_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssmblyDef_Item_AssmblyID",
                        column: x => x.AssmblyID,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssmblyDef_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssmblyDef_Process_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Process",
                        principalColumn: "ProcessId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaterialIssNote",
                columns: table => new
                {
                    MINId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IssueNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IssueDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToWhom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NoOfItems = table.Column<int>(type: "int", nullable: true),
                    MainRemark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsWithBom = table.Column<bool>(type: "bit", nullable: false),
                    AssyId = table.Column<int>(type: "int", nullable: true),
                    SubAssyId = table.Column<int>(type: "int", nullable: true),
                    SubAssyId2 = table.Column<int>(type: "int", nullable: true),
                    SubAssyId3 = table.Column<int>(type: "int", nullable: true),
                    ReqQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    StoreIssId = table.Column<int>(type: "int", nullable: false),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialIssNote", x => x.MINId);
                    table.ForeignKey(
                        name: "FK_MaterialIssNote_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaterialIssNote_Item_AssyId",
                        column: x => x.AssyId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaterialIssNote_Item_SubAssyId",
                        column: x => x.SubAssyId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaterialIssNote_Item_SubAssyId2",
                        column: x => x.SubAssyId2,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaterialIssNote_Item_SubAssyId3",
                        column: x => x.SubAssyId3,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaterialIssNote_Stores_StoreIssId",
                        column: x => x.StoreIssId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoreInterTrans",
                columns: table => new
                {
                    ISTId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ISTNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ISTDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StoreAddId = table.Column<int>(type: "int", nullable: false),
                    StoreIssueId = table.Column<int>(type: "int", nullable: false),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    MainRemark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NoOfItems = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreInterTrans", x => x.ISTId);
                    table.ForeignKey(
                        name: "FK_StoreInterTrans_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StoreInterTrans_Stores_StoreAddId",
                        column: x => x.StoreAddId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StoreInterTrans_Stores_StoreIssueId",
                        column: x => x.StoreIssueId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExpInv",
                columns: table => new
                {
                    ExpInvId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Prefix = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    ExpInvNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    ExpInvDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpInvDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustId = table.Column<int>(type: "int", nullable: false),
                    KindOfAttention = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsSameAsShipping = table.Column<bool>(type: "bit", nullable: false),
                    ShipAddrsId = table.Column<int>(type: "int", nullable: true),
                    NoOfItems = table.Column<int>(type: "int", nullable: true),
                    MainRemark = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CurrId = table.Column<int>(type: "int", nullable: false),
                    TodayVal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsCancel = table.Column<bool>(type: "bit", nullable: false),
                    CancelReason = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    InvTally = table.Column<bool>(type: "bit", nullable: false),
                    StoreIssId = table.Column<int>(type: "int", nullable: true),
                    DcCumInv = table.Column<bool>(type: "bit", nullable: false),
                    EWayNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    EWayDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VehicleNo = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    TransportMode = table.Column<int>(type: "int", nullable: true),
                    TransFrom = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    TransTo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    SubSupplyType = table.Column<int>(type: "int", nullable: true),
                    SubSupplyDesc = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TransId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TransName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TransDocNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    DriverNo = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    InvShortClose = table.Column<bool>(type: "bit", nullable: false),
                    IsTaxRequired = table.Column<bool>(type: "bit", nullable: false),
                    TotalGrossAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsItemWiseDiscAmtOrPerReq = table.Column<bool>(type: "bit", nullable: false),
                    DiscAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalBasicAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FreightCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PackingAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    PackingPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PackingCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InsuranceAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    InsurancePercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InsuranceCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    SGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    IGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TotalCGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalSGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalIGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OtherCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TCSAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    TCSPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TCSAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalTaxable = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsRoundOffEnabled = table.Column<bool>(type: "bit", nullable: false),
                    RoundOff = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TDSAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InvoiceRemovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GoodsRemovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CountryOfOrigin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CountryOfDestination = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreCarriage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlaceOfReceipt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FlightOrVesselNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PortOfLoading = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PortOfDischarge = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FinalDestination = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GrossWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NetWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PackageDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransporterAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransporterName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocketNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Incoterm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpInv", x => x.ExpInvId);
                    table.ForeignKey(
                        name: "FK_ExpInv_Currency_CurrId",
                        column: x => x.CurrId,
                        principalTable: "Currency",
                        principalColumn: "CurrId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExpInv_CustomerIndirect_ShipAddrsId",
                        column: x => x.ShipAddrsId,
                        principalTable: "CustomerIndirect",
                        principalColumn: "AltCustId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExpInv_Customer_CustId",
                        column: x => x.CustId,
                        principalTable: "Customer",
                        principalColumn: "CustId");
                    table.ForeignKey(
                        name: "FK_ExpInv_Stores_StoreIssId",
                        column: x => x.StoreIssId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LabInv",
                columns: table => new
                {
                    LabInvId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Prefix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    LabInvNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LabInvDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LabInvDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustId = table.Column<int>(type: "int", nullable: false),
                    KindOfAttention = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSameAsShipping = table.Column<bool>(type: "bit", nullable: false),
                    ShipAddrsId = table.Column<int>(type: "int", nullable: true),
                    NoOfItems = table.Column<int>(type: "int", nullable: true),
                    MainRemark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrId = table.Column<int>(type: "int", nullable: false),
                    LabInvTally = table.Column<bool>(type: "bit", nullable: false),
                    TodayVal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvCancel = table.Column<bool>(type: "bit", nullable: false),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CanceledBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShortClose = table.Column<bool>(type: "bit", nullable: false),
                    CancelReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PayTerms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveryTems = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalGrossAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsItemWiseDiscAmtOrPerReq = table.Column<bool>(type: "bit", nullable: false),
                    TDSAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalBasicAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FreightCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PackingAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    PackingPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PackingCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InsuranceAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    InsurancePercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InsuranceCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    SGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    IGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TotalCGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalSGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalIGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OtherCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TCSAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    TCSPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TCSAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalTaxable = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsRoundOffEnabled = table.Column<bool>(type: "bit", nullable: false),
                    RoundOff = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TermsId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabInv", x => x.LabInvId);
                    table.ForeignKey(
                        name: "FK_LabInv_Currency_CurrId",
                        column: x => x.CurrId,
                        principalTable: "Currency",
                        principalColumn: "CurrId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabInv_CustomerIndirect_ShipAddrsId",
                        column: x => x.ShipAddrsId,
                        principalTable: "CustomerIndirect",
                        principalColumn: "AltCustId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabInv_Customer_CustId",
                        column: x => x.CustId,
                        principalTable: "Customer",
                        principalColumn: "CustId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabInv_TermsAndConditions_TermsId",
                        column: x => x.TermsId,
                        principalTable: "TermsAndConditions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LabourDcOutgoing",
                columns: table => new
                {
                    DcId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustId = table.Column<int>(type: "int", nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DcNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DcDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DcDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransportMode = table.Column<int>(type: "int", nullable: false),
                    VehicleNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TransFrom = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransTo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NoOfItems = table.Column<int>(type: "int", nullable: false),
                    DcCancel = table.Column<bool>(type: "bit", nullable: false),
                    DcTally = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    StoreIssId = table.Column<int>(type: "int", nullable: false),
                    EwayNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EwayDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DcSign = table.Column<bool>(type: "bit", nullable: false),
                    NonReturnDc = table.Column<bool>(type: "bit", nullable: false),
                    ReceivedReturnRM = table.Column<bool>(type: "bit", nullable: false),
                    SCNRejectReworkReturn = table.Column<bool>(type: "bit", nullable: false),
                    TransId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TransName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TransDocNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    SubSupplyType = table.Column<int>(type: "int", nullable: true),
                    SubSupplyDesc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    KindofAttention = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CancelReason = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsSameAsShipping = table.Column<bool>(type: "bit", nullable: false),
                    ShippingId = table.Column<int>(type: "int", nullable: true),
                    WithoutPoDc = table.Column<bool>(type: "bit", nullable: false),
                    Manual = table.Column<bool>(type: "bit", nullable: false),
                    ShortClose = table.Column<bool>(type: "bit", nullable: false),
                    CanceledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ReduceInvAsPerRej = table.Column<bool>(type: "bit", nullable: false),
                    CanceledBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabourDcOutgoing", x => x.DcId);
                    table.ForeignKey(
                        name: "FK_LabourDcOutgoing_CustomerIndirect_ShippingId",
                        column: x => x.ShippingId,
                        principalTable: "CustomerIndirect",
                        principalColumn: "AltCustId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabourDcOutgoing_Customer_CustId",
                        column: x => x.CustId,
                        principalTable: "Customer",
                        principalColumn: "CustId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabourDcOutgoing_Stores_StoreIssId",
                        column: x => x.StoreIssId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MfgDc",
                columns: table => new
                {
                    DcId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustId = table.Column<int>(type: "int", nullable: true),
                    IsSameAsShipping = table.Column<bool>(type: "bit", nullable: false),
                    ShippingId = table.Column<int>(type: "int", nullable: true),
                    IssueStoreId = table.Column<int>(type: "int", nullable: false),
                    IsPurchaseReturn = table.Column<bool>(type: "bit", nullable: false),
                    VendorCode = table.Column<int>(type: "int", nullable: true),
                    RefGRNId = table.Column<int>(type: "int", nullable: true),
                    RefGRNNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModeofTrasnport = table.Column<int>(type: "int", nullable: false),
                    VehicleNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransFrom = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransTo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TransDocNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TransName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SubSuppType = table.Column<int>(type: "int", maxLength: 20, nullable: true),
                    EwayBillNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EwayBillDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsWithoutPoDc = table.Column<bool>(type: "bit", nullable: false),
                    DcShortClose = table.Column<bool>(type: "bit", nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DcNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DcDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DcDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Towards = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    MainRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RejRemark = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DcCancel = table.Column<bool>(type: "bit", nullable: false),
                    CancelReason = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    DcTally = table.Column<bool>(type: "bit", nullable: false),
                    ReduceInvAsPerRej = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MfgDc", x => x.DcId);
                    table.ForeignKey(
                        name: "FK_MfgDc_CustomerIndirect_ShippingId",
                        column: x => x.ShippingId,
                        principalTable: "CustomerIndirect",
                        principalColumn: "AltCustId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MfgDc_Customer_CustId",
                        column: x => x.CustId,
                        principalTable: "Customer",
                        principalColumn: "CustId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MfgDc_Stores_IssueStoreId",
                        column: x => x.IssueStoreId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MfgDc_Vendor_VendorCode",
                        column: x => x.VendorCode,
                        principalTable: "Vendor",
                        principalColumn: "VendorCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MfgInv",
                columns: table => new
                {
                    InvId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Prefix = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    InvNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    InvDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustId = table.Column<int>(type: "int", nullable: false),
                    KindOfAttention = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    IsSameAsShipping = table.Column<bool>(type: "bit", nullable: false),
                    ShipAddrsId = table.Column<int>(type: "int", nullable: true),
                    NoOfItems = table.Column<int>(type: "int", nullable: true),
                    DeliveryTems = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PayTerms = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MainRemark = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CurrId = table.Column<int>(type: "int", nullable: false),
                    TodayVal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsCancel = table.Column<bool>(type: "bit", nullable: false),
                    CancelReason = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    InvTally = table.Column<bool>(type: "bit", nullable: false),
                    StoreIssId = table.Column<int>(type: "int", nullable: true),
                    DcCumInv = table.Column<bool>(type: "bit", nullable: false),
                    EWayNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    EWayDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VehicleNo = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    TransportMode = table.Column<int>(type: "int", nullable: true),
                    TransFrom = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    TransTo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    SubSupplyType = table.Column<int>(type: "int", nullable: true),
                    SubSupplyDesc = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TransId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TransName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TransDocNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    DriverNo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    LcNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LcDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LcExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PoShortClose = table.Column<bool>(type: "bit", nullable: false),
                    TotalGrossAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsItemWiseDiscAmtOrPerReq = table.Column<bool>(type: "bit", nullable: false),
                    DiscAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalBasicAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FreightCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PackingAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    PackingPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PackingCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InsuranceAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    InsurancePercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InsuranceCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    SGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    IGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TotalCGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalSGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalIGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OtherCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TCSAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    TCSPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TCSAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalTaxable = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsRoundOffEnabled = table.Column<bool>(type: "bit", nullable: false),
                    RoundOff = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TDSAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MfgInv", x => x.InvId);
                    table.ForeignKey(
                        name: "FK_MfgInv_Currency_CurrId",
                        column: x => x.CurrId,
                        principalTable: "Currency",
                        principalColumn: "CurrId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MfgInv_CustomerIndirect_ShipAddrsId",
                        column: x => x.ShipAddrsId,
                        principalTable: "CustomerIndirect",
                        principalColumn: "AltCustId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MfgInv_Customer_CustId",
                        column: x => x.CustId,
                        principalTable: "Customer",
                        principalColumn: "CustId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MfgInv_Stores_StoreIssId",
                        column: x => x.StoreIssId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MfgPo",
                columns: table => new
                {
                    PoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MfgORLab = table.Column<bool>(type: "bit", nullable: false),
                    CustId = table.Column<int>(type: "int", nullable: false),
                    KindOfAttention = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSameAsShipping = table.Column<bool>(type: "bit", nullable: false),
                    ShipAddrsId = table.Column<int>(type: "int", nullable: true),
                    CurrId = table.Column<int>(type: "int", nullable: true),
                    TodayVal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PoTypeId = table.Column<int>(type: "int", nullable: true),
                    SaleOrderNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PONo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PODate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PODateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NoOfItems = table.Column<int>(type: "int", nullable: false),
                    PayTerms = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DeliveryTerms = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MainRemark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    isRejTrackReq = table.Column<bool>(type: "bit", nullable: false),
                    IsOpenPO = table.Column<bool>(type: "bit", nullable: false),
                    PoTally = table.Column<bool>(type: "bit", nullable: false),
                    PoCancl = table.Column<bool>(type: "bit", nullable: false),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CancelBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShortClose = table.Column<bool>(type: "bit", nullable: false),
                    RejAndRet = table.Column<bool>(type: "bit", nullable: false),
                    TotalGrossAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsItemWiseDiscAmtOrPerReq = table.Column<bool>(type: "bit", nullable: false),
                    DiscAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalBasicAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FreightCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PackingAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    PackingPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PackingCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InsuranceAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    InsurancePercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InsuranceCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    SGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    IGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TotalCGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalSGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalIGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OtherCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TCSAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    TCSPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TCSAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalTaxable = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsRoundOffEnabled = table.Column<bool>(type: "bit", nullable: false),
                    RoundOff = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OANo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OADate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OAWONO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OAWODate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OARemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OATerms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MfgPo", x => x.PoId);
                    table.ForeignKey(
                        name: "FK_MfgPo_Currency_CurrId",
                        column: x => x.CurrId,
                        principalTable: "Currency",
                        principalColumn: "CurrId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MfgPo_CustomerIndirect_ShipAddrsId",
                        column: x => x.ShipAddrsId,
                        principalTable: "CustomerIndirect",
                        principalColumn: "AltCustId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MfgPo_Customer_CustId",
                        column: x => x.CustId,
                        principalTable: "Customer",
                        principalColumn: "CustId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MfgPo_PoType_PoTypeId",
                        column: x => x.PoTypeId,
                        principalTable: "PoType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MfgQuote",
                columns: table => new
                {
                    QuoteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MfgOrLab = table.Column<bool>(type: "bit", nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    QuoteNo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    QuoteDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuoteDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustId = table.Column<int>(type: "int", nullable: false),
                    KindOfAttention = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSameAsShipping = table.Column<bool>(type: "bit", nullable: false),
                    ShipAddrsId = table.Column<int>(type: "int", nullable: true),
                    NoOfItems = table.Column<int>(type: "int", nullable: true),
                    MainRemark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrId = table.Column<int>(type: "int", nullable: false),
                    QuotationTally = table.Column<bool>(type: "bit", nullable: false),
                    TodayVal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsCancel = table.Column<bool>(type: "bit", nullable: false),
                    CancelReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShortClose = table.Column<bool>(type: "bit", nullable: false),
                    PayTerms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveryTems = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalGrossAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsItemWiseDiscAmtOrPerReq = table.Column<bool>(type: "bit", nullable: false),
                    DiscAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalBasicAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FreightCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PackingAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    PackingPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PackingCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InsuranceAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    InsurancePercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InsuranceCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    SGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    IGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TotalCGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalSGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalIGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OtherCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TCSAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    TCSPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TCSAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalTaxable = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsRoundOffEnabled = table.Column<bool>(type: "bit", nullable: false),
                    RoundOff = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TermsId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MfgQuote", x => x.QuoteId);
                    table.ForeignKey(
                        name: "FK_MfgQuote_Currency_CurrId",
                        column: x => x.CurrId,
                        principalTable: "Currency",
                        principalColumn: "CurrId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MfgQuote_CustomerIndirect_ShipAddrsId",
                        column: x => x.ShipAddrsId,
                        principalTable: "CustomerIndirect",
                        principalColumn: "AltCustId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MfgQuote_Customer_CustId",
                        column: x => x.CustId,
                        principalTable: "Customer",
                        principalColumn: "CustId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MfgQuote_TermsAndConditions_TermsId",
                        column: x => x.TermsId,
                        principalTable: "TermsAndConditions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PerformaInv",
                columns: table => new
                {
                    InvId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustId = table.Column<int>(type: "int", nullable: false),
                    KindOfAttention = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSameAsShipping = table.Column<bool>(type: "bit", nullable: false),
                    ShipAddrsId = table.Column<int>(type: "int", nullable: true),
                    CurrId = table.Column<int>(type: "int", nullable: true),
                    TodayVal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InvDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NoOfItems = table.Column<int>(type: "int", nullable: false),
                    PayTerms = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DeliveryTerms = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MainRemark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InvCancl = table.Column<bool>(type: "bit", nullable: false),
                    CancelReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalGrossAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsItemWiseDiscAmtOrPerReq = table.Column<bool>(type: "bit", nullable: false),
                    DiscAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalBasicAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FreightCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PackingAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    PackingPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PackingCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InsuranceAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    InsurancePercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InsuranceCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    SGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    IGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TotalCGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalSGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalIGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OtherCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TCSAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    TCSPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TCSAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalTaxable = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsRoundOffEnabled = table.Column<bool>(type: "bit", nullable: false),
                    RoundOff = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BankId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformaInv", x => x.InvId);
                    table.ForeignKey(
                        name: "FK_PerformaInv_Bank_BankId",
                        column: x => x.BankId,
                        principalTable: "Bank",
                        principalColumn: "BankId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PerformaInv_Currency_CurrId",
                        column: x => x.CurrId,
                        principalTable: "Currency",
                        principalColumn: "CurrId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PerformaInv_CustomerIndirect_ShipAddrsId",
                        column: x => x.ShipAddrsId,
                        principalTable: "CustomerIndirect",
                        principalColumn: "AltCustId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PerformaInv_Customer_CustId",
                        column: x => x.CustId,
                        principalTable: "Customer",
                        principalColumn: "CustId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EnquiryFeasibility",
                columns: table => new
                {
                    FesId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FesNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FesDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FesDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RefEnqId = table.Column<int>(type: "int", nullable: false),
                    RefEnqNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RefEnqDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustId = table.Column<int>(type: "int", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CompatibilityConfm = table.Column<bool>(type: "bit", nullable: false),
                    QuotRefNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PreparedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnquiryFeasibility", x => x.FesId);
                    table.ForeignKey(
                        name: "FK_EnquiryFeasibility_Customer_CustId",
                        column: x => x.CustId,
                        principalTable: "Customer",
                        principalColumn: "CustId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnquiryFeasibility_EnquirySales_RefEnqId",
                        column: x => x.RefEnqId,
                        principalTable: "EnquirySales",
                        principalColumn: "EnquiryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EnquirySalesSub",
                columns: table => new
                {
                    EnquirySubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnquiryId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UOM = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ItemCancel = table.Column<bool>(type: "bit", nullable: false),
                    ItemCancelReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CostCenterId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnquirySalesSub", x => x.EnquirySubId);
                    table.ForeignKey(
                        name: "FK_EnquirySalesSub_CostCenter_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EnquirySalesSub_EnquirySales_EnquiryId",
                        column: x => x.EnquiryId,
                        principalTable: "EnquirySales",
                        principalColumn: "EnquiryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnquirySalesSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseInvoice",
                columns: table => new
                {
                    InvId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InvDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VendorCode = table.Column<int>(type: "int", nullable: true),
                    KindOfAttention = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsSameAsShipping = table.Column<bool>(type: "bit", nullable: false),
                    ShipAddrsId = table.Column<int>(type: "int", nullable: true),
                    NoOfItems = table.Column<int>(type: "int", nullable: false),
                    MainRemark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CurrId = table.Column<int>(type: "int", nullable: false),
                    InvTally = table.Column<bool>(type: "bit", nullable: false),
                    IsAcptRejRewQtyRequired = table.Column<bool>(type: "bit", nullable: false),
                    TodayVal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvCancel = table.Column<bool>(type: "bit", nullable: false),
                    CancelReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CanceledBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShortClose = table.Column<bool>(type: "bit", nullable: false),
                    EWayNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EWayDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalGrossAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsItemWiseDiscAmtOrPerReq = table.Column<bool>(type: "bit", nullable: false),
                    DiscAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalBasicAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FreightCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PackingAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    PackingPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PackingCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InsuranceAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    InsurancePercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InsuranceCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    SGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    IGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TotalCGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalSGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalIGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OtherCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TCSAmtOrPer = table.Column<bool>(type: "bit", maxLength: 10, nullable: false),
                    TCSPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TCSAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalTaxable = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsRoundOffEnabled = table.Column<bool>(type: "bit", nullable: false),
                    RoundOff = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TDSAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TermsId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseInvoice", x => x.InvId);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoice_Currency_CurrId",
                        column: x => x.CurrId,
                        principalTable: "Currency",
                        principalColumn: "CurrId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoice_VendorInDirect_ShipAddrsId",
                        column: x => x.ShipAddrsId,
                        principalTable: "VendorInDirect",
                        principalColumn: "AltVendorCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoice_Vendor_VendorCode",
                        column: x => x.VendorCode,
                        principalTable: "Vendor",
                        principalColumn: "VendorCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseQuote",
                columns: table => new
                {
                    QuoteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchOrSub = table.Column<bool>(type: "bit", nullable: false),
                    QuoteNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuoteDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuoteDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VendorCode = table.Column<int>(type: "int", nullable: false),
                    KindOfAttention = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSameAsShipping = table.Column<bool>(type: "bit", nullable: false),
                    ShipAddrsId = table.Column<int>(type: "int", nullable: true),
                    NoOfItems = table.Column<int>(type: "int", nullable: true),
                    MainRemark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrId = table.Column<int>(type: "int", nullable: false),
                    TodayVal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TermsId = table.Column<int>(type: "int", nullable: true),
                    QuoteShortClose = table.Column<bool>(type: "bit", nullable: false),
                    QuotationTally = table.Column<bool>(type: "bit", nullable: false),
                    IsCancel = table.Column<bool>(type: "bit", nullable: false),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CancelledBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalGrossAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsItemWiseDiscAmtOrPerReq = table.Column<bool>(type: "bit", nullable: false),
                    DiscAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalBasicAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FreightCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PackingAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    PackingPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PackingCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InsuranceAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    InsurancePercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InsuranceCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    SGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    IGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TotalCGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalSGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalIGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OtherCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TCSAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    TCSPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TCSAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalTaxable = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsRoundOffEnabled = table.Column<bool>(type: "bit", nullable: false),
                    RoundOff = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseQuote", x => x.QuoteId);
                    table.ForeignKey(
                        name: "FK_PurchaseQuote_Currency_CurrId",
                        column: x => x.CurrId,
                        principalTable: "Currency",
                        principalColumn: "CurrId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseQuote_TermsAndConditions_TermsId",
                        column: x => x.TermsId,
                        principalTable: "TermsAndConditions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseQuote_VendorInDirect_ShipAddrsId",
                        column: x => x.ShipAddrsId,
                        principalTable: "VendorInDirect",
                        principalColumn: "AltVendorCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseQuote_Vendor_VendorCode",
                        column: x => x.VendorCode,
                        principalTable: "Vendor",
                        principalColumn: "VendorCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchPo",
                columns: table => new
                {
                    PoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchORSubCon = table.Column<bool>(type: "bit", nullable: false),
                    VendorCode = table.Column<int>(type: "int", nullable: false),
                    KindOfAttention = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSameAsShipping = table.Column<bool>(type: "bit", nullable: false),
                    ShipAddrsId = table.Column<int>(type: "int", nullable: true),
                    CurrId = table.Column<int>(type: "int", nullable: true),
                    TodayVal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PONo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RevisionNo = table.Column<int>(type: "int", nullable: true),
                    IsRevision = table.Column<bool>(type: "bit", nullable: false),
                    RefRevisonPoid = table.Column<int>(type: "int", nullable: true),
                    Suffix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PODate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PODateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NoOfItems = table.Column<int>(type: "int", nullable: false),
                    PayTerms = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DeliveryTerms = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MainRemark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TermsId = table.Column<int>(type: "int", nullable: true),
                    isRejTrackReq = table.Column<bool>(type: "bit", nullable: false),
                    IsOpenPO = table.Column<bool>(type: "bit", nullable: false),
                    PoTally = table.Column<bool>(type: "bit", nullable: false),
                    PoCancl = table.Column<bool>(type: "bit", nullable: false),
                    CancelReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RejAndRet = table.Column<bool>(type: "bit", nullable: false),
                    PoShortClose = table.Column<bool>(type: "bit", nullable: false),
                    TotalGrossAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsItemWiseDiscAmtOrPerReq = table.Column<bool>(type: "bit", nullable: false),
                    DiscAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalBasicAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FreightCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PackingAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    PackingPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PackingCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InsuranceAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    InsurancePercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InsuranceCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    SGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    IGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TotalCGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalSGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalIGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OtherCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TCSAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    TCSPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TCSAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalTaxable = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsRoundOffEnabled = table.Column<bool>(type: "bit", nullable: false),
                    RoundOff = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdvacewithOutTax = table.Column<bool>(type: "bit", nullable: false),
                    AdvanceAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    AdvancePercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdvanceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdvanceAmountBal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsLevel1 = table.Column<bool>(type: "bit", nullable: false),
                    IsLevel2 = table.Column<bool>(type: "bit", nullable: false),
                    IsLevel3 = table.Column<bool>(type: "bit", nullable: false),
                    IsLevel4 = table.Column<bool>(type: "bit", nullable: false),
                    Level1Sign = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Level2Sign = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Level3Sign = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Level4Sign = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Authorized = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsRejected = table.Column<bool>(type: "bit", nullable: false),
                    RejectReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchPo", x => x.PoId);
                    table.ForeignKey(
                        name: "FK_PurchPo_Currency_CurrId",
                        column: x => x.CurrId,
                        principalTable: "Currency",
                        principalColumn: "CurrId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchPo_TermsAndConditions_TermsId",
                        column: x => x.TermsId,
                        principalTable: "TermsAndConditions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchPo_VendorInDirect_ShipAddrsId",
                        column: x => x.ShipAddrsId,
                        principalTable: "VendorInDirect",
                        principalColumn: "AltVendorCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchPo_Vendor_VendorCode",
                        column: x => x.VendorCode,
                        principalTable: "Vendor",
                        principalColumn: "VendorCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubConDcOut",
                columns: table => new
                {
                    DcId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Prefix = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DcNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    DcDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DcDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StoreIssId = table.Column<int>(type: "int", nullable: false),
                    NoOfItems = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MainRemark = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DcTally = table.Column<bool>(type: "bit", nullable: false),
                    VendorCode = table.Column<int>(type: "int", nullable: true),
                    IsSameAsShipping = table.Column<bool>(type: "bit", nullable: false),
                    ShipAddrsId = table.Column<int>(type: "int", nullable: true),
                    VehicleNo = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    ModeOfTransportId = table.Column<int>(type: "int", nullable: true),
                    TransFrom = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    TransTo = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    SubSupplyType = table.Column<int>(type: "int", nullable: true),
                    SubSupplyDesc = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EwayTransId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EwayTransName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EwayTransDocNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    EwayBillDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EwayBillNumber = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DcVal = table.Column<double>(type: "float", nullable: true),
                    MainRemarks = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DcSign = table.Column<bool>(type: "bit", nullable: false),
                    Cancel = table.Column<bool>(type: "bit", nullable: false),
                    CancelReason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsVendor = table.Column<bool>(type: "bit", nullable: false),
                    DiffItemsOutIn = table.Column<bool>(type: "bit", nullable: false),
                    IsManualMaterialOut = table.Column<bool>(type: "bit", nullable: false),
                    IsWithoutPoDc = table.Column<bool>(type: "bit", nullable: false),
                    IsPurchaseRework = table.Column<bool>(type: "bit", nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubConDcOut", x => x.DcId);
                    table.ForeignKey(
                        name: "FK_SubConDcOut_Stores_StoreIssId",
                        column: x => x.StoreIssId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubConDcOut_VendorInDirect_ShipAddrsId",
                        column: x => x.ShipAddrsId,
                        principalTable: "VendorInDirect",
                        principalColumn: "AltVendorCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubConDcOut_Vendor_VendorCode",
                        column: x => x.VendorCode,
                        principalTable: "Vendor",
                        principalColumn: "VendorCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubConInv",
                columns: table => new
                {
                    InvId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InvDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VendorCode = table.Column<int>(type: "int", nullable: true),
                    KindOfAttention = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsSameAsShipping = table.Column<bool>(type: "bit", nullable: false),
                    ShipAddrsId = table.Column<int>(type: "int", nullable: true),
                    NoOfItems = table.Column<int>(type: "int", nullable: false),
                    MainRemark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CurrId = table.Column<int>(type: "int", nullable: false),
                    InvTally = table.Column<bool>(type: "bit", nullable: false),
                    IsAcptRejRewQtyRequired = table.Column<bool>(type: "bit", nullable: false),
                    TodayVal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvCancel = table.Column<bool>(type: "bit", nullable: false),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EWayNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EWayDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalGrossAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsItemWiseDiscAmtOrPerReq = table.Column<bool>(type: "bit", nullable: false),
                    DiscAmtOrPer = table.Column<bool>(type: "bit", nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalBasicAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FreightCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PackingAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    PackingPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PackingCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InsuranceAmtOrPer = table.Column<bool>(type: "bit", nullable: false),
                    InsurancePercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InsuranceCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    SGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    IGstRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TotalCGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalSGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalIGSTAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OtherCharges = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TCSAmtOrPer = table.Column<bool>(type: "bit", maxLength: 10, nullable: false),
                    TCSPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TCSAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalTaxable = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsRoundOffEnabled = table.Column<bool>(type: "bit", nullable: false),
                    RoundOff = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TDSAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TermsId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubConInv", x => x.InvId);
                    table.ForeignKey(
                        name: "FK_SubConInv_Currency_CurrId",
                        column: x => x.CurrId,
                        principalTable: "Currency",
                        principalColumn: "CurrId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubConInv_VendorInDirect_ShipAddrsId",
                        column: x => x.ShipAddrsId,
                        principalTable: "VendorInDirect",
                        principalColumn: "AltVendorCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubConInv_Vendor_VendorCode",
                        column: x => x.VendorCode,
                        principalTable: "Vendor",
                        principalColumn: "VendorCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProcessFlowChart",
                columns: table => new
                {
                    SubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id = table.Column<int>(type: "int", nullable: false),
                    CompItemId = table.Column<int>(type: "int", nullable: false),
                    RMId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    ProcId = table.Column<int>(type: "int", nullable: false),
                    RmWt = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CompWt = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ScrapWt = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ScrapEndBit = table.Column<int>(type: "int", nullable: true),
                    MachId = table.Column<int>(type: "int", nullable: true),
                    VendorCode = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Program = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CycleTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    SettingTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    SubProcess = table.Column<int>(type: "int", nullable: false),
                    UseSameSubProcess = table.Column<bool>(type: "bit", nullable: false),
                    GroupKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BOM = table.Column<bool>(type: "bit", nullable: false),
                    Inspection = table.Column<bool>(type: "bit", nullable: false),
                    ProcessSkip = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessFlowChart", x => x.SubId);
                    table.ForeignKey(
                        name: "FK_ProcessFlowChart_CompMaster_Id",
                        column: x => x.Id,
                        principalTable: "CompMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcessFlowChart_Item_CompItemId",
                        column: x => x.CompItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_ProcessFlowChart_Item_RMId",
                        column: x => x.RMId,
                        principalTable: "Item",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_ProcessFlowChart_Machine_MachId",
                        column: x => x.MachId,
                        principalTable: "Machine",
                        principalColumn: "MachineId");
                    table.ForeignKey(
                        name: "FK_ProcessFlowChart_Process_ProcId",
                        column: x => x.ProcId,
                        principalTable: "Process",
                        principalColumn: "ProcessId");
                    table.ForeignKey(
                        name: "FK_ProcessFlowChart_RawMaterial_ScrapEndBit",
                        column: x => x.ScrapEndBit,
                        principalTable: "RawMaterial",
                        principalColumn: "RMId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcessFlowChart_Vendor_VendorCode",
                        column: x => x.VendorCode,
                        principalTable: "Vendor",
                        principalColumn: "VendorCode");
                });

            migrationBuilder.CreateTable(
                name: "CalibrationHistory",
                columns: table => new
                {
                    CalibrateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstrumentId = table.Column<int>(type: "int", nullable: false),
                    PrevDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CalibrateNo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CalibrateStatus = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CalibrateCancl = table.Column<bool>(type: "bit", nullable: false),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Authorised = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CalibrateDetails = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ExpectedCalibrate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalibrationHistory", x => x.CalibrateId);
                    table.ForeignKey(
                        name: "FK_CalibrationHistory_InstrumentDetails_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "InstrumentDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockIssueTrack",
                columns: table => new
                {
                    IssueTrackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IssueId = table.Column<int>(type: "int", nullable: false),
                    AddId = table.Column<int>(type: "int", nullable: false),
                    UsedQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    StockIssueIssueId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockIssueTrack", x => x.IssueTrackId);
                    table.ForeignKey(
                        name: "FK_StockIssueTrack_StockAdd_AddId",
                        column: x => x.AddId,
                        principalTable: "StockAdd",
                        principalColumn: "AddId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockIssueTrack_StockIssue_IssueId",
                        column: x => x.IssueId,
                        principalTable: "StockIssue",
                        principalColumn: "IssueId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockIssueTrack_StockIssue_StockIssueIssueId",
                        column: x => x.StockIssueIssueId,
                        principalTable: "StockIssue",
                        principalColumn: "IssueId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ToolCribReturnSubs",
                columns: table => new
                {
                    TCReturnSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TCReturnId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    RefTCIssueSubId = table.Column<int>(type: "int", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    AccpQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RejQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RejRemark = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RewQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RewRemark = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolCribReturnSubs", x => x.TCReturnSubId);
                    table.ForeignKey(
                        name: "FK_ToolCribReturnSubs_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ToolCribReturnSubs_ToolCribIssueSub_RefTCIssueSubId",
                        column: x => x.RefTCIssueSubId,
                        principalTable: "ToolCribIssueSub",
                        principalColumn: "TCIssueSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ToolCribReturnSubs_ToolCribReturns_TCReturnId",
                        column: x => x.TCReturnId,
                        principalTable: "ToolCribReturns",
                        principalColumn: "TCReturnId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SCNGenSub",
                columns: table => new
                {
                    SCNGenSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsFromBOM = table.Column<bool>(type: "bit", nullable: false),
                    RefBomId = table.Column<int>(type: "int", nullable: true),
                    SCNGenId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UtlQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BatchNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Make = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RackNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CostId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SCNGenSub", x => x.SCNGenSubId);
                    table.ForeignKey(
                        name: "FK_SCNGenSub_AssmblyDef_RefBomId",
                        column: x => x.RefBomId,
                        principalTable: "AssmblyDef",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SCNGenSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SCNGenSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SCNGenSub_SCNGen_SCNGenId",
                        column: x => x.SCNGenId,
                        principalTable: "SCNGen",
                        principalColumn: "SCNGenId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaterialIssNoteSub",
                columns: table => new
                {
                    MINSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MINId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UtlQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    IssueQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BatchNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Make = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RackNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CostId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialIssNoteSub", x => x.MINSubId);
                    table.ForeignKey(
                        name: "FK_MaterialIssNoteSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaterialIssNoteSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaterialIssNoteSub_MaterialIssNote_MINId",
                        column: x => x.MINId,
                        principalTable: "MaterialIssNote",
                        principalColumn: "MINId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoreInterTransSub",
                columns: table => new
                {
                    ISTSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ISTId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    BatchNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreInterTransSub", x => x.ISTSubId);
                    table.ForeignKey(
                        name: "FK_StoreInterTransSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StoreInterTransSub_StoreInterTrans_ISTId",
                        column: x => x.ISTId,
                        principalTable: "StoreInterTrans",
                        principalColumn: "ISTId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractReview",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Prefix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ContractNo = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    ContractDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ContractDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PoId = table.Column<int>(type: "int", nullable: false),
                    RevisedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReviewResult = table.Column<bool>(type: "bit", nullable: true),
                    ReviewJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OANo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    OADate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractReview", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractReview_MfgPo_PoId",
                        column: x => x.PoId,
                        principalTable: "MfgPo",
                        principalColumn: "PoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RouteCard",
                columns: table => new
                {
                    RCId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MfgOrLab = table.Column<bool>(type: "bit", nullable: false),
                    IsManual = table.Column<bool>(type: "bit", nullable: false),
                    RCNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RCDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RCDateNow = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RcQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RcType = table.Column<byte>(type: "tinyint", nullable: false),
                    RcStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    CustId = table.Column<int>(type: "int", nullable: true),
                    RefPoId = table.Column<int>(type: "int", nullable: true),
                    SelectedItemType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompItemId = table.Column<int>(type: "int", nullable: false),
                    AssyId = table.Column<int>(type: "int", nullable: true),
                    SubAssyId = table.Column<int>(type: "int", nullable: true),
                    SubAssyId2 = table.Column<int>(type: "int", nullable: true),
                    SubAssyId3 = table.Column<int>(type: "int", nullable: true),
                    RMId = table.Column<int>(type: "int", nullable: true),
                    RMReqQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RMWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RMBatchNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MainRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IssueStoreId = table.Column<int>(type: "int", nullable: true),
                    AddStoreId = table.Column<int>(type: "int", nullable: true),
                    BarCodeImage = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    QrCodeImage = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteCard", x => x.RCId);
                    table.ForeignKey(
                        name: "FK_RouteCard_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteCard_Customer_CustId",
                        column: x => x.CustId,
                        principalTable: "Customer",
                        principalColumn: "CustId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteCard_Item_AssyId",
                        column: x => x.AssyId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteCard_Item_CompItemId",
                        column: x => x.CompItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RouteCard_Item_RMId",
                        column: x => x.RMId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteCard_Item_SubAssyId",
                        column: x => x.SubAssyId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteCard_Item_SubAssyId2",
                        column: x => x.SubAssyId2,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteCard_Item_SubAssyId3",
                        column: x => x.SubAssyId3,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteCard_MfgPo_RefPoId",
                        column: x => x.RefPoId,
                        principalTable: "MfgPo",
                        principalColumn: "PoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteCard_Stores_AddStoreId",
                        column: x => x.AddStoreId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteCard_Stores_IssueStoreId",
                        column: x => x.IssueStoreId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EnquiryFeasibilitySub",
                columns: table => new
                {
                    FesSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FesId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    TypeOfProject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BaseMaterial = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToolRequired = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpclReqOutsource = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TechCapaBilities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DelvryTrms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManHrReq = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RiskIfAny = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpclReqCustomer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompbltyConfm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StockAvail = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnquiryFeasibilitySub", x => x.FesSubId);
                    table.ForeignKey(
                        name: "FK_EnquiryFeasibilitySub_EnquiryFeasibility_FesId",
                        column: x => x.FesId,
                        principalTable: "EnquiryFeasibility",
                        principalColumn: "FesId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnquiryFeasibilitySub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MfgQuoteSub",
                columns: table => new
                {
                    QuoteSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuoteId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    RFQ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefEnqSubId = table.Column<int>(type: "int", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineGross = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Quotetally = table.Column<bool>(type: "bit", nullable: false),
                    LineDiscountPercent = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    LineDiscountAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LineBasicAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineCGSTRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LineSGSTRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LineIGSTRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ItemCancel = table.Column<bool>(type: "bit", nullable: false),
                    ItemCancelReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MfgQuoteSub", x => x.QuoteSubId);
                    table.ForeignKey(
                        name: "FK_MfgQuoteSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MfgQuoteSub_EnquirySalesSub_RefEnqSubId",
                        column: x => x.RefEnqSubId,
                        principalTable: "EnquirySalesSub",
                        principalColumn: "EnquirySubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MfgQuoteSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MfgQuoteSub_MfgQuote_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "MfgQuote",
                        principalColumn: "QuoteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractReviewSub",
                columns: table => new
                {
                    SubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Issue = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineCGSTRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LineSGSTRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LineIGSTRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FurtherRevision = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeliveryTerms = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LeadTime = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModeOfTransport = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerSplReq = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RiskOrderAccept = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractReviewSub", x => x.SubId);
                    table.ForeignKey(
                        name: "FK_ContractReviewSub_ContractReview_Id",
                        column: x => x.Id,
                        principalTable: "ContractReview",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContractReviewSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RouteCardSub",
                columns: table => new
                {
                    RCSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RCId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    UseSameSeq = table.Column<bool>(type: "bit", nullable: false),
                    GroupKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeqNo = table.Column<int>(type: "int", nullable: true),
                    ProcessId = table.Column<int>(type: "int", nullable: false),
                    ItemIdIn = table.Column<int>(type: "int", nullable: true),
                    ItemIdOut = table.Column<int>(type: "int", nullable: true),
                    TotalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    IssuedQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    WipQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    AccQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RejQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RewQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    NextProcessQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MachineId = table.Column<int>(type: "int", nullable: true),
                    VendorId = table.Column<int>(type: "int", nullable: true),
                    ProcessCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CycleTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    SettingTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    IsBOM = table.Column<bool>(type: "bit", nullable: false),
                    IsInspection = table.Column<bool>(type: "bit", nullable: false),
                    IsProcessSkip = table.Column<bool>(type: "bit", nullable: false),
                    ProcessSkipReason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsFinalProcess = table.Column<bool>(type: "bit", nullable: false),
                    ProcessStatus = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteCardSub", x => x.RCSubId);
                    table.ForeignKey(
                        name: "FK_RouteCardSub_Item_ItemIdIn",
                        column: x => x.ItemIdIn,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteCardSub_Item_ItemIdOut",
                        column: x => x.ItemIdOut,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteCardSub_Machine_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machine",
                        principalColumn: "MachineId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteCardSub_Process_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Process",
                        principalColumn: "ProcessId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RouteCardSub_RouteCard_RCId",
                        column: x => x.RCId,
                        principalTable: "RouteCard",
                        principalColumn: "RCId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RouteCardSub_Vendor_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendor",
                        principalColumn: "VendorCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MfgPoSub",
                columns: table => new
                {
                    PoSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PoId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    LineNo = table.Column<int>(type: "int", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    RefQuoteSubId = table.Column<int>(type: "int", nullable: true),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineGross = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineDiscountPercent = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    LineDiscountAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LineBasicAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineCGSTRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LineSGSTRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LineIGSTRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WoNO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IndentBalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    WOBalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    PerformaBalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RouteCardBalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ProdCompBalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ItemCancel = table.Column<bool>(type: "bit", nullable: false),
                    ItemCancelReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAnnexure = table.Column<bool>(type: "bit", nullable: false),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MfgPoSub", x => x.PoSubId);
                    table.ForeignKey(
                        name: "FK_MfgPoSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MfgPoSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MfgPoSub_MfgPo_PoId",
                        column: x => x.PoId,
                        principalTable: "MfgPo",
                        principalColumn: "PoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MfgPoSub_MfgQuoteSub_RefQuoteSubId",
                        column: x => x.RefQuoteSubId,
                        principalTable: "MfgQuoteSub",
                        principalColumn: "QuoteSubId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssemblyPoComponentTracker",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PoId = table.Column<int>(type: "int", nullable: true),
                    PoSubId = table.Column<int>(type: "int", nullable: true),
                    AssyId = table.Column<int>(type: "int", nullable: true),
                    ComponentId = table.Column<int>(type: "int", nullable: true),
                    MaxAllowedQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IssuedQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RemainingQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MfgPoPoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssemblyPoComponentTracker", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssemblyPoComponentTracker_Item_AssyId",
                        column: x => x.AssyId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssemblyPoComponentTracker_Item_ComponentId",
                        column: x => x.ComponentId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssemblyPoComponentTracker_MfgPoSub_PoSubId",
                        column: x => x.PoSubId,
                        principalTable: "MfgPoSub",
                        principalColumn: "PoSubId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssemblyPoComponentTracker_MfgPo_MfgPoPoId",
                        column: x => x.MfgPoPoId,
                        principalTable: "MfgPo",
                        principalColumn: "PoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobOrder",
                columns: table => new
                {
                    JobId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MfgORLab = table.Column<bool>(type: "bit", nullable: false),
                    IsManual = table.Column<bool>(type: "bit", nullable: false),
                    JobNo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    JobDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustId = table.Column<int>(type: "int", nullable: true),
                    RefPoId = table.Column<int>(type: "int", nullable: true),
                    RefPoSubId = table.Column<int>(type: "int", nullable: true),
                    POQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    JobOrderQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    JobBalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    JobTally = table.Column<bool>(type: "bit", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AssyId = table.Column<int>(type: "int", nullable: true),
                    UtilQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Cancel = table.Column<bool>(type: "bit", nullable: false),
                    CancelReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ParentJobId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobOrder", x => x.JobId);
                    table.ForeignKey(
                        name: "FK_JobOrder_Customer_CustId",
                        column: x => x.CustId,
                        principalTable: "Customer",
                        principalColumn: "CustId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobOrder_Item_AssyId",
                        column: x => x.AssyId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobOrder_JobOrder_ParentJobId",
                        column: x => x.ParentJobId,
                        principalTable: "JobOrder",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobOrder_MfgPoSub_RefPoSubId",
                        column: x => x.RefPoSubId,
                        principalTable: "MfgPoSub",
                        principalColumn: "PoSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobOrder_MfgPo_RefPoId",
                        column: x => x.RefPoId,
                        principalTable: "MfgPo",
                        principalColumn: "PoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LabourGRNSub",
                columns: table => new
                {
                    GRNSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GRNId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    DCQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    WONo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    RowRemarks = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    HeatNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    BatchNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ItemCancel = table.Column<bool>(type: "bit", nullable: false),
                    CancelItemReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefPoSubId = table.Column<int>(type: "int", nullable: true),
                    TransType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabourGRNSub", x => x.GRNSubId);
                    table.ForeignKey(
                        name: "FK_LabourGRNSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabourGRNSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabourGRNSub_LabourGRN_GRNId",
                        column: x => x.GRNId,
                        principalTable: "LabourGRN",
                        principalColumn: "GRNId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabourGRNSub_MfgPoSub_RefPoSubId",
                        column: x => x.RefPoSubId,
                        principalTable: "MfgPoSub",
                        principalColumn: "PoSubId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MaterialReqSub",
                columns: table => new
                {
                    MreqSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MreqId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: true),
                    CustId = table.Column<int>(type: "int", nullable: true),
                    PurchQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ItemCancel = table.Column<bool>(type: "bit", nullable: false),
                    ItemCancelReason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    VendorCode = table.Column<int>(type: "int", nullable: true),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    RefPoSubId = table.Column<int>(type: "int", nullable: true),
                    RefPoId = table.Column<int>(type: "int", nullable: true),
                    TotalGross = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RefBomId = table.Column<int>(type: "int", nullable: true),
                    MainAssyId = table.Column<int>(type: "int", nullable: true),
                    MfgPoIndentQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialReqSub", x => x.MreqSubId);
                    table.ForeignKey(
                        name: "FK_MaterialReqSub_AssmblyDef_RefBomId",
                        column: x => x.RefBomId,
                        principalTable: "AssmblyDef",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaterialReqSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaterialReqSub_Customer_CustId",
                        column: x => x.CustId,
                        principalTable: "Customer",
                        principalColumn: "CustId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaterialReqSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaterialReqSub_MaterialReq_MreqId",
                        column: x => x.MreqId,
                        principalTable: "MaterialReq",
                        principalColumn: "MreqId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaterialReqSub_MfgPoSub_RefPoSubId",
                        column: x => x.RefPoSubId,
                        principalTable: "MfgPoSub",
                        principalColumn: "PoSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaterialReqSub_MfgPo_RefPoId",
                        column: x => x.RefPoId,
                        principalTable: "MfgPo",
                        principalColumn: "PoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaterialReqSub_Vendor_VendorCode",
                        column: x => x.VendorCode,
                        principalTable: "Vendor",
                        principalColumn: "VendorCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PerformaInvSub",
                columns: table => new
                {
                    InvSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    RefPoSubId = table.Column<int>(type: "int", nullable: true),
                    RefPoNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefPoDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineGross = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineDiscountPercent = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    LineDiscountAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LineBasicAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineCGSTRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LineSGSTRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LineIGSTRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformaInvSub", x => x.InvSubId);
                    table.ForeignKey(
                        name: "FK_PerformaInvSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PerformaInvSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PerformaInvSub_MfgPoSub_RefPoSubId",
                        column: x => x.RefPoSubId,
                        principalTable: "MfgPoSub",
                        principalColumn: "PoSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PerformaInvSub_PerformaInv_InvId",
                        column: x => x.InvId,
                        principalTable: "PerformaInv",
                        principalColumn: "InvId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionIssueCompSub",
                columns: table => new
                {
                    IssueSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    IssueId = table.Column<int>(type: "int", nullable: false),
                    TransType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ProcessCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RcSubId = table.Column<int>(type: "int", nullable: true),
                    ProcessId = table.Column<int>(type: "int", nullable: true),
                    MachineId = table.Column<int>(type: "int", nullable: true),
                    RefPoSubId = table.Column<int>(type: "int", nullable: true),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    BatchNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HeatNo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionIssueCompSub", x => x.IssueSubId);
                    table.ForeignKey(
                        name: "FK_ProductionIssueCompSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionIssueCompSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionIssueCompSub_Machine_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machine",
                        principalColumn: "MachineId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionIssueCompSub_MfgPoSub_RefPoSubId",
                        column: x => x.RefPoSubId,
                        principalTable: "MfgPoSub",
                        principalColumn: "PoSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionIssueCompSub_Process_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Process",
                        principalColumn: "ProcessId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionIssueCompSub_ProductionIssueComp_IssueId",
                        column: x => x.IssueId,
                        principalTable: "ProductionIssueComp",
                        principalColumn: "IssueId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionIssueCompSub_RouteCardSub_RcSubId",
                        column: x => x.RcSubId,
                        principalTable: "RouteCardSub",
                        principalColumn: "RCSubId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobOrderSub",
                columns: table => new
                {
                    JobSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UtilQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ReqQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ItemCancel = table.Column<bool>(type: "bit", nullable: false),
                    ItemCancelReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CostId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobOrderSub", x => x.JobSubId);
                    table.ForeignKey(
                        name: "FK_JobOrderSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobOrderSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobOrderSub_JobOrder_JobId",
                        column: x => x.JobId,
                        principalTable: "JobOrder",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LabourSCNSub",
                columns: table => new
                {
                    SCNSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SCNId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    AcceptQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BalQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RejectQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ReworkQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    SCNBalQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RefGRNSubId = table.Column<int>(type: "int", nullable: true),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    ItemCancel = table.Column<bool>(type: "bit", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InspectionNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NCRNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CancelItemReason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    QtyConvert = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    HeatNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    BatchNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabourSCNSub", x => x.SCNSubId);
                    table.ForeignKey(
                        name: "FK_LabourSCNSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabourSCNSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabourSCNSub_LabourGRNSub_RefGRNSubId",
                        column: x => x.RefGRNSubId,
                        principalTable: "LabourGRNSub",
                        principalColumn: "GRNSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabourSCNSub_LabourSCN_SCNId",
                        column: x => x.SCNId,
                        principalTable: "LabourSCN",
                        principalColumn: "SCNId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EnquiryPurchasesub",
                columns: table => new
                {
                    EnquirySubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    EnquiryId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CostCenterId = table.Column<int>(type: "int", nullable: true),
                    RefMReqSubId = table.Column<int>(type: "int", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ItemCancel = table.Column<bool>(type: "bit", nullable: false),
                    ItemCancelReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnquiryPurchasesub", x => x.EnquirySubId);
                    table.ForeignKey(
                        name: "FK_EnquiryPurchasesub_CostCenter_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EnquiryPurchasesub_EnquiryPurchase_EnquiryId",
                        column: x => x.EnquiryId,
                        principalTable: "EnquiryPurchase",
                        principalColumn: "EnquiryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnquiryPurchasesub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnquiryPurchasesub_MaterialReqSub_RefMReqSubId",
                        column: x => x.RefMReqSubId,
                        principalTable: "MaterialReqSub",
                        principalColumn: "MreqSubId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionIssueAssySub",
                columns: table => new
                {
                    IssueSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IssueId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    IssueQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    BatchNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RefJobOrderSubId = table.Column<int>(type: "int", nullable: false),
                    RefJobId = table.Column<int>(type: "int", nullable: false),
                    AssyId = table.Column<int>(type: "int", nullable: true),
                    CostId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionIssueAssySub", x => x.IssueSubId);
                    table.ForeignKey(
                        name: "FK_ProductionIssueAssySub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionIssueAssySub_Item_AssyId",
                        column: x => x.AssyId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionIssueAssySub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionIssueAssySub_JobOrderSub_RefJobOrderSubId",
                        column: x => x.RefJobOrderSubId,
                        principalTable: "JobOrderSub",
                        principalColumn: "JobSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionIssueAssySub_JobOrder_RefJobId",
                        column: x => x.RefJobId,
                        principalTable: "JobOrder",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionIssueAssySub_ProductionIssueAssy_IssueId",
                        column: x => x.IssueId,
                        principalTable: "ProductionIssueAssy",
                        principalColumn: "IssueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LabourDcOutgoingSub",
                columns: table => new
                {
                    DcSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DcId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RejectQty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ReworkQty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RejectRowRem = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    AltQty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DcRowRem = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    ItemCancel = table.Column<bool>(type: "bit", nullable: false),
                    CancelItemReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefSCNSubId = table.Column<int>(type: "int", nullable: true),
                    RefGRNSubId = table.Column<int>(type: "int", nullable: true),
                    RefPoSubId = table.Column<int>(type: "int", nullable: true),
                    RcSubIds = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BatchNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    WONo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    HeatNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    RCNos = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabourDcOutgoingSub", x => x.DcSubId);
                    table.ForeignKey(
                        name: "FK_LabourDcOutgoingSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabourDcOutgoingSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabourDcOutgoingSub_LabourDcOutgoing_DcId",
                        column: x => x.DcId,
                        principalTable: "LabourDcOutgoing",
                        principalColumn: "DcId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabourDcOutgoingSub_LabourGRNSub_RefGRNSubId",
                        column: x => x.RefGRNSubId,
                        principalTable: "LabourGRNSub",
                        principalColumn: "GRNSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabourDcOutgoingSub_LabourSCNSub_RefSCNSubId",
                        column: x => x.RefSCNSubId,
                        principalTable: "LabourSCNSub",
                        principalColumn: "SCNSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabourDcOutgoingSub_MfgPoSub_RefPoSubId",
                        column: x => x.RefPoSubId,
                        principalTable: "MfgPoSub",
                        principalColumn: "PoSubId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EnquiryPurchaseVendorAssign",
                columns: table => new
                {
                    EnqPurchVendorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnquiryId = table.Column<int>(type: "int", nullable: true),
                    VendorCode = table.Column<int>(type: "int", nullable: true),
                    EnquirySubId = table.Column<int>(type: "int", nullable: true),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ItemStatus = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnquiryPurchaseVendorAssign", x => x.EnqPurchVendorId);
                    table.ForeignKey(
                        name: "FK_EnquiryPurchaseVendorAssign_EnquiryPurchase_EnquiryId",
                        column: x => x.EnquiryId,
                        principalTable: "EnquiryPurchase",
                        principalColumn: "EnquiryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EnquiryPurchaseVendorAssign_EnquiryPurchasesub_EnquirySubId",
                        column: x => x.EnquirySubId,
                        principalTable: "EnquiryPurchasesub",
                        principalColumn: "EnquirySubId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnquiryPurchaseVendorAssign_Vendor_VendorCode",
                        column: x => x.VendorCode,
                        principalTable: "Vendor",
                        principalColumn: "VendorCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LabInvSub",
                columns: table => new
                {
                    LabInvSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LabInvId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    RFQ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefDcSubId = table.Column<int>(type: "int", nullable: true),
                    LabourDcOutgoingSubDcSubId = table.Column<int>(type: "int", nullable: true),
                    RefDcNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefDcDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ItemCancel = table.Column<bool>(type: "bit", nullable: false),
                    CancelItemReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LineGross = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineDiscountPercent = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    LineDiscountAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LineBasicAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineCGSTRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LineSGSTRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LineIGSTRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabInvSub", x => x.LabInvSubId);
                    table.ForeignKey(
                        name: "FK_LabInvSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabInvSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabInvSub_LabInv_LabInvId",
                        column: x => x.LabInvId,
                        principalTable: "LabInv",
                        principalColumn: "LabInvId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabInvSub_LabourDcOutgoingSub_LabourDcOutgoingSubDcSubId",
                        column: x => x.LabourDcOutgoingSubDcSubId,
                        principalTable: "LabourDcOutgoingSub",
                        principalColumn: "DcSubId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LabourDcReturnCompTrack",
                columns: table => new
                {
                    TrackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RefDcSubId = table.Column<int>(type: "int", nullable: true),
                    RefSCNSubId = table.Column<int>(type: "int", nullable: true),
                    RefGRNSubId = table.Column<int>(type: "int", nullable: false),
                    RefPoSubId = table.Column<int>(type: "int", nullable: true),
                    ItemIdIn = table.Column<int>(type: "int", nullable: true),
                    QtyIn = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ItemIdOut = table.Column<int>(type: "int", nullable: true),
                    QtyOut = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LabourDcOutgoingDcId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabourDcReturnCompTrack", x => x.TrackId);
                    table.ForeignKey(
                        name: "FK_LabourDcReturnCompTrack_Item_ItemIdIn",
                        column: x => x.ItemIdIn,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabourDcReturnCompTrack_Item_ItemIdOut",
                        column: x => x.ItemIdOut,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabourDcReturnCompTrack_LabourDcOutgoingSub_RefDcSubId",
                        column: x => x.RefDcSubId,
                        principalTable: "LabourDcOutgoingSub",
                        principalColumn: "DcSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabourDcReturnCompTrack_LabourDcOutgoing_LabourDcOutgoingDcId",
                        column: x => x.LabourDcOutgoingDcId,
                        principalTable: "LabourDcOutgoing",
                        principalColumn: "DcId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabourDcReturnCompTrack_LabourGRNSub_RefGRNSubId",
                        column: x => x.RefGRNSubId,
                        principalTable: "LabourGRNSub",
                        principalColumn: "GRNSubId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabourDcReturnCompTrack_LabourSCNSub_RefSCNSubId",
                        column: x => x.RefSCNSubId,
                        principalTable: "LabourSCNSub",
                        principalColumn: "SCNSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabourDcReturnCompTrack_MfgPoSub_RefPoSubId",
                        column: x => x.RefPoSubId,
                        principalTable: "MfgPoSub",
                        principalColumn: "PoSubId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseQuoteSub",
                columns: table => new
                {
                    QuoteSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuoteId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BalQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RefEnqVendorAssignId = table.Column<int>(type: "int", nullable: true),
                    QuoteTally = table.Column<bool>(type: "bit", nullable: false),
                    ItemCancel = table.Column<bool>(type: "bit", nullable: false),
                    ItemShortClose = table.Column<bool>(type: "bit", nullable: false),
                    ItemCancelReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LineDiscountPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LineDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LineBasicAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineCGSTRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LineSGSTRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LineIGSTRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseQuoteSub", x => x.QuoteSubId);
                    table.ForeignKey(
                        name: "FK_PurchaseQuoteSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseQuoteSub_EnquiryPurchaseVendorAssign_RefEnqVendorAssignId",
                        column: x => x.RefEnqVendorAssignId,
                        principalTable: "EnquiryPurchaseVendorAssign",
                        principalColumn: "EnqPurchVendorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseQuoteSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseQuoteSub_PurchaseQuote_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "PurchaseQuote",
                        principalColumn: "QuoteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchPoSub",
                columns: table => new
                {
                    PoSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PoId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    RefQuoteSubId = table.Column<int>(type: "int", nullable: true),
                    RefMReqSubId = table.Column<int>(type: "int", nullable: true),
                    ProcessId = table.Column<int>(type: "int", nullable: true),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineGross = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineDiscountPercent = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    LineDiscountAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LineBasicAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineCGSTRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LineSGSTRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LineIGSTRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ItemCancel = table.Column<bool>(type: "bit", nullable: false),
                    ItemCancelReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchPoSub", x => x.PoSubId);
                    table.ForeignKey(
                        name: "FK_PurchPoSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchPoSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchPoSub_MaterialReqSub_RefMReqSubId",
                        column: x => x.RefMReqSubId,
                        principalTable: "MaterialReqSub",
                        principalColumn: "MreqSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchPoSub_Process_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Process",
                        principalColumn: "ProcessId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchPoSub_PurchPo_PoId",
                        column: x => x.PoId,
                        principalTable: "PurchPo",
                        principalColumn: "PoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchPoSub_PurchaseQuoteSub_RefQuoteSubId",
                        column: x => x.RefQuoteSubId,
                        principalTable: "PurchaseQuoteSub",
                        principalColumn: "QuoteSubId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubConDcOutSub",
                columns: table => new
                {
                    DcSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    DcId = table.Column<int>(type: "int", nullable: false),
                    TransType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ProcessCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RcSubId = table.Column<int>(type: "int", nullable: true),
                    ProcessId = table.Column<int>(type: "int", nullable: true),
                    MachineId = table.Column<int>(type: "int", nullable: true),
                    RefPoSubId = table.Column<int>(type: "int", nullable: true),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    ItemCancel = table.Column<bool>(type: "bit", nullable: false),
                    ItemCancelReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    BatchNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HeatNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubConDcOutSub", x => x.DcSubId);
                    table.ForeignKey(
                        name: "FK_SubConDcOutSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubConDcOutSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubConDcOutSub_Machine_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machine",
                        principalColumn: "MachineId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubConDcOutSub_Process_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Process",
                        principalColumn: "ProcessId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubConDcOutSub_PurchPoSub_RefPoSubId",
                        column: x => x.RefPoSubId,
                        principalTable: "PurchPoSub",
                        principalColumn: "PoSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubConDcOutSub_RouteCardSub_RcSubId",
                        column: x => x.RcSubId,
                        principalTable: "RouteCardSub",
                        principalColumn: "RCSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubConDcOutSub_SubConDcOut_DcId",
                        column: x => x.DcId,
                        principalTable: "SubConDcOut",
                        principalColumn: "DcId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubConGRNSub",
                columns: table => new
                {
                    GRNSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GRNId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    TransType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: true),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    CategoryCode = table.Column<int>(type: "int", nullable: true),
                    RefDcSubId = table.Column<int>(type: "int", nullable: true),
                    RefPoSubId = table.Column<int>(type: "int", nullable: true),
                    RefRcSubId = table.Column<int>(type: "int", nullable: true),
                    ProcessId = table.Column<int>(type: "int", nullable: true),
                    MachineId = table.Column<int>(type: "int", nullable: true),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubConGRNSub", x => x.GRNSubId);
                    table.ForeignKey(
                        name: "FK_SubConGRNSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubConGRNSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubConGRNSub_Machine_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machine",
                        principalColumn: "MachineId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubConGRNSub_Process_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Process",
                        principalColumn: "ProcessId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubConGRNSub_PurchPoSub_RefPoSubId",
                        column: x => x.RefPoSubId,
                        principalTable: "PurchPoSub",
                        principalColumn: "PoSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubConGRNSub_RouteCardSub_RefRcSubId",
                        column: x => x.RefRcSubId,
                        principalTable: "RouteCardSub",
                        principalColumn: "RCSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubConGRNSub_SubConDcOutSub_RefDcSubId",
                        column: x => x.RefDcSubId,
                        principalTable: "SubConDcOutSub",
                        principalColumn: "DcSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubConGRNSub_SubConGRN_GRNId",
                        column: x => x.GRNId,
                        principalTable: "SubConGRN",
                        principalColumn: "GRNId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubConGRNTrack",
                columns: table => new
                {
                    TrackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RefGRNSubId = table.Column<int>(type: "int", nullable: false),
                    RefRcSubId = table.Column<int>(type: "int", nullable: true),
                    RefDCSubId = table.Column<int>(type: "int", nullable: true),
                    RefPoSubId = table.Column<int>(type: "int", nullable: true),
                    ItemIdIn = table.Column<int>(type: "int", nullable: true),
                    QtyIn = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ItemIdOut = table.Column<int>(type: "int", nullable: true),
                    QtyOut = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubConGRNGRNId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubConGRNTrack", x => x.TrackId);
                    table.ForeignKey(
                        name: "FK_SubConGRNTrack_Item_ItemIdIn",
                        column: x => x.ItemIdIn,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubConGRNTrack_Item_ItemIdOut",
                        column: x => x.ItemIdOut,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubConGRNTrack_PurchPoSub_RefPoSubId",
                        column: x => x.RefPoSubId,
                        principalTable: "PurchPoSub",
                        principalColumn: "PoSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubConGRNTrack_RouteCardSub_RefRcSubId",
                        column: x => x.RefRcSubId,
                        principalTable: "RouteCardSub",
                        principalColumn: "RCSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubConGRNTrack_SubConDcOutSub_RefDCSubId",
                        column: x => x.RefDCSubId,
                        principalTable: "SubConDcOutSub",
                        principalColumn: "DcSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubConGRNTrack_SubConGRNSub_RefGRNSubId",
                        column: x => x.RefGRNSubId,
                        principalTable: "SubConGRNSub",
                        principalColumn: "GRNSubId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubConGRNTrack_SubConGRN_SubConGRNGRNId",
                        column: x => x.SubConGRNGRNId,
                        principalTable: "SubConGRN",
                        principalColumn: "GRNId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubConSCNSub",
                columns: table => new
                {
                    SCNSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SCNId = table.Column<int>(type: "int", nullable: false),
                    RefGRNSubId = table.Column<int>(type: "int", nullable: true),
                    RcSubId = table.Column<int>(type: "int", nullable: true),
                    PoSubId = table.Column<int>(type: "int", nullable: true),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    AccQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RejQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RejReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RewQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RewReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    InsNO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NCRNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubConSCNSub", x => x.SCNSubId);
                    table.ForeignKey(
                        name: "FK_SubConSCNSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubConSCNSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubConSCNSub_PurchPoSub_PoSubId",
                        column: x => x.PoSubId,
                        principalTable: "PurchPoSub",
                        principalColumn: "PoSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubConSCNSub_RouteCardSub_RcSubId",
                        column: x => x.RcSubId,
                        principalTable: "RouteCardSub",
                        principalColumn: "RCSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubConSCNSub_SubConGRNSub_RefGRNSubId",
                        column: x => x.RefGRNSubId,
                        principalTable: "SubConGRNSub",
                        principalColumn: "GRNSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubConSCNSub_SubConSCN_SCNId",
                        column: x => x.SCNId,
                        principalTable: "SubConSCN",
                        principalColumn: "SCNId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubConInvSub",
                columns: table => new
                {
                    InvSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    RefSCNSubId = table.Column<int>(type: "int", nullable: true),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RejectQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ReworkQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineDiscountPercent = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    LineDiscountAmount = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    LineBasicAmount = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    LineCGSTRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    LineSGSTRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    LineIGSTRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ItemCancel = table.Column<bool>(type: "bit", nullable: false),
                    ItemCancelReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CancelItemReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubConInvSub", x => x.InvSubId);
                    table.ForeignKey(
                        name: "FK_SubConInvSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubConInvSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubConInvSub_SubConInv_InvId",
                        column: x => x.InvId,
                        principalTable: "SubConInv",
                        principalColumn: "InvId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubConInvSub_SubConSCNSub_RefSCNSubId",
                        column: x => x.RefSCNSubId,
                        principalTable: "SubConSCNSub",
                        principalColumn: "SCNSubId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DailyLeaveRecord",
                columns: table => new
                {
                    DailyLeaveRecordID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsWeekend = table.Column<bool>(type: "bit", nullable: false),
                    DayType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LeaveApplicationID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyLeaveRecord", x => x.DailyLeaveRecordID);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeLeaveBalance",
                columns: table => new
                {
                    LeaveBalanceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeID = table.Column<int>(type: "int", nullable: false),
                    LeaveTypeId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    TotalAllocatedLeave = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LeavesTaken = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeLeaveBalance", x => x.LeaveBalanceID);
                    table.ForeignKey(
                        name: "FK_EmployeeLeaveBalance_LeaveType_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveType",
                        principalColumn: "LeaveTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpInvSub",
                columns: table => new
                {
                    ExpInvSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExpInvId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    RefDcSubId = table.Column<int>(type: "int", nullable: true),
                    RefPoSubId = table.Column<int>(type: "int", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineGross = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineDiscountPercent = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    LineDiscountAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LineBasicAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineCGSTRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LineSGSTRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LineIGSTRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ItemCancel = table.Column<bool>(type: "bit", nullable: false),
                    ItemCancelReason = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RcSubIds = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    WeightPerPiece = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PackateDetails = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpInvSub", x => x.ExpInvSubId);
                    table.ForeignKey(
                        name: "FK_ExpInvSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExpInvSub_ExpInv_ExpInvId",
                        column: x => x.ExpInvId,
                        principalTable: "ExpInv",
                        principalColumn: "ExpInvId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExpInvSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExpInvSub_MfgPoSub_RefPoSubId",
                        column: x => x.RefPoSubId,
                        principalTable: "MfgPoSub",
                        principalColumn: "PoSubId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinalInspection",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectNo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InspectDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalQty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    InspectBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    PONo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    InspectData = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    TotRows = table.Column<int>(type: "int", nullable: true),
                    DcType = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DCNo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DcDate = table.Column<DateTime>(type: "datetime2", maxLength: 250, nullable: false),
                    RefMfgDcSubID = table.Column<int>(type: "int", nullable: true),
                    RefLabDcID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CostCenter = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    InsDateNow = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcceptQty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NCrDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NCRNo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    NCRQty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RejQty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ReWork = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BatchNo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    SheetSlNo = table.Column<int>(type: "int", nullable: false),
                    IsRandom = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinalInspection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinalInspection_Customer_CustId",
                        column: x => x.CustId,
                        principalTable: "Customer",
                        principalColumn: "CustId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FinalInspection_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncomingInspections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Suffix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InspectNo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    InspectDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DcType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Grnno = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DCNo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ProductionLogId = table.Column<long>(type: "bigint", nullable: true),
                    RefPurchaseGRNSubId = table.Column<int>(type: "int", nullable: true),
                    RefProductionAssyGRNSubId = table.Column<int>(type: "int", nullable: true),
                    RefProductionCompGRNSubId = table.Column<int>(type: "int", nullable: true),
                    DcDate = table.Column<DateTime>(type: "datetime2", maxLength: 50, nullable: true),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BalQty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AcceptQty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RejQty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ReWorkQty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SuppId = table.Column<int>(type: "int", maxLength: 250, nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    IsRandom = table.Column<bool>(type: "bit", nullable: false),
                    InspectBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    InspectionData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotRows = table.Column<int>(type: "int", nullable: false),
                    CostCenter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsDateNow = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NCrDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NCRNo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    NCRQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BatchNo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    SheetSlNo = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessId = table.Column<int>(type: "int", nullable: true),
                    NoDimensionEntry = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingInspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncomingInspections_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IncomingInspections_Process_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Process",
                        principalColumn: "ProcessId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionLog",
                columns: table => new
                {
                    LogId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LogNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LogDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LogDateNow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RCId = table.Column<int>(type: "int", nullable: true),
                    RCQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RCProcessId = table.Column<int>(type: "int", nullable: true),
                    ProcessId = table.Column<int>(type: "int", nullable: true),
                    ProcessCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CycleTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    SettingTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    ShiftId = table.Column<int>(type: "int", nullable: true),
                    ShiftStartTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    ShiftEndTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    TotalHours = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MachineIdleTime = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MachineId = table.Column<int>(type: "int", nullable: true),
                    Operator = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustId = table.Column<int>(type: "int", nullable: true),
                    ItemIdOut = table.Column<int>(type: "int", nullable: true),
                    QtyOut = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    QtyOutPerUnit = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ItemIdIn = table.Column<int>(type: "int", nullable: true),
                    InputQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BatchNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AccQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RejQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RejReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RewQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RewReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RewProcessId = table.Column<int>(type: "int", nullable: true),
                    IssueStoreId = table.Column<int>(type: "int", nullable: true),
                    AddStoreId = table.Column<int>(type: "int", nullable: true),
                    ReturnQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ReturnReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    IsQcRequired = table.Column<bool>(type: "bit", nullable: false),
                    InspectId = table.Column<int>(type: "int", nullable: true),
                    Possible = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExpectedQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TargetQtyForTotalHours = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EfficiencyPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BonusEfficiencyPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EffectiveWorkingMinutes = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LogStatus = table.Column<bool>(type: "bit", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionLog", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_ProductionLog_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionLog_Customer_CustId",
                        column: x => x.CustId,
                        principalTable: "Customer",
                        principalColumn: "CustId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionLog_IncomingInspections_InspectId",
                        column: x => x.InspectId,
                        principalTable: "IncomingInspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionLog_Item_ItemIdIn",
                        column: x => x.ItemIdIn,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionLog_Item_ItemIdOut",
                        column: x => x.ItemIdOut,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionLog_Machine_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machine",
                        principalColumn: "MachineId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionLog_Process_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Process",
                        principalColumn: "ProcessId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionLog_Process_RewProcessId",
                        column: x => x.RewProcessId,
                        principalTable: "Process",
                        principalColumn: "ProcessId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionLog_RouteCardSub_RCProcessId",
                        column: x => x.RCProcessId,
                        principalTable: "RouteCardSub",
                        principalColumn: "RCSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionLog_RouteCard_RCId",
                        column: x => x.RCId,
                        principalTable: "RouteCard",
                        principalColumn: "RCId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionLog_ShiftAllocation_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "ShiftAllocation",
                        principalColumn: "ShiftId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionLog_Stores_AddStoreId",
                        column: x => x.AddStoreId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionLog_Stores_IssueStoreId",
                        column: x => x.IssueStoreId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionReturnAssySub",
                columns: table => new
                {
                    ReturnSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: true),
                    QtyReturned = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RefJobOrderId = table.Column<int>(type: "int", nullable: true),
                    RefIssueSubId = table.Column<int>(type: "int", nullable: true),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    InspectId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionReturnAssySub", x => x.ReturnSubId);
                    table.ForeignKey(
                        name: "FK_ProductionReturnAssySub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReturnAssySub_IncomingInspections_InspectId",
                        column: x => x.InspectId,
                        principalTable: "IncomingInspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReturnAssySub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReturnAssySub_JobOrder_RefJobOrderId",
                        column: x => x.RefJobOrderId,
                        principalTable: "JobOrder",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReturnAssySub_ProductionIssueAssySub_RefIssueSubId",
                        column: x => x.RefIssueSubId,
                        principalTable: "ProductionIssueAssySub",
                        principalColumn: "IssueSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReturnAssySub_ProductionReturnAssy_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "ProductionReturnAssy",
                        principalColumn: "ReturnId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionReturnCompSub",
                columns: table => new
                {
                    ReturnSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    TransType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: true),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    CategoryCode = table.Column<int>(type: "int", nullable: true),
                    RefIssueSubId = table.Column<int>(type: "int", nullable: true),
                    RefPoSubId = table.Column<int>(type: "int", nullable: true),
                    RefRcSubId = table.Column<int>(type: "int", nullable: true),
                    ProcessId = table.Column<int>(type: "int", nullable: true),
                    MachineId = table.Column<int>(type: "int", nullable: true),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    InspectId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionReturnCompSub", x => x.ReturnSubId);
                    table.ForeignKey(
                        name: "FK_ProductionReturnCompSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReturnCompSub_IncomingInspections_InspectId",
                        column: x => x.InspectId,
                        principalTable: "IncomingInspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReturnCompSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReturnCompSub_Machine_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machine",
                        principalColumn: "MachineId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReturnCompSub_MfgPoSub_RefPoSubId",
                        column: x => x.RefPoSubId,
                        principalTable: "MfgPoSub",
                        principalColumn: "PoSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReturnCompSub_Process_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Process",
                        principalColumn: "ProcessId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReturnCompSub_ProductionIssueCompSub_RefIssueSubId",
                        column: x => x.RefIssueSubId,
                        principalTable: "ProductionIssueCompSub",
                        principalColumn: "IssueSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReturnCompSub_ProductionReturnComp_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "ProductionReturnComp",
                        principalColumn: "ReturnId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionReturnCompSub_RouteCardSub_RefRcSubId",
                        column: x => x.RefRcSubId,
                        principalTable: "RouteCardSub",
                        principalColumn: "RCSubId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseGRNSub",
                columns: table => new
                {
                    GRNSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GRNId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    DNQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ExtraQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ExtraBalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    SCNRejRevertedPOQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsOpenPo = table.Column<bool>(type: "bit", nullable: false),
                    RefPoSubId = table.Column<int>(type: "int", nullable: true),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    ItemCancel = table.Column<bool>(type: "bit", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeatNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BatchNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ItemCancelReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InspectId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseGRNSub", x => x.GRNSubId);
                    table.ForeignKey(
                        name: "FK_PurchaseGRNSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseGRNSub_IncomingInspections_InspectId",
                        column: x => x.InspectId,
                        principalTable: "IncomingInspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseGRNSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseGRNSub_PurchPoSub_RefPoSubId",
                        column: x => x.RefPoSubId,
                        principalTable: "PurchPoSub",
                        principalColumn: "PoSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseGRNSub_PurchaseGRN_GRNId",
                        column: x => x.GRNId,
                        principalTable: "PurchaseGRN",
                        principalColumn: "GRNId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionLogSub",
                columns: table => new
                {
                    LogSubId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LogId = table.Column<long>(type: "bigint", nullable: false),
                    LogSettingId = table.Column<int>(type: "int", nullable: false),
                    ProductionStopageSettingsId = table.Column<int>(type: "int", nullable: true),
                    StopageTime = table.Column<TimeSpan>(type: "time", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionLogSub", x => x.LogSubId);
                    table.ForeignKey(
                        name: "FK_ProductionLogSub_ProductionLogSetting_ProductionStopageSettingsId",
                        column: x => x.ProductionStopageSettingsId,
                        principalTable: "ProductionLogSetting",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionLogSub_ProductionLog_LogId",
                        column: x => x.LogId,
                        principalTable: "ProductionLog",
                        principalColumn: "LogId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionReturnAssyTrack",
                columns: table => new
                {
                    TrackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RefReturnSubId = table.Column<int>(type: "int", nullable: false),
                    RefJobOrderId = table.Column<int>(type: "int", nullable: true),
                    RefIssueSubId = table.Column<int>(type: "int", nullable: true),
                    ReturnItemId = table.Column<int>(type: "int", nullable: true),
                    ReturnQty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IssueItemId = table.Column<int>(type: "int", nullable: true),
                    IssueQty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductionReturnAssyReturnId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionReturnAssyTrack", x => x.TrackId);
                    table.ForeignKey(
                        name: "FK_ProductionReturnAssyTrack_Item_IssueItemId",
                        column: x => x.IssueItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReturnAssyTrack_Item_ReturnItemId",
                        column: x => x.ReturnItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReturnAssyTrack_JobOrder_RefJobOrderId",
                        column: x => x.RefJobOrderId,
                        principalTable: "JobOrder",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReturnAssyTrack_ProductionIssueAssySub_RefIssueSubId",
                        column: x => x.RefIssueSubId,
                        principalTable: "ProductionIssueAssySub",
                        principalColumn: "IssueSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReturnAssyTrack_ProductionReturnAssySub_RefReturnSubId",
                        column: x => x.RefReturnSubId,
                        principalTable: "ProductionReturnAssySub",
                        principalColumn: "ReturnSubId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionReturnAssyTrack_ProductionReturnAssy_ProductionReturnAssyReturnId",
                        column: x => x.ProductionReturnAssyReturnId,
                        principalTable: "ProductionReturnAssy",
                        principalColumn: "ReturnId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionSCNAssySub",
                columns: table => new
                {
                    SCNSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SCNId = table.Column<int>(type: "int", nullable: false),
                    RefReturnSubId = table.Column<int>(type: "int", nullable: true),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    AccQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RejQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RejReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RewQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RewReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    InsNO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NCRNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionSCNAssySub", x => x.SCNSubId);
                    table.ForeignKey(
                        name: "FK_ProductionSCNAssySub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionSCNAssySub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionSCNAssySub_ProductionReturnAssySub_RefReturnSubId",
                        column: x => x.RefReturnSubId,
                        principalTable: "ProductionReturnAssySub",
                        principalColumn: "ReturnSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionSCNAssySub_ProductionSCNAssy_SCNId",
                        column: x => x.SCNId,
                        principalTable: "ProductionSCNAssy",
                        principalColumn: "SCNId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionReturnCompTrack",
                columns: table => new
                {
                    TrackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RefReturnSubId = table.Column<int>(type: "int", nullable: false),
                    RefRcSubId = table.Column<int>(type: "int", nullable: true),
                    RefIssueSubId = table.Column<int>(type: "int", nullable: true),
                    ItemIdIn = table.Column<int>(type: "int", nullable: true),
                    QtyIn = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ItemIdOut = table.Column<int>(type: "int", nullable: true),
                    QtyOut = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductionReturnCompReturnId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionReturnCompTrack", x => x.TrackId);
                    table.ForeignKey(
                        name: "FK_ProductionReturnCompTrack_Item_ItemIdIn",
                        column: x => x.ItemIdIn,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReturnCompTrack_Item_ItemIdOut",
                        column: x => x.ItemIdOut,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReturnCompTrack_ProductionIssueCompSub_RefIssueSubId",
                        column: x => x.RefIssueSubId,
                        principalTable: "ProductionIssueCompSub",
                        principalColumn: "IssueSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReturnCompTrack_ProductionReturnCompSub_RefReturnSubId",
                        column: x => x.RefReturnSubId,
                        principalTable: "ProductionReturnCompSub",
                        principalColumn: "ReturnSubId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionReturnCompTrack_ProductionReturnComp_ProductionReturnCompReturnId",
                        column: x => x.ProductionReturnCompReturnId,
                        principalTable: "ProductionReturnComp",
                        principalColumn: "ReturnId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReturnCompTrack_RouteCardSub_RefRcSubId",
                        column: x => x.RefRcSubId,
                        principalTable: "RouteCardSub",
                        principalColumn: "RCSubId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionSCNCompSub",
                columns: table => new
                {
                    SCNSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SCNId = table.Column<int>(type: "int", nullable: false),
                    RefReturnSubId = table.Column<int>(type: "int", nullable: true),
                    RcSubId = table.Column<int>(type: "int", nullable: true),
                    PoSubId = table.Column<int>(type: "int", nullable: true),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    AccQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RejQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RejReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RewQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RewReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    InsNO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NCRNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionSCNCompSub", x => x.SCNSubId);
                    table.ForeignKey(
                        name: "FK_ProductionSCNCompSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionSCNCompSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionSCNCompSub_MfgPoSub_PoSubId",
                        column: x => x.PoSubId,
                        principalTable: "MfgPoSub",
                        principalColumn: "PoSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionSCNCompSub_ProductionReturnCompSub_RefReturnSubId",
                        column: x => x.RefReturnSubId,
                        principalTable: "ProductionReturnCompSub",
                        principalColumn: "ReturnSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionSCNCompSub_ProductionSCNComp_SCNId",
                        column: x => x.SCNId,
                        principalTable: "ProductionSCNComp",
                        principalColumn: "SCNId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionSCNCompSub_RouteCardSub_RcSubId",
                        column: x => x.RcSubId,
                        principalTable: "RouteCardSub",
                        principalColumn: "RCSubId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseSCNSub",
                columns: table => new
                {
                    SCNSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SCNId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UnitConvert = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    AcceptQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BalQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RejectQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RejectBalQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReworkQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RefGRNSubId = table.Column<int>(type: "int", nullable: true),
                    RefPoSubId = table.Column<int>(type: "int", nullable: true),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    ItemCancel = table.Column<bool>(type: "bit", nullable: false),
                    ItemCancelReason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InspectionNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NCRNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    QtyConvert = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    HeatNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    BatchNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseSCNSub", x => x.SCNSubId);
                    table.ForeignKey(
                        name: "FK_PurchaseSCNSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseSCNSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseSCNSub_PurchPoSub_RefPoSubId",
                        column: x => x.RefPoSubId,
                        principalTable: "PurchPoSub",
                        principalColumn: "PoSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseSCNSub_PurchaseGRNSub_RefGRNSubId",
                        column: x => x.RefGRNSubId,
                        principalTable: "PurchaseGRNSub",
                        principalColumn: "GRNSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseSCNSub_PurchaseSCN_SCNId",
                        column: x => x.SCNId,
                        principalTable: "PurchaseSCN",
                        principalColumn: "SCNId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MfgDcSub",
                columns: table => new
                {
                    DcSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DcId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    LineId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RefPoSubId = table.Column<int>(type: "int", nullable: true),
                    RefPoNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefPoDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PoDueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ItemCancel = table.Column<bool>(type: "bit", nullable: false),
                    RejectQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ReworkQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RejRowRemark = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ItemCancelReason = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    TypeOfPo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RcSubIds = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    RefSCNSubId = table.Column<int>(type: "int", nullable: true),
                    InspectId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MfgDcSub", x => x.DcSubId);
                    table.ForeignKey(
                        name: "FK_MfgDcSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MfgDcSub_FinalInspection_InspectId",
                        column: x => x.InspectId,
                        principalTable: "FinalInspection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MfgDcSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MfgDcSub_MfgDc_DcId",
                        column: x => x.DcId,
                        principalTable: "MfgDc",
                        principalColumn: "DcId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MfgDcSub_MfgPoSub_RefPoSubId",
                        column: x => x.RefPoSubId,
                        principalTable: "MfgPoSub",
                        principalColumn: "PoSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MfgDcSub_PurchaseSCNSub_RefSCNSubId",
                        column: x => x.RefSCNSubId,
                        principalTable: "PurchaseSCNSub",
                        principalColumn: "SCNSubId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseInvoiceSub",
                columns: table => new
                {
                    InvSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    RefPoSubId = table.Column<int>(type: "int", nullable: true),
                    RefSCNSubId = table.Column<int>(type: "int", nullable: true),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RejectQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ReworkQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineDiscountPercent = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    LineDiscountAmount = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    LineBasicAmount = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    LineCGSTRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    LineSGSTRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    LineIGSTRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ItemCancel = table.Column<bool>(type: "bit", nullable: false),
                    CancelItemReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseInvoiceSub", x => x.InvSubId);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceSub_PurchPoSub_RefPoSubId",
                        column: x => x.RefPoSubId,
                        principalTable: "PurchPoSub",
                        principalColumn: "PoSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceSub_PurchaseInvoice_InvId",
                        column: x => x.InvId,
                        principalTable: "PurchaseInvoice",
                        principalColumn: "InvId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceSub_PurchaseSCNSub_RefSCNSubId",
                        column: x => x.RefSCNSubId,
                        principalTable: "PurchaseSCNSub",
                        principalColumn: "SCNSubId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MfgInvSub",
                columns: table => new
                {
                    InvSubId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvId = table.Column<int>(type: "int", nullable: false),
                    SlNo = table.Column<int>(type: "int", nullable: false),
                    RefDcSubId = table.Column<int>(type: "int", nullable: true),
                    RefPoSubId = table.Column<int>(type: "int", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    CostId = table.Column<int>(type: "int", nullable: true),
                    Qty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BalQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineGross = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineDiscountPercent = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    LineDiscountAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LineBasicAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineCGSTRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LineSGSTRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LineIGSTRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ItemCancel = table.Column<bool>(type: "bit", nullable: false),
                    ItemCancelReason = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RcSubIds = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MfgInvSub", x => x.InvSubId);
                    table.ForeignKey(
                        name: "FK_MfgInvSub_CostCenter_CostId",
                        column: x => x.CostId,
                        principalTable: "CostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MfgInvSub_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MfgInvSub_MfgDcSub_RefDcSubId",
                        column: x => x.RefDcSubId,
                        principalTable: "MfgDcSub",
                        principalColumn: "DcSubId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MfgInvSub_MfgInv_InvId",
                        column: x => x.InvId,
                        principalTable: "MfgInv",
                        principalColumn: "InvId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MfgInvSub_MfgPoSub_RefPoSubId",
                        column: x => x.RefPoSubId,
                        principalTable: "MfgPoSub",
                        principalColumn: "PoSubId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveApplication",
                columns: table => new
                {
                    LeaveAppID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeaveApplicationNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmployeeID = table.Column<int>(type: "int", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LeaveTypeId = table.Column<int>(type: "int", nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalDays = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BalDaysBeforeLeaveApp = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LeaveStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppliedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Level1 = table.Column<bool>(type: "bit", nullable: false),
                    Level2 = table.Column<bool>(type: "bit", nullable: false),
                    Level3 = table.Column<bool>(type: "bit", nullable: false),
                    Level1_Sign = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Level2_sign = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Level3_sign = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Authorized = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveApplication", x => x.LeaveAppID);
                    table.ForeignKey(
                        name: "FK_LeaveApplication_LeaveType_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveType",
                        principalColumn: "LeaveTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Staff",
                columns: table => new
                {
                    StaffID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffRefId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    StaffName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DOB = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DOJ = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PhoneNo = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    Gmail = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PresentAddr = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    PermanentAddr = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    RelationType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FatherHusbName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    City = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Height = table.Column<float>(type: "real", nullable: false),
                    BloodGroup = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Criteria = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    Reporting = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    LngRead = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    LngWrite = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    LngSpeak = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    TotExp = table.Column<float>(type: "real", nullable: false),
                    RelExp = table.Column<float>(type: "real", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    WorkingHrs = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    GraceHrs = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    OtCount = table.Column<bool>(type: "bit", nullable: false),
                    IsUser = table.Column<bool>(type: "bit", nullable: false),
                    OtherDet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Img = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    UANNo = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    Aadhaar = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    PAN = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ESICodeNo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PFCodeNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    AccNo = table.Column<string>(type: "nvarchar(18)", maxLength: 18, nullable: true),
                    IFSC = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    BranchName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    BankAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    BankState = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LeaveApplicationLeaveAppID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staff", x => x.StaffID);
                    table.ForeignKey(
                        name: "FK_Staff_LeaveApplication_LeaveApplicationLeaveAppID",
                        column: x => x.LeaveApplicationLeaveAppID,
                        principalTable: "LeaveApplication",
                        principalColumn: "LeaveAppID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffEducation",
                columns: table => new
                {
                    Slno = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffID = table.Column<int>(type: "int", nullable: false),
                    Degree = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Institution = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FromYear = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    ToYear = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffEducation", x => x.Slno);
                    table.ForeignKey(
                        name: "FK_StaffEducation_Staff_StaffID",
                        column: x => x.StaffID,
                        principalTable: "Staff",
                        principalColumn: "StaffID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffEmergency",
                columns: table => new
                {
                    SlNo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Relation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Landline = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MobileNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffEmergency", x => x.SlNo);
                    table.ForeignKey(
                        name: "FK_StaffEmergency_Staff_StaffID",
                        column: x => x.StaffID,
                        principalTable: "Staff",
                        principalColumn: "StaffID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffFamilyDetail",
                columns: table => new
                {
                    Slno = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Relation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Landline = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Mobno = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffFamilyDetail", x => x.Slno);
                    table.ForeignKey(
                        name: "FK_StaffFamilyDetail_Staff_StaffID",
                        column: x => x.StaffID,
                        principalTable: "Staff",
                        principalColumn: "StaffID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffId = table.Column<int>(type: "int", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserPassword = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    LevelAuthorization = table.Column<bool>(type: "bit", nullable: false),
                    EmailId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    EmailAppPassword = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    EmailServerName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    EmailPortNo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    StateCodesCsv = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsViewOnly = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserAuthority",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    IsMfgQuote = table.Column<bool>(type: "bit", nullable: false),
                    IsMfgQuoteLevel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPR = table.Column<bool>(type: "bit", nullable: false),
                    IsPRLevel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPO = table.Column<bool>(type: "bit", nullable: false),
                    IsPOLevel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPurchSCN = table.Column<bool>(type: "bit", nullable: false),
                    IsPurchSCNLevel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSubConSCN = table.Column<bool>(type: "bit", nullable: false),
                    IsSubConSCNLevel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsProdAssySCN = table.Column<bool>(type: "bit", nullable: false),
                    IsProdAssySCNLevel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsProdCompSCN = table.Column<bool>(type: "bit", nullable: false),
                    IsProdCompSCNLevel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsLabourSCN = table.Column<bool>(type: "bit", nullable: false),
                    IsLabourSCNLevel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsLeave = table.Column<bool>(type: "bit", nullable: false),
                    IsLeaveLevel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRc = table.Column<bool>(type: "bit", nullable: false),
                    IsRcLevel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAuthority", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAuthority_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ScreenCode = table.Column<int>(type: "int", nullable: false),
                    CanView = table.Column<bool>(type: "bit", nullable: false),
                    CanCreate = table.Column<bool>(type: "bit", nullable: false),
                    CanEdit = table.Column<bool>(type: "bit", nullable: false),
                    CanDelete = table.Column<bool>(type: "bit", nullable: false),
                    IsHide = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRights_Screens_ScreenCode",
                        column: x => x.ScreenCode,
                        principalTable: "Screens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRights_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Category",
                columns: new[] { "CategoryCode", "CategoryDesc", "CategoryName", "CreatedBy", "CreatedDate", "IsSystemDefined", "ModifiedBy", "ModifiedDate" },
                values: new object[,]
                {
                    { 1, "Basic unprocessed materials used in the manufacturing of products.", "RAW MATERIAL", null, null, true, null, null },
                    { 2, "Individual parts or elements that make up a final product or assembly.", "COMPONENT", null, null, true, null, null },
                    { 3, "A group of components put together to form a complete unit or system.", "ASSEMBLY", null, null, true, null, null },
                    { 4, "Tools used to remove material from a workpiece through cutting operations.", "CUTTING TOOLS", null, null, true, null, null },
                    { 5, "Devices used for measuring, inspecting, or testing during production.", "GAUGES & INSTRUMENTS", null, null, true, null, null },
                    { 6, "Custom supports or holders used to securely position workpieces during manufacturing.", "FIXTURES", null, null, true, null, null },
                    { 7, "An assembled unit that is part of a larger final assembly.", "SUB-ASSEMBLY", null, null, true, null, null },
                    { 8, "Partially completed products that require further processing before becoming final goods.", "SEMI-FINISHED GOODS", null, null, true, null, null },
                    { 9, "Standard items purchased from vendors and used directly in production or assembly.", "BOUGHTOUT ITEMS", null, null, true, null, null },
                    { 10, "Fluids used to reduce friction and wear between surfaces in contact.", "OIL AND LUBRICANTS", null, null, true, null, null },
                    { 11, "Completed products ready for sale or delivery.", "FINISHED GOODS", null, null, true, null, null },
                    { 12, "Replaceable cutting edges or tips used in machining operations.", "INSERTS", null, null, true, null, null },
                    { 13, "Physical assets used in production, such as machinery and equipment.", "CAPITAL GOODS", null, null, true, null, null },
                    { 14, "Mechanical components like screws, bolts, and nuts used to join objects together.", "FASTENERS", null, null, true, null, null },
                    { 15, "Standardized items used commonly across different processes or assemblies.", "STD ITEM", null, null, true, null, null },
                    { 16, "General tools and equipment used in manufacturing or maintenance.", "HARDWARE", null, null, true, null, null }
                });

            migrationBuilder.InsertData(
                table: "Currency",
                columns: new[] { "CurrId", "CreatedBy", "CreatedDate", "CurrName", "CurrSub", "IsSystemDefined", "ModifiedBy", "ModifiedDate", "Symbol" },
                values: new object[,]
                {
                    { 1, null, null, "Rupees", "Paise", true, null, null, "₹" },
                    { 2, null, null, "US Dollar", "Cent", true, null, null, "$" },
                    { 3, null, null, "Euro", "Cent", true, null, null, "€" }
                });

            migrationBuilder.InsertData(
                table: "InspectionSettings",
                columns: new[] { "Id", "SamplesPerSheet", "SettingName" },
                values: new object[] { 1, 15, "Samples Per Sheet" });

            migrationBuilder.InsertData(
                table: "ProductionLogSetting",
                columns: new[] { "Id", "Definition", "EfficiencyPerc", "IsActive", "ProdStopType" },
                values: new object[,]
                {
                    { 1, "Time spent on part inspection, testing, quality checks.", 100, true, "Testing" },
                    { 2, "Time lost due to machine failure or mechanical issues.", 100, true, "Machine Breakdown" },
                    { 3, "Delay due to tools not available or not issued.", 100, true, "Tool Shortage" },
                    { 4, "Machine stopped for loading and validating new NC program.", 100, true, "New Program Setup" },
                    { 5, "Waiting for fixture correction, availability, or repair.", 100, true, "Fixture Issue / No Fixture" },
                    { 6, "Operator not available due to workload distribution.", 100, true, "Waiting for Skilled Operator" },
                    { 7, "Operator in meeting or undergoing training.", 100, true, "Meeting / Training" },
                    { 8, "Delay due to material movement or preparation.", 100, true, "Waiting for Load" },
                    { 9, "Routine chip removal to maintain cutting efficiency.", 100, true, "Chip Clearing" },
                    { 10, "Delay for overhead crane or lifting equipment.", 100, true, "Waiting for Crane" },
                    { 11, "Machine stoppage for coolant refilling or circulation checks.", 100, true, "Coolant Refill / Level Low" },
                    { 12, "Shift absence resulting in machine idle time.", 100, true, "Operator Absent" },
                    { 13, "Scheduled maintenance activities.", 100, true, "Planned Maintenance" },
                    { 14, "Grid or internal power shutdown.", 100, true, "Power Failure" },
                    { 15, "Tool presetting or inspection outside machine.", 100, true, "Tool Inspection / Preset" },
                    { 16, "Lack of operator availability resulting in stoppage.", 0, true, "Operator Shortage" },
                    { 17, "Machine stopped due to defects requiring reprocessing.", 0, true, "Rework" },
                    { 18, "Changes done to NC program during production.", 0, true, "Program Editing" },
                    { 19, "Delay in receiving materials or consumables.", 0, true, "Store Delay" },
                    { 20, "Waiting for NC program transfer or file retrieval.", 0, true, "Program Search / Transfer" },
                    { 21, "Grinding delay due to part correction or inaccuracy.", 0, true, "Part Truing / Inaccuracy (Grinding)" },
                    { 22, "Stoppage due to incorrect raw material issued.", 0, true, "Wrong Material Issued" },
                    { 23, "Material found rejected before machining.", 0, true, "Material Quality Issue" },
                    { 24, "Scrap generated requiring stoppage.", 0, true, "In-Process Rejection" },
                    { 25, "Stoppage due to missing or unclear drawing.", 0, true, "Drawing Not Available" },
                    { 26, "Waiting for updated revision drawing or documents.", 0, true, "Revision Change Delay" },
                    { 27, "Stoppage due to missing inspection gauges.", 0, true, "Gauge / Instrument Unavailable" },
                    { 28, "Delays due to slow operation or incorrect methods.", 0, true, "Operator Low Productivity" },
                    { 29, "Operator not working despite machine availability.", 0, true, "Excess Idle Time" },
                    { 30, "Stoppage due to missing production documents.", 0, true, "Document Missing / Route Card Missing" }
                });

            migrationBuilder.InsertData(
                table: "ScreenManagements",
                columns: new[] { "Id", "Description", "Display", "Required", "ScreenName", "Topic" },
                values: new object[,]
                {
                    { 1, "If enabled, the system will check whether the Material Weight is properly assigned for each component when saving an item.", "Material Weight", true, "Items", "Enable this option to validate component weight before saving the item." },
                    { 2, "Enable this option if you want to grant users the ability to manage or lead operations within a specific state or region.", "Assign State Leads", true, "Users", "Allow assigning specific users as state-level leads" },
                    { 3, "When enabled, the system will automatically assign a unique SalesOrder number to each new quote created, following a predefined format or sequence.", "Auto Generate SalesOrder No.", false, "Sales-Order", "Automatically generate SalesOrder numbers for new PO" },
                    { 4, "When enabled, the user must manually define the process sequence, machine, and supplier for the Route Card instead of using predefined item routing.", "Manual RC", false, "Route Card", "Manual Process Allocation" }
                });

            migrationBuilder.InsertData(
                table: "Screens",
                columns: new[] { "Id", "IsPrintRequired", "ScreenCode", "ScreenName" },
                values: new object[,]
                {
                    { 1, false, 1, "User" },
                    { 2, false, 2, "Category" },
                    { 3, false, 3, "UOM" },
                    { 4, false, 4, "State" },
                    { 5, false, 5, "Currency" },
                    { 6, false, 6, "User Rights" },
                    { 7, false, 7, "Store" },
                    { 8, false, 8, "Raw Material" },
                    { 9, false, 9, "Factors" },
                    { 10, false, 10, "Process" },
                    { 11, false, 11, "Machine" },
                    { 12, false, 12, "Grouping" },
                    { 13, false, 13, "Item" },
                    { 14, false, 14, "HSN Master" },
                    { 15, false, 15, "Customer" },
                    { 16, false, 16, "Expense" },
                    { 17, false, 17, "Screen Management" },
                    { 18, false, 18, "Vendor" },
                    { 19, false, 19, "BOM" },
                    { 20, false, 20, "Income" },
                    { 21, false, 21, "Currency Today" },
                    { 22, false, 22, "Bank" },
                    { 23, false, 23, "Company" },
                    { 24, false, 24, "Correspondences" },
                    { 25, false, 25, "Master Upload" },
                    { 26, false, 26, "Holiday List" },
                    { 27, false, 27, "Staff" },
                    { 28, false, 28, "LeaveType" },
                    { 29, false, 29, "Employee Leave Balance" },
                    { 30, false, 30, "Leave Application" },
                    { 31, false, 31, "Project Type Master" },
                    { 32, false, 32, "Cost-Center" },
                    { 33, false, 33, "User Level Authorization" },
                    { 34, false, 34, "Shift Allocation" },
                    { 35, false, 35, "Item Rate-Updation" },
                    { 36, true, 36, "Manufacturing Quotation" },
                    { 37, false, 37, "Terms and Conditions" },
                    { 38, false, 38, "Leads" },
                    { 39, true, 39, "Enquiry Sales" },
                    { 40, false, 40, "Authorization" },
                    { 41, false, 41, "Sales Order" },
                    { 42, false, 42, "Manufacturing DC" },
                    { 43, false, 43, "General Settings" },
                    { 44, false, 44, "Store Map" },
                    { 45, false, 45, "Performa Invoice" },
                    { 46, false, 46, "Stock-Add" },
                    { 47, false, 47, "Material Requisition" },
                    { 48, false, 48, "Material Issue-Note" },
                    { 49, false, 49, "Enquiry Purchase" },
                    { 50, false, 50, "Material Requirement Analysis" },
                    { 51, false, 51, "Print Management" },
                    { 52, false, 52, "Enquiry Feasibility" },
                    { 53, false, 53, "Job Order" },
                    { 54, false, 54, "Production Issue WO Assembly" },
                    { 55, false, 55, "Production Return GRN Assembly" },
                    { 56, false, 56, "Production SCN Assembly" },
                    { 57, false, 57, "Tool-Crib Issue" },
                    { 58, false, 58, "Tool-Crib Return" },
                    { 59, false, 59, "Instant Search" },
                    { 60, false, 60, "Route Card" },
                    { 61, false, 61, "Production Log Setting" },
                    { 62, false, 62, "Daily Production Log" },
                    { 63, false, 63, "Process Flow-RC" },
                    { 64, false, 64, "MaintenanceSchedule" },
                    { 65, false, 65, "MaintenanceProcess" },
                    { 66, false, 66, "BreakdownMaintenance" },
                    { 67, false, 67, "CalibrationHistoryAndMaintenance" },
                    { 68, false, 68, "Production Issue WO Component" },
                    { 69, false, 69, "Inter Store Transfer" },
                    { 70, false, 70, "Production Return Component" },
                    { 71, false, 71, "Production SCN Component" },
                    { 72, false, 72, "Contract Review" },
                    { 73, false, 73, "Contract Review CheckList" },
                    { 74, false, 74, "Stock Position" },
                    { 75, false, 75, "Purchase-Quotation" },
                    { 76, false, 76, "Purchase Order" },
                    { 77, false, 77, "Purchase GRN" },
                    { 78, false, 78, "Purchase SCN" },
                    { 79, false, 79, "Purchase Invoice" },
                    { 80, false, 80, "Manufacturing Invoice" },
                    { 81, false, 81, "Sub-Contract DC-Out" },
                    { 82, false, 82, "Sub-Contrect GRN" },
                    { 83, false, 83, "Sub-Contract SCN" },
                    { 84, false, 84, "Sub-Contract Invoice" },
                    { 85, false, 85, "Export Invoice" },
                    { 86, false, 86, "MasterInspection" },
                    { 87, false, 87, "FinalInspection" },
                    { 88, false, 88, "IncomingInspection" },
                    { 89, false, 89, "InspectionSettings" },
                    { 90, false, 90, "DefectInfo" },
                    { 91, false, 91, "Assembly Requirement Analysis" },
                    { 92, false, 92, "Labour GRN" },
                    { 93, false, 93, "Labour SCN" },
                    { 94, false, 94, "Labour Delivery Challan" },
                    { 95, false, 95, "Labour Invoice" }
                });

            migrationBuilder.InsertData(
                table: "ShiftAllocation",
                columns: new[] { "ShiftId", "BreakEndTime", "BreakStartTime", "CreatedBy", "CreatedDate", "EarlyLeaveAllowed", "LateMarkAllowed", "ModifiedBy", "ModifiedDate", "OverTimeAllowed", "ShiftCode", "ShiftEndTime", "ShiftName", "ShiftStartTime", "TotalWorkingHours" },
                values: new object[,]
                {
                    { 1, new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0), "System", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10, 10, null, null, false, "A", new TimeOnly(14, 0, 0), "Morning Shift", new TimeOnly(6, 0, 0), new TimeSpan(0, 8, 0, 0, 0) },
                    { 2, new TimeOnly(18, 30, 0), new TimeOnly(18, 0, 0), "System", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10, 10, null, null, false, "B", new TimeOnly(22, 0, 0), "Evening Shift", new TimeOnly(14, 0, 0), new TimeSpan(0, 8, 0, 0, 0) },
                    { 3, new TimeOnly(2, 30, 0), new TimeOnly(2, 0, 0), "System", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10, 10, null, null, false, "C", new TimeOnly(6, 0, 0), "Night Shift", new TimeOnly(22, 0, 0), new TimeSpan(0, 8, 0, 0, 0) },
                    { 4, new TimeOnly(12, 30, 0), new TimeOnly(12, 0, 0), "System", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10, 10, null, null, false, "D", new TimeOnly(16, 0, 0), "Flex Day", new TimeOnly(8, 0, 0), new TimeSpan(0, 8, 0, 0, 0) },
                    { 5, new TimeOnly(4, 30, 0), new TimeOnly(4, 0, 0), "System", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10, 10, null, null, false, "G", new TimeOnly(8, 0, 0), "Graveyard Shift", new TimeOnly(0, 0, 0), new TimeSpan(0, 8, 0, 0, 0) },
                    { 6, new TimeOnly(13, 30, 0), new TimeOnly(13, 0, 0), "System", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10, 10, null, null, false, "E", new TimeOnly(17, 0, 0), "Executive Shift", new TimeOnly(9, 0, 0), new TimeSpan(0, 8, 0, 0, 0) }
                });

            migrationBuilder.InsertData(
                table: "State",
                columns: new[] { "StateCode", "IsSystemDefined", "StateName" },
                values: new object[,]
                {
                    { 1, true, "JAMMU AND KASHMIR" },
                    { 2, true, "HIMACHAL PRADESH" },
                    { 3, true, "PUNJAB" },
                    { 4, true, "CHANDIGARH" },
                    { 5, true, "UTTARAKHAND" },
                    { 6, true, "HARYANA" },
                    { 7, true, "DELHI" },
                    { 8, true, "RAJASTHAN" },
                    { 9, true, "UTTAR PRADESH" },
                    { 10, true, "BIHAR" },
                    { 11, true, "SIKKIM" },
                    { 12, true, "ARUNACHAL PRADESH" },
                    { 13, true, "NAGALAND" },
                    { 14, true, "MANIPUR" },
                    { 15, true, "MIZORAM" },
                    { 16, true, "TRIPURA" },
                    { 17, true, "MEGHALAYA" },
                    { 18, true, "ASSAM" },
                    { 19, true, "WEST BENGAL" },
                    { 20, true, "JHARKHAND" },
                    { 21, true, "ODISHA" },
                    { 22, true, "CHHATTISGARH" },
                    { 23, true, "MADHYA PRADESH" },
                    { 24, true, "GUJARAT" },
                    { 25, true, "DAMAN AND DIU" },
                    { 26, true, "DADRA AND NAGAR HAVELI" },
                    { 27, true, "MAHARASHTRA" },
                    { 29, true, "KARNATAKA" },
                    { 30, true, "GOA" },
                    { 31, true, "LAKSHADWEEP" },
                    { 32, true, "KERALA" },
                    { 33, true, "TAMIL NADU" },
                    { 34, true, "PUDUCHERRY" },
                    { 35, true, "ANDAMAN AND NICOBAR" },
                    { 36, true, "TELANGANA" },
                    { 37, true, "ANDHRA PRADESH" },
                    { 38, true, "LADAKH" },
                    { 96, true, "OTHER COUNTRIES" },
                    { 97, true, "Other Territory" },
                    { 99, true, "OTHER COUNTRIES" }
                });

            migrationBuilder.InsertData(
                table: "Stores",
                columns: new[] { "StoreId", "CanAdd", "CanIssue", "CreatedBy", "CreatedDate", "IsSystemDefined", "ModifiedBy", "ModifiedDate", "StoreDesc", "StoreName" },
                values: new object[,]
                {
                    { 1, true, true, null, null, true, null, null, "Central store for all incoming and outgoing materials.", "MAIN STORE" },
                    { 2, true, true, null, null, true, null, null, "Stores all materials received directly from vendors.", "PURCHASE STORE" },
                    { 3, true, true, null, null, true, null, null, "Used for storing items related to subcontract operations.", "SUB-CONTRACT STORE" },
                    { 4, true, true, null, null, true, null, null, "Used for managing labour-related tools and materials.", "LABOUR STORE" },
                    { 5, true, true, null, null, true, null, null, "Holds raw materials and components for internal production use.", "PRODUCTION STORE" },
                    { 6, true, true, null, null, true, null, null, "Stores all rejected or defective materials after quality checks.", "REJECTION STORE" },
                    { 7, true, true, null, null, true, null, null, "Stores items that need rework or correction before reuse.", "REWORK STORE" },
                    { 8, true, true, null, null, true, null, null, "Stores finished products ready for dispatch or delivery.", "FINISH GOODS (FG) STORE" },
                    { 9, true, true, null, null, true, null, null, "Handles items tracked and moved via route cards in production.", "ROUTECARD STORE" }
                });

            migrationBuilder.InsertData(
                table: "UOM",
                columns: new[] { "UnitCode", "IsSystemDefined", "UnitDescription" },
                values: new object[,]
                {
                    { "BAG", true, "BAGS" },
                    { "BAL", true, "BALE" },
                    { "BDL", true, "BUNDLES" },
                    { "BKL", true, "BUCKLES" },
                    { "BOU", true, "BILLION OF UNITS" },
                    { "BOX", true, "BOX" },
                    { "BTL", true, "BOTTLES" },
                    { "BUN", true, "BUNCHES" },
                    { "CAN", true, "CANS" },
                    { "CBM", true, "CUBIC METERS" },
                    { "CCM", true, "CUBIC CENTIMETERS" },
                    { "CMS", true, "CENTIMETERS" },
                    { "CTN", true, "CARTONS" },
                    { "DOZ", true, "DOZENS" },
                    { "DRM", true, "DRUMS" },
                    { "GGK", true, "GREAT GROSS" },
                    { "GMS", true, "GRAMMES" },
                    { "GRS", true, "GROSS" },
                    { "GYD", true, "GROSS YARDS" },
                    { "HRS", true, "HOURS" },
                    { "KGS", true, "KILOGRAMS" },
                    { "KLR", true, "KILOLITRE" },
                    { "KME", true, "KILOMETRE" },
                    { "LTR", true, "LITRES" },
                    { "MLS", true, "MILLI LITRES" },
                    { "MLT", true, "MILILITRE" },
                    { "MM", true, "Mili Meter" },
                    { "MTR", true, "METERS" },
                    { "MTS", true, "METRIC TON" },
                    { "NOS", true, "NUMBERS" },
                    { "OTH", true, "OTHERS" },
                    { "PAC", true, "PACKS" },
                    { "PCS", true, "PIECES" },
                    { "PRS", true, "PAIRS" },
                    { "QTL", true, "QUINTAL" },
                    { "ROL", true, "ROLLS" },
                    { "SCM", true, "Square Centimeter" },
                    { "SET", true, "SETS" },
                    { "SQF", true, "SQUARE FEET" },
                    { "SQM", true, "SQUARE METERS" },
                    { "SQY", true, "SQUARE YARDS" },
                    { "TBS", true, "TABLETS" },
                    { "TGM", true, "TEN GROSS" },
                    { "THD", true, "THOUSANDS" },
                    { "TON", true, "TONNES" },
                    { "TUB", true, "TUBES" },
                    { "UGS", true, "US GALLONS" },
                    { "UNT", true, "UNITS" },
                    { "YDS", true, "YARDS" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedBy", "CreatedDate", "EmailAppPassword", "EmailId", "EmailPortNo", "EmailServerName", "IsActive", "IsViewOnly", "LevelAuthorization", "ModifiedBy", "ModifiedDate", "PhoneNumber", "Role", "StaffId", "StateCodesCsv", "UserName", "UserPassword" },
                values: new object[] { 1, null, null, null, "admin@example.com", null, null, true, false, false, null, null, "9999999999", 0, null, null, "Administrator", "AQAAAAIAAYagAAAAEBDHR4whgjIYMVkEU8I4FUjARxtH1DI/eoKgzld07jJ5NSwY+iIDLIiFRt7Q1YxcYQ==" });

            migrationBuilder.InsertData(
                table: "StoreMap",
                columns: new[] { "Id", "FormName", "ScreenName", "SlNo", "StoreId" },
                values: new object[,]
                {
                    { 1, "GRNForm", "GRN - Purchase", 1, 2 },
                    { 2, "SCNQualityForm", "Store Credit Note - Quality", 2, 1 },
                    { 3, "PMIssueForm", "Produced Material - Issue", 3, 1 },
                    { 4, "PMReturnForm", "Produced Material - Return", 4, 5 },
                    { 5, "SCNProductionForm", "Store Credit Note - Production", 5, 1 },
                    { 6, "SalesDCForm", "Sales DC Outwards - Manufacturing", 6, 1 },
                    { 7, "LabourDCInForm", "Labour DC Inwards", 7, 4 },
                    { 8, "LabourDCOutForm", "Labour DC Outwards", 8, 1 },
                    { 9, "SCNLabourForm", "Store Credit Note - Labour", 9, 1 },
                    { 10, "SubContractDCForm", "SubContract DC Outwards", 10, 1 },
                    { 11, "SubConGRNForm", "SubCon GRN", 11, 3 },
                    { 12, "SubConSCNForm", "SubCon Store Credit Note", 12, 1 },
                    { 13, "SCNGeneralForm", "Store Credit Note - General", 13, 1 },
                    { 14, "MaterialIssueForm", "Material Issue Note", 14, 1 },
                    { 15, "InterStoreTransferForm", "Inter Store Transfer", 15, 1 },
                    { 16, "ToolCribIssueForm", "Tool Crib Issue Note", 16, 1 },
                    { 17, "ToolCribReturnForm", "Tool Crib Return Note", 17, 1 },
                    { 18, "PurchaseReqForm", "Purchase Requisition", 18, 1 },
                    { 19, "PMIssueAssyForm", "Produced Material Issue (IWO) Assy", 19, 1 },
                    { 20, "PMReturnAssyForm", "Produced Material Return (GRN) Assy", 20, 5 },
                    { 21, "SCNProductionAssyForm", "Store Credit Note - Production (GRN) Assy", 21, 1 },
                    { 22, "ProdLogIssueForm", "Production Log Issue Store", 22, 1 },
                    { 23, "ProdLogReturnForm", "Production Log Return Store", 23, 1 }
                });

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
                    { 95, true, true, true, true, null, null, false, null, 95, null, null, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalHistory_ActionDate",
                table: "ApprovalHistory",
                column: "ActionDate");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalHistory_RecordId",
                table: "ApprovalHistory",
                column: "RecordId");

            migrationBuilder.CreateIndex(
                name: "IX_AssemblyModify_AssyId",
                table: "AssemblyModify",
                column: "AssyId");

            migrationBuilder.CreateIndex(
                name: "IX_AssemblyModify_ItemId",
                table: "AssemblyModify",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AssemblyModify_ProcessId",
                table: "AssemblyModify",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_AssemblyPoComponentTracker_AssyId",
                table: "AssemblyPoComponentTracker",
                column: "AssyId");

            migrationBuilder.CreateIndex(
                name: "IX_AssemblyPoComponentTracker_ComponentId",
                table: "AssemblyPoComponentTracker",
                column: "ComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssemblyPoComponentTracker_MfgPoPoId",
                table: "AssemblyPoComponentTracker",
                column: "MfgPoPoId");

            migrationBuilder.CreateIndex(
                name: "IX_AssemblyPoComponentTracker_PoSubId",
                table: "AssemblyPoComponentTracker",
                column: "PoSubId");

            migrationBuilder.CreateIndex(
                name: "IX_AssmblyDef_AssmblyID_ItemId",
                table: "AssmblyDef",
                columns: new[] { "AssmblyID", "ItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssmblyDef_CostId",
                table: "AssmblyDef",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_AssmblyDef_ItemId",
                table: "AssmblyDef",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AssmblyDef_ProcessId",
                table: "AssmblyDef",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_BreakdownMaintenance_MachineId",
                table: "BreakdownMaintenance",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationHistory_InstrumentId",
                table: "CalibrationHistory",
                column: "InstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyDetails_CurrencyCode",
                table: "CompanyDetails",
                column: "CurrencyCode");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyDetails_StateCode",
                table: "CompanyDetails",
                column: "StateCode");

            migrationBuilder.CreateIndex(
                name: "IX_CompMaster_CompItemId",
                table: "CompMaster",
                column: "CompItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CompMaster_RMId",
                table: "CompMaster",
                column: "RMId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactPerson_CustId",
                table: "ContactPerson",
                column: "CustId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactPerson_LeadId",
                table: "ContactPerson",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractReview_PoId",
                table: "ContractReview",
                column: "PoId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractReviewSub_Id",
                table: "ContractReviewSub",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ContractReviewSub_ItemId",
                table: "ContractReviewSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CostCenter_CustId",
                table: "CostCenter",
                column: "CustId");

            migrationBuilder.CreateIndex(
                name: "IX_CostCenter_ProjectTypeId",
                table: "CostCenter",
                column: "ProjectTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyToday_CurrId",
                table: "CurrencyToday",
                column: "CurrId");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_CurrId",
                table: "Customer",
                column: "CurrId");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_StateId",
                table: "Customer",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerIndirect_CustId",
                table: "CustomerIndirect",
                column: "CustId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyLeaveRecord_LeaveApplicationID",
                table: "DailyLeaveRecord",
                column: "LeaveApplicationID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaveBalance_EmployeeID",
                table: "EmployeeLeaveBalance",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaveBalance_LeaveTypeId",
                table: "EmployeeLeaveBalance",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryFeasibility_CustId",
                table: "EnquiryFeasibility",
                column: "CustId");

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryFeasibility_RefEnqId",
                table: "EnquiryFeasibility",
                column: "RefEnqId");

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryFeasibilitySub_FesId",
                table: "EnquiryFeasibilitySub",
                column: "FesId");

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryFeasibilitySub_ItemId",
                table: "EnquiryFeasibilitySub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryPurchasesub_CostCenterId",
                table: "EnquiryPurchasesub",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryPurchasesub_EnquiryId",
                table: "EnquiryPurchasesub",
                column: "EnquiryId");

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryPurchasesub_ItemId",
                table: "EnquiryPurchasesub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryPurchasesub_RefMReqSubId",
                table: "EnquiryPurchasesub",
                column: "RefMReqSubId");

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryPurchaseVendorAssign_EnquiryId",
                table: "EnquiryPurchaseVendorAssign",
                column: "EnquiryId");

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryPurchaseVendorAssign_EnquirySubId",
                table: "EnquiryPurchaseVendorAssign",
                column: "EnquirySubId");

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryPurchaseVendorAssign_VendorCode",
                table: "EnquiryPurchaseVendorAssign",
                column: "VendorCode");

            migrationBuilder.CreateIndex(
                name: "IX_EnquirySales_CustId",
                table: "EnquirySales",
                column: "CustId");

            migrationBuilder.CreateIndex(
                name: "IX_EnquirySalesSub_CostCenterId",
                table: "EnquirySalesSub",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_EnquirySalesSub_EnquiryId",
                table: "EnquirySalesSub",
                column: "EnquiryId");

            migrationBuilder.CreateIndex(
                name: "IX_EnquirySalesSub_ItemId",
                table: "EnquirySalesSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpInv_CurrId",
                table: "ExpInv",
                column: "CurrId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpInv_CustId",
                table: "ExpInv",
                column: "CustId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpInv_ShipAddrsId",
                table: "ExpInv",
                column: "ShipAddrsId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpInv_StoreIssId",
                table: "ExpInv",
                column: "StoreIssId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpInvSub_CostId",
                table: "ExpInvSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpInvSub_ExpInvId",
                table: "ExpInvSub",
                column: "ExpInvId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpInvSub_ItemId",
                table: "ExpInvSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpInvSub_RefDcSubId",
                table: "ExpInvSub",
                column: "RefDcSubId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpInvSub_RefPoSubId",
                table: "ExpInvSub",
                column: "RefPoSubId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalInspection_CustId",
                table: "FinalInspection",
                column: "CustId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalInspection_ItemId",
                table: "FinalInspection",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalInspection_RefMfgDcSubID",
                table: "FinalInspection",
                column: "RefMfgDcSubID");

            migrationBuilder.CreateIndex(
                name: "IX_FinalInspectionRef_ItemID",
                table: "FinalInspectionRef",
                column: "ItemID");

            migrationBuilder.CreateIndex(
                name: "IX_GroupingSub_Factors",
                table: "GroupingSub",
                column: "Factors");

            migrationBuilder.CreateIndex(
                name: "IX_GroupingSub_GroupingId",
                table: "GroupingSub",
                column: "GroupingId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupingSub_Processid",
                table: "GroupingSub",
                column: "Processid");

            migrationBuilder.CreateIndex(
                name: "IX_IncomingInspectionRef_ItemID",
                table: "IncomingInspectionRef",
                column: "ItemID");

            migrationBuilder.CreateIndex(
                name: "IX_IncomingInspections_ItemId",
                table: "IncomingInspections",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomingInspections_ProcessId",
                table: "IncomingInspections",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomingInspections_ProductionLogId",
                table: "IncomingInspections",
                column: "ProductionLogId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomingInspections_RefProductionAssyGRNSubId",
                table: "IncomingInspections",
                column: "RefProductionAssyGRNSubId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomingInspections_RefProductionCompGRNSubId",
                table: "IncomingInspections",
                column: "RefProductionCompGRNSubId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomingInspections_RefPurchaseGRNSubId",
                table: "IncomingInspections",
                column: "RefPurchaseGRNSubId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionRef_ItemID",
                table: "InspectionRef",
                column: "ItemID");

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentDetails_ItemId",
                table: "InstrumentDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Item_CategoryCode",
                table: "Item",
                column: "CategoryCode");

            migrationBuilder.CreateIndex(
                name: "IX_Item_CreatedDate",
                table: "Item",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Item_Hide",
                table: "Item",
                column: "Hide");

            migrationBuilder.CreateIndex(
                name: "IX_Item_Hide_CreatedDate",
                table: "Item",
                columns: new[] { "Hide", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Item_ItemCode",
                table: "Item",
                column: "ItemCode");

            migrationBuilder.CreateIndex(
                name: "IX_Item_ItemName",
                table: "Item",
                column: "ItemName");

            migrationBuilder.CreateIndex(
                name: "IX_Item_MeasureUnit",
                table: "Item",
                column: "MeasureUnit");

            migrationBuilder.CreateIndex(
                name: "IX_Item_RmId",
                table: "Item",
                column: "RmId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemProductAssign_CustId",
                table: "ItemProductAssign",
                column: "CustId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemProductAssign_ItemId",
                table: "ItemProductAssign",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSub_CustomerId",
                table: "ItemSub",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSub_ItemId",
                table: "ItemSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSub_VendorId",
                table: "ItemSub",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_JobOrder_AssyId",
                table: "JobOrder",
                column: "AssyId");

            migrationBuilder.CreateIndex(
                name: "IX_JobOrder_CustId",
                table: "JobOrder",
                column: "CustId");

            migrationBuilder.CreateIndex(
                name: "IX_JobOrder_ParentJobId",
                table: "JobOrder",
                column: "ParentJobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobOrder_RefPoId",
                table: "JobOrder",
                column: "RefPoId");

            migrationBuilder.CreateIndex(
                name: "IX_JobOrder_RefPoSubId",
                table: "JobOrder",
                column: "RefPoSubId");

            migrationBuilder.CreateIndex(
                name: "IX_JobOrderSub_CostId",
                table: "JobOrderSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_JobOrderSub_ItemId",
                table: "JobOrderSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_JobOrderSub_JobId",
                table: "JobOrderSub",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_LabInv_CurrId",
                table: "LabInv",
                column: "CurrId");

            migrationBuilder.CreateIndex(
                name: "IX_LabInv_CustId",
                table: "LabInv",
                column: "CustId");

            migrationBuilder.CreateIndex(
                name: "IX_LabInv_ShipAddrsId",
                table: "LabInv",
                column: "ShipAddrsId");

            migrationBuilder.CreateIndex(
                name: "IX_LabInv_TermsId",
                table: "LabInv",
                column: "TermsId");

            migrationBuilder.CreateIndex(
                name: "IX_LabInvSub_CostId",
                table: "LabInvSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_LabInvSub_ItemId",
                table: "LabInvSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_LabInvSub_LabInvId",
                table: "LabInvSub",
                column: "LabInvId");

            migrationBuilder.CreateIndex(
                name: "IX_LabInvSub_LabourDcOutgoingSubDcSubId",
                table: "LabInvSub",
                column: "LabourDcOutgoingSubDcSubId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourDcOutgoing_CustId",
                table: "LabourDcOutgoing",
                column: "CustId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourDcOutgoing_ShippingId",
                table: "LabourDcOutgoing",
                column: "ShippingId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourDcOutgoing_StoreIssId",
                table: "LabourDcOutgoing",
                column: "StoreIssId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourDcOutgoingSub_CostId",
                table: "LabourDcOutgoingSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourDcOutgoingSub_DcId",
                table: "LabourDcOutgoingSub",
                column: "DcId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourDcOutgoingSub_ItemId",
                table: "LabourDcOutgoingSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourDcOutgoingSub_RefGRNSubId",
                table: "LabourDcOutgoingSub",
                column: "RefGRNSubId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourDcOutgoingSub_RefPoSubId",
                table: "LabourDcOutgoingSub",
                column: "RefPoSubId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourDcOutgoingSub_RefSCNSubId",
                table: "LabourDcOutgoingSub",
                column: "RefSCNSubId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourDcReturnCompTrack_ItemIdIn",
                table: "LabourDcReturnCompTrack",
                column: "ItemIdIn");

            migrationBuilder.CreateIndex(
                name: "IX_LabourDcReturnCompTrack_ItemIdOut",
                table: "LabourDcReturnCompTrack",
                column: "ItemIdOut");

            migrationBuilder.CreateIndex(
                name: "IX_LabourDcReturnCompTrack_LabourDcOutgoingDcId",
                table: "LabourDcReturnCompTrack",
                column: "LabourDcOutgoingDcId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourDcReturnCompTrack_RefDcSubId",
                table: "LabourDcReturnCompTrack",
                column: "RefDcSubId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourDcReturnCompTrack_RefGRNSubId",
                table: "LabourDcReturnCompTrack",
                column: "RefGRNSubId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourDcReturnCompTrack_RefPoSubId",
                table: "LabourDcReturnCompTrack",
                column: "RefPoSubId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourDcReturnCompTrack_RefSCNSubId",
                table: "LabourDcReturnCompTrack",
                column: "RefSCNSubId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourGRN_AddStoreId",
                table: "LabourGRN",
                column: "AddStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourGRN_CustId",
                table: "LabourGRN",
                column: "CustId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourGRNSub_CostId",
                table: "LabourGRNSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourGRNSub_GRNId",
                table: "LabourGRNSub",
                column: "GRNId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourGRNSub_ItemId",
                table: "LabourGRNSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourGRNSub_RefPoSubId",
                table: "LabourGRNSub",
                column: "RefPoSubId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourSCN_CustId",
                table: "LabourSCN",
                column: "CustId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourSCN_StoreIssId",
                table: "LabourSCN",
                column: "StoreIssId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourSCN_StoreIssueStoreId",
                table: "LabourSCN",
                column: "StoreIssueStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourSCNSub_CostId",
                table: "LabourSCNSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourSCNSub_ItemId",
                table: "LabourSCNSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourSCNSub_RefGRNSubId",
                table: "LabourSCNSub",
                column: "RefGRNSubId");

            migrationBuilder.CreateIndex(
                name: "IX_LabourSCNSub_SCNId",
                table: "LabourSCNSub",
                column: "SCNId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_CurrId",
                table: "Leads",
                column: "CurrId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_StateId",
                table: "Leads",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveApplication_EmployeeID",
                table: "LeaveApplication",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveApplication_LeaveTypeId",
                table: "LeaveApplication",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceSchedule_MachineId",
                table: "MaintenanceSchedule",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_MasterInspection_ItemId",
                table: "MasterInspection",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MasterInspection_ProcessId",
                table: "MasterInspection",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssNote_AssyId",
                table: "MaterialIssNote",
                column: "AssyId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssNote_CostId",
                table: "MaterialIssNote",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssNote_StoreIssId",
                table: "MaterialIssNote",
                column: "StoreIssId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssNote_SubAssyId",
                table: "MaterialIssNote",
                column: "SubAssyId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssNote_SubAssyId2",
                table: "MaterialIssNote",
                column: "SubAssyId2");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssNote_SubAssyId3",
                table: "MaterialIssNote",
                column: "SubAssyId3");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssNoteSub_CostId",
                table: "MaterialIssNoteSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssNoteSub_ItemId",
                table: "MaterialIssNoteSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssNoteSub_MINId",
                table: "MaterialIssNoteSub",
                column: "MINId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialReqSub_CostId",
                table: "MaterialReqSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialReqSub_CustId",
                table: "MaterialReqSub",
                column: "CustId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialReqSub_ItemId",
                table: "MaterialReqSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialReqSub_MreqId",
                table: "MaterialReqSub",
                column: "MreqId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialReqSub_RefBomId",
                table: "MaterialReqSub",
                column: "RefBomId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialReqSub_RefPoId",
                table: "MaterialReqSub",
                column: "RefPoId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialReqSub_RefPoSubId",
                table: "MaterialReqSub",
                column: "RefPoSubId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialReqSub_VendorCode",
                table: "MaterialReqSub",
                column: "VendorCode");

            migrationBuilder.CreateIndex(
                name: "IX_MfgDc_CustId",
                table: "MfgDc",
                column: "CustId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgDc_IssueStoreId",
                table: "MfgDc",
                column: "IssueStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgDc_ShippingId",
                table: "MfgDc",
                column: "ShippingId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgDc_VendorCode",
                table: "MfgDc",
                column: "VendorCode");

            migrationBuilder.CreateIndex(
                name: "IX_MfgDcSub_CostId",
                table: "MfgDcSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgDcSub_DcId",
                table: "MfgDcSub",
                column: "DcId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgDcSub_InspectId",
                table: "MfgDcSub",
                column: "InspectId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgDcSub_ItemId",
                table: "MfgDcSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgDcSub_RefPoSubId",
                table: "MfgDcSub",
                column: "RefPoSubId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgDcSub_RefSCNSubId",
                table: "MfgDcSub",
                column: "RefSCNSubId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgInv_CurrId",
                table: "MfgInv",
                column: "CurrId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgInv_CustId",
                table: "MfgInv",
                column: "CustId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgInv_ShipAddrsId",
                table: "MfgInv",
                column: "ShipAddrsId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgInv_StoreIssId",
                table: "MfgInv",
                column: "StoreIssId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgInvSub_CostId",
                table: "MfgInvSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgInvSub_InvId",
                table: "MfgInvSub",
                column: "InvId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgInvSub_ItemId",
                table: "MfgInvSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgInvSub_RefDcSubId",
                table: "MfgInvSub",
                column: "RefDcSubId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgInvSub_RefPoSubId",
                table: "MfgInvSub",
                column: "RefPoSubId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgPo_CurrId",
                table: "MfgPo",
                column: "CurrId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgPo_CustId",
                table: "MfgPo",
                column: "CustId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgPo_PoTypeId",
                table: "MfgPo",
                column: "PoTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgPo_ShipAddrsId",
                table: "MfgPo",
                column: "ShipAddrsId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgPoSub_CostId",
                table: "MfgPoSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgPoSub_ItemId",
                table: "MfgPoSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgPoSub_PoId",
                table: "MfgPoSub",
                column: "PoId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgPoSub_RefQuoteSubId",
                table: "MfgPoSub",
                column: "RefQuoteSubId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgQuote_CurrId",
                table: "MfgQuote",
                column: "CurrId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgQuote_CustId",
                table: "MfgQuote",
                column: "CustId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgQuote_QuoteNo_Suffix",
                table: "MfgQuote",
                columns: new[] { "QuoteNo", "Suffix" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MfgQuote_ShipAddrsId",
                table: "MfgQuote",
                column: "ShipAddrsId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgQuote_TermsId",
                table: "MfgQuote",
                column: "TermsId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgQuoteSub_CostId",
                table: "MfgQuoteSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgQuoteSub_ItemId",
                table: "MfgQuoteSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgQuoteSub_QuoteId",
                table: "MfgQuoteSub",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_MfgQuoteSub_RefEnqSubId",
                table: "MfgQuoteSub",
                column: "RefEnqSubId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformaInv_BankId",
                table: "PerformaInv",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformaInv_CurrId",
                table: "PerformaInv",
                column: "CurrId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformaInv_CustId",
                table: "PerformaInv",
                column: "CustId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformaInv_ShipAddrsId",
                table: "PerformaInv",
                column: "ShipAddrsId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformaInvSub_CostId",
                table: "PerformaInvSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformaInvSub_InvId",
                table: "PerformaInvSub",
                column: "InvId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformaInvSub_ItemId",
                table: "PerformaInvSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformaInvSub_RefPoSubId",
                table: "PerformaInvSub",
                column: "RefPoSubId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessFlowChart_CompItemId",
                table: "ProcessFlowChart",
                column: "CompItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessFlowChart_Id",
                table: "ProcessFlowChart",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessFlowChart_MachId",
                table: "ProcessFlowChart",
                column: "MachId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessFlowChart_ProcId",
                table: "ProcessFlowChart",
                column: "ProcId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessFlowChart_RMId",
                table: "ProcessFlowChart",
                column: "RMId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessFlowChart_ScrapEndBit",
                table: "ProcessFlowChart",
                column: "ScrapEndBit");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessFlowChart_VendorCode",
                table: "ProcessFlowChart",
                column: "VendorCode");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionIssueAssy_StoreIssId",
                table: "ProductionIssueAssy",
                column: "StoreIssId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionIssueAssySub_AssyId",
                table: "ProductionIssueAssySub",
                column: "AssyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionIssueAssySub_CostId",
                table: "ProductionIssueAssySub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionIssueAssySub_IssueId",
                table: "ProductionIssueAssySub",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionIssueAssySub_ItemId",
                table: "ProductionIssueAssySub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionIssueAssySub_RefJobId",
                table: "ProductionIssueAssySub",
                column: "RefJobId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionIssueAssySub_RefJobOrderSubId",
                table: "ProductionIssueAssySub",
                column: "RefJobOrderSubId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionIssueComp_CustId",
                table: "ProductionIssueComp",
                column: "CustId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionIssueComp_StoreIssId",
                table: "ProductionIssueComp",
                column: "StoreIssId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionIssueCompSub_CostId",
                table: "ProductionIssueCompSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionIssueCompSub_IssueId",
                table: "ProductionIssueCompSub",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionIssueCompSub_ItemId",
                table: "ProductionIssueCompSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionIssueCompSub_MachineId",
                table: "ProductionIssueCompSub",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionIssueCompSub_ProcessId",
                table: "ProductionIssueCompSub",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionIssueCompSub_RcSubId",
                table: "ProductionIssueCompSub",
                column: "RcSubId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionIssueCompSub_RefPoSubId",
                table: "ProductionIssueCompSub",
                column: "RefPoSubId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLog_AddStoreId",
                table: "ProductionLog",
                column: "AddStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLog_CostId",
                table: "ProductionLog",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLog_CustId",
                table: "ProductionLog",
                column: "CustId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLog_InspectId",
                table: "ProductionLog",
                column: "InspectId",
                unique: true,
                filter: "[InspectId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLog_IssueStoreId",
                table: "ProductionLog",
                column: "IssueStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLog_ItemIdIn",
                table: "ProductionLog",
                column: "ItemIdIn");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLog_ItemIdOut",
                table: "ProductionLog",
                column: "ItemIdOut");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLog_MachineId",
                table: "ProductionLog",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLog_ProcessId",
                table: "ProductionLog",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLog_RCId",
                table: "ProductionLog",
                column: "RCId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLog_RCProcessId",
                table: "ProductionLog",
                column: "RCProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLog_RewProcessId",
                table: "ProductionLog",
                column: "RewProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLog_ShiftId",
                table: "ProductionLog",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLogSub_LogId",
                table: "ProductionLogSub",
                column: "LogId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionLogSub_ProductionStopageSettingsId",
                table: "ProductionLogSub",
                column: "ProductionStopageSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnAssy_AddStoreId",
                table: "ProductionReturnAssy",
                column: "AddStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnAssySub_CostId",
                table: "ProductionReturnAssySub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnAssySub_InspectId",
                table: "ProductionReturnAssySub",
                column: "InspectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnAssySub_ItemId",
                table: "ProductionReturnAssySub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnAssySub_RefIssueSubId",
                table: "ProductionReturnAssySub",
                column: "RefIssueSubId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnAssySub_RefJobOrderId",
                table: "ProductionReturnAssySub",
                column: "RefJobOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnAssySub_ReturnId",
                table: "ProductionReturnAssySub",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnAssyTrack_IssueItemId",
                table: "ProductionReturnAssyTrack",
                column: "IssueItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnAssyTrack_ProductionReturnAssyReturnId",
                table: "ProductionReturnAssyTrack",
                column: "ProductionReturnAssyReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnAssyTrack_RefIssueSubId",
                table: "ProductionReturnAssyTrack",
                column: "RefIssueSubId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnAssyTrack_RefJobOrderId",
                table: "ProductionReturnAssyTrack",
                column: "RefJobOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnAssyTrack_RefReturnSubId",
                table: "ProductionReturnAssyTrack",
                column: "RefReturnSubId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnAssyTrack_ReturnItemId",
                table: "ProductionReturnAssyTrack",
                column: "ReturnItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnComp_AddStoreId",
                table: "ProductionReturnComp",
                column: "AddStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnComp_CustId",
                table: "ProductionReturnComp",
                column: "CustId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnCompSub_CostId",
                table: "ProductionReturnCompSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnCompSub_InspectId",
                table: "ProductionReturnCompSub",
                column: "InspectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnCompSub_ItemId",
                table: "ProductionReturnCompSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnCompSub_MachineId",
                table: "ProductionReturnCompSub",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnCompSub_ProcessId",
                table: "ProductionReturnCompSub",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnCompSub_RefIssueSubId",
                table: "ProductionReturnCompSub",
                column: "RefIssueSubId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnCompSub_RefPoSubId",
                table: "ProductionReturnCompSub",
                column: "RefPoSubId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnCompSub_RefRcSubId",
                table: "ProductionReturnCompSub",
                column: "RefRcSubId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnCompSub_ReturnId",
                table: "ProductionReturnCompSub",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnCompTrack_ItemIdIn",
                table: "ProductionReturnCompTrack",
                column: "ItemIdIn");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnCompTrack_ItemIdOut",
                table: "ProductionReturnCompTrack",
                column: "ItemIdOut");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnCompTrack_ProductionReturnCompReturnId",
                table: "ProductionReturnCompTrack",
                column: "ProductionReturnCompReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnCompTrack_RefIssueSubId",
                table: "ProductionReturnCompTrack",
                column: "RefIssueSubId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnCompTrack_RefRcSubId",
                table: "ProductionReturnCompTrack",
                column: "RefRcSubId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReturnCompTrack_RefReturnSubId",
                table: "ProductionReturnCompTrack",
                column: "RefReturnSubId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSCNAssy_AddStoreId",
                table: "ProductionSCNAssy",
                column: "AddStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSCNAssy_IssueStoreId",
                table: "ProductionSCNAssy",
                column: "IssueStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSCNAssySub_CostId",
                table: "ProductionSCNAssySub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSCNAssySub_ItemId",
                table: "ProductionSCNAssySub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSCNAssySub_RefReturnSubId",
                table: "ProductionSCNAssySub",
                column: "RefReturnSubId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSCNAssySub_SCNId",
                table: "ProductionSCNAssySub",
                column: "SCNId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSCNComp_AddStoreId",
                table: "ProductionSCNComp",
                column: "AddStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSCNComp_IssueStoreId",
                table: "ProductionSCNComp",
                column: "IssueStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSCNCompSub_CostId",
                table: "ProductionSCNCompSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSCNCompSub_ItemId",
                table: "ProductionSCNCompSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSCNCompSub_PoSubId",
                table: "ProductionSCNCompSub",
                column: "PoSubId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSCNCompSub_RcSubId",
                table: "ProductionSCNCompSub",
                column: "RcSubId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSCNCompSub_RefReturnSubId",
                table: "ProductionSCNCompSub",
                column: "RefReturnSubId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSCNCompSub_SCNId",
                table: "ProductionSCNCompSub",
                column: "SCNId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseGRN_AddStoreId",
                table: "PurchaseGRN",
                column: "AddStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseGRN_VendorCode",
                table: "PurchaseGRN",
                column: "VendorCode");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseGRNSub_CostId",
                table: "PurchaseGRNSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseGRNSub_GRNId",
                table: "PurchaseGRNSub",
                column: "GRNId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseGRNSub_InspectId",
                table: "PurchaseGRNSub",
                column: "InspectId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseGRNSub_ItemId",
                table: "PurchaseGRNSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseGRNSub_RefPoSubId",
                table: "PurchaseGRNSub",
                column: "RefPoSubId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoice_CurrId",
                table: "PurchaseInvoice",
                column: "CurrId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoice_ShipAddrsId",
                table: "PurchaseInvoice",
                column: "ShipAddrsId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoice_VendorCode",
                table: "PurchaseInvoice",
                column: "VendorCode");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceSub_CostId",
                table: "PurchaseInvoiceSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceSub_InvId",
                table: "PurchaseInvoiceSub",
                column: "InvId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceSub_ItemId",
                table: "PurchaseInvoiceSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceSub_RefPoSubId",
                table: "PurchaseInvoiceSub",
                column: "RefPoSubId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceSub_RefSCNSubId",
                table: "PurchaseInvoiceSub",
                column: "RefSCNSubId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseQuote_CurrId",
                table: "PurchaseQuote",
                column: "CurrId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseQuote_ShipAddrsId",
                table: "PurchaseQuote",
                column: "ShipAddrsId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseQuote_TermsId",
                table: "PurchaseQuote",
                column: "TermsId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseQuote_VendorCode",
                table: "PurchaseQuote",
                column: "VendorCode");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseQuoteSub_CostId",
                table: "PurchaseQuoteSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseQuoteSub_ItemId",
                table: "PurchaseQuoteSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseQuoteSub_QuoteId",
                table: "PurchaseQuoteSub",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseQuoteSub_RefEnqVendorAssignId",
                table: "PurchaseQuoteSub",
                column: "RefEnqVendorAssignId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseSCN_AddStoreId",
                table: "PurchaseSCN",
                column: "AddStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseSCN_StoreIssId",
                table: "PurchaseSCN",
                column: "StoreIssId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseSCN_VendorCode",
                table: "PurchaseSCN",
                column: "VendorCode");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseSCNSub_CostId",
                table: "PurchaseSCNSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseSCNSub_ItemId",
                table: "PurchaseSCNSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseSCNSub_RefGRNSubId",
                table: "PurchaseSCNSub",
                column: "RefGRNSubId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseSCNSub_RefPoSubId",
                table: "PurchaseSCNSub",
                column: "RefPoSubId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseSCNSub_SCNId",
                table: "PurchaseSCNSub",
                column: "SCNId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchPo_CurrId",
                table: "PurchPo",
                column: "CurrId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchPo_ShipAddrsId",
                table: "PurchPo",
                column: "ShipAddrsId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchPo_TermsId",
                table: "PurchPo",
                column: "TermsId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchPo_VendorCode",
                table: "PurchPo",
                column: "VendorCode");

            migrationBuilder.CreateIndex(
                name: "IX_PurchPoSub_CostId",
                table: "PurchPoSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchPoSub_ItemId",
                table: "PurchPoSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchPoSub_PoId",
                table: "PurchPoSub",
                column: "PoId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchPoSub_ProcessId",
                table: "PurchPoSub",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchPoSub_RefMReqSubId",
                table: "PurchPoSub",
                column: "RefMReqSubId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchPoSub_RefQuoteSubId",
                table: "PurchPoSub",
                column: "RefQuoteSubId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteCard_AddStoreId",
                table: "RouteCard",
                column: "AddStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteCard_AssyId",
                table: "RouteCard",
                column: "AssyId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteCard_CompItemId",
                table: "RouteCard",
                column: "CompItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteCard_CostId",
                table: "RouteCard",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteCard_CustId",
                table: "RouteCard",
                column: "CustId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteCard_IssueStoreId",
                table: "RouteCard",
                column: "IssueStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteCard_RefPoId",
                table: "RouteCard",
                column: "RefPoId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteCard_RMId",
                table: "RouteCard",
                column: "RMId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteCard_SubAssyId",
                table: "RouteCard",
                column: "SubAssyId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteCard_SubAssyId2",
                table: "RouteCard",
                column: "SubAssyId2");

            migrationBuilder.CreateIndex(
                name: "IX_RouteCard_SubAssyId3",
                table: "RouteCard",
                column: "SubAssyId3");

            migrationBuilder.CreateIndex(
                name: "IX_RouteCardSub_ItemIdIn",
                table: "RouteCardSub",
                column: "ItemIdIn");

            migrationBuilder.CreateIndex(
                name: "IX_RouteCardSub_ItemIdOut",
                table: "RouteCardSub",
                column: "ItemIdOut");

            migrationBuilder.CreateIndex(
                name: "IX_RouteCardSub_MachineId",
                table: "RouteCardSub",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteCardSub_ProcessId",
                table: "RouteCardSub",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteCardSub_RCId",
                table: "RouteCardSub",
                column: "RCId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteCardSub_VendorId",
                table: "RouteCardSub",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_SCNGen_AssyId",
                table: "SCNGen",
                column: "AssyId");

            migrationBuilder.CreateIndex(
                name: "IX_SCNGen_StoreId",
                table: "SCNGen",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_SCNGen_SubAssyId",
                table: "SCNGen",
                column: "SubAssyId");

            migrationBuilder.CreateIndex(
                name: "IX_SCNGen_SubAssyId2",
                table: "SCNGen",
                column: "SubAssyId2");

            migrationBuilder.CreateIndex(
                name: "IX_SCNGen_SubAssyId3",
                table: "SCNGen",
                column: "SubAssyId3");

            migrationBuilder.CreateIndex(
                name: "IX_SCNGenSub_CostId",
                table: "SCNGenSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_SCNGenSub_ItemId",
                table: "SCNGenSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SCNGenSub_RefBomId",
                table: "SCNGenSub",
                column: "RefBomId");

            migrationBuilder.CreateIndex(
                name: "IX_SCNGenSub_SCNGenId",
                table: "SCNGenSub",
                column: "SCNGenId");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_LeaveApplicationLeaveAppID",
                table: "Staff",
                column: "LeaveApplicationLeaveAppID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffEducation_StaffID",
                table: "StaffEducation",
                column: "StaffID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffEmergency_StaffID",
                table: "StaffEmergency",
                column: "StaffID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffFamilyDetail_StaffID",
                table: "StaffFamilyDetail",
                column: "StaffID");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdd_ItemId",
                table: "StockAdd",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdd_ScreenCode",
                table: "StockAdd",
                column: "ScreenCode");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdd_StoreId",
                table: "StockAdd",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_StockIssue_ItemId",
                table: "StockIssue",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StockIssue_ScreenCode",
                table: "StockIssue",
                column: "ScreenCode");

            migrationBuilder.CreateIndex(
                name: "IX_StockIssue_StoreId",
                table: "StockIssue",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_StockIssueTrack_AddId",
                table: "StockIssueTrack",
                column: "AddId");

            migrationBuilder.CreateIndex(
                name: "IX_StockIssueTrack_IssueId",
                table: "StockIssueTrack",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_StockIssueTrack_StockIssueIssueId",
                table: "StockIssueTrack",
                column: "StockIssueIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreInterTrans_CostId",
                table: "StoreInterTrans",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreInterTrans_StoreAddId",
                table: "StoreInterTrans",
                column: "StoreAddId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreInterTrans_StoreIssueId",
                table: "StoreInterTrans",
                column: "StoreIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreInterTransSub_ISTId",
                table: "StoreInterTransSub",
                column: "ISTId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreInterTransSub_ItemId",
                table: "StoreInterTransSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreMap_StoreId",
                table: "StoreMap",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConDcOut_ShipAddrsId",
                table: "SubConDcOut",
                column: "ShipAddrsId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConDcOut_StoreIssId",
                table: "SubConDcOut",
                column: "StoreIssId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConDcOut_VendorCode",
                table: "SubConDcOut",
                column: "VendorCode");

            migrationBuilder.CreateIndex(
                name: "IX_SubConDcOutSub_CostId",
                table: "SubConDcOutSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConDcOutSub_DcId",
                table: "SubConDcOutSub",
                column: "DcId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConDcOutSub_ItemId",
                table: "SubConDcOutSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConDcOutSub_MachineId",
                table: "SubConDcOutSub",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConDcOutSub_ProcessId",
                table: "SubConDcOutSub",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConDcOutSub_RcSubId",
                table: "SubConDcOutSub",
                column: "RcSubId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConDcOutSub_RefPoSubId",
                table: "SubConDcOutSub",
                column: "RefPoSubId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConGRN_AddStoreId",
                table: "SubConGRN",
                column: "AddStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConGRN_VendorCode",
                table: "SubConGRN",
                column: "VendorCode");

            migrationBuilder.CreateIndex(
                name: "IX_SubConGRNSub_CostId",
                table: "SubConGRNSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConGRNSub_GRNId",
                table: "SubConGRNSub",
                column: "GRNId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConGRNSub_ItemId",
                table: "SubConGRNSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConGRNSub_MachineId",
                table: "SubConGRNSub",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConGRNSub_ProcessId",
                table: "SubConGRNSub",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConGRNSub_RefDcSubId",
                table: "SubConGRNSub",
                column: "RefDcSubId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConGRNSub_RefPoSubId",
                table: "SubConGRNSub",
                column: "RefPoSubId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConGRNSub_RefRcSubId",
                table: "SubConGRNSub",
                column: "RefRcSubId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConGRNTrack_ItemIdIn",
                table: "SubConGRNTrack",
                column: "ItemIdIn");

            migrationBuilder.CreateIndex(
                name: "IX_SubConGRNTrack_ItemIdOut",
                table: "SubConGRNTrack",
                column: "ItemIdOut");

            migrationBuilder.CreateIndex(
                name: "IX_SubConGRNTrack_RefDCSubId",
                table: "SubConGRNTrack",
                column: "RefDCSubId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConGRNTrack_RefGRNSubId",
                table: "SubConGRNTrack",
                column: "RefGRNSubId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConGRNTrack_RefPoSubId",
                table: "SubConGRNTrack",
                column: "RefPoSubId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConGRNTrack_RefRcSubId",
                table: "SubConGRNTrack",
                column: "RefRcSubId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConGRNTrack_SubConGRNGRNId",
                table: "SubConGRNTrack",
                column: "SubConGRNGRNId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConInv_CurrId",
                table: "SubConInv",
                column: "CurrId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConInv_ShipAddrsId",
                table: "SubConInv",
                column: "ShipAddrsId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConInv_VendorCode",
                table: "SubConInv",
                column: "VendorCode");

            migrationBuilder.CreateIndex(
                name: "IX_SubConInvSub_CostId",
                table: "SubConInvSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConInvSub_InvId",
                table: "SubConInvSub",
                column: "InvId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConInvSub_ItemId",
                table: "SubConInvSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConInvSub_RefSCNSubId",
                table: "SubConInvSub",
                column: "RefSCNSubId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConSCN_AddStoreId",
                table: "SubConSCN",
                column: "AddStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConSCN_IssueStoreId",
                table: "SubConSCN",
                column: "IssueStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConSCN_VendorCode",
                table: "SubConSCN",
                column: "VendorCode");

            migrationBuilder.CreateIndex(
                name: "IX_SubConSCNSub_CostId",
                table: "SubConSCNSub",
                column: "CostId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConSCNSub_ItemId",
                table: "SubConSCNSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConSCNSub_PoSubId",
                table: "SubConSCNSub",
                column: "PoSubId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConSCNSub_RcSubId",
                table: "SubConSCNSub",
                column: "RcSubId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConSCNSub_RefGRNSubId",
                table: "SubConSCNSub",
                column: "RefGRNSubId");

            migrationBuilder.CreateIndex(
                name: "IX_SubConSCNSub_SCNId",
                table: "SubConSCNSub",
                column: "SCNId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolCribIssue_CategoryCode",
                table: "ToolCribIssue",
                column: "CategoryCode");

            migrationBuilder.CreateIndex(
                name: "IX_ToolCribIssue_StoreId",
                table: "ToolCribIssue",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolCribIssueSub_ItemId",
                table: "ToolCribIssueSub",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolCribIssueSub_TCIssueId",
                table: "ToolCribIssueSub",
                column: "TCIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolCribReturns_StoreId",
                table: "ToolCribReturns",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolCribReturnSubs_ItemId",
                table: "ToolCribReturnSubs",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolCribReturnSubs_RefTCIssueSubId",
                table: "ToolCribReturnSubs",
                column: "RefTCIssueSubId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolCribReturnSubs_TCReturnId",
                table: "ToolCribReturnSubs",
                column: "TCReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAuthority_UserId",
                table: "UserAuthority",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRights_ScreenCode",
                table: "UserRights",
                column: "ScreenCode");

            migrationBuilder.CreateIndex(
                name: "IX_UserRights_UserId",
                table: "UserRights",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_User_CreatedDate",
                table: "Users",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_User_EmailId",
                table: "Users",
                column: "EmailId",
                unique: true,
                filter: "[EmailId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_User_Paging",
                table: "Users",
                columns: new[] { "CreatedDate", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_User_PhoneNumber",
                table: "Users",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_User_Role_IsActive",
                table: "Users",
                columns: new[] { "Role", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_User_StaffId",
                table: "Users",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_User_UserName",
                table: "Users",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendor_CurrId",
                table: "Vendor",
                column: "CurrId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendor_StateCode",
                table: "Vendor",
                column: "StateCode");

            migrationBuilder.CreateIndex(
                name: "IX_VendorContact_VendorCode",
                table: "VendorContact",
                column: "VendorCode");

            migrationBuilder.CreateIndex(
                name: "IX_VendorInDirect_VendorCode",
                table: "VendorInDirect",
                column: "VendorCode");

            migrationBuilder.AddForeignKey(
                name: "FK_DailyLeaveRecord_LeaveApplication_LeaveApplicationID",
                table: "DailyLeaveRecord",
                column: "LeaveApplicationID",
                principalTable: "LeaveApplication",
                principalColumn: "LeaveAppID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeLeaveBalance_Staff_EmployeeID",
                table: "EmployeeLeaveBalance",
                column: "EmployeeID",
                principalTable: "Staff",
                principalColumn: "StaffID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExpInvSub_MfgDcSub_RefDcSubId",
                table: "ExpInvSub",
                column: "RefDcSubId",
                principalTable: "MfgDcSub",
                principalColumn: "DcSubId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinalInspection_MfgDcSub_RefMfgDcSubID",
                table: "FinalInspection",
                column: "RefMfgDcSubID",
                principalTable: "MfgDcSub",
                principalColumn: "DcSubId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IncomingInspections_ProductionLog_ProductionLogId",
                table: "IncomingInspections",
                column: "ProductionLogId",
                principalTable: "ProductionLog",
                principalColumn: "LogId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IncomingInspections_ProductionReturnAssySub_RefProductionAssyGRNSubId",
                table: "IncomingInspections",
                column: "RefProductionAssyGRNSubId",
                principalTable: "ProductionReturnAssySub",
                principalColumn: "ReturnSubId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IncomingInspections_ProductionReturnCompSub_RefProductionCompGRNSubId",
                table: "IncomingInspections",
                column: "RefProductionCompGRNSubId",
                principalTable: "ProductionReturnCompSub",
                principalColumn: "ReturnSubId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IncomingInspections_PurchaseGRNSub_RefPurchaseGRNSubId",
                table: "IncomingInspections",
                column: "RefPurchaseGRNSubId",
                principalTable: "PurchaseGRNSub",
                principalColumn: "GRNSubId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveApplication_Staff_EmployeeID",
                table: "LeaveApplication",
                column: "EmployeeID",
                principalTable: "Staff",
                principalColumn: "StaffID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssmblyDef_Item_AssmblyID",
                table: "AssmblyDef");

            migrationBuilder.DropForeignKey(
                name: "FK_AssmblyDef_Item_ItemId",
                table: "AssmblyDef");

            migrationBuilder.DropForeignKey(
                name: "FK_EnquiryPurchasesub_Item_ItemId",
                table: "EnquiryPurchasesub");

            migrationBuilder.DropForeignKey(
                name: "FK_EnquirySalesSub_Item_ItemId",
                table: "EnquirySalesSub");

            migrationBuilder.DropForeignKey(
                name: "FK_FinalInspection_Item_ItemId",
                table: "FinalInspection");

            migrationBuilder.DropForeignKey(
                name: "FK_IncomingInspections_Item_ItemId",
                table: "IncomingInspections");

            migrationBuilder.DropForeignKey(
                name: "FK_JobOrder_Item_AssyId",
                table: "JobOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_JobOrderSub_Item_ItemId",
                table: "JobOrderSub");

            migrationBuilder.DropForeignKey(
                name: "FK_MaterialReqSub_Item_ItemId",
                table: "MaterialReqSub");

            migrationBuilder.DropForeignKey(
                name: "FK_MfgDcSub_Item_ItemId",
                table: "MfgDcSub");

            migrationBuilder.DropForeignKey(
                name: "FK_MfgPoSub_Item_ItemId",
                table: "MfgPoSub");

            migrationBuilder.DropForeignKey(
                name: "FK_MfgQuoteSub_Item_ItemId",
                table: "MfgQuoteSub");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionIssueAssySub_Item_AssyId",
                table: "ProductionIssueAssySub");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionIssueAssySub_Item_ItemId",
                table: "ProductionIssueAssySub");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionIssueCompSub_Item_ItemId",
                table: "ProductionIssueCompSub");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLog_Item_ItemIdIn",
                table: "ProductionLog");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLog_Item_ItemIdOut",
                table: "ProductionLog");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionReturnAssySub_Item_ItemId",
                table: "ProductionReturnAssySub");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionReturnCompSub_Item_ItemId",
                table: "ProductionReturnCompSub");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseGRNSub_Item_ItemId",
                table: "PurchaseGRNSub");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseQuoteSub_Item_ItemId",
                table: "PurchaseQuoteSub");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseSCNSub_Item_ItemId",
                table: "PurchaseSCNSub");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchPoSub_Item_ItemId",
                table: "PurchPoSub");

            migrationBuilder.DropForeignKey(
                name: "FK_RouteCard_Item_AssyId",
                table: "RouteCard");

            migrationBuilder.DropForeignKey(
                name: "FK_RouteCard_Item_CompItemId",
                table: "RouteCard");

            migrationBuilder.DropForeignKey(
                name: "FK_RouteCard_Item_RMId",
                table: "RouteCard");

            migrationBuilder.DropForeignKey(
                name: "FK_RouteCard_Item_SubAssyId",
                table: "RouteCard");

            migrationBuilder.DropForeignKey(
                name: "FK_RouteCard_Item_SubAssyId2",
                table: "RouteCard");

            migrationBuilder.DropForeignKey(
                name: "FK_RouteCard_Item_SubAssyId3",
                table: "RouteCard");

            migrationBuilder.DropForeignKey(
                name: "FK_RouteCardSub_Item_ItemIdIn",
                table: "RouteCardSub");

            migrationBuilder.DropForeignKey(
                name: "FK_RouteCardSub_Item_ItemIdOut",
                table: "RouteCardSub");

            migrationBuilder.DropForeignKey(
                name: "FK_AssmblyDef_Process_ProcessId",
                table: "AssmblyDef");

            migrationBuilder.DropForeignKey(
                name: "FK_IncomingInspections_Process_ProcessId",
                table: "IncomingInspections");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionIssueCompSub_Process_ProcessId",
                table: "ProductionIssueCompSub");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLog_Process_ProcessId",
                table: "ProductionLog");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLog_Process_RewProcessId",
                table: "ProductionLog");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionReturnCompSub_Process_ProcessId",
                table: "ProductionReturnCompSub");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchPoSub_Process_ProcessId",
                table: "PurchPoSub");

            migrationBuilder.DropForeignKey(
                name: "FK_RouteCardSub_Process_ProcessId",
                table: "RouteCardSub");

            migrationBuilder.DropForeignKey(
                name: "FK_JobOrder_MfgPoSub_RefPoSubId",
                table: "JobOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_MaterialReqSub_MfgPoSub_RefPoSubId",
                table: "MaterialReqSub");

            migrationBuilder.DropForeignKey(
                name: "FK_MfgDcSub_MfgPoSub_RefPoSubId",
                table: "MfgDcSub");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionIssueCompSub_MfgPoSub_RefPoSubId",
                table: "ProductionIssueCompSub");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionReturnCompSub_MfgPoSub_RefPoSubId",
                table: "ProductionReturnCompSub");

            migrationBuilder.DropForeignKey(
                name: "FK_JobOrder_MfgPo_RefPoId",
                table: "JobOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_MaterialReqSub_MfgPo_RefPoId",
                table: "MaterialReqSub");

            migrationBuilder.DropForeignKey(
                name: "FK_RouteCard_MfgPo_RefPoId",
                table: "RouteCard");

            migrationBuilder.DropForeignKey(
                name: "FK_AssmblyDef_CostCenter_CostId",
                table: "AssmblyDef");

            migrationBuilder.DropForeignKey(
                name: "FK_EnquiryPurchasesub_CostCenter_CostCenterId",
                table: "EnquiryPurchasesub");

            migrationBuilder.DropForeignKey(
                name: "FK_JobOrderSub_CostCenter_CostId",
                table: "JobOrderSub");

            migrationBuilder.DropForeignKey(
                name: "FK_MaterialReqSub_CostCenter_CostId",
                table: "MaterialReqSub");

            migrationBuilder.DropForeignKey(
                name: "FK_MfgDcSub_CostCenter_CostId",
                table: "MfgDcSub");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionIssueAssySub_CostCenter_CostId",
                table: "ProductionIssueAssySub");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionIssueCompSub_CostCenter_CostId",
                table: "ProductionIssueCompSub");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLog_CostCenter_CostId",
                table: "ProductionLog");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionReturnAssySub_CostCenter_CostId",
                table: "ProductionReturnAssySub");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionReturnCompSub_CostCenter_CostId",
                table: "ProductionReturnCompSub");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseGRNSub_CostCenter_CostId",
                table: "PurchaseGRNSub");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseQuoteSub_CostCenter_CostId",
                table: "PurchaseQuoteSub");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseSCNSub_CostCenter_CostId",
                table: "PurchaseSCNSub");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchPoSub_CostCenter_CostId",
                table: "PurchPoSub");

            migrationBuilder.DropForeignKey(
                name: "FK_RouteCard_CostCenter_CostId",
                table: "RouteCard");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionIssueCompSub_Machine_MachineId",
                table: "ProductionIssueCompSub");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLog_Machine_MachineId",
                table: "ProductionLog");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionReturnCompSub_Machine_MachineId",
                table: "ProductionReturnCompSub");

            migrationBuilder.DropForeignKey(
                name: "FK_RouteCardSub_Machine_MachineId",
                table: "RouteCardSub");

            migrationBuilder.DropForeignKey(
                name: "FK_Customer_Currency_CurrId",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseQuote_Currency_CurrId",
                table: "PurchaseQuote");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchPo_Currency_CurrId",
                table: "PurchPo");

            migrationBuilder.DropForeignKey(
                name: "FK_Vendor_Currency_CurrId",
                table: "Vendor");

            migrationBuilder.DropForeignKey(
                name: "FK_Customer_State_StateId",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_Vendor_State_StateCode",
                table: "Vendor");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerIndirect_Customer_CustId",
                table: "CustomerIndirect");

            migrationBuilder.DropForeignKey(
                name: "FK_FinalInspection_Customer_CustId",
                table: "FinalInspection");

            migrationBuilder.DropForeignKey(
                name: "FK_JobOrder_Customer_CustId",
                table: "JobOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_MaterialReqSub_Customer_CustId",
                table: "MaterialReqSub");

            migrationBuilder.DropForeignKey(
                name: "FK_MfgDc_Customer_CustId",
                table: "MfgDc");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionIssueComp_Customer_CustId",
                table: "ProductionIssueComp");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLog_Customer_CustId",
                table: "ProductionLog");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionReturnComp_Customer_CustId",
                table: "ProductionReturnComp");

            migrationBuilder.DropForeignKey(
                name: "FK_RouteCard_Customer_CustId",
                table: "RouteCard");

            migrationBuilder.DropForeignKey(
                name: "FK_Staff_LeaveApplication_LeaveApplicationLeaveAppID",
                table: "Staff");

            migrationBuilder.DropForeignKey(
                name: "FK_EnquiryPurchasesub_EnquiryPurchase_EnquiryId",
                table: "EnquiryPurchasesub");

            migrationBuilder.DropForeignKey(
                name: "FK_EnquiryPurchaseVendorAssign_EnquiryPurchase_EnquiryId",
                table: "EnquiryPurchaseVendorAssign");

            migrationBuilder.DropForeignKey(
                name: "FK_EnquiryPurchasesub_MaterialReqSub_RefMReqSubId",
                table: "EnquiryPurchasesub");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchPoSub_MaterialReqSub_RefMReqSubId",
                table: "PurchPoSub");

            migrationBuilder.DropForeignKey(
                name: "FK_EnquiryPurchaseVendorAssign_EnquiryPurchasesub_EnquirySubId",
                table: "EnquiryPurchaseVendorAssign");

            migrationBuilder.DropForeignKey(
                name: "FK_EnquiryPurchaseVendorAssign_Vendor_VendorCode",
                table: "EnquiryPurchaseVendorAssign");

            migrationBuilder.DropForeignKey(
                name: "FK_MfgDc_Vendor_VendorCode",
                table: "MfgDc");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseGRN_Vendor_VendorCode",
                table: "PurchaseGRN");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseQuote_Vendor_VendorCode",
                table: "PurchaseQuote");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseSCN_Vendor_VendorCode",
                table: "PurchaseSCN");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchPo_Vendor_VendorCode",
                table: "PurchPo");

            migrationBuilder.DropForeignKey(
                name: "FK_RouteCardSub_Vendor_VendorId",
                table: "RouteCardSub");

            migrationBuilder.DropForeignKey(
                name: "FK_VendorInDirect_Vendor_VendorCode",
                table: "VendorInDirect");

            migrationBuilder.DropForeignKey(
                name: "FK_MfgDc_CustomerIndirect_ShippingId",
                table: "MfgDc");

            migrationBuilder.DropForeignKey(
                name: "FK_MfgDc_Stores_IssueStoreId",
                table: "MfgDc");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionIssueAssy_Stores_StoreIssId",
                table: "ProductionIssueAssy");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionIssueComp_Stores_StoreIssId",
                table: "ProductionIssueComp");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLog_Stores_AddStoreId",
                table: "ProductionLog");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLog_Stores_IssueStoreId",
                table: "ProductionLog");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionReturnAssy_Stores_AddStoreId",
                table: "ProductionReturnAssy");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionReturnComp_Stores_AddStoreId",
                table: "ProductionReturnComp");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseGRN_Stores_AddStoreId",
                table: "PurchaseGRN");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseSCN_Stores_AddStoreId",
                table: "PurchaseSCN");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseSCN_Stores_StoreIssId",
                table: "PurchaseSCN");

            migrationBuilder.DropForeignKey(
                name: "FK_RouteCard_Stores_AddStoreId",
                table: "RouteCard");

            migrationBuilder.DropForeignKey(
                name: "FK_RouteCard_Stores_IssueStoreId",
                table: "RouteCard");

            migrationBuilder.DropForeignKey(
                name: "FK_FinalInspection_MfgDcSub_RefMfgDcSubID",
                table: "FinalInspection");

            migrationBuilder.DropForeignKey(
                name: "FK_IncomingInspections_ProductionLog_ProductionLogId",
                table: "IncomingInspections");

            migrationBuilder.DropForeignKey(
                name: "FK_IncomingInspections_ProductionReturnAssySub_RefProductionAssyGRNSubId",
                table: "IncomingInspections");

            migrationBuilder.DropForeignKey(
                name: "FK_IncomingInspections_ProductionReturnCompSub_RefProductionCompGRNSubId",
                table: "IncomingInspections");

            migrationBuilder.DropForeignKey(
                name: "FK_IncomingInspections_PurchaseGRNSub_RefPurchaseGRNSubId",
                table: "IncomingInspections");

            migrationBuilder.DropTable(
                name: "ApprovalHistory");

            migrationBuilder.DropTable(
                name: "AssemblyModify");

            migrationBuilder.DropTable(
                name: "AssemblyPoComponentTracker");

            migrationBuilder.DropTable(
                name: "BreakdownMaintenance");

            migrationBuilder.DropTable(
                name: "CalibrationHistory");

            migrationBuilder.DropTable(
                name: "CompanyDetails");

            migrationBuilder.DropTable(
                name: "ContactPerson");

            migrationBuilder.DropTable(
                name: "ContractReviewMaster");

            migrationBuilder.DropTable(
                name: "ContractReviewSub");

            migrationBuilder.DropTable(
                name: "Correspondences");

            migrationBuilder.DropTable(
                name: "CurrencyToday");

            migrationBuilder.DropTable(
                name: "DailyLeaveRecord");

            migrationBuilder.DropTable(
                name: "DefectInfo");

            migrationBuilder.DropTable(
                name: "EmployeeLeaveBalance");

            migrationBuilder.DropTable(
                name: "EnquiryFeasibilitySub");

            migrationBuilder.DropTable(
                name: "Expense");

            migrationBuilder.DropTable(
                name: "ExpInvSub");

            migrationBuilder.DropTable(
                name: "FinalInspectionRef");

            migrationBuilder.DropTable(
                name: "GroupingSub");

            migrationBuilder.DropTable(
                name: "HolidayList");

            migrationBuilder.DropTable(
                name: "HSNMaster");

            migrationBuilder.DropTable(
                name: "Income");

            migrationBuilder.DropTable(
                name: "IncomingInspectionRef");

            migrationBuilder.DropTable(
                name: "InspectionRef");

            migrationBuilder.DropTable(
                name: "InspectionSettings");

            migrationBuilder.DropTable(
                name: "ItemProductAssign");

            migrationBuilder.DropTable(
                name: "ItemSub");

            migrationBuilder.DropTable(
                name: "LabInvSub");

            migrationBuilder.DropTable(
                name: "LabourDcReturnCompTrack");

            migrationBuilder.DropTable(
                name: "MaintenanceProcess");

            migrationBuilder.DropTable(
                name: "MaintenanceSchedule");

            migrationBuilder.DropTable(
                name: "MasterInspection");

            migrationBuilder.DropTable(
                name: "MaterialIssNoteSub");

            migrationBuilder.DropTable(
                name: "MfgInvSub");

            migrationBuilder.DropTable(
                name: "PerformaInvSub");

            migrationBuilder.DropTable(
                name: "PrintSetting");

            migrationBuilder.DropTable(
                name: "ProcessFlowChart");

            migrationBuilder.DropTable(
                name: "ProductionLogSub");

            migrationBuilder.DropTable(
                name: "ProductionReturnAssyTrack");

            migrationBuilder.DropTable(
                name: "ProductionReturnCompTrack");

            migrationBuilder.DropTable(
                name: "ProductionSCNAssySub");

            migrationBuilder.DropTable(
                name: "ProductionSCNCompSub");

            migrationBuilder.DropTable(
                name: "PurchaseInvoiceSub");

            migrationBuilder.DropTable(
                name: "SCNGenSub");

            migrationBuilder.DropTable(
                name: "ScreenManagements");

            migrationBuilder.DropTable(
                name: "StaffEducation");

            migrationBuilder.DropTable(
                name: "StaffEmergency");

            migrationBuilder.DropTable(
                name: "StaffFamilyDetail");

            migrationBuilder.DropTable(
                name: "StockIssueTrack");

            migrationBuilder.DropTable(
                name: "StoreInterTransSub");

            migrationBuilder.DropTable(
                name: "StoreMap");

            migrationBuilder.DropTable(
                name: "SubConGRNTrack");

            migrationBuilder.DropTable(
                name: "SubConInvSub");

            migrationBuilder.DropTable(
                name: "ToolCribReturnSubs");

            migrationBuilder.DropTable(
                name: "UserAuthority");

            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.DropTable(
                name: "UserRights");

            migrationBuilder.DropTable(
                name: "VendorContact");

            migrationBuilder.DropTable(
                name: "InstrumentDetails");

            migrationBuilder.DropTable(
                name: "Leads");

            migrationBuilder.DropTable(
                name: "ContractReview");

            migrationBuilder.DropTable(
                name: "EnquiryFeasibility");

            migrationBuilder.DropTable(
                name: "ExpInv");

            migrationBuilder.DropTable(
                name: "Factors");

            migrationBuilder.DropTable(
                name: "Grouping");

            migrationBuilder.DropTable(
                name: "LabInv");

            migrationBuilder.DropTable(
                name: "LabourDcOutgoingSub");

            migrationBuilder.DropTable(
                name: "MaterialIssNote");

            migrationBuilder.DropTable(
                name: "MfgInv");

            migrationBuilder.DropTable(
                name: "PerformaInv");

            migrationBuilder.DropTable(
                name: "CompMaster");

            migrationBuilder.DropTable(
                name: "ProductionLogSetting");

            migrationBuilder.DropTable(
                name: "ProductionSCNAssy");

            migrationBuilder.DropTable(
                name: "ProductionSCNComp");

            migrationBuilder.DropTable(
                name: "PurchaseInvoice");

            migrationBuilder.DropTable(
                name: "SCNGen");

            migrationBuilder.DropTable(
                name: "StockAdd");

            migrationBuilder.DropTable(
                name: "StockIssue");

            migrationBuilder.DropTable(
                name: "StoreInterTrans");

            migrationBuilder.DropTable(
                name: "SubConInv");

            migrationBuilder.DropTable(
                name: "SubConSCNSub");

            migrationBuilder.DropTable(
                name: "ToolCribIssueSub");

            migrationBuilder.DropTable(
                name: "ToolCribReturns");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "LabourDcOutgoing");

            migrationBuilder.DropTable(
                name: "LabourSCNSub");

            migrationBuilder.DropTable(
                name: "Bank");

            migrationBuilder.DropTable(
                name: "Screens");

            migrationBuilder.DropTable(
                name: "SubConGRNSub");

            migrationBuilder.DropTable(
                name: "SubConSCN");

            migrationBuilder.DropTable(
                name: "ToolCribIssue");

            migrationBuilder.DropTable(
                name: "LabourGRNSub");

            migrationBuilder.DropTable(
                name: "LabourSCN");

            migrationBuilder.DropTable(
                name: "SubConDcOutSub");

            migrationBuilder.DropTable(
                name: "SubConGRN");

            migrationBuilder.DropTable(
                name: "LabourGRN");

            migrationBuilder.DropTable(
                name: "SubConDcOut");

            migrationBuilder.DropTable(
                name: "Item");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "RawMaterial");

            migrationBuilder.DropTable(
                name: "UOM");

            migrationBuilder.DropTable(
                name: "Process");

            migrationBuilder.DropTable(
                name: "MfgPoSub");

            migrationBuilder.DropTable(
                name: "MfgQuoteSub");

            migrationBuilder.DropTable(
                name: "EnquirySalesSub");

            migrationBuilder.DropTable(
                name: "MfgQuote");

            migrationBuilder.DropTable(
                name: "EnquirySales");

            migrationBuilder.DropTable(
                name: "MfgPo");

            migrationBuilder.DropTable(
                name: "PoType");

            migrationBuilder.DropTable(
                name: "CostCenter");

            migrationBuilder.DropTable(
                name: "ProjectTypeMaster");

            migrationBuilder.DropTable(
                name: "Machine");

            migrationBuilder.DropTable(
                name: "Currency");

            migrationBuilder.DropTable(
                name: "State");

            migrationBuilder.DropTable(
                name: "Customer");

            migrationBuilder.DropTable(
                name: "LeaveApplication");

            migrationBuilder.DropTable(
                name: "LeaveType");

            migrationBuilder.DropTable(
                name: "Staff");

            migrationBuilder.DropTable(
                name: "EnquiryPurchase");

            migrationBuilder.DropTable(
                name: "MaterialReqSub");

            migrationBuilder.DropTable(
                name: "AssmblyDef");

            migrationBuilder.DropTable(
                name: "MaterialReq");

            migrationBuilder.DropTable(
                name: "EnquiryPurchasesub");

            migrationBuilder.DropTable(
                name: "Vendor");

            migrationBuilder.DropTable(
                name: "CustomerIndirect");

            migrationBuilder.DropTable(
                name: "Stores");

            migrationBuilder.DropTable(
                name: "MfgDcSub");

            migrationBuilder.DropTable(
                name: "FinalInspection");

            migrationBuilder.DropTable(
                name: "MfgDc");

            migrationBuilder.DropTable(
                name: "PurchaseSCNSub");

            migrationBuilder.DropTable(
                name: "PurchaseSCN");

            migrationBuilder.DropTable(
                name: "ProductionLog");

            migrationBuilder.DropTable(
                name: "ShiftAllocation");

            migrationBuilder.DropTable(
                name: "ProductionReturnAssySub");

            migrationBuilder.DropTable(
                name: "ProductionIssueAssySub");

            migrationBuilder.DropTable(
                name: "ProductionReturnAssy");

            migrationBuilder.DropTable(
                name: "JobOrderSub");

            migrationBuilder.DropTable(
                name: "ProductionIssueAssy");

            migrationBuilder.DropTable(
                name: "JobOrder");

            migrationBuilder.DropTable(
                name: "ProductionReturnCompSub");

            migrationBuilder.DropTable(
                name: "ProductionIssueCompSub");

            migrationBuilder.DropTable(
                name: "ProductionReturnComp");

            migrationBuilder.DropTable(
                name: "ProductionIssueComp");

            migrationBuilder.DropTable(
                name: "RouteCardSub");

            migrationBuilder.DropTable(
                name: "RouteCard");

            migrationBuilder.DropTable(
                name: "PurchaseGRNSub");

            migrationBuilder.DropTable(
                name: "IncomingInspections");

            migrationBuilder.DropTable(
                name: "PurchPoSub");

            migrationBuilder.DropTable(
                name: "PurchaseGRN");

            migrationBuilder.DropTable(
                name: "PurchPo");

            migrationBuilder.DropTable(
                name: "PurchaseQuoteSub");

            migrationBuilder.DropTable(
                name: "EnquiryPurchaseVendorAssign");

            migrationBuilder.DropTable(
                name: "PurchaseQuote");

            migrationBuilder.DropTable(
                name: "TermsAndConditions");

            migrationBuilder.DropTable(
                name: "VendorInDirect");
        }
    }
}
