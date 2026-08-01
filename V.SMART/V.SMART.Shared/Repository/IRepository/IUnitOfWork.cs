
using Microsoft.EntityFrameworkCore.Storage;
using V.SMART.Shared.Data.Planning;
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

namespace V.SMART.Shared.Repository.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        void DetachEntity<TEntity>(TEntity entity) where TEntity : class;

        //Screen management
        IUserThemePrefRepository UserThemePreferences {  get; }
        IUserPreferenceRepository UserPreferences {  get; }
        IPrintSettingRepository PrintSettings { get; }
        IScreenManagementRepository ScreenManagements { get; }
        IScreenRepository Screens { get; }
        ITermsAndConditionsRepository TermsAndConditions { get; }
        IStoreMapRepository StoreMaps { get; }
        IProductionLogSettingRepository ProductionLogSettings { get; }
        IInspectionSettingsRepository InspectSettings { get; }
        IHRMasterSettingRepository HRMasterSettings { get; }
        IBiometricExcelSettingRepository BiometricExcelSettings { get; }
        ISalaryHeadPrintSettingRepository SalaryHeadPrintSettings { get; }

        //Utilities
        ICorrespondanceRepository Correspondances { get; }

        //Admin
        IUserRepository Users { get; }
        IUserRightsRepository UserRights { get; }
        IUserAuthorityRepository UserAuthorities { get; }

        //Inventory - Item
        ICategoryRepository Categories { get; }
        IUOMRepository UOMs { get; }
        IItemRepository ItemRepositories { get; }
        IItemCustomerAssignRepository ItemCustomerAssigns { get; }
        IHSNMasterRepository HSNMasters { get; }
        IItemProductAssignRepository ItemProductAssigns { get; }
        IStoreRepository Stores { get; }
        IRawMaterialRepository RawMaterial { get; }
        IFactorRepository Factors { get; }
        IProcessRepository Processes { get; }
        IMachineRepository Machines { get; }
        IGroupingRepository Groupings { get; }
        IGroupingSubRepository GroupingSubs { get; }
        IAssemblyRepository AssmblyDefs { get; }
        IAssemblyModifyRepository AssemblyModifies { get; }
        ICompMasterRepository CompMasters { get; }
        IItemSubRepository ItemSubs { get; }
        IProcessFlowChartRepository ProcessFlowCharts { get; }

        IAssemblyDefLabourRepository AssmblyDefLabs { get; }

        //General
        IStateRepository States { get; }
        IVendorRepository Vendors { get; }
        ICustomerRepository Customers { get; }
        ICustomerIndirectRepository CustomerIndirects { get; }
        IContactPersonRepository ContactPersons { get; }
        IVendorContactRepository VendorContacts { get; }
        IVendorInDirectRepository VendorInDirects { get; }


        //Accounts
        IExpenseRepository Expenses { get; }
        ICurrencyRepository Currencyis { get; }
        IIncomeRepository Incomes { get; }
        ICurrencyTodayRepository CurrencyTodays { get; }
        IBanksRepository Banks { get; }
        IProjectTypeMasterRepository ProjectTypeMasters { get; }
        ICostCenterRepository CostCenters { get; }



        //Human Resource
        IStaffRepository Staffs { get; }
        IStaffEducationRepository StaffEducations { get; }
        IStaffEmergencyRepository StaffEmergencys { get; }
        IStaffFamilyDetailRepository StaffFamilyDetails { get; }
        IHolidayListRepository HolidayLists { get; }
        ILeaveTypeRepository LeaveTypes { get; }
        IEmployeeLeaveBalanceRepository EmployeeLeaveBalances { get; }
        ILeaveApplicationRepository LeaveApplications { get; }
        IDailyLeaveRecordRepository DailyLeaveRecords { get; }
        IShiftAllocationRepository ShiftAllocations { get; }

        //sbm
        ICandidateRepository Candidate { get; }
        


        //Company
        ICompanyRepository Companies { get; }


        //Sales and labour
        ILeadsRepository Leads { get; }
        IEnquirySalesRepository EnquirySaless { get; }
        IEnquirySalesSubRepository EnquirySalesSubs { get; }
        IEnquiryFeasibilityRepository EnquiryFeasibilities { get; }
        IEnquiryFeasibilitySubRepository EnquiryFeasibilitySubs { get; }

        IMfgQuoteRepository MfgQuotes { get; }
        IMfgQuoteSubRepository MfgQuoteSubs { get; }
        IMfgPoTypeRepository PoTypes { get; }
        IMfgPoRepository MfgPos { get; }
        IMfgPoSubRepository MfgPoSubs { get; }
        IAssyPOCompTrackerRepository AssyPOCompTrackers { get; }

        //ContractReview
        IContractReviewRepository ContractReviews { get; }
        IContractReviewSubRepository ContractReviewSubs { get; }
        IContractReviewMasterRepository ContractReviewMasters { get; }

        IPerformaInvRepository PerformaInvs { get; }
        IPerformaInvSubRepository PerformaInvSubs { get; }
        IMfgDcRepository MfgDcs { get; }
        IMfgDcSubRepository MfgDcSubs { get; }

        IMfgInvRepository MfgInvs { get; }
        IMfgInvSubRepository MfgInvSubs { get; }

        IExpInvRepository ExpInvs { get; }
        IExpInvSubRepository ExpInvSubs { get; }


        // ===== Labour ======
        ILabourGRNRepository LabourGRNs { get; }
        ILabourGRNSubRepository LabourGRNSubs { get; }

        ILabourSCNRepository LabourSCNs { get; }
        ILabourSCNSubRepository LabourSCNSubs { get; }

        ILabourDcOutgoingRepository LabourDcOutgoings { get; }
        ILabourDcOutgoingSubRepository LabourDcOutgoingSubs { get; }
        ILabourDcReturnCompTrackRepository LabourDcReturnCompTracks { get; }

        ILabInvRepository LabInvs { get; }
        ILabInvSubRepository LabInvSubs { get; }

        //CreditNote--------------------------
        ICreditNoteRepository CreditNotes { get; }
        ICreditNoteSubRepository CreditNoteSubs { get; }


        //Out-Sourcing
        IMaterialReqRepository MaterialReqs { get; }
        IMaterialReqSubRepository MaterialReqSubs { get; }

        IEnquiryPurchaseRepository EnquiryPurchases { get; }
        IEnquiryPurchaseSubRepository EnquiryPurchaseSubs { get; }
        IPurchaseQuoteRepository PurchaseQuotes { get; }
        IPurchaseQuoteSubRepository PurchaseQuotesSubs { get; }

        IEnquiryPurchaseVendorAssignRepository EnquiryPurchaseVendorAssigns { get; }
        IPurchPoRepository PurchPos { get; }
        IPurchPoSubRepository PurchPoSubs { get; }

        IPurchaseGRNRepository PurchaseGRNs { get; }
        IPurchaseGRNSubRepository PurchaseGRNSubs { get; }

        IPurchaseSCNRepository PurchaseSCNs { get; }
        IPurchaseSCNSubRepository PurchaseSCNSubs { get; }

        IPurchaseInvoiceRepository PurchaseInvoices { get; }
        IPurchaseInvoiceSubRepository PurchaseInvoiceSubs { get; }

        ISubConDCOutRepository SubConDCOuts { get; }
        ISubConDCOutSubRepository SubConDCOutSubs { get; }

        ISubConGRNRepository SubConGRNs { get; }
        ISubConGRNSubRepository SubConGRNSubs { get; }
        ISubConGRNTrackRepository SubConGRNTracks { get; }

        ISubConSCNRepository SubConSCNs { get; }
        ISubConSCNSubRepository SubConSCNSubs { get; }

        ISubConInvRepository SubConInvs { get; }
        ISubConInvSubRepository SubConInvSubs { get; }

        //DebitNote--------------------------
        IDebitNoteRepository DebitNotes { get; }
        IDebitNoteSubRepository DebitNoteSubs { get; }

        //Inventory-Stock
        ISCNGenRepository SCNGens { get; }
        ISCNGenSubRepository SCNGenSubs { get; }

        IMaterialIssueNoteRepository MINs { get; }
        IMaterialIssueNoteSubRepository MINSubs { get; }

        IStockAddRepository StockAdds { get; }
        IStockIssueRepository StockIssues { get; }
        IStockIssueTrackRepository StockIssueTracks { get; }

        IToolCribIssueRepository ToolCribIssues { get; }
        IToolCribIssueSubRepository ToolCribIssueSubs { get; }
        IToolCribReturnRepository ToolCribReturns { get; }
        IToolCribReturnSubRepository ToolCribReturnSubs { get; }

        IStoreInterTransRepository StoreInterTrans { get; }
        IStoreInterTransSubRepository StoreInterTransSubs { get; }

        //Planning
        IApprovalHistoryRepository ApprovalHistories { get; }
        IJobOrderRepository JobOrders { get; }
        IJobOrderSubRepository JobOrderSubs{ get; }

        IRcReleaseRepository RcReleases { get; }
        IRcReleaseSubRepository RcReleaseSubs { get; }

        IRouteCardRepository RouteCards { get; }
        IRouteCardSubRepository RouteCardSubs { get; }

        IEstimateRepository Estimates { get; }//Estimation 
        IEstimateSubRepository EstimateSubs { get; }

        IProductionIssueAssyRepository ProductionIssueAssys { get; }
        IProductionIssueAssySubRepository ProductionIssueAssySubs { get; }

        IProductionReturnAssyRepository ProductionReturnAssys { get; }
        IProductionReturnAssySubRepository ProductionReturnAssySubs { get; }
        IProductionReturnAssyTrackRepository ProductionReturnAssyTracks { get; }

        IProductionSCNAssyRepository ProductionSCNAssys { get; }
        IProductionSCNAssySubRepository ProductionSCNAssySubs { get; }

        IProductionLogRepository ProductionLogs { get; }
        IProductionLogSubRepository ProductionLogSubs { get; }

        IProductionIssueCompRepository ProductionIssueComps { get; }
        IProductionIssueCompSubRepository ProductionIssueCompSubs { get; }

        IProductionReturnCompRepository  ProductionReturnComps { get; }
        IProductionReturnCompSubRepository  ProductionReturnCompSubs { get; }
        IProductionReturnCompTrackRepository ProductionReturnCompTracks { get; }

        IProductionSCNCompRepository ProductionSCNComps { get; }
        IProductionSCNCompSubRepository ProductionSCNCompSubs { get; }

        //Maintenance 
        IMaintenanceScheduleRepository MaintenanceSchedules { get; }
        IMaintenanceProcessRepository MaintenanceProcesses { get; }
        IBreakdownMaintenanceRepository BreakdownMaintenances { get; }
        IInstrumentDetailsRepository AllInstrumentDetails { get; }
        ICalibrationHistoryRepository CalibrationHistories { get; }


        //Inspection and Quality
        IDefectInfoRepository DefectInfos { get; }
        IMasterInspectionRepository MasterInspections { get; }
        IInspectionRefRepository InspectionRefs { get; }

        IFinalInspectionRepository FinalInspections { get; }
        IFinalInspectionRefRepository FinalInspectionRefs { get; }

        IIncomingInspectionRepository IncomingInspections { get; }
        IIncomingInspectionRefRepository IncomingInspectionRefs { get; }


        IDcRunningNumberRepository DcRunningNumbers { get; }

        IInvoiceAutoRunningNumberRepository InvoiceAutoRunningNumbers { get; }

        IAssemblyChargeRepository AssemblyCharges { get; }



        // Track Reports

        ILabourTrackRepository LabourTracks { get; }

        ///Accounts Module
        IPaymentsRepository Payments { get; }
        IPaymentsSubRepository PaymentsSubs { get; }

        IAdvaceadjustmentRepository Advaceadjustments { get; }
        IAdvaceadjustmentSubRepository AdvaceadjustmentSubs { get; }
        IFundTransRepository fundTranss { get; }
        IReceiptsRepository Receiptss { get; }
        IReceiptsSubRepository ReceiptsSubs { get; }

        IServiceBillsRepository ServiceBills { get; }
        IServiceBillsSubRepository ServiceBillsSubs { get; }

        IRejectionMasterRepository RejectionMasters { get; }

        IUserColumnPreferenceRepository UserColumnPreferences { get; }

        //Human Resource    
        IAttendanceRepository Attendances { get; }
        ISalaryRepository Salaries { get; }
        IStaffLoanRepository StaffLoans { get; }

        IOfferLetterRepository Offers { get; }
        IOfferLetterSubRepository OffersSubs { get; }

        IAppointmentLetterRepository AppointmentLetters { get; }
        IAppointmentLetterSubRepository AppointmentLetterSubs { get; }


        IStockRequestIssueRepository StockRequestIssues { get; }
        IStockRequestIssueSubRepository StockRequestIssueSubs { get; }
    }
}
