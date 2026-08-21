
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using V.SMART.Shared.Data.AccountsModule;
using V.SMART.Shared.Data.CashFlow.ServiceBills;
using V.SMART.Shared.Data.Configuration;
using V.SMART.Shared.Data.Configuration.MasterConfiguration;
using V.SMART.Shared.Data.DataSeeder;
using V.SMART.Shared.Data.DcAutoRunning;
using V.SMART.Shared.Data.Enum;
using V.SMART.Shared.Data.GeneralSettingsMaster;
using V.SMART.Shared.Data.HumanResource.AppointmentLetter;
using V.SMART.Shared.Data.HumanResource.Attendance;
using V.SMART.Shared.Data.HumanResource.OfferLetter;
using V.SMART.Shared.Data.HumanResource.Salary;
using V.SMART.Shared.Data.HumanResource.StaffLoan;
using V.SMART.Shared.Data.Inspection.FinalInspection;
using V.SMART.Shared.Data.Inspection.IncomingInspection;
using V.SMART.Shared.Data.Inspection.MasterInspection;
using V.SMART.Shared.Data.Inventory_Stock_;
using V.SMART.Shared.Data.Inventory_Stock_.InterStoreTransfer;
using V.SMART.Shared.Data.Inventory_Stock_.MaterialIssueNote;
using V.SMART.Shared.Data.Inventory_Stock_.StockIssueRequest;
using V.SMART.Shared.Data.Inventory_Stock_.StoreTransferNote;
using V.SMART.Shared.Data.Inventory_Stock_.ToolCrib;
using V.SMART.Shared.Data.InvoiceAutoRunning;
using V.SMART.Shared.Data.Maintenance.BreakdownMaintenance;
using V.SMART.Shared.Data.Maintenance.CalibrationHistoryAndMaintenance;
using V.SMART.Shared.Data.Maintenance.Maintenance_Process;
using V.SMART.Shared.Data.Maintenance.MaintenanceSchedule;
using V.SMART.Shared.Data.Master;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.Data.Master.Admin_Module;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.Data.Master.MasterScreeenManagement;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.Master.Rejection_Module;
using V.SMART.Shared.Data.OutSourcing;
using V.SMART.Shared.Data.OutSourcing.Debit_Note;
using V.SMART.Shared.Data.OutSourcing.Purchase_Invoice;
using V.SMART.Shared.Data.OutSourcing.PurchaseEnquiry;
using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using V.SMART.Shared.Data.OutSourcing.PurchasePo;
using V.SMART.Shared.Data.OutSourcing.PurchaseSCN;
using V.SMART.Shared.Data.OutSourcing.SubContractDC;
using V.SMART.Shared.Data.OutSourcing.SubContractGRN;
using V.SMART.Shared.Data.OutSourcing.SubContractInvoice;
using V.SMART.Shared.Data.OutSourcing.SubContractSCN;
using V.SMART.Shared.Data.Planning;
using V.SMART.Shared.Data.Planning.AssyJobOrder;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.Data.Planning.Estimaton;
using V.SMART.Shared.Data.Production.DailyProductionLog;
using V.SMART.Shared.Data.Production.ProductionComponent;
using V.SMART.Shared.Data.Production.ProductionIssueWOAssy;
using V.SMART.Shared.Data.Production.ProductionReturnGrnAssy;
using V.SMART.Shared.Data.Production.ProductionSCNAssembly;
using V.SMART.Shared.Data.PurchaseAndSubcontract.Purchase_Quote;
using V.SMART.Shared.Data.SalesAndLabour.ContractReview;
using V.SMART.Shared.Data.SalesAndLabour.Credit_Note;
using V.SMART.Shared.Data.SalesAndLabour.Export;
using V.SMART.Shared.Data.SalesAndLabour.Fesibility;
using V.SMART.Shared.Data.SalesAndLabour.Labour_SCN;
using V.SMART.Shared.Data.SalesAndLabour.LabourDC;
using V.SMART.Shared.Data.SalesAndLabour.LabourGRN;
using V.SMART.Shared.Data.SalesAndLabour.Leads;
using V.SMART.Shared.Data.SalesAndLabour.PerformaInvoice;
using V.SMART.Shared.Data.SalesAndLabour.SalesDC;
using V.SMART.Shared.Data.SalesAndLabour.SalesEnquiry;
using V.SMART.Shared.Data.SalesAndLabour.SalesInvoice;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.Data.SalesAndLabour_Module;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.Data.Utilities;
using V.SMART.Shared.Data.Utilities.Correspondence;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.InventoryViewModel.StockAddVM;
using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchPoVM;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProuctionCompViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.AccountsReportViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.AccountsVM;
using V.SMART.Shared.ViewModels.ReportViewModel.AnalysisReportViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.AnalysisViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.CreditNoteListViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.DebitNoteListViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.GRNPendingVM;
using V.SMART.Shared.ViewModels.ReportViewModel.JobOrderStatusVM;
using V.SMART.Shared.ViewModels.ReportViewModel.LabourStatusVM;
using V.SMART.Shared.ViewModels.ReportViewModel.NewFolder;
using V.SMART.Shared.ViewModels.ReportViewModel.OutSourcingRptVM;
using V.SMART.Shared.ViewModels.ReportViewModel.PoPendingModel;
using V.SMART.Shared.ViewModels.ReportViewModel.POTrackVM;
using V.SMART.Shared.ViewModels.ReportViewModel.ProdAssStatusVM;
using V.SMART.Shared.ViewModels.ReportViewModel.ProdCompStatusVM;
using V.SMART.Shared.ViewModels.ReportViewModel.PurchaseGRNStatusViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.PurchaseSCNStatusViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.PurchaseStatusVM;
using V.SMART.Shared.ViewModels.ReportViewModel.Ratings;
using V.SMART.Shared.ViewModels.ReportViewModel.RatingsVM;
using V.SMART.Shared.ViewModels.ReportViewModel.SalesStatusVM;
using V.SMART.Shared.ViewModels.ReportViewModel.TaxDetailsReportViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.ToolCribIssueStatusViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.ToolCribReturnStatusViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.LabourTrackViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.PurchaseSalesTrackViewModel;

