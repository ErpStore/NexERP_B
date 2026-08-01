
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IPlanningService;
using V.SMART.Shared.BusinessLayer.BusinessService.ProductionService;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.GeneralSettingsMaster;
using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.Data.Planning;
using V.SMART.Shared.Data.Utilities;
using V.SMART.Shared.Repository.AccountsRepository;
using V.SMART.Shared.Repository.CashFlowRepository.ServiceBillsRepository;
using V.SMART.Shared.Repository.DcRunningNoRep;
using V.SMART.Shared.Repository.HumanResourceRepository.AppointmentLetterRepository;
using V.SMART.Shared.Repository.HumanResourceRepository.AttendanceRepository;
using V.SMART.Shared.Repository.HumanResourceRepository.EmployeeLoanRepository;
using V.SMART.Shared.Repository.HumanResourceRepository.OfferLetterRepository;
using V.SMART.Shared.Repository.HumanResourceRepository.PayrollRepository;
using V.SMART.Shared.Repository.InspectionRepository.FinalInspectionRepository;
using V.SMART.Shared.Repository.InspectionRepository.IncominInspectionRepository;
using V.SMART.Shared.Repository.InspectionRepository.MasterInspectionRepository;
using V.SMART.Shared.Repository.InventoryStockRepository;
using V.SMART.Shared.Repository.InventoryStockRepository.MaterialIssueNoteRepository;
using V.SMART.Shared.Repository.InventoryStockRepository.StockAdditionRepository;
using V.SMART.Shared.Repository.InventoryStockRepository.StockRequestIssueRepository;
using V.SMART.Shared.Repository.InventoryStockRepository.StoreInterTransferRepository;
using V.SMART.Shared.Repository.InventoryStockRepository.TooCribIssueRepo;
using V.SMART.Shared.Repository.InventoryStockRepository.ToolCribReturnRepo;
using V.SMART.Shared.Repository.InvoiceAutoRunnNo;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Repository.IRepository.IAccountRepository.IPaymentsRepository;
using V.SMART.Shared.Repository.IRepository.ICashFlowRepository.IServiceBillsRepository;
using V.SMART.Shared.Repository.IRepository.IDcRunninigNo;
using V.SMART.Shared.Repository.IRepository.IHumanResourceRepository.IAppointmentLetterRepository;
using V.SMART.Shared.Repository.IRepository.IHumanResourceRepository.IAttendanceRepository;
using V.SMART.Shared.Repository.IRepository.IHumanResourceRepository.IEmployeeRepository;
using V.SMART.Shared.Repository.IRepository.IHumanResourceRepository.IOfferLetterRepository;
using V.SMART.Shared.Repository.IRepository.IHumanResourceRepository.IPayrollRepository;
using V.SMART.Shared.Repository.IRepository.IInspectionRepository.IFinalInspection;
using V.SMART.Shared.Repository.IRepository.IInspectionRepository.IIncomingInspectionRepository;
using V.SMART.Shared.Repository.IRepository.IInspectionRepository.IMasterInspection;
using V.SMART.Shared.Repository.IRepository.IInventoryStockRepository;
using V.SMART.Shared.Repository.IRepository.IInventoryStockRepository.IMaterialIssueNoteRepository;
using V.SMART.Shared.Repository.IRepository.IInventoryStockRepository.IStockAdditionRepository;
using V.SMART.Shared.Repository.IRepository.IInventoryStockRepository.IStockRequestIssueRepository;
using V.SMART.Shared.Repository.IRepository.IInventoryStockRepository.IStoreInterTransferRepository;
using V.SMART.Shared.Repository.IRepository.IInventoryStockRepository.IToolCribIssueRepo;
using V.SMART.Shared.Repository.IRepository.IInventoryStockRepository.IToolCribReturnRepo;
using V.SMART.Shared.Repository.IRepository.IinvoiceRunningNo;
using V.SMART.Shared.Repository.IRepository.IMaintenanceRepository.IBreakdownMaintenance;
using V.SMART.Shared.Repository.IRepository.IMaintenanceRepository.ICalibrationHistoryAndMaintenance;
using V.SMART.Shared.Repository.IRepository.IMaintenanceRepository.IMaintenanceProcess;
using V.SMART.Shared.Repository.IRepository.IMaintenanceRepository.IMaintenanceSchedule;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IAccounts;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IAccountsRepository;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IAdminRepository;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IAdmins;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.ICompany;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IGeneralRepository;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IGenerals;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IHumanResource_Master;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IHumanResourceMaster;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IItemRepository;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IItems;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IMasterSettings;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.ITermsAndConditions;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IDebitNote_Repository;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IMaterialRequisitionRepo;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IPurchaseGRN_Repository;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IPurchaseInvoice_Repository;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IPurchaseSCN_Repository;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IPurchOrSubConPoRepository;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IPurchOrSubConRepository;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.ISubConINVoiceRepository;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.ISubContractDCOutRepository;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.ISubContractGRNRepository;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.ISubContractSCNRepository;
using V.SMART.Shared.Repository.IRepository.IPlanningRepository;
using V.SMART.Shared.Repository.IRepository.IPlanningRepository.IEstimatationRepository;
using V.SMART.Shared.Repository.IRepository.IPlanningRepository.IJobOrderRepo;
using V.SMART.Shared.Repository.IRepository.IPlanningRepository.IRouteCardRepo;
using V.SMART.Shared.Repository.IRepository.IProductionRepository.IProductionCompRepo;
using V.SMART.Shared.Repository.IRepository.IProductionRepository.IProductionIssueWOAssyRepo;
using V.SMART.Shared.Repository.IRepository.IProductionRepository.IProductionLogRepo;
using V.SMART.Shared.Repository.IRepository.IProductionRepository.IProductionReturnAssyRepo;
using V.SMART.Shared.Repository.IRepository.IProductionRepository.IProductionSCNAssyRepo;
using V.SMART.Shared.Repository.IRepository.IPurchaseAndSubConRepository.IPurchQuote;
using V.SMART.Shared.Repository.IRepository.IReportRepository.ITrackReportRepository;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.IContractReviewRepository;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ICreditNote_Repository;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.IExportInvoiceRepository;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ILabourDcOutgoing_Repository;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ILabourGRN_Repository;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ILabourSCN_Repository;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.IMfgInvoice;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.IMfgLeads;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.IMfgQuotation;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.IPerformaInvoiceRepository;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ISalesDCRepoditory;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ISalesEnquiry;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ISalesFesiblityRepoSitory;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ISalesPoRepository;
using V.SMART.Shared.Repository.IRepository.ISettingsRepository;
using V.SMART.Shared.Repository.IRepository.IUtilitiesRepository;
using V.SMART.Shared.Repository.MaintenanceRepository.BreakdownMaintenanceRepository;
using V.SMART.Shared.Repository.MaintenanceRepository.CalibrationHistoryAndMaintenanceRepository;
using V.SMART.Shared.Repository.MaintenanceRepository.MaintenanceProcessRepository;
using V.SMART.Shared.Repository.MaintenanceRepository.MaintenanceScheduleRepository;
using V.SMART.Shared.Repository.MasterRepository.Accounts;
using V.SMART.Shared.Repository.MasterRepository.AdminRepository;
using V.SMART.Shared.Repository.MasterRepository.Admins;
using V.SMART.Shared.Repository.MasterRepository.Company;
using V.SMART.Shared.Repository.MasterRepository.ExpenseRepository;
using V.SMART.Shared.Repository.MasterRepository.GeneralRepository;
using V.SMART.Shared.Repository.MasterRepository.Generals;
using V.SMART.Shared.Repository.MasterRepository.HumanResourceMaster;
using V.SMART.Shared.Repository.MasterRepository.Items;
using V.SMART.Shared.Repository.MasterRepository.ItemsRepository;
using V.SMART.Shared.Repository.MasterRepository.MasterSettings;
using V.SMART.Shared.Repository.MasterRepository.TermsAndCondition;
using V.SMART.Shared.Repository.OutSourcingRepository.DebitNote_Repository;
using V.SMART.Shared.Repository.OutSourcingRepository.MaterialRequsiationRepo;
using V.SMART.Shared.Repository.OutSourcingRepository.PurchaseGRN_Repository;
using V.SMART.Shared.Repository.OutSourcingRepository.PurchaseInvoice_Repository;
using V.SMART.Shared.Repository.OutSourcingRepository.PurchaseSCN_Repository;
using V.SMART.Shared.Repository.OutSourcingRepository.PurchOrSubConEnquiryRepository;
using V.SMART.Shared.Repository.OutSourcingRepository.PurchOrSubConPORepository;
using V.SMART.Shared.Repository.OutSourcingRepository.SubContractDcOutRepository;
using V.SMART.Shared.Repository.OutSourcingRepository.SubContractGRNRepository;
using V.SMART.Shared.Repository.OutSourcingRepository.SubContractInvoiceRepository;
using V.SMART.Shared.Repository.OutSourcingRepository.SubContractSCNRepository;
using V.SMART.Shared.Repository.PlanningRepository;
using V.SMART.Shared.Repository.PlanningRepository.EstimationRepository;
using V.SMART.Shared.Repository.PlanningRepository.JobOrderRepo;
using V.SMART.Shared.Repository.PlanningRepository.RouteCardRepo;
using V.SMART.Shared.Repository.ProductionRepository.ProductionIssueWOAssyRepo;
using V.SMART.Shared.Repository.ProductionRepository.ProductionLogRepo;
using V.SMART.Shared.Repository.ProductionRepository.ProductionReturnAssyRepo;
using V.SMART.Shared.Repository.ProductionRepository.ProductionSCNAssyRepo;
using V.SMART.Shared.Repository.ProductionRepository.ProuctionCompRepo;
using V.SMART.Shared.Repository.PurchaseAndSubConRepository.PurchaseQuotation;
using V.SMART.Shared.Repository.ReportRepository.TrackReportRepository;
using V.SMART.Shared.Repository.SalesAndLabourRepository;
using V.SMART.Shared.Repository.SalesAndLabourRepository.ContractReviewRepository;
using V.SMART.Shared.Repository.SalesAndLabourRepository.CreditNote_Repository;
using V.SMART.Shared.Repository.SalesAndLabourRepository.ExportInvoiceRepository;
using V.SMART.Shared.Repository.SalesAndLabourRepository.LabourDcOutgoing_Repository;
using V.SMART.Shared.Repository.SalesAndLabourRepository.LabourGRN_Repository;
using V.SMART.Shared.Repository.SalesAndLabourRepository.LabourSCN_Repository;
using V.SMART.Shared.Repository.SalesAndLabourRepository.MfgInvoice;
using V.SMART.Shared.Repository.SalesAndLabourRepository.MfgLeads;
using V.SMART.Shared.Repository.SalesAndLabourRepository.MfgQuotation;
using V.SMART.Shared.Repository.SalesAndLabourRepository.PerformaInvoiceRepository;
using V.SMART.Shared.Repository.SalesAndLabourRepository.SalesDCRepository;
using V.SMART.Shared.Repository.SalesAndLabourRepository.SalesEnquiry;
using V.SMART.Shared.Repository.SalesAndLabourRepository.SalesFesibilityRepository;
using V.SMART.Shared.Repository.SalesAndLabourRepository.SalesPoRepository;
using V.SMART.Shared.Repository.SettingsRepository;
using V.SMART.Shared.Repository.UtilitiesRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.Services.MultiCompanyService;
using V.SMARTV.SMART.Shared.Repository.CashFlowRepository.ServiceBillsRepository;

