

using ApexCharts;
using Blazored.LocalStorage;

using IQSMART.Shared.BusinessLayer.BusinessService.MasterService.AccountsService;
using IQSMART.Shared.BusinessLayer.BusinessService.OutSourcingService.DebitNote_Service;
using IQSMART.Shared.BusinessLayer.BusinessService.ReportService.TaxDeatilsReportService;
using IQSMART.Shared.BusinessLayer.BusinessService.ReportService.TrackReportService;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using V.SMART.Services;

using V.SMART.Shared.Authentication;
using V.SMART.Shared.BusinessLayer.BusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.AccountsService;
using V.SMART.Shared.BusinessLayer.BusinessService.CashFlowService;
using V.SMART.Shared.BusinessLayer.BusinessService.DashboardService;
using V.SMART.Shared.BusinessLayer.BusinessService.EInvoiceAPIService;
using V.SMART.Shared.BusinessLayer.BusinessService.HumanResourceService;
using V.SMART.Shared.BusinessLayer.BusinessService.HumanResourceService.AttendanceService;
using V.SMART.Shared.BusinessLayer.BusinessService.HumanResourceService.EmployeeLoanService;
using V.SMART.Shared.BusinessLayer.BusinessService.HumanResourceService.PayrollService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IAccountsService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ICashFlowService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IDashboardService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IEInvoiceAPI;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IHumanResourceService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IHumanResourceService.IAttendanceService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IHumanResourceService.IEmployeeLoanService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IHumanResourceService.IPayrollService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInspectionService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ILabourServices;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ILeadservice;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMaintenanceService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IAccountsService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IAdminService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IGeneralService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IHRMasterservice;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.IDebitNote_Service;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.IMaterialRequisitionService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.IPurchase_Invoice_Service;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.IPurchaseGRN_Service;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.IPurchaseSCNService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.IPurchOrSubConEnquiryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.IPurchOrSubConPoService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.ISubContractDcOutservice;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.ISubContractGRNService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.ISubContractInvoiceService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.ISubContractSCNService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IPlanningService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IProductionService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IPurchaseService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.IAccountReportService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.IAnalysisReportService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.IGSTITC;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.IPOTrack_Service;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITaxDetailsReportService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISalesService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISalesService.IEnquiryFesibility;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISettingsService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IStockAddIss_Position;
using V.SMART.Shared.BusinessLayer.BusinessService.InspectionService;
using V.SMART.Shared.BusinessLayer.BusinessService.InventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.InventoryService.StockAddIss_Position;
using V.SMART.Shared.BusinessLayer.BusinessService.LabourServices;
using V.SMART.Shared.BusinessLayer.BusinessService.LeadService;
using V.SMART.Shared.BusinessLayer.BusinessService.MaintenanceService;
using V.SMART.Shared.BusinessLayer.BusinessService.MasterService;
using V.SMART.Shared.BusinessLayer.BusinessService.MasterService.AccountsService;
using V.SMART.Shared.BusinessLayer.BusinessService.MasterService.AdminService;
using V.SMART.Shared.BusinessLayer.BusinessService.MasterService.GeneralService;
using V.SMART.Shared.BusinessLayer.BusinessService.MasterService.HRMasterService;
using V.SMART.Shared.BusinessLayer.BusinessService.MasterService.InventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.OutSourcingService.MaterialRequisitionService;
using V.SMART.Shared.BusinessLayer.BusinessService.OutSourcingService.Purchase_Invoice_Service;
using V.SMART.Shared.BusinessLayer.BusinessService.OutSourcingService.PurchaseGRN_Service;
using V.SMART.Shared.BusinessLayer.BusinessService.OutSourcingService.PurchaseSCN_Service;
using V.SMART.Shared.BusinessLayer.BusinessService.OutSourcingService.PurchOrSubConEnquiryService;
using V.SMART.Shared.BusinessLayer.BusinessService.OutSourcingService.PurchOrSubConPoService;
using V.SMART.Shared.BusinessLayer.BusinessService.OutSourcingService.SubContractDcOutService;
using V.SMART.Shared.BusinessLayer.BusinessService.OutSourcingService.SubContractGRNService;
using V.SMART.Shared.BusinessLayer.BusinessService.OutSourcingService.SubContractInvoiceService;
using V.SMART.Shared.BusinessLayer.BusinessService.OutSourcingService.SubContractSCNService;
using V.SMART.Shared.BusinessLayer.BusinessService.PlanningService;
using V.SMART.Shared.BusinessLayer.BusinessService.ProductionService;
using V.SMART.Shared.BusinessLayer.BusinessService.PurchaseService;
using V.SMART.Shared.BusinessLayer.BusinessService.ReportService.AccountReportService;
using V.SMART.Shared.BusinessLayer.BusinessService.ReportService.AnalysisReportService;
using V.SMART.Shared.BusinessLayer.BusinessService.ReportService.AnalysisService;
using V.SMART.Shared.BusinessLayer.BusinessService.ReportService.GSTITC;
using V.SMART.Shared.BusinessLayer.BusinessService.ReportService.POTrack_Service;
using V.SMART.Shared.BusinessLayer.BusinessService.ReportService.TaxDeatilsReportService;
using V.SMART.Shared.BusinessLayer.BusinessService.ReportService.TrackReportService;
using V.SMART.Shared.BusinessLayer.BusinessService.SalesService;
using V.SMART.Shared.BusinessLayer.BusinessService.SalesService.EnqiryFeasibility;
using V.SMART.Shared.BusinessLayer.BusinessService.SettingsService;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.Mappings;
using V.SMART.Shared.Repository;
using V.SMART.Shared.Repository.AccountsRepository;
using V.SMART.Shared.Repository.InventoryStockRepository;
using V.SMART.Shared.Repository.InventoryStockRepository.StockAdditionRepository;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Repository.IRepository.IAccountRepository.IPaymentsRepository;
using V.SMART.Shared.Repository.IRepository.IInventoryStockRepository;
using V.SMART.Shared.Repository.IRepository.IInventoryStockRepository.IStockAdditionRepository;
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
using V.SMART.Shared.Repository.IRepository.IPlanningRepository;
using V.SMART.Shared.Repository.IRepository.IPlanningRepository.IRouteCardRepo;
using V.SMART.Shared.Repository.IRepository.IProductionRepository.IProductionIssueWOAssyRepo;
using V.SMART.Shared.Repository.IRepository.IProductionRepository.IProductionReturnAssyRepo;
using V.SMART.Shared.Repository.IRepository.IProductionRepository.IProductionSCNAssyRepo;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.IMfgInvoice;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.IMfgLeads;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.IMfgQuotation;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.IPerformaInvoiceRepository;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ISalesDCRepoditory;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ISalesEnquiry;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ISalesPoRepository;
using V.SMART.Shared.Repository.IRepository.ISettingsRepository;
using V.SMART.Shared.Repository.IRepository.IUtilitiesRepository;
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
using V.SMART.Shared.Repository.PlanningRepository;
using V.SMART.Shared.Repository.PlanningRepository.RouteCardRepo;
using V.SMART.Shared.Repository.ProductionRepository.ProductionIssueWOAssyRepo;
using V.SMART.Shared.Repository.ProductionRepository.ProductionReturnAssyRepo;
using V.SMART.Shared.Repository.ProductionRepository.ProductionSCNAssyRepo;
using V.SMART.Shared.Repository.SalesAndLabourRepository.MfgInvoice;
using V.SMART.Shared.Repository.SalesAndLabourRepository.MfgLeads;
using V.SMART.Shared.Repository.SalesAndLabourRepository.MfgQuotation;
using V.SMART.Shared.Repository.SalesAndLabourRepository.PerformaInvoiceRepository;
using V.SMART.Shared.Repository.SalesAndLabourRepository.SalesDCRepository;
using V.SMART.Shared.Repository.SalesAndLabourRepository.SalesEnquiry;
using V.SMART.Shared.Repository.SalesAndLabourRepository.SalesPoRepository;
using V.SMART.Shared.Repository.SettingsRepository;
using V.SMART.Shared.Repository.UtilitiesRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.Services.MultiCompanyService;
using V.SMART.Shared.Services.ReportViewer;
using V.SMART.Shared.Shared;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchPoVM;
using V.SMARTV.Shared.BusinessLayer.BusinessService.IBusinessService.IAccountsService;