namespace V.SMART.Shared.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // Screen management
        public DbSet<PrintSetting> PrintSetting { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }
        public DbSet<UserThemePreference> UserThemePreferences { get; set; }
        public DbSet<ScreenManagement> ScreenManagements { get; set; }
        public DbSet<Screens> Screens { get; set; }
        public DbSet<TermsAndConditions> TermsAndConditions { get; set; }
        public DbSet<StoreMap> StoreMap { get; set; }
        public DbSet<ProductionLogSetting> ProductionLogSetting { get; set; }
        public DbSet<InspectionSettings> InspectionSettings { get; set; }

        public DbSet<HRMasterSetting> HRMasterSetting { get; set; }
        public DbSet<BiometricExcelSetting> BiometricExcelSetting { get; set; }

        public DbSet<SalaryHeadPrintSetting> SalaryHeadPrintsSetting { get; set; }


        //Correspondence
        public DbSet<Correspondence> Correspondences { get; set; }


        //Users
        public DbSet<User> Users { get; set; }
        public DbSet<UserRight> UserRights { get; set; }
        public DbSet<UserAuthority> UserAuthority { get; set; }


        //Inventory
        public DbSet<Category> Category { get; set; }
        public DbSet<UOM> UOM { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<RawMaterial> RawMaterial { get; set; }
        public DbSet<Factor> Factors { get; set; }
        public DbSet<Process> Process { get; set; }
        public DbSet<Machine> Machine { get; set; }
        public DbSet<Grouping> Grouping { get; set; }
        public DbSet<GroupingSub> GroupingSub { get; set; }
        public DbSet<HSNMaster> HSNMaster { get; set; }
        public DbSet<Item> Item { get; set; }
        public DbSet<ItemHistory> ItemHistory { get; set; }
        public DbSet<ItemProductAssign> ItemProductAssign { get; set; }
        public DbSet<AssmblyDef> AssmblyDef { get; set; }

        public DbSet<AssemblyModify> AssemblyModify { get; set; }

        public DbSet<CompMaster> CompMaster { get; set; }
        public DbSet<ItemSub> ItemSub { get; set; }
        public DbSet<ProcessFlowChart> ProcessFlowChart { get; set; }

        public DbSet<AssemblyDefLabour> AssmblyDefLabour { get; set; }//sns


        //General
        public DbSet<State> State { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<CustomerIndirect> CustomerIndirect { get; set; }
        public DbSet<ContactPerson> ContactPerson { get; set; }
        public DbSet<Vendor> Vendor { get; set; }
        public DbSet<VendorContact> VendorContact { get; set; }
        public DbSet<VendorInDirect> VendorInDirect { get; set; }


        //Accounts
        public DbSet<Expense> Expense { get; set; }
        public DbSet<Income> Income { get; set; }
        public DbSet<Currency> Currency { get; set; }
        public DbSet<CurrencyToday> CurrencyToday { get; set; }
        public DbSet<Banks> Bank { get; set; }
        public DbSet<ProjectTypeMaster> ProjectTypeMaster { get; set; }
        public DbSet<CostCenter> CostCenter { get; set; }


        //Human Resources
        public DbSet<Staff> Staff { get; set; }
        public DbSet<StaffFamilyDetails> StaffFamilyDetail { get; set; }
        public DbSet<StaffEducation> StaffEducation { get; set; }
        public DbSet<StaffEmergency> StaffEmergency { get; set; }
        public DbSet<LeaveType> LeaveType { get; set; }
        public DbSet<EmployeeLeaveBalance> EmployeeLeaveBalance { get; set; }
        public DbSet<LeaveApplication> LeaveApplication { get; set; }
        public DbSet<DailyLeaveRecord> DailyLeaveRecord { get; set; }
        public DbSet<HolidayList> HolidayList { get; set; }
        public DbSet<ShiftAllocation> ShiftAllocation { get; set; }

        public DbSet<Candidate> Candidate { get; set; }
        //Human resource
        public DbSet<OfferLetter> OfferLetter { get; set; }
        public DbSet<OfferLetterSub> OfferLetterSub { get; set; }

        public DbSet<AppointmentLetter> AppointmentLetter { get; set; }
        public DbSet<AppointmetLetterSub> AppointmetLetterSub { get; set; }

        //Comapny Details
        public DbSet<Companydetails> CompanyDetails { get; set; }


        //Sales And labour
        public DbSet<Leads> Leads { get; set; }
        public DbSet<EnquirySales> EnquirySales { get; set; }
        public DbSet<EnquirySalesSub> EnquirySalesSub { get; set; }

        public DbSet<EnquiryFeasibility> EnquiryFeasibility { get; set; }
        public DbSet<EnquiryFeasibilitySub> EnquiryFeasibilitySub { get; set; }

        public DbSet<MfgQuote> MfgQuote { get; set; }
        public DbSet<MfgQuoteSub> MfgQuoteSub { get; set; }

        public DbSet<PoType> PoType { get; set; }
        public DbSet<MfgPo> MfgPo { get; set; }
        public DbSet<MfgPoSub> MfgPoSub { get; set; }

        //ContractReview
        public DbSet<ContractReview> ContractReview { get; set; }
        public DbSet<ContractReviewSub> ContractReviewSub { get; set; }
        public DbSet<ContractReviewMaster> ContractReviewMaster { get; set; }

        public DbSet<PerformaInv> PerformaInv { get; set; }
        public DbSet<PerformaInvSub> PerformaInvSub { get; set; }

        public DbSet<MfgDc> MfgDc { get; set; }
        public DbSet<MfgDcSub> MfgDcSub { get; set; }
        public DbSet<AssemblyPoComponentTracker> AssemblyPoComponentTracker { get; set; }

        public DbSet<MfgInv> MfgInv { get; set; }
        public DbSet<MfgInvSub> MfgInvSub { get; set; }

        public DbSet<ExpInv> ExpInv { get; set; }
        public DbSet<ExpInvSub> ExpInvSub { get; set; }

        // =========  Labour  ===========
        public DbSet<LabourGRN> LabourGRN { get; set; }
        public DbSet<LabourGRNSub> LabourGRNSub { get; set; }
        public DbSet<LabourSCN> LabourSCN { get; set; }
        public DbSet<LabourSCNSub> LabourSCNSub { get; set; }
        public DbSet<LabourDcOutgoing> LabourDcOutgoing { get; set; }
        public DbSet<LabourDcOutgoingSub> LabourDcOutgoingSub { get; set; }
        public DbSet<LabourDcReturnCompTrack> LabourDcReturnCompTrack { get; set; }
        public DbSet<LabInv> LabInv { get; set; }
        public DbSet<LabInvSub> LabInvSub { get; set; }

        public DbSet<CreditNote> CreditNote { get; set; }
        public DbSet<CreditNoteSub> CreditNoteSub { get; set; }

        //Inventory(Stock)
        public DbSet<SCNGen> SCNGen { get; set; }
        public DbSet<SCNGenSub> SCNGenSub { get; set; }

        public DbSet<MaterialIssNote> MaterialIssNote { get; set; }
        public DbSet<MaterialIssNoteSub> MaterialIssNoteSub { get; set; }

        public DbSet<StockAdd> StockAdd { get; set; }
        public DbSet<StockIssue> StockIssue { get; set; }
        public DbSet<StockIssueTrack> StockIssueTrack { get; set; }
        public DbSet<StoreInterTrans> StoreInterTrans { get; set; }
        public DbSet<StoreInterTransSub> StoreInterTransSub { get; set; }

        public DbSet<ToolCribIssue> ToolCribIssue { get; set; }
        public DbSet<ToolCribIssueSub> ToolCribIssueSub { get; set; }
        public DbSet<ToolCribReturn> ToolCribReturns { get; set; }
        public DbSet<ToolCribReturnSub> ToolCribReturnSubs { get; set; }

        //OutSourcing
        public DbSet<MaterialReq> MaterialReq { get; set; }
        public DbSet<MaterialReqSub> MaterialReqSub { get; set; }
        public DbSet<EnquiryPurchase> EnquiryPurchase { get; set; }
        public DbSet<EnquiryPurchaseSub> EnquiryPurchasesub { get; set; }

        public DbSet<EnquiryPurchaseVendorAssign> EnquiryPurchaseVendorAssign { get; set; }
        public DbSet<PurchaseQuote> PurchaseQuote { get; set; }
        public DbSet<PurchaseQuoteSub> PurchaseQuoteSub { get; set; }
        public DbSet<PurchPo> PurchPo { get; set; }
        public DbSet<PurchPoSub> PurchPoSub { get; set; }

        public DbSet<PurchaseGRN> PurchaseGRN { get; set; }
        public DbSet<PurchaseGRNSub> PurchaseGRNSub { get; set; }

        public DbSet<PurchaseSCN> PurchaseSCN { get; set; }
        public DbSet<PurchaseSCNSub> PurchaseSCNSub { get; set; }

        public DbSet<PurchaseInvoice> PurchaseInvoice { get; set; }
        public DbSet<PurchaseInvoiceSub> PurchaseInvoiceSub { get; set; }

        public DbSet<SubConDcOut> SubConDcOut { get; set; }
        public DbSet<SubConDcOutSub> SubConDcOutSub { get; set; }

        public DbSet<SubConGRN> SubConGRN { get; set; }
        public DbSet<SubConGRNSub> SubConGRNSub { get; set; }
        public DbSet<SubConGRNTrack> SubConGRNTrack { get; set; }

        public DbSet<SubConSCN> SubConSCN { get; set; }
        public DbSet<SubConSCNSub> SubConSCNSub { get; set; }

        public DbSet<SubConInv> SubConInv { get; set; }
        public DbSet<SubConInvSub> SubConInvSub { get; set; }

        public DbSet<DebitNote> DebitNote { get; set; }
        public DbSet<DebitNoteSub> DebitNoteSub { get; set; }


        //Planning
        public DbSet<ApprovalHistory> ApprovalHistory { get; set; }

        public DbSet<JobOrder> JobOrder { get; set; }
        public DbSet<JobOrderSub> JobOrderSub { get; set; }


        public DbSet<RouteCardRelease> RouteCardRelease { get; set; }
        public DbSet<RouteCardReleaseSub> RouteCardReleaseSub { get; set; }


        public DbSet<RouteCard> RouteCard { get; set; }
        public DbSet<RouteCardSub> RouteCardSub { get; set; }

        public DbSet<Estimate> Estimate { get; set; }//Estimate
        public DbSet<EstimateSub> EstimateSub { get; set; }

        //Production
        public DbSet<ProductionIssueAssy> ProductionIssueAssy { get; set; }
        public DbSet<ProductionIssueAssySub> ProductionIssueAssySub { get; set; }

        public DbSet<ProductionReturnAssy> ProductionReturnAssy { get; set; }
        public DbSet<ProductionReturnAssySub> ProductionReturnAssySub { get; set; }
        public DbSet<ProductionReturnAssyTrack> ProductionReturnAssyTrack { get; set; }

        public DbSet<ProductionSCNAssy> ProductionSCNAssy { get; set; }
        public DbSet<ProductionSCNAssySub> ProductionSCNAssySub { get; set; }

        //ProductionLog
        public DbSet<ProductionLog> ProductionLog { get; set; }
        public DbSet<ProductionLogSub> ProductionLogSub { get; set; }


        //Production Component 
        public DbSet<ProductionIssueComp> ProductionIssueComp { get; set; }
        public DbSet<ProductionIssueCompSub> ProductionIssueCompSub { get; set; }
        public DbSet<ProductionReturnComp> ProductionReturnComp { get; set; }
        public DbSet<ProductionReturnCompSub> ProductionReturnCompSub { get; set; }
        public DbSet<ProductionReturnCompTrack> ProductionReturnCompTrack { get; set; }
        public DbSet<ProductionSCNComp> ProductionSCNComp { get; set; }
        public DbSet<ProductionSCNCompSub> ProductionSCNCompSub { get; set; }


        //Maintenance
        public DbSet<MaintenanceSchedule> MaintenanceSchedule { get; set; }
        public DbSet<MaintenanceProcess> MaintenanceProcess { get; set; }
        public DbSet<BreakdownMaintenance> BreakdownMaintenance { get; set; }
        public DbSet<InstrumentDetails> InstrumentDetails { get; set; }
        public DbSet<CalibrationHistory> CalibrationHistory { get; set; }


        //Inspection
        public DbSet<MasterInspection> MasterInspection { get; set; }
        public DbSet<InspectionRef> InspectionRef { get; set; }
        public DbSet<DefectInfo> DefectInfo { get; set; }

        public DbSet<FinalInspection> FinalInspection { get; set; }
        public DbSet<FinalInspectionRef> FinalInspectionRef { get; set; }

        public DbSet<IncomingInspection> IncomingInspections { get; set; }
        public DbSet<IncomingInspectionRef> IncomingInspectionRef { get; set; }



        //For Auto running number
        public DbSet<DcRunningNumber> DcRunningNumber { get; set; }
        public DbSet<InvoiceAutoRunningNumber> InvoiceAutoRunningNumber { get; set; }
        public DbSet<AssemblyCharge> AssemblyCharge { get; set; }

        public DbSet<LabourTrackSummaryVM> LabourTrackSummary { get; set; }

        //Payments
        public DbSet<Payments> Payments { get; set; }
        public DbSet<PaymentsSub> PaymentsSub { get; set; }
        public DbSet<Advaceadjustment> Advaceadjustment { get; set; }
        public DbSet<AdvaceadjustmentSub> AdvaceadjustmentSub { get; set; }
        public DbSet<FundTrans> FundTrans { get; set; }
        public DbSet<Receipts> Receipts { get; set; }
        public DbSet<ReceiptsSub> ReceiptsSub { get; set; }

        //Service
        public DbSet<ServiceBills> ServiceBills { get; set; }
        public DbSet<ServiceBillsSub> ServiceBillsSub { get; set; }

        public DbSet<PoPendingDetailsVM> PoPendingDetailsVM { get; set; }

        public DbSet<RejectionMaster> RejectionMaster { get; set; }
        public DbSet<UserColumnPreference> UserColumnPreference { get; set; }

        //Human Resource
        public DbSet<Salary> Salary { get; set; }
        public DbSet<Attendance> Attendance { get; set; }
        public DbSet<StaffLoan> StaffLoan { get; set; }
        public DbSet<StockIssueRequest> StockIssueRequest { get; set; }
        public DbSet<StockIssueRequestSub> StockIssueRequestSub { get; set; }


        public DbSet<AppVersion> AppVersions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {

            #region Configurations

            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new UserConfiguration());

            builder.ApplyConfiguration(new ItemConfiguration());

            builder.ApplyConfiguration(new CustomerConfiguration());

            #endregion

            //MfgInv
            builder.Entity<MfgInv>()
                .HasOne(m => m.Customer)
                .WithMany()
                .HasForeignKey(m => m.CustId)
                .OnDelete(DeleteBehavior.Restrict);

            //CreditNote
            builder.Entity<CreditNote>()
               .HasOne(c => c.Customer)
               .WithMany()
               .HasForeignKey(c => c.CustId)
               .OnDelete(DeleteBehavior.Restrict);

            //Feasibility
            builder.Entity<EnquiryFeasibility>()
                .HasOne(e => e.EnquirySales)
                .WithMany()
                .HasForeignKey(e => e.RefEnqId)
                .OnDelete(DeleteBehavior.Restrict);

            //Export Invoice 

            builder.Entity<ExpInv>()
                .HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustId)
                .OnDelete(DeleteBehavior.NoAction);


            //----Purchase Vendor Assign----
            builder.Entity<EnquiryPurchaseVendorAssign>()
                .HasOne(x => x.EnquiryPurchaseSub)
                .WithMany()
                .HasForeignKey(x => x.EnquirySubId)
                .OnDelete(DeleteBehavior.Cascade);

            //Purchase Quoatation Sub
            builder.Entity<PurchaseQuoteSub>()
                .HasOne(pqs => pqs.EnquiryPurchaseVendorAssign)
                .WithMany()
                .HasForeignKey(pqs => pqs.RefEnqVendorAssignId)
                .OnDelete(DeleteBehavior.Restrict);

            //Purchase SCN
            builder.Entity<PurchaseSCN>()
            .HasOne(p => p.StoreAdd)
            .WithMany()
            .HasForeignKey(p => p.StoreIssId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PurchaseSCN>()
            .HasOne(p => p.StoreIssue)
            .WithMany()
            .HasForeignKey(p => p.StoreIssId)
            .OnDelete(DeleteBehavior.Restrict);

            //------Vendor-----
            builder.Entity<Vendor>()
                 .HasOne(v => v.Currency)
                 .WithMany()
                 .HasForeignKey(v => v.CurrId)
                 .OnDelete(DeleteBehavior.Restrict);

            // --- StockIssueTrack configuration ---
            builder.Entity<StockIssueTrack>()
               .HasOne(t => t.StockIssue)
               .WithMany()
               .HasForeignKey(t => t.IssueId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StockIssueTrack>()
                .HasOne(t => t.StockAdd)
                .WithMany()
                .HasForeignKey(t => t.AddId)
                .OnDelete(DeleteBehavior.Restrict);



            builder.Entity<ProcessFlowChart>()
                .HasOne(p => p.ComponentItem)
                .WithMany()
                .HasForeignKey(p => p.CompItemId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ProcessFlowChart>()
                .HasOne(p => p.RawMaterialItem)
                .WithMany()
                .HasForeignKey(p => p.RMId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ProcessFlowChart>()
                .HasOne(p => p.Process)
                .WithMany()
                .HasForeignKey(p => p.ProcId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ProcessFlowChart>()
                .HasOne(p => p.Machine)
                .WithMany()
                .HasForeignKey(p => p.MachId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ProcessFlowChart>()
                .HasOne(p => p.Vendor)
                .WithMany()
                .HasForeignKey(p => p.VendorCode)
                .OnDelete(DeleteBehavior.NoAction);


            builder.Entity<ProductionIssueAssySub>()
                .HasOne(s => s.ProductionIssueAssyParent)
                .WithMany(p => p.ProductionIssueAssySubs)
                .HasForeignKey(s => s.IssueId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProductionIssueAssySub>()
                .HasOne(p => p.JobOrderSub)
                .WithMany()
                .HasForeignKey(p => p.RefJobOrderSubId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionIssueAssySub>()
                .HasOne(p => p.JobOrder)
                .WithMany()
                .HasForeignKey(p => p.RefJobId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionIssueAssySub>()
                .HasOne(p => p.Item)
                .WithMany()
                .HasForeignKey(p => p.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionIssueAssySub>()
                .HasOne(p => p.AssyItem)
                .WithMany()
                .HasForeignKey(p => p.AssyId)
                .OnDelete(DeleteBehavior.Restrict);


            //Unique MfgQuoteNo
            builder.Entity<MfgQuote>()
                    .HasIndex(q => new { q.QuoteNo, q.Suffix })
                    .IsUnique();


            builder.Entity<ApprovalHistory>()
               .HasIndex(h => h.RecordId);

            builder.Entity<ApprovalHistory>()
                .HasIndex(h => h.ActionDate);


            // --- AssmblyDef configuration ---
            builder.Entity<AssmblyDef>()
                .HasIndex(x => new { x.AssmblyID, x.ItemId })
                .IsUnique();

            builder.Entity<AssmblyDef>()
                .HasOne(x => x.AssemblyItem)
                .WithMany()
                .HasForeignKey(x => x.AssmblyID)
                .OnDelete(DeleteBehavior.Restrict); // Avoid cascade

            builder.Entity<AssmblyDef>()
                .HasOne(x => x.PartItem)
                .WithMany()
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Restrict); // Avoid cascade

            builder.Entity<AssmblyDef>()
                .HasOne(x => x.Process)
                .WithMany()
                .HasForeignKey(x => x.ProcessId)
                .OnDelete(DeleteBehavior.Cascade); // Allow cascade

            //---AssmblyDefLabour  configuration-- -
            builder.Entity<AssemblyDefLabour>()
               .HasIndex(x => new { x.AssmblyID, x.ItemId })
               .IsUnique();

            builder.Entity<AssemblyDefLabour>()
                .HasOne(x => x.AssemblyItem)
                .WithMany()
                .HasForeignKey(x => x.AssmblyID)
                .OnDelete(DeleteBehavior.Restrict); // Avoid cascade

            builder.Entity<AssemblyDefLabour>()
                .HasOne(x => x.PartItem)
                .WithMany()
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Restrict); // Avoid cascade

            builder.Entity<AssemblyDefLabour>()
                .HasOne(x => x.Process)
                .WithMany()
                .HasForeignKey(x => x.ProcessId)
                .OnDelete(DeleteBehavior.Cascade); // Allow cascade

            // --- MfgQuote configuration ---
            builder.Entity<MfgQuote>()
                .HasOne(m => m.Customer)
                .WithMany()
                .HasForeignKey(m => m.CustId)
                .OnDelete(DeleteBehavior.Restrict); // Avoid cascade

            builder.Entity<MfgQuote>()
                .HasOne(m => m.Currency)
                .WithMany()
                .HasForeignKey(m => m.CurrId)
                .OnDelete(DeleteBehavior.Restrict); // Avoid cascade


            //Stock Issue Track configuration done above

            builder.Entity<StoreInterTrans>()
                .HasOne(s => s.StoreIssue)
                .WithMany()
                .HasForeignKey(s => s.StoreIssueId)
                .OnDelete(DeleteBehavior.Restrict); // or NoAction


            // route card release configuration
            builder.Entity<RouteCardReleaseSub>()
                .HasOne(x => x.RouteCardRelease)
                .WithMany(x => x.RouteCardReleaseSubs)
                .HasForeignKey(x => x.RouteCardReleaseId)
                .OnDelete(DeleteBehavior.Restrict);



            //MfgPOSub TracKaer

            builder.Entity<AssemblyPoComponentTracker>()
                .HasOne(x => x.Assembly)
                .WithMany()
                .HasForeignKey(x => x.AssyId)
                .OnDelete(DeleteBehavior.Restrict); // Master table

            builder.Entity<AssemblyPoComponentTracker>()
                .HasOne(x => x.Component)
                .WithMany()
                .HasForeignKey(x => x.ComponentId)
                .OnDelete(DeleteBehavior.Restrict); // Master table

            builder.Entity<AssemblyPoComponentTracker>()
                .HasOne(x => x.MfgPoSub)
                .WithMany()
                .HasForeignKey(x => x.PoSubId)
                .OnDelete(DeleteBehavior.Cascade); // ✅ Only cascade here


            // Labour
            builder.Entity<LabInv>()
                .HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LabourSCN>()
                .HasOne(x => x.StoreIssue)
                .WithMany()
                .HasForeignKey(x => x.StoreIssId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LabourSCN>()
                .HasOne(x => x.StoreAdd)
                .WithMany()
                .HasForeignKey(x => x.StoreIssId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LabInvSub>()
                .HasOne(x => x.LabourDcOutgoingSub)
                .WithMany()
                .HasForeignKey(x => x.RefDcSubId)
                .OnDelete(DeleteBehavior.Restrict);

            //CreditNote
            builder.Entity<CreditNoteSub>()
                 .HasOne(x => x.MfgInvSub)
                 .WithMany(m => m.CreditNoteSubs)
                 .HasForeignKey(x => x.RefInvSubId)
                 .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CreditNoteSub>()
                .HasOne(x => x.LabInvSub)
                .WithMany(m => m.CreditNoteSubs)
                .HasForeignKey(x => x.LabInvSubId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CreditNoteSub>()
                .HasOne(x => x.ExpInvSub)
                .WithMany(m => m.CreditNoteSubs)
                .HasForeignKey(x => x.ExpInvSubId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PurchasePoPendingListVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });

            builder.Entity<MfgPoPendingVM>()
            .HasNoKey()
            .ToView(null);

            builder.Entity<LabourGRNStatusListVM>()
           .HasNoKey()
           .ToView(null);

            builder.Entity<LabourSCNStatusListVM>()
           .HasNoKey()
           .ToView(null);

            builder.Entity<LabourTrackSummaryVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); 
            });
            builder.Entity<SalesTrackVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); // query type only
            });
            builder.Entity<LabourDcOutgoingStatusVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); 
            });
            builder.Entity<LabourInvoiceStatusVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); 
            });
            builder.Entity<JobOrderStatusListVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); 
            });

            builder.Entity<ManufacturingInvoiceStatusListVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); 
            });
            builder.Entity<ExportInvoiceStatusVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); 
            });
            builder.Entity<ProductionIssueAssyStatusVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); 
            });
            builder.Entity<ProductionReturnAssyStatusVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); 
            });
            builder.Entity<ProductionSCNAssyStatusVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); 
            });
            builder.Entity<ProductionIssueCompStatusVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); 
            });
            builder.Entity<ProductionReturnCompStatusVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); 
            });
            builder.Entity<ProductionSCNCompStatusVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); 
            });
            builder.Entity<PurchaseInvoiceStatusVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); 
            });
            builder.Entity<PurchaseGRNStatusVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); 
            });
            builder.Entity<PurchaseSCNStatusVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); 
            });
            builder.Entity<ToolCribIssueStatusVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); // query type only
            });

            builder.Entity<ToolCribReturnStatusVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); // query type only
            });

            builder.Entity<CreditNoteListVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); // query type only
            });

            builder.Entity<DebitNoteListVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); // query type only
            });
            builder.Entity<StockLedgerReportVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); // query type only
            });
            builder.Entity<StockAnalysisVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); // query type only
            });
            builder.Entity<LabourDcInOutTallyVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); // query type only
            });
            builder.Entity<SubContractDcInOutVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); // query type only
            });

            builder.Entity<ComboModelVM>().HasNoKey();

            builder.Entity<PoPendingDetailsVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });

            builder.Entity<ItemWiseReportVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });

            builder.Entity<ToolCribVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });

            builder.Entity<BOMAssemblySpVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });

            builder.Entity<ConfirmationOfAccountsVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });


            //Accounts
            builder.Entity<FundTrans>()
                  .HasOne(x => x.Banks)
                  .WithMany()
                  .HasForeignKey(x => x.BankId)
                  .OnDelete(DeleteBehavior.Cascade); // or SetNull if BankId can be null

            builder.Entity<FundTrans>()
                    .HasOne(x => x.Payments)
                    .WithMany()
                    .HasForeignKey(x => x.PaymentRefId)
                    .OnDelete(DeleteBehavior.NoAction); // or SetNull

            builder.Entity<FundTrans>()
                    .HasOne(x => x.Receipts)
                    .WithMany()
                    .HasForeignKey(x => x.ReceiptRefId)
                    .OnDelete(DeleteBehavior.NoAction); // or SetNull

            builder.Entity<FundTrans>()
                   .HasOne(x => x.Advaceadjustment)
                   .WithMany()
                   .HasForeignKey(x => x.AdvanceRefId)
                   .OnDelete(DeleteBehavior.NoAction); // or SetNull

            builder.Entity<ServiceBills>()
                    .HasOne(s => s.Currency)
                    .WithMany()
                    .HasForeignKey(s => s.CurrId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PendingBillVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); // query type only
            });
            builder.Entity<PaidBillsVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); // query type only
            });

            builder.Entity<PendingStatementsVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); // query type only
            });

            //Reports
            builder.Entity<TaxDetailsVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });

            builder.Entity<ProfitLossMonthlyVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });
            builder.Entity<Profit_LossAccountsVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });
            builder.Entity<ItemModificationReportVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });

            builder.Entity<DaybookVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });

            builder.Entity<InwardPOTrack>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });
            builder.Entity<PurchasePOTrack>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });

            builder.Entity<StockPositionSummaryVM>()
             .HasNoKey()
             .ToView(null);

            builder.Entity<LabourPendingVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); // query type only
            });

            builder.Entity<LabourPendingSummaryVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); // query type only
            });

            builder.Entity<ProductionPendingVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); // query type only
            });

            builder.Entity<RejectionVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });

            builder.Entity<Item>().ToTable(tb => tb.HasTrigger("TR_ItemHistory"));

            builder.Entity<GSTITCVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });
            builder.Entity<RatingVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });

            builder.Entity<PurchaseSalesTrackVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });
            builder.Entity<RouteCardAnalysisVM>()
            .HasNoKey()
            .ToView(null);

            builder.Entity<RouteCardAnalysisDetailsVM>()
           .HasNoKey()
           .ToView(null);

            builder.Entity<MaterialReqPendingVM>()
           .HasNoKey()
           .ToView(null);

            builder.Entity<TDSReportVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });

            builder.Entity<HSNSummaryVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });

            builder.Entity<CreditNoteDebitNoteSummaryVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });

            builder.Entity<PurchaseQuotationPendingList>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });

            builder.Entity<PurchaseEnquiryPendingList>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });



            builder.Entity<SubContractDcPendingVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });
            builder.Entity<SubContractGRNPendingVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });
            builder.Entity<SubContractSCNPendingVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });
            builder.Entity<SubConInvoicePendingVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });
            builder.Entity<PrPoratingVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });
            builder.Entity<JobOrderAnalysisVM>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
            });
           

            // Avoid cascade

            // --- Global Restrict for all FKs unless explicitly set ---
            foreach (var relationship in builder.Model.GetEntityTypes() 
                         .SelectMany(e => e.GetForeignKeys()))
            {
                if (relationship.DeleteBehavior != DeleteBehavior.Cascade &&
                    relationship.DeleteBehavior != DeleteBehavior.NoAction &&
                    relationship.DeleteBehavior != DeleteBehavior.Restrict)
                {
                    relationship.DeleteBehavior = DeleteBehavior.Restrict;
                }
            }


            builder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    StaffId = null,  // Assuming no staff assigned for the admin user
                    UserName = "Administrator",
                    UserPassword = "AQAAAAIAAYagAAAAEBDHR4whgjIYMVkEU8I4FUjARxtH1DI/eoKgzld07jJ5NSwY+iIDLIiFRt7Q1YxcYQ==",
                    EmailId = "admin@example.com",
                    PhoneNumber = "9999999999",
                    IsActive = true,
                    Role = UserRole.Administrator
                }
            );

            builder.Entity<Screens>().ToTable("Screens");
            builder.Entity<Screens>().HasData(
                new Screens { Id = 1, ScreenCode = 1, ScreenName = "User", IsPrintRequired = false },
                new Screens { Id = 2, ScreenCode = 2, ScreenName = "Category", IsPrintRequired = false },
                new Screens { Id = 3, ScreenCode = 3, ScreenName = "UOM", IsPrintRequired = false },
                new Screens { Id = 4, ScreenCode = 4, ScreenName = "State", IsPrintRequired = false },
                new Screens { Id = 5, ScreenCode = 5, ScreenName = "Currency", IsPrintRequired = false },
                new Screens { Id = 6, ScreenCode = 6, ScreenName = "User Rights", IsPrintRequired = false },
                new Screens { Id = 7, ScreenCode = 7, ScreenName = "Store", IsPrintRequired = false },
                new Screens { Id = 8, ScreenCode = 8, ScreenName = "Raw Material", IsPrintRequired = false },
                new Screens { Id = 9, ScreenCode = 9, ScreenName = "Factors", IsPrintRequired = false },
                new Screens { Id = 10, ScreenCode = 10, ScreenName = "Process", IsPrintRequired = false },
                new Screens { Id = 11, ScreenCode = 11, ScreenName = "Machine", IsPrintRequired = false },
                new Screens { Id = 12, ScreenCode = 12, ScreenName = "Grouping", IsPrintRequired = false },
                new Screens { Id = 13, ScreenCode = 13, ScreenName = "Item", IsPrintRequired = false },
                new Screens { Id = 14, ScreenCode = 14, ScreenName = "HSN Master", IsPrintRequired = false },
                new Screens { Id = 15, ScreenCode = 15, ScreenName = "Customer", IsPrintRequired = false },
                new Screens { Id = 16, ScreenCode = 16, ScreenName = "Expense", IsPrintRequired = false },
                new Screens { Id = 17, ScreenCode = 17, ScreenName = "Screen Management", IsPrintRequired = false },
                new Screens { Id = 18, ScreenCode = 18, ScreenName = "Vendor", IsPrintRequired = false },
                new Screens { Id = 19, ScreenCode = 19, ScreenName = "BOM", IsPrintRequired = false },
                new Screens { Id = 20, ScreenCode = 20, ScreenName = "Income", IsPrintRequired = false },
                new Screens { Id = 21, ScreenCode = 21, ScreenName = "Currency Today", IsPrintRequired = false },
                new Screens { Id = 22, ScreenCode = 22, ScreenName = "Bank", IsPrintRequired = false },
                new Screens { Id = 23, ScreenCode = 23, ScreenName = "Company", IsPrintRequired = false },
                new Screens { Id = 24, ScreenCode = 24, ScreenName = "Correspondences", IsPrintRequired = false },
                new Screens { Id = 25, ScreenCode = 25, ScreenName = "Master Upload", IsPrintRequired = false },
                new Screens { Id = 26, ScreenCode = 26, ScreenName = "Holiday List", IsPrintRequired = false },
                new Screens { Id = 27, ScreenCode = 27, ScreenName = "Staff", IsPrintRequired = false },
                new Screens { Id = 28, ScreenCode = 28, ScreenName = "LeaveType", IsPrintRequired = false },
                new Screens { Id = 29, ScreenCode = 29, ScreenName = "Employee Leave Balance", IsPrintRequired = false },
                new Screens { Id = 30, ScreenCode = 30, ScreenName = "Leave Application", IsPrintRequired = false },
                new Screens { Id = 31, ScreenCode = 31, ScreenName = "Project Type Master", IsPrintRequired = false },
                new Screens { Id = 32, ScreenCode = 32, ScreenName = "Cost-Center", IsPrintRequired = false },
                new Screens { Id = 33, ScreenCode = 33, ScreenName = "User Level Authorization", IsPrintRequired = false },
                new Screens { Id = 34, ScreenCode = 34, ScreenName = "Shift Allocation", IsPrintRequired = false },
                new Screens { Id = 35, ScreenCode = 35, ScreenName = "Item Rate-Updation", IsPrintRequired = false },
                new Screens { Id = 36, ScreenCode = 36, ScreenName = "Manufacturing Quotation", IsPrintRequired = true },
                new Screens { Id = 37, ScreenCode = 37, ScreenName = "Terms and Conditions", IsPrintRequired = false },
                new Screens { Id = 38, ScreenCode = 38, ScreenName = "Leads", IsPrintRequired = false },
                new Screens { Id = 39, ScreenCode = 39, ScreenName = "Enquiry Sales", IsPrintRequired = true },
                new Screens { Id = 40, ScreenCode = 40, ScreenName = "Authorization", IsPrintRequired = false },
                new Screens { Id = 41, ScreenCode = 41, ScreenName = "Sales Order", IsPrintRequired = false },
                new Screens { Id = 42, ScreenCode = 42, ScreenName = "Manufacturing DC", IsPrintRequired = false },
                new Screens { Id = 43, ScreenCode = 43, ScreenName = "General Settings", IsPrintRequired = false },
                new Screens { Id = 44, ScreenCode = 44, ScreenName = "Store Map", IsPrintRequired = false },
                new Screens { Id = 45, ScreenCode = 45, ScreenName = "Performa Invoice", IsPrintRequired = false },
                new Screens { Id = 46, ScreenCode = 46, ScreenName = "Stock-Add", IsPrintRequired = false },
                new Screens { Id = 47, ScreenCode = 47, ScreenName = "Material Requisition", IsPrintRequired = false },
                new Screens { Id = 48, ScreenCode = 48, ScreenName = "Material Issue-Note", IsPrintRequired = false },
                new Screens { Id = 49, ScreenCode = 49, ScreenName = "Enquiry Purchase", IsPrintRequired = false },
                new Screens { Id = 50, ScreenCode = 50, ScreenName = "Material Requirement Analysis", IsPrintRequired = false },
                new Screens { Id = 51, ScreenCode = 51, ScreenName = "Print Management", IsPrintRequired = false },
                new Screens { Id = 52, ScreenCode = 52, ScreenName = "Enquiry Feasibility", IsPrintRequired = false },
                new Screens { Id = 53, ScreenCode = 53, ScreenName = "Job Order", IsPrintRequired = false },
                new Screens { Id = 54, ScreenCode = 54, ScreenName = "Production Issue WO Assembly", IsPrintRequired = false },
                new Screens { Id = 55, ScreenCode = 55, ScreenName = "Production Return GRN Assembly", IsPrintRequired = false },
                new Screens { Id = 56, ScreenCode = 56, ScreenName = "Production SCN Assembly", IsPrintRequired = false },
                new Screens { Id = 57, ScreenCode = 57, ScreenName = "Tool-Crib Issue", IsPrintRequired = false },
                new Screens { Id = 58, ScreenCode = 58, ScreenName = "Tool-Crib Return", IsPrintRequired = false },
                new Screens { Id = 59, ScreenCode = 59, ScreenName = "Instant Search", IsPrintRequired = false },
                new Screens { Id = 60, ScreenCode = 60, ScreenName = "Route Card", IsPrintRequired = false },
                new Screens { Id = 61, ScreenCode = 61, ScreenName = "Production Log Setting", IsPrintRequired = false },
                new Screens { Id = 62, ScreenCode = 62, ScreenName = "Daily Production Log", IsPrintRequired = false },
                new Screens { Id = 63, ScreenCode = 63, ScreenName = "Process Flow-RC", IsPrintRequired = false },


                new Screens { Id = 64, ScreenCode = 64, ScreenName = "MaintenanceSchedule", IsPrintRequired = false },
                new Screens { Id = 65, ScreenCode = 65, ScreenName = "MaintenanceProcess", IsPrintRequired = false },
                new Screens { Id = 66, ScreenCode = 66, ScreenName = "BreakdownMaintenance", IsPrintRequired = false },
                new Screens { Id = 67, ScreenCode = 67, ScreenName = "CalibrationHistoryAndMaintenance", IsPrintRequired = false },

                new Screens { Id = 68, ScreenCode = 68, ScreenName = "Production Issue WO Component", IsPrintRequired = false },
                new Screens { Id = 69, ScreenCode = 69, ScreenName = "Inter Store Transfer", IsPrintRequired = false },
                new Screens { Id = 70, ScreenCode = 70, ScreenName = "Production Return Component", IsPrintRequired = false },

                new Screens { Id = 71, ScreenCode = 71, ScreenName = "Production SCN Component", IsPrintRequired = false },
                new Screens { Id = 72, ScreenCode = 72, ScreenName = "Contract Review", IsPrintRequired = false },
                new Screens { Id = 73, ScreenCode = 73, ScreenName = "Contract Review CheckList", IsPrintRequired = false },
                new Screens { Id = 74, ScreenCode = 74, ScreenName = "Stock Position", IsPrintRequired = false },
                new Screens { Id = 75, ScreenCode = 75, ScreenName = "Purchase-Quotation", IsPrintRequired = false },
                new Screens { Id = 76, ScreenCode = 76, ScreenName = "Purchase Order", IsPrintRequired = false },
                new Screens { Id = 77, ScreenCode = 77, ScreenName = "Purchase GRN", IsPrintRequired = false },
                new Screens { Id = 78, ScreenCode = 78, ScreenName = "Purchase SCN", IsPrintRequired = false },
                new Screens { Id = 79, ScreenCode = 79, ScreenName = "Purchase Invoice", IsPrintRequired = false },
                new Screens { Id = 80, ScreenCode = 80, ScreenName = "Manufacturing Invoice", IsPrintRequired = false },

                new Screens { Id = 81, ScreenCode = 81, ScreenName = "Sub-Contract DC-Out", IsPrintRequired = false },
                new Screens { Id = 82, ScreenCode = 82, ScreenName = "Sub-Contrect GRN", IsPrintRequired = false },
                new Screens { Id = 83, ScreenCode = 83, ScreenName = "Sub-Contract SCN", IsPrintRequired = false },
                new Screens { Id = 84, ScreenCode = 84, ScreenName = "Sub-Contract Invoice", IsPrintRequired = false },
                new Screens { Id = 85, ScreenCode = 85, ScreenName = "Export Invoice", IsPrintRequired = false },

                new Screens { Id = 86, ScreenCode = 86, ScreenName = "MasterInspection", IsPrintRequired = false },
                new Screens { Id = 87, ScreenCode = 87, ScreenName = "FinalInspection", IsPrintRequired = false },
                new Screens { Id = 88, ScreenCode = 88, ScreenName = "IncomingInspection", IsPrintRequired = false },
                new Screens { Id = 89, ScreenCode = 89, ScreenName = "InspectionSettings", IsPrintRequired = false },

                new Screens { Id = 90, ScreenCode = 90, ScreenName = "DefectInfo", IsPrintRequired = false },
                new Screens { Id = 91, ScreenCode = 91, ScreenName = "Assembly Requirement Analysis", IsPrintRequired = false },

                new Screens { Id = 92, ScreenCode = 92, ScreenName = "Labour GRN", IsPrintRequired = false },
                new Screens { Id = 93, ScreenCode = 93, ScreenName = "Labour SCN", IsPrintRequired = false },
                new Screens { Id = 94, ScreenCode = 94, ScreenName = "Labour Delivery Challan", IsPrintRequired = false },
                new Screens { Id = 95, ScreenCode = 95, ScreenName = "Labour Invoice", IsPrintRequired = false },

                new Screens { Id = 96, ScreenCode = 96, ScreenName = "Route Card Release", IsPrintRequired = false },
                new Screens { Id = 97, ScreenCode = 97, ScreenName = "Excel Upload", IsPrintRequired = false },

                new Screens { Id = 98, ScreenCode = 98, ScreenName = "Credit Note", IsPrintRequired = false },

                new Screens { Id = 99, ScreenCode = 99, ScreenName = "Printing Map", IsPrintRequired = false },

                new Screens { Id = 100, ScreenCode = 100, ScreenName = "LabourCostManagement", IsPrintRequired = false },

                new Screens { Id = 101, ScreenCode = 101, ScreenName = "BOMLabourCost", IsPrintRequired = false },

                new Screens { Id = 102, ScreenCode = 102, ScreenName = "Sales Track Report", IsPrintRequired = false },

                new Screens { Id = 103, ScreenCode = 103, ScreenName = "Debit Note", IsPrintRequired = false },

                new Screens { Id = 104, ScreenCode = 104, ScreenName = "Tags", IsPrintRequired = false },

                new Screens { Id = 105, ScreenCode = 105, ScreenName = "Labour Track Report", IsPrintRequired = false },
                new Screens { Id = 106, ScreenCode = 106, ScreenName = "Payments", IsPrintRequired = false },
                new Screens { Id = 107, ScreenCode = 107, ScreenName = "Advaceadjustment", IsPrintRequired = false },
                new Screens { Id = 108, ScreenCode = 108, ScreenName = "Receipts", IsPrintRequired = false },
                new Screens { Id = 109, ScreenCode = 109, ScreenName = "Fundtransactions", IsPrintRequired = false },

                new Screens { Id = 110, ScreenCode = 110, ScreenName = "Dashboard", IsPrintRequired = false },
                new Screens { Id = 111, ScreenCode = 111, ScreenName = "Stock Ledger", IsPrintRequired = false },
                new Screens { Id = 112, ScreenCode = 112, ScreenName = "Stock Analysis", IsPrintRequired = false },
                new Screens { Id = 113, ScreenCode = 113, ScreenName = "ViewTallyDc-In-Out", IsPrintRequired = false },

                new Screens { Id = 114, ScreenCode = 114, ScreenName = "Bill Pending List", IsPrintRequired = false },
                new Screens { Id = 115, ScreenCode = 115, ScreenName = "Bill Paid List", IsPrintRequired = false },
                new Screens { Id = 116, ScreenCode = 116, ScreenName = "Service Bills", IsPrintRequired = false },
                new Screens { Id = 117, ScreenCode = 117, ScreenName = "Po Pendings", IsPrintRequired = false },
                new Screens { Id = 118, ScreenCode = 118, ScreenName = "Pending Statements", IsPrintRequired = false },
                new Screens { Id = 119, ScreenCode = 119, ScreenName = "ToolCribIssue Summary", IsPrintRequired = false },
                new Screens { Id = 120, ScreenCode = 120, ScreenName = "ItemHistory", IsPrintRequired = false },
                new Screens { Id = 121, ScreenCode = 121, ScreenName = "Confirmation Of Accounts", IsPrintRequired = false },
                new Screens { Id = 122, ScreenCode = 122, ScreenName = "Stock Position(Internal & External)", IsPrintRequired = false },
                new Screens { Id = 123, ScreenCode = 123, ScreenName = "TaxDetails Report", IsPrintRequired = false },
                new Screens { Id = 124, ScreenCode = 124, ScreenName = "Profit & Loss Accounts", IsPrintRequired = false },
                new Screens { Id = 125, ScreenCode = 125, ScreenName = "Item Modification", IsPrintRequired = false },
                new Screens { Id = 126, ScreenCode = 126, ScreenName = "RejectionMaster", IsPrintRequired = false },
                new Screens { Id = 127, ScreenCode = 127, ScreenName = "Day Book", IsPrintRequired = false },
                new Screens { Id = 128, ScreenCode = 128, ScreenName = "Labour Pending", IsPrintRequired = false },
                new Screens { Id = 129, ScreenCode = 129, ScreenName = "View Po Track", IsPrintRequired = false },
                new Screens { Id = 130, ScreenCode = 130, ScreenName = "Production Pending Summary", IsPrintRequired = false },
                new Screens { Id = 131, ScreenCode = 131, ScreenName = "Rejection Analysis", IsPrintRequired = false },
                new Screens { Id = 132, ScreenCode = 132, ScreenName = "GSTITC04", IsPrintRequired = false },


                new Screens { Id = 133, ScreenCode = 133, ScreenName = "HRMaster", IsPrintRequired = false },
                new Screens { Id = 134, ScreenCode = 134, ScreenName = "Biometric Excel Set", IsPrintRequired = false },
                new Screens { Id = 135, ScreenCode = 135, ScreenName = "Salary Head Print Setting", IsPrintRequired = false },


                new Screens { Id = 136, ScreenCode = 136, ScreenName = "Salary", IsPrintRequired = true },
                new Screens { Id = 137, ScreenCode = 137, ScreenName = "Attendance", IsPrintRequired = false },
                new Screens { Id = 138, ScreenCode = 138, ScreenName = "StaffLoan", IsPrintRequired = false },
                new Screens { Id = 139, ScreenCode = 139, ScreenName = "BOM Labour", IsPrintRequired = false },
                new Screens { Id = 140, ScreenCode = 140, ScreenName = "Ratings", IsPrintRequired = false },
                new Screens { Id = 141, ScreenCode = 141, ScreenName = "Purchase Sales Track", IsPrintRequired = false },
                new Screens { Id = 142, ScreenCode = 142, ScreenName = "Estimation", IsPrintRequired = false },
                new Screens { Id = 143, ScreenCode = 143, ScreenName = "Route Card Analysis", IsPrintRequired = false },
                new Screens { Id = 144, ScreenCode = 144, ScreenName = "Stock Issue-Request", IsPrintRequired = false },
                new Screens { Id = 145, ScreenCode = 145, ScreenName = "CreditDebit Summary Report", IsPrintRequired = false },
                new Screens { Id = 146, ScreenCode = 146, ScreenName = "TDSummary Report", IsPrintRequired = false },
                new Screens { Id = 147, ScreenCode = 147, ScreenName = "HSNSummary Report", IsPrintRequired = false },
                new Screens { Id = 148, ScreenCode = 148, ScreenName = "PR PO Rating Report", IsPrintRequired = false },
                new Screens { Id = 149, ScreenCode = 149, ScreenName = "Candidate", IsPrintRequired = false },
                new Screens { Id = 150, ScreenCode = 150, ScreenName = "Offer Letter", IsPrintRequired = false },
                new Screens { Id = 151, ScreenCode = 151, ScreenName = "Appointment Letter", IsPrintRequired = false },
                new Screens { Id = 152, ScreenCode = 152, ScreenName = "Joborder Track", IsPrintRequired = false }

            );


            builder.Entity<InspectionSettings>().HasData(
                new InspectionSettings
                {
                    Id = 1,
                    SettingName = "Samples Per Sheet",
                    SamplesPerSheet = 15
                }
            );

            builder.Entity<ScreenManagement>().HasData(
                new ScreenManagement
                {
                    Id = 1,
                    ScreenName = "Items",
                    Display = "Material Weight",
                    Topic = "Enable this option to validate component weight before saving the item.",
                    Description = "If enabled, the system will check whether the Material Weight is properly assigned for each component when saving an item.",
                    Required = true
                },

                new ScreenManagement
                {
                    Id = 2,
                    ScreenName = "Users",
                    Display = "Assign State Leads",
                    Topic = "Allow assigning specific users as state-level leads",
                    Description = "Enable this option if you want to grant users the ability to manage or lead operations within a specific state or region.",
                    Required = true
                },

                new ScreenManagement
                {
                    Id = 3,
                    ScreenName = "Sales-Order",
                    Display = "Auto Generate SalesOrder No.",
                    Topic = "Automatically generate SalesOrder numbers for new PO",
                    Description = "When enabled, the system will automatically assign a unique SalesOrder number to each new quote created, following a predefined format or sequence.",
                    Required = false
                },

                new ScreenManagement
                {
                    Id = 4,
                    ScreenName = "Route Card",
                    Display = "Manual RC",
                    Topic = "Manual Process Allocation",
                    Description = "When enabled, the user must manually define the process sequence, machine, and supplier for the Route Card instead of using predefined item routing.",
                    Required = false
                },

                 new ScreenManagement
                 {
                     Id = 5,
                     ScreenName = "Labour GRN",
                     Display = "Rate Control",
                     Topic = "Rate Lock Setting",
                     Description = "When this setting is enabled, the Rate field in Labour GRN becomes non-editable. The system will automatically take the rate from the Reference PO, and the rate cannot be modified.",
                     Required = false
                 },

                 new ScreenManagement
                 {
                     Id = 6,
                     ScreenName = "Labour Delivery Challan",
                     Display = "Rate Control",
                     Topic = "Rate Lock Setting",
                     Description = "When this setting is enabled, the Rate field in Labour Delivery Challan becomes non-editable. The system will automatically take the rate from the Reference PO, and the rate cannot be modified.",
                     Required = false
                 },

                 new ScreenManagement
                 {
                     Id = 7,
                     ScreenName = "Labour Invoice",
                     Display = "Rate Control",
                     Topic = "Rate Lock Setting",
                     Description = "When this setting is enabled, the Rate field in Labour Invoice becomes non-editable. The system will automatically take the rate from the Reference PO, and the rate cannot be modified.",
                     Required = false
                 },

                 new ScreenManagement
                 {
                    Id = 8,
                    ScreenName = "Items",
                    Display = "Customer-Based Item Restriction",
                    Topic = "Restrict Item Loading Based on Customer Allocation",
                    Description = "When this setting is enabled, transaction screens will load only those items that are allocated to the selected customer in the Item Master (Item Customer Assign). If disabled, all active items will be available for selection regardless of customer allocation.",
                    Required = false
                 },

                new ScreenManagement
                {
                    Id = 9,
                    ScreenName = "Labour GRN",
                    Display = "Restrict Out Item Same As In Item",
                    Topic = "Ensure Output Item Matches Input Item in Labour GRN",
                    Description = "When this setting is enabled, the Output Item in the Labour GRN must be the same as the Input Item selected in the transaction. The system will restrict users from selecting a different Output Item. If disabled, users can select any item as the Output Item in the Labour GRN process.",
                    Required = false
                },

                 new ScreenManagement
                 {
                     Id = 10,
                     ScreenName = "Manufacturing Invoice",
                     Display = "E-Way Bill",
                     Topic = "E-Way Bill Auto-Generation After E-Invoice",
                     Description = "When enabled, the E-Way Bill will be generated automatically after successful generation of the E-Invoice, eliminating the need for manual E-Way Bill creation.",
                     Required = false
                 },

                new ScreenManagement
                {
                    Id = 11,
                    ScreenName = "Manufacturing Invoice",
                    Display = "Prefix",
                    Topic = "Document Number Prefix Requirement",
                    Description = "When enabled, a prefix must be defined for the document number. The system will not allow saving or generating the invoice without a valid prefix.",
                    Required = false
                },

                new ScreenManagement
                {
                    Id = 12,
                    ScreenName = "Export Invoice",
                    Display = "E-Way Bill",
                    Topic = "E-Way Bill Auto-Generation After E-Invoice",
                    Description = "When enabled, the E-Way Bill will be generated automatically after successful generation of the E-Invoice, eliminating the need for manual E-Way Bill creation.",
                    Required = false
                },

                new ScreenManagement
                {
                    Id = 13,
                    ScreenName = "Export Invoice",
                    Display = "Prefix",
                    Topic = "Document Number Prefix Requirement",
                    Description = "When enabled, a prefix must be defined for the document number. The system will not allow saving or generating the invoice without a valid prefix.",
                    Required = false
                },

                new ScreenManagement
                {
                    Id =14 ,
                    ScreenName = "Labour Invoice",
                    Display = "Prefix",
                    Topic = "Document Number Prefix Requirement",
                    Description = "When enabled, a prefix must be defined for the document number. The system will not allow saving or generating the invoice without a valid prefix.",
                    Required = false
                },

                //SNS
                 new ScreenManagement
                 {
                     Id = 15,
                     ScreenName = "BOMLabourCost",
                     Display = "Labour Cost",
                     Topic = "Enable Labour Cost Display in Each Screen",
                     Description = "Displays the Labour Cost section in all applicable screens when enabled.",
                     Required = false
                 },
                 new ScreenManagement
                 {
                     Id = 16,
                     ScreenName = "BOMLabourCost",
                     Display = "Add Additional Charges",
                     Topic = "Enable Add Additional Charges Button in BOM Screen",
                     Description = "When enabled, allows users to add and manage labour-related additional charges within the BOM costing screen.",
                     Required = false
                 },

                 new ScreenManagement
                 {
                     Id = 17,
                     ScreenName = "BOM",
                     Display = "Customer Material Validation",
                     Topic = "BOM Customer Material Check in Receive Material",
                     Description = "When enabled, the system validates during Receive Material entry that the item exists in the BOM and is marked as Customer Material. Only such items will be allowed for receipt.",
                     Required = false
                 },

                new ScreenManagement
                {
                    Id = 18,
                    ScreenName = "Sales-Order",
                    Display = "Enable Labour Button by Default",
                    Topic = "Automatically enable Labour button in Sales PO",
                    Description = "When enabled, the Labour button in the Sales Order screen will be set to true by default for all new entries, ensuring the Labour option is readily available without manual activation.",
                    Required = false
                },

                new ScreenManagement
                {
                    Id = 19,
                    ScreenName = "Purchase Order",
                    Display = "ROL",
                    Topic = "Purchase Order - ROL Column Visibility",
                    Description = "When enabled, the Reorder Level (ROL) column will be displayed in the Purchase Order screen to help users view item reorder levels during purchase entry. And system will use MAX(ROL, Required Qty) during auto Material Requisition In AutoIndent Creation",
                    Required = false
                },

               new ScreenManagement
               {
                   Id = 20,
                   ScreenName = "Production Issue WO Assembly",
                   Display = "Month-wise Number Generation",
                   Topic = "Department-wise and Month-wise Issue Number Generation",
                   Description = "When enabled, the system will generate Production Issue numbers based on Department, Month, and Financial Year. The running number will automatically reset every month for each department, ensuring unique and structured numbering (e.g., R-03-000001-2026-27).",
                   Required = false
               },

                new ScreenManagement
                {
                    Id = 21,
                    ScreenName = "Labour GRN",
                    Display = "PO Wise Labour DC Outgoing",
                    Topic = "PO Wise Labour DC Outgoing & Auto Close",
                    Description = "When this setting is enabled, Labour DC Outgoing will be processed PO-wise. After completing the DC Outgoing against a PO, the system will automatically close the respective PO and prevent further transactions.",
                    Required = false
                },
                new ScreenManagement
                {
                    Id = 22,
                    ScreenName = "Job Order",
                    Display = "JobOrder Generate Assembly and SubAssembly",
                    Topic = "Assembly & Sub-Assembly Generation",
                    Description = "Enables automatic generation of Assembly and Sub-Assembly items during Job Order processing.",
                    Required = false
                },
                new ScreenManagement
                {
                    Id = 23,
                    ScreenName = "Job Order",
                    Display = "Assign Job Order Department-wise",
                    Topic = "Department-wise Job Order Assignment",
                    Description = "Allows assigning and managing Job Order operations across different departments based on process.",
                    Required = false
                },

                //Sharadha
                new ScreenManagement
                {
                    Id = 24,
                    ScreenName = "Labour SCN",
                    Display = "Rejection Parameters",
                    Topic = "Load Parameters for Sharadha Company",
                    Description = "When enabled, the system will provide an option in Labour SCN to select rejection parameters for recording rejection quantity.",
                    Required = false
                },

                new ScreenManagement
                {
                    Id = 25,
                    ScreenName = "Labour GRN",
                    Display = "Batch Number",
                    Topic = "Need Batch Number Option",
                    Description = "When enabled, the system will provide an option in Labour GRN to select batch number for recording batch information.",
                    Required = false
                },
                new ScreenManagement
                {
                    Id = 26,
                    ScreenName = "Labour Invoice",
                    Display = "Invoice Cancel",
                    Topic = "Need Invoice Cancel Option",
                    Description = "When enabled, the system will provide an option in Labour Invoice to cancel the invoice that has been submitted to the E-Invoice portal, after the E-Invoice is cancelled in the portal.",
                    Required = false
                },
                new ScreenManagement
                {
                    Id = 27,
                    ScreenName = "Labour Invoice",
                    Display = "Process Selection",
                    Topic = "Need Process Selection Option",
                    Description = "When enabled, the system will provide an option in Labour Invoice to select process for recording process information.",
                    Required = false
                },
                new ScreenManagement
                {
                    Id = 28,
                    ScreenName = "Process",
                    Display = "process Type Selection",
                    Topic = "Need Process Type Selection Option",
                    Description = "When enabled, a process selection textbox (Single, Multiple, Special, Standard) will be shown in the Process Screen for recording process details.",
                    Required = false
                },
                new ScreenManagement
                {
                    Id = 29,
                    ScreenName = "Sub-Contract DC-Out",
                    Display = "PO Wise Sub-Contract DC Outgoing",
                    Topic = "PO WiseSub-Contract DC Outgoing & Auto Close",
                    Description = "When this setting is enabled, Sub-Contract DC Outgoing will be processed PO-wise. After completing the DC Outgoing against a PO, the system will automatically close the respective PO and prevent further transactions.",
                    Required = false
                },
                  new ScreenManagement
                  {
                      Id = 30,
                      ScreenName = "RejectionMaster",
                      Display = "Enable Rejection Reason",
                      Topic = "Rejection Reason Selection",
                      Description = "When enabled, users are required to select a Rejection Reason from the Rejection Master while entering rejection details.",
                      Required = false
                  },
                  new ScreenManagement
                  {
                      Id = 31,
                      ScreenName = "Production Issue WO Component",
                      Display = "PO Wise Production Issue WO Component",
                      Topic = "PO Wise Production Issue WO Component & Auto Close",
                      Description = "When this setting is enabled, Production Issue WO Component will be processed PO-wise. After completing the Production Issue WO Component against a PO, the system will automatically close the respective PO and prevent further transactions.",
                      Required = false
                  },
                   new ScreenManagement
                   {
                       Id = 32,
                       ScreenName = "Production Issue WO Component",
                       Display = "Restrict Out Item Same As In Item",
                       Topic = "Ensure Output Item Matches Input Item in Production Issue WO Component",
                       Description = "When this setting is enabled, the Output Item in the Production Issue WO Component must be the same as the Input Item selected in the transaction. The system will restrict users from selecting a different Output Item. If disabled, users can select any item as the Output Item in the Production Issue GRN Component  process.",
                       Required = false
                   },
                    new ScreenManagement
                    {
                        Id = 33,
                        ScreenName = "Sub-Contract DC-Out",
                        Display = "Restrict Out Item Same As In Item",
                        Topic = "Ensure Output Item Matches Input Item in Sub-Contract DC-Out",
                        Description = "When this setting is enabled, the Output Item in the Sub-Contract DC-GRN must be the same as the Input Item selected in the transaction. The system will restrict users from selecting a different Output Item. If disabled, users can select any item as the Output Item in the Sub-Contract DC GRN process.",
                        Required = false
                    },

                    new ScreenManagement
                    {
                        Id = 34,
                        ScreenName = "Production Issue WO Component",
                        Display = "Labour GRN Link With Production Component",
                        Topic = "Link Labour GRN With Production Issue WO Component",
                        Description = "When this setting is enabled, the system will link Labour GRN transactions with the corresponding Production Issue WO Component records. Users will only be allowed to receive Labour GRN quantities against the linked Production Issue Component, ensuring proper quantity tracking and preventing unrelated Labour GRN entries. If disabled, Labour GRN can be created without enforcing a link to the Production Issue WO Component.",
                        Required = false
                    },

                    new ScreenManagement
                    {
                        Id = 35,
                        ScreenName = "Production Return Component",
                        Display = "Production Log Configuration",
                        Topic = "Sharadha Production Log Customization",
                        Description = "Enables Sharadha-specific Production Log functionality, including customized screen layout, shift selection, vat selection, and other production-related fields as per business requirements.",
                        Required = false
                    },
                    new ScreenManagement
                    {
                        Id = 36,
                        ScreenName = "Route Card",
                        Display = "Route Card Sub Process Enable",
                        Topic = "Same Sequence Process Flow",
                        Description = "When enabled, multiple processes can be assigned to the same sequence in a Route Card. The quantity is tracked across all processes within the same sequence, and the next process sequence becomes available based on the quantity completed by the required same-sequence processes.",
                        Required = false
                    }

          );


            builder.Entity<Category>().HasData(
                new Category { CategoryCode = 1, CategoryName = "RAW MATERIAL", CategoryDesc = "Basic unprocessed materials used in the manufacturing of products.", IsSystemDefined = true },
                new Category { CategoryCode = 2, CategoryName = "COMPONENT", CategoryDesc = "Individual parts or elements that make up a final product or assembly.", IsSystemDefined = true },
                new Category { CategoryCode = 3, CategoryName = "ASSEMBLY", CategoryDesc = "A group of components put together to form a complete unit or system.", IsSystemDefined = true },
                new Category { CategoryCode = 4, CategoryName = "CUTTING TOOLS", CategoryDesc = "Tools used to remove material from a workpiece through cutting operations.", IsSystemDefined = true },
                new Category { CategoryCode = 5, CategoryName = "GAUGES & INSTRUMENTS", CategoryDesc = "Devices used for measuring, inspecting, or testing during production.", IsSystemDefined = true },
                new Category { CategoryCode = 6, CategoryName = "FIXTURES", CategoryDesc = "Custom supports or holders used to securely position workpieces during manufacturing.", IsSystemDefined = true },
                new Category { CategoryCode = 7, CategoryName = "SUB-ASSEMBLY", CategoryDesc = "An assembled unit that is part of a larger final assembly.", IsSystemDefined = true },
                new Category { CategoryCode = 8, CategoryName = "SEMI-FINISHED GOODS", CategoryDesc = "Partially completed products that require further processing before becoming final goods.", IsSystemDefined = true },
                new Category { CategoryCode = 9, CategoryName = "BOUGHTOUT ITEMS", CategoryDesc = "Standard items purchased from vendors and used directly in production or assembly.", IsSystemDefined = true },
                new Category { CategoryCode = 10, CategoryName = "OIL AND LUBRICANTS", CategoryDesc = "Fluids used to reduce friction and wear between surfaces in contact.", IsSystemDefined = true },
                new Category { CategoryCode = 11, CategoryName = "FINISHED GOODS", CategoryDesc = "Completed products ready for sale or delivery.", IsSystemDefined = true },
                new Category { CategoryCode = 12, CategoryName = "INSERTS", CategoryDesc = "Replaceable cutting edges or tips used in machining operations.", IsSystemDefined = true },
                new Category { CategoryCode = 13, CategoryName = "CAPITAL GOODS", CategoryDesc = "Physical assets used in production, such as machinery and equipment.", IsSystemDefined = true },
                new Category { CategoryCode = 14, CategoryName = "FASTENERS", CategoryDesc = "Mechanical components like screws, bolts, and nuts used to join objects together.", IsSystemDefined = true },
                new Category { CategoryCode = 15, CategoryName = "STD ITEM", CategoryDesc = "Standardized items used commonly across different processes or assemblies.", IsSystemDefined = true },
                new Category { CategoryCode = 16, CategoryName = "HARDWARE", CategoryDesc = "General tools and equipment used in manufacturing or maintenance.", IsSystemDefined = true }
            );


            builder.Entity<Store>().ToTable("Stores");
            builder.Entity<Store>().HasData(
            new Store { StoreId = 1, StoreName = "MAIN STORE", StoreDesc = "Central store for all incoming and outgoing materials.", CanIssue = true, CanAdd = true, IsSystemDefined = true },
            new Store { StoreId = 2, StoreName = "PURCHASE STORE", StoreDesc = "Stores all materials received directly from vendors.", CanIssue = true, CanAdd = true, IsSystemDefined = true },
            new Store { StoreId = 3, StoreName = "SUB-CONTRACT STORE", StoreDesc = "Used for storing items related to subcontract operations.", CanIssue = true, CanAdd = true, IsSystemDefined = true },
            new Store { StoreId = 4, StoreName = "LABOUR STORE", StoreDesc = "Used for managing labour-related tools and materials.", CanIssue = true, CanAdd = true, IsSystemDefined = true },
            new Store { StoreId = 5, StoreName = "PRODUCTION STORE", StoreDesc = "Holds raw materials and components for internal production use.", CanIssue = true, CanAdd = true, IsSystemDefined = true },
            new Store { StoreId = 6, StoreName = "REJECTION STORE", StoreDesc = "Stores all rejected or defective materials after quality checks.", CanIssue = true, CanAdd = true, IsSystemDefined = true },
            new Store { StoreId = 7, StoreName = "REWORK STORE", StoreDesc = "Stores items that need rework or correction before reuse.", CanIssue = true, CanAdd = true, IsSystemDefined = true },
            new Store { StoreId = 8, StoreName = "FINISH GOODS (FG) STORE", StoreDesc = "Stores finished products ready for dispatch or delivery.", CanIssue = true, CanAdd = true, IsSystemDefined = true },
            new Store { StoreId = 9, StoreName = "ROUTECARD STORE", StoreDesc = "Handles items tracked and moved via route cards in production.", CanIssue = true, CanAdd = true, IsSystemDefined = true }
            );


            builder.Entity<UOM>().ToTable("UOM");
            builder.Entity<UOM>().HasData(
                new UOM { UnitCode = "BAG", UnitDescription = "BAGS", IsSystemDefined = true },
                new UOM { UnitCode = "BAL", UnitDescription = "BALE", IsSystemDefined = true },
                new UOM { UnitCode = "BDL", UnitDescription = "BUNDLES", IsSystemDefined = true },
                new UOM { UnitCode = "BKL", UnitDescription = "BUCKLES", IsSystemDefined = true },
                new UOM { UnitCode = "BOU", UnitDescription = "BILLION OF UNITS", IsSystemDefined = true },
                new UOM { UnitCode = "BOX", UnitDescription = "BOX", IsSystemDefined = true },
                new UOM { UnitCode = "BTL", UnitDescription = "BOTTLES", IsSystemDefined = true },
                new UOM { UnitCode = "BUN", UnitDescription = "BUNCHES", IsSystemDefined = true },
                new UOM { UnitCode = "CAN", UnitDescription = "CANS", IsSystemDefined = true },
                new UOM { UnitCode = "CCM", UnitDescription = "CUBIC CENTIMETERS", IsSystemDefined = true },
                new UOM { UnitCode = "CMS", UnitDescription = "CENTIMETERS", IsSystemDefined = true },
                new UOM { UnitCode = "CBM", UnitDescription = "CUBIC METERS", IsSystemDefined = true },
                new UOM { UnitCode = "CTN", UnitDescription = "CARTONS", IsSystemDefined = true },
                new UOM { UnitCode = "DOZ", UnitDescription = "DOZENS", IsSystemDefined = true },
                new UOM { UnitCode = "DRM", UnitDescription = "DRUMS", IsSystemDefined = true },
                new UOM { UnitCode = "GGK", UnitDescription = "GREAT GROSS", IsSystemDefined = true },
                new UOM { UnitCode = "GMS", UnitDescription = "GRAMMES", IsSystemDefined = true },
                new UOM { UnitCode = "GRS", UnitDescription = "GROSS", IsSystemDefined = true },
                new UOM { UnitCode = "GYD", UnitDescription = "GROSS YARDS", IsSystemDefined = true },
                new UOM { UnitCode = "KGS", UnitDescription = "KILOGRAMS", IsSystemDefined = true },
                new UOM { UnitCode = "KLR", UnitDescription = "KILOLITRE", IsSystemDefined = true },
                new UOM { UnitCode = "KME", UnitDescription = "KILOMETRE", IsSystemDefined = true },
                new UOM { UnitCode = "LTR", UnitDescription = "LITRES", IsSystemDefined = true },
                new UOM { UnitCode = "MLS", UnitDescription = "MILLI LITRES", IsSystemDefined = true },
                new UOM { UnitCode = "MLT", UnitDescription = "MILILITRE", IsSystemDefined = true },
                new UOM { UnitCode = "MTR", UnitDescription = "METERS", IsSystemDefined = true },
                new UOM { UnitCode = "MTS", UnitDescription = "METRIC TON", IsSystemDefined = true },
                new UOM { UnitCode = "NOS", UnitDescription = "NUMBERS", IsSystemDefined = true },
                new UOM { UnitCode = "OTH", UnitDescription = "OTHERS", IsSystemDefined = true },
                new UOM { UnitCode = "PAC", UnitDescription = "PACKS", IsSystemDefined = true },
                new UOM { UnitCode = "PCS", UnitDescription = "PIECES", IsSystemDefined = true },
                new UOM { UnitCode = "PRS", UnitDescription = "PAIRS", IsSystemDefined = true },
                new UOM { UnitCode = "QTL", UnitDescription = "QUINTAL", IsSystemDefined = true },
                new UOM { UnitCode = "ROL", UnitDescription = "ROLLS", IsSystemDefined = true },
                new UOM { UnitCode = "SET", UnitDescription = "SETS", IsSystemDefined = true },
                new UOM { UnitCode = "SQF", UnitDescription = "SQUARE FEET", IsSystemDefined = true },
                new UOM { UnitCode = "SQM", UnitDescription = "SQUARE METERS", IsSystemDefined = true },
                new UOM { UnitCode = "SQY", UnitDescription = "SQUARE YARDS", IsSystemDefined = true },
                new UOM { UnitCode = "TBS", UnitDescription = "TABLETS", IsSystemDefined = true },
                new UOM { UnitCode = "TGM", UnitDescription = "TEN GROSS", IsSystemDefined = true },
                new UOM { UnitCode = "THD", UnitDescription = "THOUSANDS", IsSystemDefined = true },
                new UOM { UnitCode = "TON", UnitDescription = "TONNES", IsSystemDefined = true },
                new UOM { UnitCode = "TUB", UnitDescription = "TUBES", IsSystemDefined = true },
                new UOM { UnitCode = "UGS", UnitDescription = "US GALLONS", IsSystemDefined = true },
                new UOM { UnitCode = "UNT", UnitDescription = "UNITS", IsSystemDefined = true },
                new UOM { UnitCode = "YDS", UnitDescription = "YARDS", IsSystemDefined = true },
                new UOM { UnitCode = "MM", UnitDescription = "Mili Meter", IsSystemDefined = true },
                new UOM { UnitCode = "HRS", UnitDescription = "HOURS", IsSystemDefined = true },
                new UOM { UnitCode = "SCM", UnitDescription = "Square Centimeter", IsSystemDefined = true }
            );


            builder.Entity<State>().ToTable("State");
            builder.Entity<State>().HasData(
                new State { StateCode = 1, StateName = "JAMMU AND KASHMIR", IsSystemDefined = true },
                new State { StateCode = 2, StateName = "HIMACHAL PRADESH", IsSystemDefined = true },
                new State { StateCode = 3, StateName = "PUNJAB", IsSystemDefined = true },
                new State { StateCode = 4, StateName = "CHANDIGARH", IsSystemDefined = true },
                new State { StateCode = 5, StateName = "UTTARAKHAND", IsSystemDefined = true },
                new State { StateCode = 6, StateName = "HARYANA", IsSystemDefined = true },
                new State { StateCode = 7, StateName = "DELHI", IsSystemDefined = true },
                new State { StateCode = 8, StateName = "RAJASTHAN", IsSystemDefined = true },
                new State { StateCode = 9, StateName = "UTTAR PRADESH", IsSystemDefined = true },
                new State { StateCode = 10, StateName = "BIHAR", IsSystemDefined = true },
                new State { StateCode = 11, StateName = "SIKKIM", IsSystemDefined = true },
                new State { StateCode = 12, StateName = "ARUNACHAL PRADESH", IsSystemDefined = true },
                new State { StateCode = 13, StateName = "NAGALAND", IsSystemDefined = true },
                new State { StateCode = 14, StateName = "MANIPUR", IsSystemDefined = true },
                new State { StateCode = 15, StateName = "MIZORAM", IsSystemDefined = true },
                new State { StateCode = 16, StateName = "TRIPURA", IsSystemDefined = true },
                new State { StateCode = 17, StateName = "MEGHALAYA", IsSystemDefined = true },
                new State { StateCode = 18, StateName = "ASSAM", IsSystemDefined = true },
                new State { StateCode = 19, StateName = "WEST BENGAL", IsSystemDefined = true },
                new State { StateCode = 20, StateName = "JHARKHAND", IsSystemDefined = true },
                new State { StateCode = 21, StateName = "ODISHA", IsSystemDefined = true },
                new State { StateCode = 22, StateName = "CHHATTISGARH", IsSystemDefined = true },
                new State { StateCode = 23, StateName = "MADHYA PRADESH", IsSystemDefined = true },
                new State { StateCode = 24, StateName = "GUJARAT", IsSystemDefined = true },
                new State { StateCode = 25, StateName = "DAMAN AND DIU", IsSystemDefined = true },
                new State { StateCode = 26, StateName = "DADRA AND NAGAR HAVELI", IsSystemDefined = true },
                new State { StateCode = 27, StateName = "MAHARASHTRA", IsSystemDefined = true },
                new State { StateCode = 29, StateName = "KARNATAKA", IsSystemDefined = true },
                new State { StateCode = 30, StateName = "GOA", IsSystemDefined = true },
                new State { StateCode = 31, StateName = "LAKSHADWEEP", IsSystemDefined = true },
                new State { StateCode = 32, StateName = "KERALA", IsSystemDefined = true },
                new State { StateCode = 33, StateName = "TAMIL NADU", IsSystemDefined = true },
                new State { StateCode = 34, StateName = "PUDUCHERRY", IsSystemDefined = true },
                new State { StateCode = 35, StateName = "ANDAMAN AND NICOBAR", IsSystemDefined = true },
                new State { StateCode = 36, StateName = "TELANGANA", IsSystemDefined = true },
                new State { StateCode = 37, StateName = "ANDHRA PRADESH", IsSystemDefined = true },
                new State { StateCode = 38, StateName = "LADAKH", IsSystemDefined = true },
                new State { StateCode = 96, StateName = "OTHER COUNTRIES", IsSystemDefined = true },
                new State { StateCode = 97, StateName = "Other Territory", IsSystemDefined = true },
                new State { StateCode = 99, StateName = "OTHER COUNTRIES", IsSystemDefined = true }
            );


            builder.Entity<Currency>().ToTable("Currency");
            builder.Entity<Currency>().HasData(
                new Currency { CurrId = 1, CurrName = "Rupees", CurrSub = "Paise", Symbol = "₹", IsSystemDefined = true },
                new Currency { CurrId = 2, CurrName = "US Dollar", CurrSub = "Cent", Symbol = "$", IsSystemDefined = true },
                new Currency { CurrId = 3, CurrName = "Euro", CurrSub = "Cent", Symbol = "€", IsSystemDefined = true }
            );

            // For Store MapFor all Screens
            builder.Entity<StoreMap>().HasData(StoreMapSeeder.GetDefaultStoreMaps());

            // For Defalt Shifts Register
            ShiftAllocationSeed.Seed(builder);
            ProductionLogSettingSeeder.Seed(builder);

        }

    }
}