namespace V.SMART.Shared.Repository
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext _db;

        //Screen management
        public IUserThemePrefRepository UserThemePreferences { get; private set; }
        public IUserPreferenceRepository UserPreferences { get; private set; }
        public IPrintSettingRepository PrintSettings { get; private set; }
        public IScreenRepository Screens { get; private set; }
        public IScreenManagementRepository ScreenManagements { get; private set; }
        public ITermsAndConditionsRepository TermsAndConditions { get; private set; }
        public IStoreMapRepository StoreMaps { get; private set; }
        public IProductionLogSettingRepository ProductionLogSettings { get; private set; }
        public IInspectionSettingsRepository InspectSettings { get; private set; }

        public IHRMasterSettingRepository HRMasterSettings { get; private set; }
        public IBiometricExcelSettingRepository BiometricExcelSettings { get; private set; }
        public ISalaryHeadPrintSettingRepository SalaryHeadPrintSettings { get; private set; }


        

        //Correspondence
        public ICorrespondanceRepository Correspondances { get; private set; }
        

        //users
        public IUserRepository Users { get; private set; }
        public IUserRightsRepository UserRights { get; private set; }
        public IUserAuthorityRepository UserAuthorities { get; private set; }


        //Inventory
        public ICategoryRepository Categories { get; private set; }
        public IUOMRepository UOMs { get; private set; }
        public IStoreRepository Stores { get; private set; }
        public IRawMaterialRepository RawMaterial { get; private set; }
        public IFactorRepository Factors { get; private set; }
        public IProcessRepository Processes { get; private set; }
        public IMachineRepository Machines { get; private set; }
        public IGroupingRepository Groupings { get; private set; }
        public IGroupingSubRepository GroupingSubs { get; private set; }
        public IItemRepository ItemRepositories { get; private set; }
        public IHSNMasterRepository HSNMasters { get; private set; }
        public IItemProductAssignRepository ItemProductAssigns { get; private set; }
        public IAssemblyRepository AssmblyDefs { get; private set; }
        public IAssemblyModifyRepository AssemblyModifies { get; private set; }
        public ICompMasterRepository CompMasters { get; private set; }
        public IItemSubRepository ItemSubs { get; private set; }
        public IProcessFlowChartRepository ProcessFlowCharts { get; private set; }
        public IItemCustomerAssignRepository ItemCustomerAssigns { get; private set; }

        public IAssemblyDefLabourRepository AssmblyDefLabs { get; private set; }

        // General
        public IStateRepository States { get; private set; }
        public ICustomerRepository Customers { get; private set; }
        public ICustomerIndirectRepository CustomerIndirects { get; private set; }
        public IContactPersonRepository ContactPersons { get; private set; }
        public IVendorRepository Vendors { get; private set; }
        public IVendorContactRepository VendorContacts { get; private set; }
        public IVendorInDirectRepository VendorInDirects { get; private set; }

        //Accounts
        public IExpenseRepository Expenses { get; private set; }
        public ICurrencyRepository Currencyis { get; private set; }
        public IIncomeRepository Incomes { get; private set; }
        public ICurrencyTodayRepository CurrencyTodays { get; private set; }
        public IBanksRepository Banks { get; private set; }
        public IProjectTypeMasterRepository ProjectTypeMasters { get; private set; }
        public ICostCenterRepository CostCenters { get; private set; }

        //Human Resource
        public IStaffRepository Staffs { get; private set; }
        public IStaffFamilyDetailRepository StaffFamilyDetails { get; private set; }
        public IStaffEducationRepository StaffEducations { get; private set; }
        public IStaffEmergencyRepository StaffEmergencys { get; private set; }
        public ILeaveTypeRepository LeaveTypes { get; private set; }
        public IEmployeeLeaveBalanceRepository EmployeeLeaveBalances { get; private set; }
        public ILeaveApplicationRepository LeaveApplications { get; private set; }

        public IDailyLeaveRecordRepository DailyLeaveRecords { get; private set; }
        public IShiftAllocationRepository ShiftAllocations { get; private set; }
        public IHolidayListRepository HolidayLists { get; private set; }

        public ICandidateRepository Candidate { get; private set; }
       

        //Company
        public ICompanyRepository Companies { get; private set; }

        //Manufacturing
        public ILeadsRepository Leads { get; private set; }

        public IEnquirySalesRepository EnquirySaless { get; private set; }
        public IEnquirySalesSubRepository EnquirySalesSubs { get; private set; }

        public IEnquiryFeasibilityRepository EnquiryFeasibilities { get; private set; }
        public IEnquiryFeasibilitySubRepository EnquiryFeasibilitySubs { get; private set; }

        public IMfgQuoteRepository MfgQuotes { get; private set; }
        public IMfgQuoteSubRepository MfgQuoteSubs { get; private set; }

        public IMfgPoTypeRepository PoTypes { get; private set; }
        public IMfgPoRepository MfgPos { get; private set; }
        public IMfgPoSubRepository MfgPoSubs { get; private set; }
        public IAssyPOCompTrackerRepository AssyPOCompTrackers { get; private set; }


        public IContractReviewRepository ContractReviews { get; private set; }
        public IContractReviewSubRepository ContractReviewSubs { get; private set; }
        public IContractReviewMasterRepository ContractReviewMasters { get; private set; }


        public IPerformaInvRepository PerformaInvs { get; private set; }
        public IPerformaInvSubRepository PerformaInvSubs { get; private set; }

        public IMfgDcRepository MfgDcs { get; private set; }
        public IMfgDcSubRepository MfgDcSubs { get; private set; }


        public IMfgInvRepository MfgInvs { get; private set; }
        public IMfgInvSubRepository MfgInvSubs  { get; private set; }

        public IExpInvRepository ExpInvs { get; private set; }
        public IExpInvSubRepository ExpInvSubs { get; private set; }



        //------------Labour------------------------------------
        public ILabourGRNRepository LabourGRNs { get; private set; }
        public ILabourGRNSubRepository LabourGRNSubs { get; private set; }

        public ILabourSCNRepository LabourSCNs { get; private set; }
        public ILabourSCNSubRepository LabourSCNSubs { get; private set; }

        public ILabourDcOutgoingRepository LabourDcOutgoings { get; private set; }
        public ILabourDcOutgoingSubRepository LabourDcOutgoingSubs { get; private set; }

        public ILabourDcReturnCompTrackRepository LabourDcReturnCompTracks { get; private set; }

        public ILabInvRepository LabInvs { get; private set; }
        public ILabInvSubRepository LabInvSubs { get; private set; }

        //CreditNote--------------------
        public ICreditNoteRepository CreditNotes { get; private set; }
        public ICreditNoteSubRepository CreditNoteSubs { get; private set; }



        //Out-Sourcing
        public IMaterialReqRepository MaterialReqs { get; private set; }
        public IMaterialReqSubRepository MaterialReqSubs { get; private set; }

        public IEnquiryPurchaseRepository EnquiryPurchases { get; private set; }
        public IEnquiryPurchaseSubRepository EnquiryPurchaseSubs { get; private set; }
        public IPurchaseQuoteRepository PurchaseQuotes { get; private set; }
        public IPurchaseQuoteSubRepository PurchaseQuotesSubs { get; private set; }
        public IEnquiryPurchaseVendorAssignRepository EnquiryPurchaseVendorAssigns { get; private set; }

        public IPurchPoRepository PurchPos { get; private set; }
        public IPurchPoSubRepository PurchPoSubs { get; private set; }

        public IPurchaseGRNRepository PurchaseGRNs { get; private set; }
        public IPurchaseGRNSubRepository PurchaseGRNSubs { get; private set; }

        public IPurchaseSCNRepository PurchaseSCNs { get; private set; }
        public IPurchaseSCNSubRepository PurchaseSCNSubs { get; private set; }

        public IPurchaseInvoiceRepository PurchaseInvoices { get; private set; }
        public IPurchaseInvoiceSubRepository PurchaseInvoiceSubs { get; private set; }

        public ISubConDCOutRepository SubConDCOuts { get; private set; }
        public ISubConDCOutSubRepository SubConDCOutSubs { get; private set; }

        public ISubConGRNRepository SubConGRNs { get; private set; }
        public ISubConGRNSubRepository SubConGRNSubs { get; private set; }
        public ISubConGRNTrackRepository SubConGRNTracks { get; private set; }

        public ISubConSCNRepository SubConSCNs { get; private set; }
        public ISubConSCNSubRepository SubConSCNSubs { get; private set; }

        public ISubConInvRepository SubConInvs { get; private set; }
        public ISubConInvSubRepository SubConInvSubs { get; private set; }

        public IDebitNoteRepository DebitNotes { get; private set; }
        public IDebitNoteSubRepository DebitNoteSubs { get; private set; }


        //Inventory - Stock 
        public ISCNGenRepository SCNGens { get; private set; }
        public ISCNGenSubRepository SCNGenSubs { get; private set; }

        public IMaterialIssueNoteRepository MINs { get; private set; }
        public IMaterialIssueNoteSubRepository MINSubs { get; private set; }
        public IStockAddRepository StockAdds { get; private set; }
        public IStockIssueRepository StockIssues { get; private set; }
        public IStockIssueTrackRepository StockIssueTracks { get; private set; }

        public IToolCribIssueRepository ToolCribIssues { get; private set; }
        public IToolCribIssueSubRepository ToolCribIssueSubs { get; private set; }
        public IToolCribReturnRepository ToolCribReturns { get; private set; }
        public IToolCribReturnSubRepository ToolCribReturnSubs { get; private set; }

        public IStoreInterTransRepository StoreInterTrans { get; private set; }
        public IStoreInterTransSubRepository StoreInterTransSubs { get; private set; }


        //Planning 
        public IApprovalHistoryRepository ApprovalHistories { get; private set; }

        public IJobOrderRepository JobOrders { get; private set; }
        public IJobOrderSubRepository JobOrderSubs { get; private set; }

        public IRcReleaseRepository RcReleases{ get; private set; }
        public IRcReleaseSubRepository RcReleaseSubs { get; private set; }

        public IRouteCardRepository RouteCards { get; private set; }
        public IRouteCardSubRepository RouteCardSubs { get; private set; }

        public IEstimateRepository Estimates { get; private set; }//Estimation
        public IEstimateSubRepository EstimateSubs { get; private set; }

        //Production
        public IProductionIssueAssyRepository ProductionIssueAssys { get; private set; }
        public IProductionIssueAssySubRepository ProductionIssueAssySubs { get; private set; }

        public IProductionReturnAssyRepository ProductionReturnAssys { get; private set; }
        public IProductionReturnAssySubRepository ProductionReturnAssySubs { get; private set; }
        public IProductionReturnAssyTrackRepository ProductionReturnAssyTracks { get; private set; }

        public IProductionSCNAssyRepository ProductionSCNAssys { get; private set; }
        public IProductionSCNAssySubRepository ProductionSCNAssySubs { get; private set; }

        public IProductionLogRepository ProductionLogs { get; private set; }
        public IProductionLogSubRepository ProductionLogSubs { get; private set; }

        public IProductionIssueCompRepository ProductionIssueComps { get; private set; }
        public IProductionIssueCompSubRepository ProductionIssueCompSubs { get; private set; }

        public IProductionReturnCompRepository ProductionReturnComps { get; private set; }
        public IProductionReturnCompSubRepository ProductionReturnCompSubs { get; private set; }
        public IProductionReturnCompTrackRepository ProductionReturnCompTracks { get; private set; }


        public IProductionSCNCompRepository ProductionSCNComps { get; private set; }
        public IProductionSCNCompSubRepository ProductionSCNCompSubs { get; private set; }



        //Maintenance
        public IMaintenanceScheduleRepository MaintenanceSchedules { get; private set; }
        public IMaintenanceProcessRepository MaintenanceProcesses { get; private set; }
        public IBreakdownMaintenanceRepository BreakdownMaintenances { get; private set; }

        public IInstrumentDetailsRepository AllInstrumentDetails { get; private set; }
        public ICalibrationHistoryRepository CalibrationHistories { get; private set; }

        //Inspection
        public IDefectInfoRepository DefectInfos { get; private set; }
        public IMasterInspectionRepository MasterInspections { get; private set; }
        public IInspectionRefRepository InspectionRefs { get; private set; }
        public IFinalInspectionRepository FinalInspections { get; private set; }
        public IFinalInspectionRefRepository FinalInspectionRefs { get; private set; }
        public IIncomingInspectionRepository IncomingInspections { get; private set; }
        public IIncomingInspectionRefRepository IncomingInspectionRefs { get; private set; }


        //-------------------------------------------------------------
        public IDcRunningNumberRepository DcRunningNumbers { get; private set; } 
        public IInvoiceAutoRunningNumberRepository InvoiceAutoRunningNumbers { get; private set; }

        //SNS
        public IAssemblyChargeRepository AssemblyCharges { get; private set; }

        //Paymnets Module
        public IPaymentsRepository Payments { get; private set; }
        public IPaymentsSubRepository PaymentsSubs { get; private set; }
        public IAdvaceadjustmentRepository Advaceadjustments { get; private set; }
        public IAdvaceadjustmentSubRepository AdvaceadjustmentSubs { get; private set; }
        public IFundTransRepository fundTranss { get; private set; }
        public IReceiptsRepository Receiptss { get; private set; }
        public IReceiptsSubRepository ReceiptsSubs { get; private set; }

        public IServiceBillsRepository ServiceBills { get; private set; }
        public IServiceBillsSubRepository ServiceBillsSubs { get; private set; }

        public IRejectionMasterRepository RejectionMasters { get; private set; }
        public IUserColumnPreferenceRepository UserColumnPreferences { get; private set; }


        //Human Resource
        public IAttendanceRepository Attendances { get; private set; }
        public ISalaryRepository Salaries { get; private set; }
        public IStaffLoanRepository StaffLoans { get; private set; }

        public IOfferLetterRepository Offers { get; private set; }
        public IOfferLetterSubRepository OffersSubs { get; private set; }


        public IAppointmentLetterRepository AppointmentLetters { get; private set; }
        public IAppointmentLetterSubRepository AppointmentLetterSubs { get; private set; }


        public IStockRequestIssueRepository StockRequestIssues { get; private set; }
        public IStockRequestIssueSubRepository StockRequestIssueSubs { get; private set; }


        // Reports 
        #region Track Reports

        public ILabourTrackRepository LabourTracks { get; private set; }

        #endregion



        public UnitOfWork(ITenantDbContextFactory dbFactory, IPasswordHasher<User> passwordHasher, ILoggingService loggingService,
                            CurrentUserService currentUserService)
        {
            _db = dbFactory.CreateDbContext();

            //Admins
            Users = new UserRepository(_db, passwordHasher, loggingService, currentUserService);
            UserRights = new UserRightsRepository(_db, loggingService);
            UserAuthorities = new UserAuthorityRepository(_db, loggingService, currentUserService);

            //Inventory
            Categories = new CategoryRepository(_db, loggingService, currentUserService);
            UOMs = new UOMRepository(_db, loggingService);
            States = new StateRepository(_db, loggingService);
            Currencyis = new CurrencyRepository(_db, loggingService, currentUserService);
            Screens = new ScreenRepository(_db, loggingService);
            Stores = new StoreRepository(_db, loggingService, currentUserService);
            RawMaterial = new RawMaterialRepository(_db, loggingService, currentUserService);
            Factors = new FactorRepository(_db, loggingService, currentUserService);
            Processes = new ProcessRepository(_db, loggingService, currentUserService);
            Machines = new MachineRepository(_db, loggingService, currentUserService);
            Groupings = new GroupingRepository(_db, loggingService, currentUserService);
            GroupingSubs = new GroupingSubRepository(_db, loggingService, currentUserService);
            ItemRepositories = new ItemRepository(_db, loggingService, currentUserService);
            ItemCustomerAssigns = new ItemCustomerAssignRepository(_db, loggingService, currentUserService);

            HSNMasters = new HSNMasterRepository(_db, loggingService);
            ItemProductAssigns = new ItemProductAssignRepository(_db, loggingService, currentUserService);
            AssmblyDefs = new AssemblyRepository(_db, loggingService, currentUserService);

            AssemblyModifies = new AssemblyModifyRepository(_db, loggingService, currentUserService);

            CompMasters = new CompMasterRepository(_db, loggingService, currentUserService);
            ItemSubs = new ItemSubRepository(_db, loggingService, currentUserService);
            ProcessFlowCharts = new ProcessFlowChartRepository(_db, loggingService, currentUserService);
            AssmblyDefLabs = new AssemblyDefLabourRepository(_db, loggingService, currentUserService);


            //General
            Customers = new CustomerRepository(_db, currentUserService, loggingService);
            CustomerIndirects = new CustomerIndirectRepository(_db, loggingService, currentUserService);
            ContactPersons = new ContactPersonRepository(_db, loggingService, currentUserService);
            Vendors = new VendorRepository(_db, loggingService, currentUserService);
            VendorContacts = new VendorContactRepository(_db, loggingService, currentUserService);
            VendorInDirects = new VendorInDirectRepository(_db, loggingService, currentUserService);

            //Accounts
            Expenses = new ExpenseRepository(_db, loggingService, currentUserService);
            Incomes = new IncomeRepository(_db, loggingService, currentUserService);
            CurrencyTodays = new CurrencyTodayRepository(_db, loggingService, currentUserService);
            Banks = new BanksRepository(_db, loggingService, currentUserService);
            Companies = new CompanyRepository(_db, loggingService, currentUserService);
            ProjectTypeMasters = new ProjectTypeMasterRepository(_db, loggingService, currentUserService);
            CostCenters = new CostCenterRepository(_db, loggingService, currentUserService);

            //Human Resource
            HolidayLists = new HolidayListRepository(_db, loggingService, currentUserService);
            Staffs = new StaffRepository(_db, loggingService, currentUserService);
            StaffEmergencys = new StaffEmergencyRepository(_db, loggingService, currentUserService);
            StaffFamilyDetails = new StaffFamilyDetailRepository(_db, loggingService, currentUserService);
            StaffEducations = new StaffEducationRepository(_db, loggingService, currentUserService);
            LeaveTypes = new LeaveTypeRepository(_db, loggingService, currentUserService);
            EmployeeLeaveBalances = new EmployeeLeaveBalanceRepository(_db, loggingService, currentUserService);
            LeaveApplications = new LeaveApplicationRepository(_db, loggingService, currentUserService);
            DailyLeaveRecords = new DailyLeaveRecordRepository(_db, loggingService, currentUserService);
            ShiftAllocations = new ShiftAllocationRepository(_db, loggingService, currentUserService);

            //SBM
            Candidate = new CandidateRepository(_db, loggingService, currentUserService);
           

            //Settings 
            UserThemePreferences = new UserThemePrefRepository(_db, loggingService);
            UserPreferences = new UserPreferenceRepository(_db, loggingService);
            ScreenManagements = new ScreenManagementRepository(_db, loggingService);
            TermsAndConditions = new TermsAndConditionsRepository(_db, loggingService, currentUserService);
            StoreMaps = new StoreMapRepository(_db, loggingService, currentUserService);
            PrintSettings = new PrintSettingRepository(_db, loggingService, currentUserService);
            ProductionLogSettings = new ProductionLogSettingsRepository(_db, loggingService, currentUserService);
            InspectSettings = new InspectionSettingsRepository(_db, loggingService, currentUserService);

            HRMasterSettings = new HRMasterSettingRepository(_db, loggingService, currentUserService);
            BiometricExcelSettings = new BiometricExcelSettingRepository(_db, loggingService, currentUserService);
            SalaryHeadPrintSettings = new SalaryHeadPrintSettingRepository(_db, loggingService, currentUserService);

            //Manufacturing
            Leads = new LeadsRepository(_db, loggingService, currentUserService);

            EnquirySaless = new EnquirySalesRepository(_db, loggingService, currentUserService);
            EnquirySalesSubs = new EnquirySalesSubRepository(_db, loggingService, currentUserService);

            EnquiryFeasibilities = new EnquiryFeasibilityRepository(_db, loggingService, currentUserService);
            EnquiryFeasibilitySubs = new EnquiryFeasibilitySubRepository(_db, loggingService, currentUserService);

            MfgQuotes = new MfgQuoteRepository(_db, loggingService, currentUserService);
            MfgQuoteSubs = new MfgQuoteSubRepository(_db, loggingService, currentUserService);

            PoTypes = new MfgPoTypeRepository(_db, loggingService, currentUserService);
            MfgPos = new MfgPoRepository(_db, loggingService, currentUserService);
            MfgPoSubs = new MfgPoSubRepository(_db, loggingService, currentUserService);
            AssyPOCompTrackers = new AssyPOCompTrackerRepository(_db, loggingService, currentUserService);

            ContractReviews = new ContractReviewRepository(_db, loggingService, currentUserService);
            ContractReviewSubs = new ContractReviewSubRepository(_db, loggingService, currentUserService);
            ContractReviewMasters = new ContractReviewMasterRepository(_db, loggingService, currentUserService);

            PerformaInvs = new PerformaInvRepository(_db, loggingService, currentUserService);
            PerformaInvSubs = new PerformaInvSubRepository(_db, loggingService, currentUserService);

            MfgDcs = new MfgDcRepository(_db, loggingService, currentUserService);
            MfgDcSubs = new MfgDcSubRepository(_db, loggingService, currentUserService);

            MfgInvs = new MfgInvRepository(_db, loggingService, currentUserService);
            MfgInvSubs = new MfgInvSubRepository(_db, loggingService, currentUserService);

            ExpInvs = new ExpInvRepository(_db, loggingService, currentUserService);
            ExpInvSubs = new ExpInvSubRepository(_db, loggingService, currentUserService);


            //---------------------Labour---------------------------------------

            LabourGRNs = new LabourGRNRepository(_db, loggingService, currentUserService);
            LabourGRNSubs = new LabourGRNSubRepository(_db, loggingService, currentUserService);

            LabourSCNs = new LabourSCNRepository(_db, loggingService, currentUserService);
            LabourSCNSubs = new LabourSCNSubRepository(_db, loggingService, currentUserService);

            LabourDcOutgoings = new LabourDcOutgoingRepository(_db, loggingService, currentUserService);
            LabourDcOutgoingSubs = new LabourDcOutgoingSubRepository(_db, loggingService, currentUserService);
            LabourDcReturnCompTracks = new LabourDcReturnCompTrackRepository(_db, loggingService);

            LabInvs = new LabInvRepository(_db, loggingService, currentUserService);
            LabInvSubs = new LabInvSubRepository(_db, loggingService, currentUserService);


            //CreditNote------------------------------
            CreditNotes = new CreditNoteRepository(_db, loggingService, currentUserService);
            CreditNoteSubs = new CreditNoteSubRepository(_db, loggingService, currentUserService);
            //-------------------------------------------

         
        //OutSourcing
            MaterialReqs = new MaterialReqRepository(_db, loggingService);
            MaterialReqSubs = new MaterialReqSubRepository(_db, loggingService);

            EnquiryPurchases = new EnquiryPurchaseRepository(_db, loggingService, currentUserService);
            EnquiryPurchaseSubs = new EnquiryPurchaseSubRepository(_db, loggingService, currentUserService);

            EnquiryPurchaseVendorAssigns = new EnquiryPurchaseVendorAssignRepository(_db, loggingService, currentUserService);

            PurchaseQuotes = new PurchaseQuoteRepository(_db, loggingService, currentUserService);
            PurchaseQuotesSubs = new PurchaseQuoteSubRepository(_db, loggingService, currentUserService);
            PurchPos = new PurchPoRepository(_db, loggingService, currentUserService);
            PurchPoSubs = new PurchPoSubRepository(_db, loggingService, currentUserService);

            PurchaseGRNs = new PurchaseGRNRepository(_db, loggingService, currentUserService);
            PurchaseGRNSubs = new PurchaseGRNSubRepository(_db, loggingService, currentUserService);

            PurchaseSCNs = new PurchaseSCNRepository(_db, loggingService, currentUserService);
            PurchaseSCNSubs = new PurchaseSCNSubRepository(_db, loggingService, currentUserService);

            PurchaseInvoices = new PurchaseInvoiceRepository(_db, loggingService, currentUserService);
            PurchaseInvoiceSubs = new PurchaseInvoiceSubRepository(_db, loggingService, currentUserService);

            SubConDCOuts = new SubConDcOutRepository(_db, loggingService);
            SubConDCOutSubs = new SubConDcOutSubRepository(_db, loggingService);

            SubConGRNs = new SubConGRNRepository(_db, loggingService);
            SubConGRNSubs = new SubConGRNSubRepository(_db, loggingService);
            SubConGRNTracks = new SubConGRNTrackRepository(_db, loggingService);

            SubConSCNs = new SubConSCNRepository(_db, loggingService);
            SubConSCNSubs = new SubConSCNSubRepository(_db, loggingService);

            SubConInvs = new SubConInvRepository(_db, loggingService,currentUserService);
            SubConInvSubs = new SubConInvSubRepository(_db, loggingService, currentUserService);

            DebitNotes = new DebitNoteRepository(_db, loggingService, currentUserService);
            DebitNoteSubs = new DebitNoteSubRepository(_db, loggingService, currentUserService);

            //Inventory - Stock
            SCNGens = new SCNGenRepository(_db, loggingService);
            SCNGenSubs = new SCNGenSubRepository(_db, loggingService);

            MINs = new MaterialIssNoteRepository(_db, loggingService);
            MINSubs = new MaterialIssueNoteSubRepository(_db, loggingService);

            StockAdds = new StockAddRepository(_db, loggingService);
            StockIssues = new StockIssueRepository(_db, loggingService);
            StockIssueTracks = new StockIssueTrackRepository(_db, loggingService);

            ToolCribIssues = new ToolCribIssueRepository(_db, loggingService, currentUserService);
            ToolCribIssueSubs = new ToolCribIssueSubRepository(_db, loggingService, currentUserService);
            ToolCribReturns = new ToolCribReturnRepository(_db, loggingService, currentUserService);
            ToolCribReturnSubs = new ToolCribReturnSubRepository(_db, loggingService, currentUserService);

            StoreInterTrans = new StoreInterTransRepository(_db, loggingService);
            StoreInterTransSubs = new StoreInterTransSubRepository(_db, loggingService);

            //Planning
            ApprovalHistories = new ApprovalHistoryRepository(_db, loggingService, currentUserService);
            JobOrders = new JobOrderRepository(_db, loggingService);
            JobOrderSubs = new JobOrderSubRepository(_db, loggingService);

            RcReleases = new RcReleaseRepository(_db, loggingService);
            RcReleaseSubs = new RcReleaseSubRepository(_db, loggingService);

            RouteCards = new RouteCardRepository(_db, loggingService);
            RouteCardSubs = new RouteCardSubRepository(_db, loggingService);

            Estimates = new EstimateRepository(_db, loggingService, currentUserService);//Estimation 

            EstimateSubs = new EstimateSubRepository(_db, loggingService, currentUserService);

            //Production
            ProductionIssueAssys = new ProductionIssueAssyRepository(_db, loggingService, currentUserService);
            ProductionIssueAssySubs = new ProductionIssueAssySubRepository(_db, loggingService);

            ProductionReturnAssys = new ProductionReturnAssyRepository(_db, loggingService);
            ProductionReturnAssySubs = new ProductionReturnAssySubRepository(_db, loggingService);
            ProductionReturnAssyTracks = new ProductionReturnAssyTrackRepository(_db, loggingService);

            ProductionSCNAssys = new ProductionSCNAssyRepository(_db, loggingService);
            ProductionSCNAssySubs = new ProductionSCNAssySubRepository(_db, loggingService);

            ProductionLogs = new ProductionLogRepository(_db, loggingService);
            ProductionLogSubs = new ProductionLogSubRepository(_db, loggingService);

            ProductionIssueComps = new ProductionIssueCompRepository(_db, loggingService);
            ProductionIssueCompSubs = new ProductionIssueCompSubRepository(_db, loggingService);

            ProductionReturnComps = new ProductionReturnCompRepository(_db, loggingService);
            ProductionReturnCompSubs = new ProductionReturnCompSubRepository(_db, loggingService);
            ProductionReturnCompTracks = new ProductionReturnCompTrackRepository(_db, loggingService);

            ProductionSCNComps = new ProductionSCNCompRepository(_db, loggingService);
            ProductionSCNCompSubs = new ProductionSCNCompSubRepository(_db, loggingService);


            //Maintenance
            MaintenanceSchedules = new MaintenanceScheduleRepository(_db, loggingService, currentUserService);
            MaintenanceProcesses = new MaintenanceProcessRepository(_db, loggingService, currentUserService);
            BreakdownMaintenances = new BreakdownMaintenanceRepository(_db, loggingService, currentUserService);
            AllInstrumentDetails = new InstrumentDetailsRepository(_db, loggingService, currentUserService);
            CalibrationHistories = new CalibrationHistoryRepository(_db, loggingService, currentUserService);

            //Inspection
            MasterInspections = new MasterInspectionRepository(_db, loggingService, currentUserService);
            InspectionRefs = new InspectionRefRepository(_db, loggingService, currentUserService);

            FinalInspections = new FinalInspectionRepository(_db, loggingService, currentUserService);
            FinalInspectionRefs = new FinalInspectionRefRepository(_db, loggingService, currentUserService);

            IncomingInspections = new IncomingInspectionRepository(_db, loggingService, currentUserService);
            IncomingInspectionRefs = new IncomingInspectionRefRepository(_db, loggingService, currentUserService);

            DefectInfos = new DefectInfoRepository(_db, loggingService, currentUserService);

            //Correspondence
            Correspondances = new CorrespondanceRepository(_db, loggingService, currentUserService);



            DcRunningNumbers = new DcRunningNumberRepository(_db, loggingService, currentUserService);

            InvoiceAutoRunningNumbers = new InvoiceAutoRunningNumberRepository(_db, loggingService, currentUserService);

            //SNS
            AssemblyCharges = new AssemblyChargeRepository(_db, loggingService, currentUserService);

            //Reports
            //Track reports

            LabourTracks = new LabourTrackRepository(_db, loggingService);

            ///Account module

            Payments = new PaymentsRepository(_db, loggingService, currentUserService);
            PaymentsSubs = new PaymentsSubRepository(_db, loggingService, currentUserService);
            Advaceadjustments = new AdvaceadjustmentRepository(_db, loggingService, currentUserService);
            AdvaceadjustmentSubs = new AdvaceadjustmentSubRepository(_db, loggingService, currentUserService);
            Receiptss = new ReceiptsRepository(_db, loggingService, currentUserService);
            fundTranss = new FundTransRepository(_db, loggingService, currentUserService);
            ReceiptsSubs = new ReceiptsSubRepository(_db, loggingService, currentUserService);

            ServiceBills = new ServiceBillsRepository(_db, loggingService, currentUserService);
            ServiceBillsSubs = new ServiceBillsSubRepository(_db, loggingService, currentUserService);

            RejectionMasters = new RejectionMasterRepository(_db, loggingService, currentUserService);

            UserColumnPreferences = new UserColumnPreferenceRepository(_db, loggingService);


            //Human Resource
            Attendances = new AttendanceRepository(_db, loggingService, currentUserService);
            Salaries = new SalaryRepository(_db, loggingService);
            StaffLoans = new StaffLoanRepository(_db, loggingService, currentUserService);

            Offers = new OfferLetterRepository(_db, loggingService, currentUserService);
            OffersSubs = new OfferLetterSubRepository(_db, loggingService, currentUserService);
            AppointmentLetters = new AppointmentLetterRepository(_db, loggingService, currentUserService);
            AppointmentLetterSubs = new AppointmentLetterSubRepository(_db, loggingService, currentUserService);

            StockRequestIssues = new StockRequestIssueRepository(_db, loggingService);
            StockRequestIssueSubs = new StockRequestIssueSubRepository(_db, loggingService);

        }

        public async Task<int> SaveAsync()
        {
            return await _db.SaveChangesAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _db.Database.BeginTransactionAsync();
        }

        public void DetachEntity<TEntity>(TEntity entity) where TEntity : class
        {
            var dbEntityEntry = _db.Entry(entity);
            if (dbEntityEntry != null)
                dbEntityEntry.State = EntityState.Detached;
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }


}