namespace V.SMART
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // =========================
            // Dependency Registrations
            // =========================

            //Report Service
            builder.Services.AddScoped<ReportService>();


            // Tenant-aware context factory
            builder.Services.AddScoped<ITenantProvider, TenantProvider>();
            builder.Services.AddScoped<ITenantDbContextFactory, TenantDbContextFactory>();
            builder.Services.AddScoped<ExcelExportService>();
            builder.Services.AddScoped<IExcelTemplateService, ExcelTemplateService>();


            // Tenant-aware ApplicationDbContext (created via factory)
            builder.Services.AddScoped<ApplicationDbContext>(sp =>
            {
                var factory = sp.GetRequiredService<ITenantDbContextFactory>();
                return factory.CreateDbContext();
            });

            //Report Service

            builder.Services.AddScoped<ReportService>();

            builder.Services.AddScoped<IPathProvider, DesktopPathProvider>();


            // Master Database — the connection string comes from configuration, never from a
            // literal in this file (M0-03-02, docs/CONFIGURATION.md).
            // MauiAppBuilder.Configuration has no environment-variable provider by default,
            // unlike WebApplicationBuilder, so the variable is read directly. The
            // configuration fall-back keeps the value overridable by any provider that is
            // registered before this point.
            var masterDbConnectionString =
                Environment.GetEnvironmentVariable("ConnectionStrings__MasterDb")
                ?? builder.Configuration.GetConnectionString("MasterDb");

            if (string.IsNullOrWhiteSpace(masterDbConnectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'ConnectionStrings:MasterDb' is not configured." +
                    Environment.NewLine +
                    "Set the environment variable ConnectionStrings__MasterDb before starting the desktop host," +
                    Environment.NewLine +
                    "    PowerShell:  $env:ConnectionStrings__MasterDb = \"<master connection string>\"" +
                    Environment.NewLine +
                    "There is deliberately no default value. See docs/CONFIGURATION.md.");
            }

            builder.Services.AddDbContext<MasterDbContext>(options =>
                options.UseSqlServer(masterDbConnectionString));

            // Core Libraries
            builder.Services.AddMudServices();
            builder.Services.AddBlazoredLocalStorage();

            builder.Services.AddApexCharts();

            // HttpClient with timeout
            builder.Services.AddScoped<HttpClient>(sp =>
            {
                return new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(30)
                };
            });

            builder.Services.AddAuthorizationCore();

            builder.Services.AddScoped<ThemeStateService>();
            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
            builder.Services.AddScoped<CurrentUserService>();
            builder.Services.AddScoped<IColumnPreferenceService, ColumnPreferenceService>();
            builder.Services.AddScoped<ILoggingService, FileLoggingService>();
            builder.Services.AddScoped<ICorrespondanceRepository, CorrespondanceRepository>();
            builder.Services.AddScoped<IFileUploadService, MauiFileUploadService>();
            builder.Services.AddSingleton<IFileOpener, DesktopFileOpener>();

            // Register AutoMapper: scan the assembly that contains MappingProfileMarker
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(typeof(MappingProfileMarker).Assembly);
            });

            // Repository & UnitOfWork
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // =========================
            // Repositories
            // =========================

            // Screen & User
            builder.Services.AddScoped<IScreenRepository, ScreenRepository>();
            builder.Services.AddScoped<IScreenManagementRepository, ScreenManagementRepository>();
            builder.Services.AddScoped<IUserThemePreferenceService, UserThemePreferenceService>();


            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserRightsRepository, UserRightsRepository>();
            builder.Services.AddScoped<IUserAuthorityRepository, UserAuthorityRepository>();

            // Master Data
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IUOMRepository, UOMRepository>();
            builder.Services.AddScoped<IStoreRepository, StoreRepository>();
            builder.Services.AddScoped<IRawMaterialRepository, RawMaterialRepository>();
            builder.Services.AddScoped<IFactorRepository, FactorRepository>();
            builder.Services.AddScoped<IProcessRepository, ProcessRepository>();
            builder.Services.AddScoped<IMachineRepository, MachineRepository>();
            builder.Services.AddScoped<IGroupingRepository, GroupingRepository>();
            builder.Services.AddScoped<IGroupingSubRepository, GroupingSubRepository>();
            builder.Services.AddScoped<IItemRepository, ItemRepository>();
            builder.Services.AddScoped<IHSNMasterRepository, HSNMasterRepository>();
            builder.Services.AddScoped<IItemProductAssignRepository, ItemProductAssignRepository>();
            builder.Services.AddScoped<IAssemblyRepository, AssemblyRepository>();
            builder.Services.AddScoped<ICompMasterRepository, CompMasterRepository>();
            builder.Services.AddScoped<IItemSubRepository, ItemSubRepository>();
            builder.Services.AddScoped<IProcessFlowChartRepository, ProcessFlowChartRepository>();
            builder.Services.AddScoped<IStateRepository, StateRepository>();
            builder.Services.AddScoped<IAssemblyDefLabourRepository, AssemblyDefLabourRepository>();

            // Customer & Vendor
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<ICustomerIndirectRepository, CustomerIndirectRepository>();
            builder.Services.AddScoped<IContactPersonRepository, ContactPersonRepository>();
            builder.Services.AddScoped<IVendorRepository, VendorRepository>();
            builder.Services.AddScoped<IVendorContactRepository, VendorContactRepository>();
            builder.Services.AddScoped<IVendorInDirectRepository, VendorInDirectRepository>();

            // Finance
            builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
            builder.Services.AddScoped<IIncomeRepository, IncomeRepository>();
            builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
            builder.Services.AddScoped<ICurrencyTodayRepository, CurrencyTodayRepository>();
            builder.Services.AddScoped<IBanksRepository, BanksRepository>();
            builder.Services.AddScoped<ICostCenterRepository, CostCenterRepository>();

            // HR
            builder.Services.AddScoped<IStaffRepository, StaffRepository>();
            builder.Services.AddScoped<IStaffFamilyDetailRepository, StaffFamilyDetailRepository>();
            builder.Services.AddScoped<IStaffEducationRepository, StaffEducationRepository>();
            builder.Services.AddScoped<IStaffEmergencyRepository, StaffEmergencyRepository>();
            builder.Services.AddScoped<ILeaveTypeRepository, LeaveTypeRepository>();
            builder.Services.AddScoped<IEmployeeLeaveBalanceRepository, EmployeeLeaveBalanceRepository>();
            builder.Services.AddScoped<ILeaveApplicationRepository, LeaveApplicationRepository>();
            builder.Services.AddScoped<IDailyLeaveRecordRepository, DailyLeaveRecordRepository>();
            builder.Services.AddScoped<IShiftAllocationRepository, ShiftAllocationRepository>();
            builder.Services.AddScoped<IHolidayListRepository, HolidayListRepository>();
            builder.Services.AddScoped<ICandidateService, CandidateService>();
            builder.Services.AddScoped<IOfferLetterService, OfferLetterService>();
            builder.Services.AddScoped<IAppointmentLetterService, AppointmentLetterService>();

            // Company
            builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
            builder.Services.AddScoped<IProjectTypeMasterRepository, ProjectTypeMasterRepository>();

            // Sales & Lead
            builder.Services.AddScoped<ILeadsRepository, LeadsRepository>();
            builder.Services.AddScoped<IEnquirySalesRepository, EnquirySalesRepository>();
            builder.Services.AddScoped<IEnquirySalesSubRepository, EnquirySalesSubRepository>();
            builder.Services.AddScoped<IMfgQuoteRepository, MfgQuoteRepository>();
            builder.Services.AddScoped<IMfgQuoteSubRepository, MfgQuoteSubRepository>();
            builder.Services.AddScoped<IMfgPoRepository, MfgPoRepository>();
            builder.Services.AddScoped<IMfgPoSubRepository, MfgPoSubRepository>();

            builder.Services.AddScoped<IContractReviewService, ContractReviewService>();

            builder.Services.AddScoped<IPerformaInvRepository, PerformaInvRepository>();
            builder.Services.AddScoped<IPerformaInvSubRepository, PerformaInvSubRepository>();

            builder.Services.AddScoped<IMfgDcRepository, MfgDcRepository>();
            builder.Services.AddScoped<IMfgDcSubRepository, MfgDcSubRepository>();

            builder.Services.AddScoped<IMfgInvRepository, MfgInvRepository>();
            builder.Services.AddScoped<IMfgInvSubRepository, MfgInvSubRepository>();

            //Inventory - Stock
            builder.Services.AddScoped<IStockAddIssPosition, StockAddIssPosition>();

            builder.Services.AddScoped<ISCNGenRepository, SCNGenRepository>();
            builder.Services.AddScoped<ISCNGenSubRepository, SCNGenSubRepository>();
            builder.Services.AddScoped<IStockAddRepository, StockAddRepository>();
            builder.Services.AddScoped<IStockIssueRepository, StockIssueRepository>();

            builder.Services.AddScoped<IStoreInterTransService, StoreInterTransService>();

            // Approval
            builder.Services.AddScoped<IApprovalHistoryRepository, ApprovalHistoryRepository>();

            //Planning
            builder.Services.AddScoped<IRouteCardRepository, RouteCardRepository>();
            builder.Services.AddScoped<IRouteCardSubRepository, RouteCardSubRepository>();

            builder.Services.AddScoped<IProductionIssueAssyRepository, ProductionIssueAssyRepository>();
            builder.Services.AddScoped<IProductionIssueAssySubRepository, ProductionIssueAssySubRepository>();

            builder.Services.AddScoped<IProductionReturnAssyRepository, ProductionReturnAssyRepository>();
            builder.Services.AddScoped<IProductionReturnAssySubRepository, ProductionReturnAssySubRepository>();

            builder.Services.AddScoped<IProductionSCNAssyRepository, ProductionSCNAssyRepository>();
            builder.Services.AddScoped<IProductionSCNAssySubRepository, ProductionSCNAssySubRepository>();

            // Terms & Correspondence
            builder.Services.AddScoped<ITermsAndConditionsRepository, TermsAndConditionsRepository>();
            builder.Services.AddScoped<ICorrespondanceRepository, CorrespondanceRepository>();
            builder.Services.AddScoped<IStoreMapRepository, StoreMapRepository>();

            // =========================
            // Services
            // =========================

            builder.Services.AddScoped<IDashboardService, DashboardService>();

            builder.Services.AddScoped<ForeignKeyUsageChecker>();

            //BarCode And QR Code
            builder.Services.AddScoped<BarcodeService>();

            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IUserAuthorityService, UserAuthorityservice>();
            builder.Services.AddScoped<IUserRightService, UserRightService>();


            builder.Services.AddScoped<IStoreService, StoreService>();
            builder.Services.AddScoped<IHSNService, HSNService>();
            builder.Services.AddScoped<IRawMaterialService, RawMaterialService>();
            builder.Services.AddScoped<IProcessService, ProcessService>();
            builder.Services.AddScoped<IFactorservice, FactorService>();
            builder.Services.AddScoped<IGroupingService, GroupingService>();
            builder.Services.AddScoped<IMachineService, MachineService>();

            // Accounts Service
            builder.Services.AddScoped<IExpenseService, ExpenseService>();
            builder.Services.AddScoped<IIncomeService, IncomeService>();
            builder.Services.AddScoped<ICurrencyService, CurrencyService>();

            // HumanResource Service
            builder.Services.AddScoped<ILeaveTypeService, LeaveTypeService>();
            builder.Services.AddScoped<IEmployeeLeavebalanceService, EmployeeLeavebalanceService>();
            builder.Services.AddScoped<IShiftAllocationService, ShiftAllocationService>();




            builder.Services.AddScoped<IPrintSettingService, PrintSettingService>();
            builder.Services.AddScoped<IProdLogSettingsService, ProdLogSettingService>();
            builder.Services.AddScoped<IHRMasterService, HRMasterService>();
            builder.Services.AddScoped<IBiometricExcelSettingService, BiometricExcelSettingService>();
            builder.Services.AddScoped<ISalaryHeadPrintSettingService, SalaryHeadPrintSettingService>();


            builder.Services.AddScoped<IEmployeeService, EmployeeService>();

            builder.Services.AddScoped<IVendorService, VendorService>();
            builder.Services.AddScoped<ICustomerService, CustomerService>();

            builder.Services.AddScoped<IItemService, ItemService>();
            builder.Services.AddScoped<ICostCenterService, CostCenterService>();

            builder.Services.AddScoped<IAssemblyDefService, AssemblyDefService>();

            builder.Services.AddScoped<ICommonService, CommonService>();
            builder.Services.AddScoped<ICalculationService, CalculationService>();
            builder.Services.AddScoped<ILeaveApplicationService, LeaveApplicationService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IBankService, BankService>();
            builder.Services.AddScoped<ICompanyService, CompanyService>();
            builder.Services.AddScoped<ITermsAndConditionsService, TermsAndConditionsService>();

            builder.Services.AddScoped<ILeadService, LeadService>();
            builder.Services.AddScoped<IEnquirySalesService, EnquirySalesService>();
            builder.Services.AddScoped<IEnqiryFeasibilityService, EnqiryFeasibilityService>();

            builder.Services.AddScoped<IMfgQuotationService, MfgQuotationService>();
            builder.Services.AddScoped<IMfgPoService, MfgPoService>();
            builder.Services.AddScoped<IContractReviewService, ContractReviewService>();

            builder.Services.AddScoped<IPerformaInvService, PerformaInvService>();
            builder.Services.AddScoped<IMfgDcService, MfgDcService>();

            builder.Services.AddScoped<IMfgInvService, MfgInvService>();
            builder.Services.AddScoped<IExpInvService, ExpInvService>();

            builder.Services.AddScoped<ILabourDcOutgoingService, LabourDcOutgoingService>();
            builder.Services.AddScoped<ILabourGRNService, LabourGRNService>();
            builder.Services.AddScoped<ILabourSCNService, LabourSCNService>();
            builder.Services.AddScoped<ILabourInvoiceService, LabourInvoiceService>();

            builder.Services.AddScoped<ICreditNoteService, CreditNoteService>();

            //OutSourcing Service
            builder.Services.AddScoped<IMaterialReqService, MaterialReqService>();
            builder.Services.AddScoped<IEnquiryPurchaseService, EnquiryPurchaseService>();
            builder.Services.AddScoped<IPurchaseQuoteService, PurchaseQuoteService>();
            builder.Services.AddScoped<IPurchPoService, PuchPoService>();
            builder.Services.AddScoped<PoSelectionService>();

            builder.Services.AddScoped<IPurchaseGRNService, PurchaseGRNService>();
            builder.Services.AddScoped<IPurchaseSCNService, PurchaseSCNService>();
            builder.Services.AddScoped<IPurchaseInvoiceService, PurchaseInvoiceService>();

            builder.Services.AddScoped<ISubConDcOutService, SubConDcOutService>();
            builder.Services.AddScoped<ISubConGRNService, SubConGRNService>();
            builder.Services.AddScoped<ISubConSCNService, SubConSCNService>();
            builder.Services.AddScoped<ISubConInvService, SubConInvService>();

            builder.Services.AddScoped<IDebitNoteService, DebitNoteService>();

            //Inventory -Stock Service
            builder.Services.AddScoped<ISCNGenService, SCNGenService>();
            builder.Services.AddScoped<IStockManagerService, StockManagerService>();
            builder.Services.AddScoped<IMINService, MINService>();

            builder.Services.AddScoped<IToolCribIssueService, ToolCribIssueService>();
            builder.Services.AddScoped<IToolCribReturnService, ToolCribReturnService>();

            //Planning -Service
            builder.Services.AddScoped<IApprovalService, ApprovalService>();
            builder.Services.AddScoped<IMaterialReqAnalysisService, MaterialReqAnalysisService>();
            builder.Services.AddScoped<IJobOrderService, JobOrderService>();

            builder.Services.AddScoped<IRcReleaseService, RcReleaseService>();
            builder.Services.AddScoped<IRouteCardService, RouteCardService>();
            builder.Services.AddScoped<IProcessLogService, ProcessLogService>();
            builder.Services.AddScoped<IRouteCardService, RouteCardService>();


            //production Service
            builder.Services.AddScoped<IProductionIssueAssyService, ProductionIssueAssyService>();
            builder.Services.AddScoped<IProductionReturnAssyService, ProductionReturnAssyService>();
            builder.Services.AddScoped<IProductionSCNAssyservice, ProductionSCNAssyService>();

            builder.Services.AddScoped<IProductionLogService, ProductionLogService>();

            builder.Services.AddScoped<IProductionIssueCompService, ProductionIssueCompService>();
            builder.Services.AddScoped<IProductionReturnCompService, ProductionReturnCompService>();
            builder.Services.AddScoped<IProductionSCNCompService, ProductionSCNCompService>();

            //Settings service
            builder.Services.AddScoped<IStoreMappingService, StoreMapService>();

            //Maintainance Services
            builder.Services.AddScoped<IMaintenanceScheduleService, MaintenanceScheduleService>();
            builder.Services.AddScoped<IMaintenanceProcessService, MaintenanceProcessService>();
            builder.Services.AddScoped<IBreakdownMaintenanceService, BreakdownMaintenanceService>();
            builder.Services.AddScoped<ICalibrationHistoryAndMaintenanceService, CalibrationHistoryAndMaintenanceService>();


            //Inspection Services
            builder.Services.AddScoped<IMasterInspectionService, MasterInspectionService>();
            builder.Services.AddScoped<IFinalInspectionService, FinalInspectionService>();
            builder.Services.AddScoped<IIncomingInspectionService, IncomingInspectionService>();
            builder.Services.AddScoped<IDefectInfoService, DefectInfoService>();

            builder.Services.AddScoped<IAssemblyRequirementService, AssemblyRequirementService>();

            builder.Services.AddScoped<IInspectionSettingsService, InspectionSettingsService>();


            //Human Resource
            builder.Services.AddScoped<IAttendanceService, AttendanceService>();
            builder.Services.AddScoped<ISalaryService, SalaryService>();
            builder.Services.AddScoped<IStaffLoanService, StaffLoanService>();

            //Eway
            builder.Services.AddScoped<IEWayDatabaseService, EWayDatabaseService>();
            //EInvoice
            builder.Services.AddScoped<IEinvoiceDatabaseService, EinvoiceDatabaseService>();

            //SNS
             builder.Services.AddScoped<ICostingService, CostingService>();


            builder.Services.AddScoped<UserSession>();
           
            // Report Service
           builder.Services.AddScoped<IReportExecutor, ReportExecutor>();

            //Purchase Sales Track Report
            builder.Services.AddScoped<IPurchaseSalesTrackReportServices, PurchaseSalesTrackReportServices>();
            // Track Reports Service
            builder.Services.AddScoped<ILabourTrackReportService, LabourTrackReportService>();
            builder.Services.AddScoped<ISalesTrackReportService, SalesTrackReportService>();
            builder.Services.AddScoped<IViewTallyDCInOutTrackService, ViewTallyDCInOutTrackService>();
            builder.Services.AddScoped<IPOTrackService, POTrackService>();
            builder.Services.AddScoped<IJobOrdeAnalysisService, JobOrderAnalysisService>();
            builder.Services.AddScoped<IPoPendingServices, PoPendingServices>();
            builder.Services.AddScoped<ILabourPendingService, LabourPendingService>();
            builder.Services.AddScoped<IProductionPendingService, ProductionPendingService>();

            builder.Services.AddScoped<IItemWiseReportService, ItemWiseReportService>();
            builder.Services.AddScoped<IDaybookService, DaybookService>();
            builder.Services.AddScoped<ITaxDetailsService, TaxDetailsService>();
            //Stock Analysis
            builder.Services.AddScoped<IStockLedgerReportService, StockLedgerReportService>();
            builder.Services.AddScoped<IStockAnalysisService, StockAnalysisService>();
            builder.Services.AddScoped<IRouteCardAnalysisService, RouteCardAnalysisService>();
            builder.Services.AddScoped<IItemModificationReportServices, ItemModificationReportServices>();
            builder.Services.AddScoped<IRejectionService, RejectionService>();
            builder.Services.AddScoped<IRatingService, GetRatingsService>();



            //Accounts Service
            builder.Services.AddScoped<IGSTITCService, GSTITCService>();
            builder.Services.AddScoped<IPaymentsService, PaymentsService>();
            builder.Services.AddScoped<IAdvaceAdjustmentService, AdvaceAdjustmentService>();
            builder.Services.AddScoped<IFundTransRepository, FundTransRepository>();
            builder.Services.AddScoped<IReceiptsService, ReceiptsService>();

            builder.Services.AddScoped<IServiceBillsService, ServiceBillsService>();

            builder.Services.AddScoped<IPendingBillsService, PendingBillsService>();
            builder.Services.AddScoped<IPaidBillsService, PaidBillsService>();
            builder.Services.AddScoped<IPendingStatementsService, PendingStatementsService>();
            builder.Services.AddScoped<IProfit_LossAccountService, Profit_LossAccountsService>();
            builder.Services.AddScoped<IAccountReportService, AccountReportService>();//Confirmation of accounts
            builder.Services.AddScoped<IRejectionMasterService, RejectionMasterService>();
            builder.Services.AddSingleton<SessionTimeoutService>();
            builder.Services.AddScoped<IStockIssueRequestService, StockIssueRequestService>();
            builder.Services.AddScoped<ISummaryAndGraphsService, SummaryAndGraphsService>();


            builder.Services.AddScoped<ITDSSummaryService, TDSSummaryService>();
            builder.Services.AddScoped<IHSNSummaryService, HSNSummaryService>();
            builder.Services.AddScoped<ICreditDebitSummaryService, CreditDebitSummaryService>();




            //11072026-------------------------------------------------------------------
            builder.Services.AddScoped<IAppVersionService,AppVersionService>();
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppEnvironment"] = "Desktop"
            });
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            //----------------------------------------------------------------------------
            // =========================
            // Blazor / UI
            // =========================

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}

