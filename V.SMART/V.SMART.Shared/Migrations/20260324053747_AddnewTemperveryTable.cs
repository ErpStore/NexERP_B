using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddnewTemperveryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.RenameTable(
                name: "VendorInDirect",
                newName: "VendorInDirect",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "VendorContact",
                newName: "VendorContact",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Vendor",
                newName: "Vendor",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "Users",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "UserRights",
                newName: "UserRights",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "UserPreferences",
                newName: "UserPreferences",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "UserAuthority",
                newName: "UserAuthority",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "UOM",
                newName: "UOM",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ToolCribReturnSubs",
                newName: "ToolCribReturnSubs",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ToolCribReturns",
                newName: "ToolCribReturns",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ToolCribIssueSub",
                newName: "ToolCribIssueSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ToolCribIssue",
                newName: "ToolCribIssue",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "TermsAndConditions",
                newName: "TermsAndConditions",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "SubConSCNSub",
                newName: "SubConSCNSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "SubConSCN",
                newName: "SubConSCN",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "SubConInvSub",
                newName: "SubConInvSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "SubConInv",
                newName: "SubConInv",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "SubConGRNTrack",
                newName: "SubConGRNTrack",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "SubConGRNSub",
                newName: "SubConGRNSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "SubConGRN",
                newName: "SubConGRN",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "SubConDcOutSub",
                newName: "SubConDcOutSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "SubConDcOut",
                newName: "SubConDcOut",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Stores",
                newName: "Stores",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "StoreMap",
                newName: "StoreMap",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "StoreInterTransSub",
                newName: "StoreInterTransSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "StoreInterTrans",
                newName: "StoreInterTrans",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "StockIssueTrack",
                newName: "StockIssueTrack",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "StockIssue",
                newName: "StockIssue",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "StockAdd",
                newName: "StockAdd",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "State",
                newName: "State",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "StaffFamilyDetail",
                newName: "StaffFamilyDetail",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "StaffEmergency",
                newName: "StaffEmergency",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "StaffEducation",
                newName: "StaffEducation",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Staff",
                newName: "Staff",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ShiftAllocation",
                newName: "ShiftAllocation",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Screens",
                newName: "Screens",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ScreenManagements",
                newName: "ScreenManagements",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "SCNGenSub",
                newName: "SCNGenSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "SCNGen",
                newName: "SCNGen",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "RouteCardSub",
                newName: "RouteCardSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "RouteCardReleaseSub",
                newName: "RouteCardReleaseSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "RouteCardRelease",
                newName: "RouteCardRelease",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "RouteCard",
                newName: "RouteCard",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "RawMaterial",
                newName: "RawMaterial",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "PurchPoSub",
                newName: "PurchPoSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "PurchPo",
                newName: "PurchPo",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "PurchaseSCNSub",
                newName: "PurchaseSCNSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "PurchaseSCN",
                newName: "PurchaseSCN",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "PurchaseQuoteSub",
                newName: "PurchaseQuoteSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "PurchaseQuote",
                newName: "PurchaseQuote",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "PurchaseInvoiceSub",
                newName: "PurchaseInvoiceSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "PurchaseInvoice",
                newName: "PurchaseInvoice",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "PurchaseGRNSub",
                newName: "PurchaseGRNSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "PurchaseGRN",
                newName: "PurchaseGRN",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ProjectTypeMaster",
                newName: "ProjectTypeMaster",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ProductionSCNCompSub",
                newName: "ProductionSCNCompSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ProductionSCNComp",
                newName: "ProductionSCNComp",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ProductionSCNAssySub",
                newName: "ProductionSCNAssySub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ProductionSCNAssy",
                newName: "ProductionSCNAssy",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ProductionReturnCompTrack",
                newName: "ProductionReturnCompTrack",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ProductionReturnCompSub",
                newName: "ProductionReturnCompSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ProductionReturnComp",
                newName: "ProductionReturnComp",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ProductionReturnAssyTrack",
                newName: "ProductionReturnAssyTrack",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ProductionReturnAssySub",
                newName: "ProductionReturnAssySub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ProductionReturnAssy",
                newName: "ProductionReturnAssy",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ProductionLogSub",
                newName: "ProductionLogSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ProductionLogSetting",
                newName: "ProductionLogSetting",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ProductionLog",
                newName: "ProductionLog",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ProductionIssueCompSub",
                newName: "ProductionIssueCompSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ProductionIssueComp",
                newName: "ProductionIssueComp",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ProductionIssueAssySub",
                newName: "ProductionIssueAssySub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ProductionIssueAssy",
                newName: "ProductionIssueAssy",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ProcessFlowChart",
                newName: "ProcessFlowChart",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Process",
                newName: "Process",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "PrintSetting",
                newName: "PrintSetting",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "PrintingMap",
                newName: "PrintingMap",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "PoType",
                newName: "PoType",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "PerformaInvSub",
                newName: "PerformaInvSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "PerformaInv",
                newName: "PerformaInv",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MfgQuoteSub",
                newName: "MfgQuoteSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MfgQuote",
                newName: "MfgQuote",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MfgPoSub",
                newName: "MfgPoSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MfgPo",
                newName: "MfgPo",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MfgInvSub",
                newName: "MfgInvSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MfgInv",
                newName: "MfgInv",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MfgDcSub",
                newName: "MfgDcSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MfgDc",
                newName: "MfgDc",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MaterialReqSub",
                newName: "MaterialReqSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MaterialReq",
                newName: "MaterialReq",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MaterialIssNoteSub",
                newName: "MaterialIssNoteSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MaterialIssNote",
                newName: "MaterialIssNote",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MasterInspection",
                newName: "MasterInspection",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MaintenanceSchedule",
                newName: "MaintenanceSchedule",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "MaintenanceProcess",
                newName: "MaintenanceProcess",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Machine",
                newName: "Machine",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "LeaveType",
                newName: "LeaveType",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "LeaveApplication",
                newName: "LeaveApplication",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Leads",
                newName: "Leads",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "LabourSCNSub",
                newName: "LabourSCNSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "LabourSCN",
                newName: "LabourSCN",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "LabourGRNSub",
                newName: "LabourGRNSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "LabourGRN",
                newName: "LabourGRN",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "LabourDcReturnCompTrack",
                newName: "LabourDcReturnCompTrack",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "LabourDcOutgoingSub",
                newName: "LabourDcOutgoingSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "LabourDcOutgoing",
                newName: "LabourDcOutgoing",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "LabInvSub",
                newName: "LabInvSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "LabInv",
                newName: "LabInv",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "JobOrderSub",
                newName: "JobOrderSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "JobOrder",
                newName: "JobOrder",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ItemSub",
                newName: "ItemSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ItemProductAssign",
                newName: "ItemProductAssign",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ItemCustomerAssign",
                newName: "ItemCustomerAssign",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Item",
                newName: "Item",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "InvoiceAutoRunningNumber",
                newName: "InvoiceAutoRunningNumber",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "InstrumentDetails",
                newName: "InstrumentDetails",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "InspectionSettings",
                newName: "InspectionSettings",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "InspectionRef",
                newName: "InspectionRef",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "IncomingInspections",
                newName: "IncomingInspections",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "IncomingInspectionRef",
                newName: "IncomingInspectionRef",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Income",
                newName: "Income",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "HSNMaster",
                newName: "HSNMaster",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "HolidayList",
                newName: "HolidayList",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "GroupingSub",
                newName: "GroupingSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Grouping",
                newName: "Grouping",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "FinalInspectionRef",
                newName: "FinalInspectionRef",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "FinalInspection",
                newName: "FinalInspection",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Factors",
                newName: "Factors",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ExpInvSub",
                newName: "ExpInvSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ExpInv",
                newName: "ExpInv",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Expense",
                newName: "Expense",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "EnquirySalesSub",
                newName: "EnquirySalesSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "EnquirySales",
                newName: "EnquirySales",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "EnquiryPurchaseVendorAssign",
                newName: "EnquiryPurchaseVendorAssign",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "EnquiryPurchasesub",
                newName: "EnquiryPurchasesub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "EnquiryPurchase",
                newName: "EnquiryPurchase",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "EnquiryFeasibilitySub",
                newName: "EnquiryFeasibilitySub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "EnquiryFeasibility",
                newName: "EnquiryFeasibility",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "EmployeeLeaveBalance",
                newName: "EmployeeLeaveBalance",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "DefectInfo",
                newName: "DefectInfo",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "DcRunningNumber",
                newName: "DcRunningNumber",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "DailyLeaveRecord",
                newName: "DailyLeaveRecord",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "CustomerIndirect",
                newName: "CustomerIndirect",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Customer",
                newName: "Customer",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "CurrencyToday",
                newName: "CurrencyToday",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Currency",
                newName: "Currency",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "CreditNoteSub",
                newName: "CreditNoteSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "CreditNote",
                newName: "CreditNote",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "CostCenter",
                newName: "CostCenter",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Correspondences",
                newName: "Correspondences",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ContractReviewSub",
                newName: "ContractReviewSub",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ContractReviewMaster",
                newName: "ContractReviewMaster",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ContractReview",
                newName: "ContractReview",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ContactPerson",
                newName: "ContactPerson",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "CompMaster",
                newName: "CompMaster",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "CompanyDetails",
                newName: "CompanyDetails",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Category",
                newName: "Category",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "CalibrationHistory",
                newName: "CalibrationHistory",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "BreakdownMaintenance",
                newName: "BreakdownMaintenance",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Bank",
                newName: "Bank",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "AssmblyDef",
                newName: "AssmblyDef",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "AssemblyPoComponentTracker",
                newName: "AssemblyPoComponentTracker",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "AssemblyModify",
                newName: "AssemblyModify",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ApprovalHistory",
                newName: "ApprovalHistory",
                newSchema: "dbo");

            migrationBuilder.CreateTable(
                name: "MDBs",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MDBs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MDBs",
                schema: "dbo");

            migrationBuilder.RenameTable(
                name: "VendorInDirect",
                schema: "dbo",
                newName: "VendorInDirect");

            migrationBuilder.RenameTable(
                name: "VendorContact",
                schema: "dbo",
                newName: "VendorContact");

            migrationBuilder.RenameTable(
                name: "Vendor",
                schema: "dbo",
                newName: "Vendor");

            migrationBuilder.RenameTable(
                name: "Users",
                schema: "dbo",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "UserRights",
                schema: "dbo",
                newName: "UserRights");

            migrationBuilder.RenameTable(
                name: "UserPreferences",
                schema: "dbo",
                newName: "UserPreferences");

            migrationBuilder.RenameTable(
                name: "UserAuthority",
                schema: "dbo",
                newName: "UserAuthority");

            migrationBuilder.RenameTable(
                name: "UOM",
                schema: "dbo",
                newName: "UOM");

            migrationBuilder.RenameTable(
                name: "ToolCribReturnSubs",
                schema: "dbo",
                newName: "ToolCribReturnSubs");

            migrationBuilder.RenameTable(
                name: "ToolCribReturns",
                schema: "dbo",
                newName: "ToolCribReturns");

            migrationBuilder.RenameTable(
                name: "ToolCribIssueSub",
                schema: "dbo",
                newName: "ToolCribIssueSub");

            migrationBuilder.RenameTable(
                name: "ToolCribIssue",
                schema: "dbo",
                newName: "ToolCribIssue");

            migrationBuilder.RenameTable(
                name: "TermsAndConditions",
                schema: "dbo",
                newName: "TermsAndConditions");

            migrationBuilder.RenameTable(
                name: "SubConSCNSub",
                schema: "dbo",
                newName: "SubConSCNSub");

            migrationBuilder.RenameTable(
                name: "SubConSCN",
                schema: "dbo",
                newName: "SubConSCN");

            migrationBuilder.RenameTable(
                name: "SubConInvSub",
                schema: "dbo",
                newName: "SubConInvSub");

            migrationBuilder.RenameTable(
                name: "SubConInv",
                schema: "dbo",
                newName: "SubConInv");

            migrationBuilder.RenameTable(
                name: "SubConGRNTrack",
                schema: "dbo",
                newName: "SubConGRNTrack");

            migrationBuilder.RenameTable(
                name: "SubConGRNSub",
                schema: "dbo",
                newName: "SubConGRNSub");

            migrationBuilder.RenameTable(
                name: "SubConGRN",
                schema: "dbo",
                newName: "SubConGRN");

            migrationBuilder.RenameTable(
                name: "SubConDcOutSub",
                schema: "dbo",
                newName: "SubConDcOutSub");

            migrationBuilder.RenameTable(
                name: "SubConDcOut",
                schema: "dbo",
                newName: "SubConDcOut");

            migrationBuilder.RenameTable(
                name: "Stores",
                schema: "dbo",
                newName: "Stores");

            migrationBuilder.RenameTable(
                name: "StoreMap",
                schema: "dbo",
                newName: "StoreMap");

            migrationBuilder.RenameTable(
                name: "StoreInterTransSub",
                schema: "dbo",
                newName: "StoreInterTransSub");

            migrationBuilder.RenameTable(
                name: "StoreInterTrans",
                schema: "dbo",
                newName: "StoreInterTrans");

            migrationBuilder.RenameTable(
                name: "StockIssueTrack",
                schema: "dbo",
                newName: "StockIssueTrack");

            migrationBuilder.RenameTable(
                name: "StockIssue",
                schema: "dbo",
                newName: "StockIssue");

            migrationBuilder.RenameTable(
                name: "StockAdd",
                schema: "dbo",
                newName: "StockAdd");

            migrationBuilder.RenameTable(
                name: "State",
                schema: "dbo",
                newName: "State");

            migrationBuilder.RenameTable(
                name: "StaffFamilyDetail",
                schema: "dbo",
                newName: "StaffFamilyDetail");

            migrationBuilder.RenameTable(
                name: "StaffEmergency",
                schema: "dbo",
                newName: "StaffEmergency");

            migrationBuilder.RenameTable(
                name: "StaffEducation",
                schema: "dbo",
                newName: "StaffEducation");

            migrationBuilder.RenameTable(
                name: "Staff",
                schema: "dbo",
                newName: "Staff");

            migrationBuilder.RenameTable(
                name: "ShiftAllocation",
                schema: "dbo",
                newName: "ShiftAllocation");

            migrationBuilder.RenameTable(
                name: "Screens",
                schema: "dbo",
                newName: "Screens");

            migrationBuilder.RenameTable(
                name: "ScreenManagements",
                schema: "dbo",
                newName: "ScreenManagements");

            migrationBuilder.RenameTable(
                name: "SCNGenSub",
                schema: "dbo",
                newName: "SCNGenSub");

            migrationBuilder.RenameTable(
                name: "SCNGen",
                schema: "dbo",
                newName: "SCNGen");

            migrationBuilder.RenameTable(
                name: "RouteCardSub",
                schema: "dbo",
                newName: "RouteCardSub");

            migrationBuilder.RenameTable(
                name: "RouteCardReleaseSub",
                schema: "dbo",
                newName: "RouteCardReleaseSub");

            migrationBuilder.RenameTable(
                name: "RouteCardRelease",
                schema: "dbo",
                newName: "RouteCardRelease");

            migrationBuilder.RenameTable(
                name: "RouteCard",
                schema: "dbo",
                newName: "RouteCard");

            migrationBuilder.RenameTable(
                name: "RawMaterial",
                schema: "dbo",
                newName: "RawMaterial");

            migrationBuilder.RenameTable(
                name: "PurchPoSub",
                schema: "dbo",
                newName: "PurchPoSub");

            migrationBuilder.RenameTable(
                name: "PurchPo",
                schema: "dbo",
                newName: "PurchPo");

            migrationBuilder.RenameTable(
                name: "PurchaseSCNSub",
                schema: "dbo",
                newName: "PurchaseSCNSub");

            migrationBuilder.RenameTable(
                name: "PurchaseSCN",
                schema: "dbo",
                newName: "PurchaseSCN");

            migrationBuilder.RenameTable(
                name: "PurchaseQuoteSub",
                schema: "dbo",
                newName: "PurchaseQuoteSub");

            migrationBuilder.RenameTable(
                name: "PurchaseQuote",
                schema: "dbo",
                newName: "PurchaseQuote");

            migrationBuilder.RenameTable(
                name: "PurchaseInvoiceSub",
                schema: "dbo",
                newName: "PurchaseInvoiceSub");

            migrationBuilder.RenameTable(
                name: "PurchaseInvoice",
                schema: "dbo",
                newName: "PurchaseInvoice");

            migrationBuilder.RenameTable(
                name: "PurchaseGRNSub",
                schema: "dbo",
                newName: "PurchaseGRNSub");

            migrationBuilder.RenameTable(
                name: "PurchaseGRN",
                schema: "dbo",
                newName: "PurchaseGRN");

            migrationBuilder.RenameTable(
                name: "ProjectTypeMaster",
                schema: "dbo",
                newName: "ProjectTypeMaster");

            migrationBuilder.RenameTable(
                name: "ProductionSCNCompSub",
                schema: "dbo",
                newName: "ProductionSCNCompSub");

            migrationBuilder.RenameTable(
                name: "ProductionSCNComp",
                schema: "dbo",
                newName: "ProductionSCNComp");

            migrationBuilder.RenameTable(
                name: "ProductionSCNAssySub",
                schema: "dbo",
                newName: "ProductionSCNAssySub");

            migrationBuilder.RenameTable(
                name: "ProductionSCNAssy",
                schema: "dbo",
                newName: "ProductionSCNAssy");

            migrationBuilder.RenameTable(
                name: "ProductionReturnCompTrack",
                schema: "dbo",
                newName: "ProductionReturnCompTrack");

            migrationBuilder.RenameTable(
                name: "ProductionReturnCompSub",
                schema: "dbo",
                newName: "ProductionReturnCompSub");

            migrationBuilder.RenameTable(
                name: "ProductionReturnComp",
                schema: "dbo",
                newName: "ProductionReturnComp");

            migrationBuilder.RenameTable(
                name: "ProductionReturnAssyTrack",
                schema: "dbo",
                newName: "ProductionReturnAssyTrack");

            migrationBuilder.RenameTable(
                name: "ProductionReturnAssySub",
                schema: "dbo",
                newName: "ProductionReturnAssySub");

            migrationBuilder.RenameTable(
                name: "ProductionReturnAssy",
                schema: "dbo",
                newName: "ProductionReturnAssy");

            migrationBuilder.RenameTable(
                name: "ProductionLogSub",
                schema: "dbo",
                newName: "ProductionLogSub");

            migrationBuilder.RenameTable(
                name: "ProductionLogSetting",
                schema: "dbo",
                newName: "ProductionLogSetting");

            migrationBuilder.RenameTable(
                name: "ProductionLog",
                schema: "dbo",
                newName: "ProductionLog");

            migrationBuilder.RenameTable(
                name: "ProductionIssueCompSub",
                schema: "dbo",
                newName: "ProductionIssueCompSub");

            migrationBuilder.RenameTable(
                name: "ProductionIssueComp",
                schema: "dbo",
                newName: "ProductionIssueComp");

            migrationBuilder.RenameTable(
                name: "ProductionIssueAssySub",
                schema: "dbo",
                newName: "ProductionIssueAssySub");

            migrationBuilder.RenameTable(
                name: "ProductionIssueAssy",
                schema: "dbo",
                newName: "ProductionIssueAssy");

            migrationBuilder.RenameTable(
                name: "ProcessFlowChart",
                schema: "dbo",
                newName: "ProcessFlowChart");

            migrationBuilder.RenameTable(
                name: "Process",
                schema: "dbo",
                newName: "Process");

            migrationBuilder.RenameTable(
                name: "PrintSetting",
                schema: "dbo",
                newName: "PrintSetting");

            migrationBuilder.RenameTable(
                name: "PrintingMap",
                schema: "dbo",
                newName: "PrintingMap");

            migrationBuilder.RenameTable(
                name: "PoType",
                schema: "dbo",
                newName: "PoType");

            migrationBuilder.RenameTable(
                name: "PerformaInvSub",
                schema: "dbo",
                newName: "PerformaInvSub");

            migrationBuilder.RenameTable(
                name: "PerformaInv",
                schema: "dbo",
                newName: "PerformaInv");

            migrationBuilder.RenameTable(
                name: "MfgQuoteSub",
                schema: "dbo",
                newName: "MfgQuoteSub");

            migrationBuilder.RenameTable(
                name: "MfgQuote",
                schema: "dbo",
                newName: "MfgQuote");

            migrationBuilder.RenameTable(
                name: "MfgPoSub",
                schema: "dbo",
                newName: "MfgPoSub");

            migrationBuilder.RenameTable(
                name: "MfgPo",
                schema: "dbo",
                newName: "MfgPo");

            migrationBuilder.RenameTable(
                name: "MfgInvSub",
                schema: "dbo",
                newName: "MfgInvSub");

            migrationBuilder.RenameTable(
                name: "MfgInv",
                schema: "dbo",
                newName: "MfgInv");

            migrationBuilder.RenameTable(
                name: "MfgDcSub",
                schema: "dbo",
                newName: "MfgDcSub");

            migrationBuilder.RenameTable(
                name: "MfgDc",
                schema: "dbo",
                newName: "MfgDc");

            migrationBuilder.RenameTable(
                name: "MaterialReqSub",
                schema: "dbo",
                newName: "MaterialReqSub");

            migrationBuilder.RenameTable(
                name: "MaterialReq",
                schema: "dbo",
                newName: "MaterialReq");

            migrationBuilder.RenameTable(
                name: "MaterialIssNoteSub",
                schema: "dbo",
                newName: "MaterialIssNoteSub");

            migrationBuilder.RenameTable(
                name: "MaterialIssNote",
                schema: "dbo",
                newName: "MaterialIssNote");

            migrationBuilder.RenameTable(
                name: "MasterInspection",
                schema: "dbo",
                newName: "MasterInspection");

            migrationBuilder.RenameTable(
                name: "MaintenanceSchedule",
                schema: "dbo",
                newName: "MaintenanceSchedule");

            migrationBuilder.RenameTable(
                name: "MaintenanceProcess",
                schema: "dbo",
                newName: "MaintenanceProcess");

            migrationBuilder.RenameTable(
                name: "Machine",
                schema: "dbo",
                newName: "Machine");

            migrationBuilder.RenameTable(
                name: "LeaveType",
                schema: "dbo",
                newName: "LeaveType");

            migrationBuilder.RenameTable(
                name: "LeaveApplication",
                schema: "dbo",
                newName: "LeaveApplication");

            migrationBuilder.RenameTable(
                name: "Leads",
                schema: "dbo",
                newName: "Leads");

            migrationBuilder.RenameTable(
                name: "LabourSCNSub",
                schema: "dbo",
                newName: "LabourSCNSub");

            migrationBuilder.RenameTable(
                name: "LabourSCN",
                schema: "dbo",
                newName: "LabourSCN");

            migrationBuilder.RenameTable(
                name: "LabourGRNSub",
                schema: "dbo",
                newName: "LabourGRNSub");

            migrationBuilder.RenameTable(
                name: "LabourGRN",
                schema: "dbo",
                newName: "LabourGRN");

            migrationBuilder.RenameTable(
                name: "LabourDcReturnCompTrack",
                schema: "dbo",
                newName: "LabourDcReturnCompTrack");

            migrationBuilder.RenameTable(
                name: "LabourDcOutgoingSub",
                schema: "dbo",
                newName: "LabourDcOutgoingSub");

            migrationBuilder.RenameTable(
                name: "LabourDcOutgoing",
                schema: "dbo",
                newName: "LabourDcOutgoing");

            migrationBuilder.RenameTable(
                name: "LabInvSub",
                schema: "dbo",
                newName: "LabInvSub");

            migrationBuilder.RenameTable(
                name: "LabInv",
                schema: "dbo",
                newName: "LabInv");

            migrationBuilder.RenameTable(
                name: "JobOrderSub",
                schema: "dbo",
                newName: "JobOrderSub");

            migrationBuilder.RenameTable(
                name: "JobOrder",
                schema: "dbo",
                newName: "JobOrder");

            migrationBuilder.RenameTable(
                name: "ItemSub",
                schema: "dbo",
                newName: "ItemSub");

            migrationBuilder.RenameTable(
                name: "ItemProductAssign",
                schema: "dbo",
                newName: "ItemProductAssign");

            migrationBuilder.RenameTable(
                name: "ItemCustomerAssign",
                schema: "dbo",
                newName: "ItemCustomerAssign");

            migrationBuilder.RenameTable(
                name: "Item",
                schema: "dbo",
                newName: "Item");

            migrationBuilder.RenameTable(
                name: "InvoiceAutoRunningNumber",
                schema: "dbo",
                newName: "InvoiceAutoRunningNumber");

            migrationBuilder.RenameTable(
                name: "InstrumentDetails",
                schema: "dbo",
                newName: "InstrumentDetails");

            migrationBuilder.RenameTable(
                name: "InspectionSettings",
                schema: "dbo",
                newName: "InspectionSettings");

            migrationBuilder.RenameTable(
                name: "InspectionRef",
                schema: "dbo",
                newName: "InspectionRef");

            migrationBuilder.RenameTable(
                name: "IncomingInspections",
                schema: "dbo",
                newName: "IncomingInspections");

            migrationBuilder.RenameTable(
                name: "IncomingInspectionRef",
                schema: "dbo",
                newName: "IncomingInspectionRef");

            migrationBuilder.RenameTable(
                name: "Income",
                schema: "dbo",
                newName: "Income");

            migrationBuilder.RenameTable(
                name: "HSNMaster",
                schema: "dbo",
                newName: "HSNMaster");

            migrationBuilder.RenameTable(
                name: "HolidayList",
                schema: "dbo",
                newName: "HolidayList");

            migrationBuilder.RenameTable(
                name: "GroupingSub",
                schema: "dbo",
                newName: "GroupingSub");

            migrationBuilder.RenameTable(
                name: "Grouping",
                schema: "dbo",
                newName: "Grouping");

            migrationBuilder.RenameTable(
                name: "FinalInspectionRef",
                schema: "dbo",
                newName: "FinalInspectionRef");

            migrationBuilder.RenameTable(
                name: "FinalInspection",
                schema: "dbo",
                newName: "FinalInspection");

            migrationBuilder.RenameTable(
                name: "Factors",
                schema: "dbo",
                newName: "Factors");

            migrationBuilder.RenameTable(
                name: "ExpInvSub",
                schema: "dbo",
                newName: "ExpInvSub");

            migrationBuilder.RenameTable(
                name: "ExpInv",
                schema: "dbo",
                newName: "ExpInv");

            migrationBuilder.RenameTable(
                name: "Expense",
                schema: "dbo",
                newName: "Expense");

            migrationBuilder.RenameTable(
                name: "EnquirySalesSub",
                schema: "dbo",
                newName: "EnquirySalesSub");

            migrationBuilder.RenameTable(
                name: "EnquirySales",
                schema: "dbo",
                newName: "EnquirySales");

            migrationBuilder.RenameTable(
                name: "EnquiryPurchaseVendorAssign",
                schema: "dbo",
                newName: "EnquiryPurchaseVendorAssign");

            migrationBuilder.RenameTable(
                name: "EnquiryPurchasesub",
                schema: "dbo",
                newName: "EnquiryPurchasesub");

            migrationBuilder.RenameTable(
                name: "EnquiryPurchase",
                schema: "dbo",
                newName: "EnquiryPurchase");

            migrationBuilder.RenameTable(
                name: "EnquiryFeasibilitySub",
                schema: "dbo",
                newName: "EnquiryFeasibilitySub");

            migrationBuilder.RenameTable(
                name: "EnquiryFeasibility",
                schema: "dbo",
                newName: "EnquiryFeasibility");

            migrationBuilder.RenameTable(
                name: "EmployeeLeaveBalance",
                schema: "dbo",
                newName: "EmployeeLeaveBalance");

            migrationBuilder.RenameTable(
                name: "DefectInfo",
                schema: "dbo",
                newName: "DefectInfo");

            migrationBuilder.RenameTable(
                name: "DcRunningNumber",
                schema: "dbo",
                newName: "DcRunningNumber");

            migrationBuilder.RenameTable(
                name: "DailyLeaveRecord",
                schema: "dbo",
                newName: "DailyLeaveRecord");

            migrationBuilder.RenameTable(
                name: "CustomerIndirect",
                schema: "dbo",
                newName: "CustomerIndirect");

            migrationBuilder.RenameTable(
                name: "Customer",
                schema: "dbo",
                newName: "Customer");

            migrationBuilder.RenameTable(
                name: "CurrencyToday",
                schema: "dbo",
                newName: "CurrencyToday");

            migrationBuilder.RenameTable(
                name: "Currency",
                schema: "dbo",
                newName: "Currency");

            migrationBuilder.RenameTable(
                name: "CreditNoteSub",
                schema: "dbo",
                newName: "CreditNoteSub");

            migrationBuilder.RenameTable(
                name: "CreditNote",
                schema: "dbo",
                newName: "CreditNote");

            migrationBuilder.RenameTable(
                name: "CostCenter",
                schema: "dbo",
                newName: "CostCenter");

            migrationBuilder.RenameTable(
                name: "Correspondences",
                schema: "dbo",
                newName: "Correspondences");

            migrationBuilder.RenameTable(
                name: "ContractReviewSub",
                schema: "dbo",
                newName: "ContractReviewSub");

            migrationBuilder.RenameTable(
                name: "ContractReviewMaster",
                schema: "dbo",
                newName: "ContractReviewMaster");

            migrationBuilder.RenameTable(
                name: "ContractReview",
                schema: "dbo",
                newName: "ContractReview");

            migrationBuilder.RenameTable(
                name: "ContactPerson",
                schema: "dbo",
                newName: "ContactPerson");

            migrationBuilder.RenameTable(
                name: "CompMaster",
                schema: "dbo",
                newName: "CompMaster");

            migrationBuilder.RenameTable(
                name: "CompanyDetails",
                schema: "dbo",
                newName: "CompanyDetails");

            migrationBuilder.RenameTable(
                name: "Category",
                schema: "dbo",
                newName: "Category");

            migrationBuilder.RenameTable(
                name: "CalibrationHistory",
                schema: "dbo",
                newName: "CalibrationHistory");

            migrationBuilder.RenameTable(
                name: "BreakdownMaintenance",
                schema: "dbo",
                newName: "BreakdownMaintenance");

            migrationBuilder.RenameTable(
                name: "Bank",
                schema: "dbo",
                newName: "Bank");

            migrationBuilder.RenameTable(
                name: "AssmblyDef",
                schema: "dbo",
                newName: "AssmblyDef");

            migrationBuilder.RenameTable(
                name: "AssemblyPoComponentTracker",
                schema: "dbo",
                newName: "AssemblyPoComponentTracker");

            migrationBuilder.RenameTable(
                name: "AssemblyModify",
                schema: "dbo",
                newName: "AssemblyModify");

            migrationBuilder.RenameTable(
                name: "ApprovalHistory",
                schema: "dbo",
                newName: "ApprovalHistory");
        }
    }
}
