// M2-B07 — the single composition root for the V.SMART domain graph.
//
// Before this file existed the same graph was registered independently in
// V.SMART.Web/Program.cs (242 registrations) and V.SMART/MauiProgram.cs (243), and the two
// had already drifted (KB-060 R-26). V.SMART.Api registered exactly one business service.
//
// Host-specific abstractions are deliberately NOT registered here — each host supplies its
// own implementation and that seam is the point:
//     IPathProvider, IFileOpener, IFileUploadService, a bare HttpClient,
//     AuthenticationStateProvider, AddHttpClient()
// plus IJSRuntime, which two domain services still take (see the notes below).
//
// Union reconciliation (Confirmed, M2-B07 — measured, not estimated). Normalising every
// registration call on master and de-duplicating:
//     V.SMART.Web/Program.cs      242 matched lines -> 240 distinct, of which one (`:253`)
//                                 is commented out  -> 239 real
//     V.SMART/MauiProgram.cs      243 matched lines -> 239 distinct
//     union of the two                              =  249 distinct
//     after M2-B07: this file (238 distinct) + the 11 distinct host registrations that
//     remain across V.SMART.Web/Program.cs and V.SMART/MauiProgram.cs
//                                                   =  249 distinct
// i.e. nothing was dropped and nothing was invented. The 2 duplicate lines left in this file
// (ICorrespondanceRepository, IEnquiryPurchaseService) are pre-existing duplicates of the two
// former roots, carried over verbatim rather than tidied — last registration wins, identical
// implementation either way.
//
// The one deliberate reclassification: IExcelTemplateService moved from host to domain (see
// the comment at its registration below). The one deliberate host addition: AddHttpClient()
// in V.SMART.Api/Program.cs, which the API lacked and four business services need.
//
// This file moves registrations only. No service implementation, constructor or lifetime
// semantic is changed.

using ApexCharts;
using AutoMapper;
using Blazored.LocalStorage;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using IQSMART.Shared.BusinessLayer.BusinessService.MasterService.AccountsService;
using IQSMART.Shared.BusinessLayer.BusinessService.OutSourcingService.DebitNote_Service;
using IQSMART.Shared.BusinessLayer.BusinessService.ReportService.RatingService;
using IQSMART.Shared.BusinessLayer.BusinessService.ReportService.TaxDeatilsReportService;
using IQSMART.Shared.BusinessLayer.BusinessService.ReportService.TrackReportService;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.IRatingService;
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
using V.SMART.Shared.Repository.IRepository.IPlanningRepository.IJobOrderRepo;
using V.SMART.Shared.Repository.IRepository.IProductionRepository.IProductionIssueWOAssyRepo;
using V.SMART.Shared.Repository.IRepository.IReportRepository.ITrackReportRepository;
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
using V.SMART.Shared.Repository.PlanningRepository.JobOrderRepo;
using V.SMART.Shared.Repository.ProductionRepository.ProductionIssueWOAssyRepo;
using V.SMART.Shared.Repository.ReportRepository.TrackReportRepository;
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
using V.SMART.Shared.Repository.IRepository.IPlanningRepository.IRouteCardRepo;
using V.SMART.Shared.Repository.IRepository.IProductionRepository.IProductionReturnAssyRepo;
using V.SMART.Shared.Repository.IRepository.IProductionRepository.IProductionSCNAssyRepo;
using V.SMART.Shared.Repository.PlanningRepository.RouteCardRepo;
using V.SMART.Shared.Repository.ProductionRepository.ProductionReturnAssyRepo;
using V.SMART.Shared.Repository.ProductionRepository.ProductionSCNAssyRepo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace V.SMART.Shared.DependencyInjection
{
    /// <summary>
    /// Registers every host-agnostic V.SMART domain service, repository, mapping and
    /// data-access component. Called once by each of the three hosts.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the V.SMART domain object graph to <paramref name="services"/>.
        /// </summary>
        /// <remarks>
        /// The host remains responsible for the seams: <c>IPathProvider</c>,
        /// <c>IFileOpener</c>, <c>IFileUploadService</c>, a bare <c>HttpClient</c>,
        /// <c>AuthenticationStateProvider</c>, <c>AddHttpClient()</c> and, in Blazor hosts,
        /// <c>IJSRuntime</c>.
        /// <para>
        /// <c>V.SMART.Api</c> supplies <c>AuthenticationStateProvider</c> and
        /// <c>AddHttpClient()</c> but none of the rest, so exactly seven registrations here
        /// stay unresolvable in that host until M2-B06 / M2-B08 — that gap is expected, not a
        /// defect: <c>ReportService</c> (IPathProvider), <c>IUserService</c> (IPathProvider +
        /// IJSRuntime), <c>IGSTITCService</c> (IPathProvider),
        /// <c>IUserThemePreferenceService</c> (IJSRuntime), <c>ICompanyService</c>
        /// (IFileUploadService), <c>IItemService</c> (IFileUploadService) and
        /// <c>IEnquirySalesService</c> (IPathProvider, transitively via <c>ReportService</c>).
        /// The count was measured by starting V.SMART.Api with ValidateOnBuild = true, which
        /// is why that host now opts out of build-time validation explicitly
        /// (V.SMART.Api/Program.cs) — ASP.NET Core turns ValidateOnBuild on by itself in the
        /// Development environment, so leaving the line unwritten does not leave it off.
        /// </para>
        /// <para>
        /// Verified by <c>tests/V.SMART.Shared.Tests/DependencyInjection/AddVSmartDomainTests.cs</c>:
        /// with the seams supplied, the whole graph passes
        /// <c>BuildServiceProvider(validateScopes: true, validateOnBuild: true)</c>.
        /// </para>
        /// </remarks>
        public static IServiceCollection AddVSmartDomain(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            #region Infrastructure — data access, tenancy, mapping, cross-cutting

            // The environment variable is read first because MauiAppBuilder.Configuration has
            // no environment-variable provider by default, unlike WebApplicationBuilder
            // (M0-03-02). For the two web hosts the two sources agree, because their
            // configuration already includes the environment-variable provider.
            var masterDbConnectionString =
                Environment.GetEnvironmentVariable("ConnectionStrings__MasterDb")
                ?? configuration.GetConnectionString("MasterDb");

            if (string.IsNullOrWhiteSpace(masterDbConnectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'ConnectionStrings:MasterDb' is not configured." +
                    Environment.NewLine +
                    "Set the environment variable ConnectionStrings__MasterDb before starting the host," +
                    Environment.NewLine +
                    "    PowerShell:  $env:ConnectionStrings__MasterDb = \"<master connection string>\"" +
                    Environment.NewLine +
                    "There is deliberately no default value. See docs/CONFIGURATION.md.");
            }

            services.AddDbContext<MasterDbContext>(options =>
                options.UseSqlServer(masterDbConnectionString));

            // Tenant-aware context factory
            services.AddScoped<ITenantProvider, TenantProvider>();
            services.AddScoped<ITenantDbContextFactory, TenantDbContextFactory>();

            // Tenant-aware ApplicationDbContext — resolved through the factory, NEVER
            // AddDbContext. Replacing this shape would silently break database-per-tenant
            // isolation.
            services.AddScoped<ApplicationDbContext>(sp =>
            {
                var factory = sp.GetRequiredService<ITenantDbContextFactory>();
                return factory.CreateDbContext();
            });

            // Register AutoMapper: scan the assembly that contains MappingProfileMarker
            services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(typeof(MappingProfileMarker).Assembly);
            });

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<UserSession>();
            services.AddScoped<CurrentUserService>();
            services.AddScoped<ILoggingService, FileLoggingService>();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddScoped<ExcelExportService>();

            // Domain, not host UI, despite having been registered next to the UI services in
            // both former composition roots: ExcelTemplateService takes only IUnitOfWork,
            // ICommonService, CurrentUserService and ILoggingService
            // (V.SMART/V.SMART.Shared/Services/ExcelTemplateService.cs:27-32), and seven
            // business services take IExcelTemplateService in their constructors — leaving it
            // host-side would make all seven unresolvable in V.SMART.Api, which is exactly the
            // hole this task exists to close.
            services.AddScoped<IExcelTemplateService, ExcelTemplateService>();

            services.AddScoped<ICorrespondanceRepository, CorrespondanceRepository>();

            // Requires IPathProvider from the host (see the remarks on this method).
            services.AddScoped<ReportService>();

            #endregion

            #region Repositories and business services — moved verbatim from V.SMART.Web/Program.cs


            // ===============================
            // Repositories
            // ===============================

            // Screen & User
            services.AddScoped<IScreenRepository, ScreenRepository>();
            services.AddScoped<IScreenManagementRepository, ScreenManagementRepository>();


            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserRightsRepository, UserRightsRepository>();
            services.AddScoped<IUserAuthorityRepository, UserAuthorityRepository>();



            // Master Data
            services.AddScoped<IUserRightService, UserRightService>();
            services.AddScoped<IUserThemePreferenceService, UserThemePreferenceService>();

            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IUOMRepository, UOMRepository>();
            services.AddScoped<IStoreRepository, StoreRepository>();
            services.AddScoped<IRawMaterialRepository, RawMaterialRepository>();
            services.AddScoped<IFactorRepository, FactorRepository>();
            services.AddScoped<IProcessRepository, ProcessRepository>();
            services.AddScoped<IMachineRepository, MachineRepository>();
            services.AddScoped<IGroupingRepository, GroupingRepository>();
            services.AddScoped<IGroupingSubRepository, GroupingSubRepository>();
            services.AddScoped<IItemRepository, ItemRepository>();
            services.AddScoped<IHSNMasterRepository, HSNMasterRepository>();
            services.AddScoped<IItemProductAssignRepository, ItemProductAssignRepository>();
            services.AddScoped<IAssemblyRepository, AssemblyRepository>();
            services.AddScoped<ICompMasterRepository, CompMasterRepository>();
            services.AddScoped<IItemSubRepository, ItemSubRepository>();
            services.AddScoped<IProcessFlowChartRepository, ProcessFlowChartRepository>();
            services.AddScoped<IStateRepository, StateRepository>();
            services.AddScoped<IAssemblyDefLabourRepository, AssemblyDefLabourRepository>();

            // Customer & Vendor
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ICustomerIndirectRepository, CustomerIndirectRepository>();
            services.AddScoped<IContactPersonRepository, ContactPersonRepository>();
            services.AddScoped<IVendorRepository, VendorRepository>();
            services.AddScoped<IVendorContactRepository, VendorContactRepository>();
            services.AddScoped<IVendorInDirectRepository, VendorInDirectRepository>();

            // Finance
            services.AddScoped<IExpenseRepository, ExpenseRepository>();
            services.AddScoped<IIncomeRepository, IncomeRepository>();
            services.AddScoped<ICurrencyRepository, CurrencyRepository>();
            services.AddScoped<ICurrencyTodayRepository, CurrencyTodayRepository>();
            services.AddScoped<IBanksRepository, BanksRepository>();
            services.AddScoped<ICostCenterRepository, CostCenterRepository>();

            // HR
            services.AddScoped<IStaffRepository, StaffRepository>();
            services.AddScoped<IStaffFamilyDetailRepository, StaffFamilyDetailRepository>();
            services.AddScoped<IStaffEducationRepository, StaffEducationRepository>();
            services.AddScoped<IStaffEmergencyRepository, StaffEmergencyRepository>();
            services.AddScoped<ILeaveTypeRepository, LeaveTypeRepository>();
            services.AddScoped<IEmployeeLeaveBalanceRepository, EmployeeLeaveBalanceRepository>();
            services.AddScoped<ILeaveApplicationRepository, LeaveApplicationRepository>();
            services.AddScoped<IDailyLeaveRecordRepository, DailyLeaveRecordRepository>();
            services.AddScoped<IShiftAllocationRepository, ShiftAllocationRepository>();
            services.AddScoped<IHolidayListRepository, HolidayListRepository>();

            // Company
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<IProjectTypeMasterRepository, ProjectTypeMasterRepository>();

            // Sales
            services.AddScoped<ILeadsRepository, LeadsRepository>();
            services.AddScoped<IEnquirySalesRepository, EnquirySalesRepository>();
            services.AddScoped<IEnquirySalesSubRepository, EnquirySalesSubRepository>();
            services.AddScoped<IMfgQuoteRepository, MfgQuoteRepository>();
            services.AddScoped<IMfgQuoteSubRepository, MfgQuoteSubRepository>();

            services.AddScoped<IMfgPoRepository, MfgPoRepository>();
            services.AddScoped<IMfgPoSubRepository, MfgPoSubRepository>();

            services.AddScoped<IPerformaInvRepository, PerformaInvRepository>();
            services.AddScoped<IPerformaInvSubRepository, PerformaInvSubRepository>();

            services.AddScoped<IMfgDcRepository, MfgDcRepository>();
            services.AddScoped<IMfgDcSubRepository, MfgDcSubRepository>();

            services.AddScoped<IMfgInvRepository, MfgInvRepository>();
            services.AddScoped<IMfgInvSubRepository, MfgInvSubRepository>();

            // Approval
            services.AddScoped<IApprovalHistoryRepository, ApprovalHistoryRepository>();

            //Inventory - Stock
            services.AddScoped<ISCNGenRepository, SCNGenRepository>();
            services.AddScoped<ISCNGenSubRepository, SCNGenSubRepository>();
            services.AddScoped<IStockAddRepository, StockAddRepository>();
            services.AddScoped<IStockIssueRepository, StockIssueRepository>();

            services.AddScoped<IStoreInterTransService, StoreInterTransService>();

            //Planning
            services.AddScoped<IProductionIssueAssyRepository, ProductionIssueAssyRepository>();
            services.AddScoped<IProductionIssueAssySubRepository, ProductionIssueAssySubRepository>();
            services.AddScoped<IJobOrderRepository, JobOrderRepository>();
            services.AddScoped<IJobOrderSubRepository, JobOrderSubRepository>();

            // Terms & Correspondence
            services.AddScoped<ITermsAndConditionsRepository, TermsAndConditionsRepository>();
            services.AddScoped<ICorrespondanceRepository, CorrespondanceRepository>();
            services.AddScoped<IStoreMapRepository, StoreMapRepository>();

            // ===============================
            // Services
            // ===============================

            //BarCode And QR Code
            services.AddScoped<BarcodeService>();


            //Screen & User Services
            services.AddScoped<IPrintSettingService, PrintSettingService>();
            services.AddScoped<IProdLogSettingsService, ProdLogSettingService>();
            services.AddScoped<IHRMasterService, HRMasterService>();
            services.AddScoped<IBiometricExcelSettingService, BiometricExcelSettingService>();
            services.AddScoped<ISalaryHeadPrintSettingService, SalaryHeadPrintSettingService>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserAuthorityService, UserAuthorityservice>();


            //Incentory Services
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProcessService, ProcessService>();
            services.AddScoped<IFactorservice, FactorService>();
            services.AddScoped<IGroupingService, GroupingService>();
            services.AddScoped<IStoreService, StoreService>();
            services.AddScoped<IHSNService, HSNService>();
            services.AddScoped<IRawMaterialService, RawMaterialService>();
            services.AddScoped<IStoreMappingService, StoreMapService>();
            services.AddScoped<IAssemblyDefLabourService, AssemblyDefLabourService>();

            // Accounts Service
            services.AddScoped<IExpenseService, ExpenseService>();
            services.AddScoped<IIncomeService, IncomeService>();
            services.AddScoped<ICurrencyService, CurrencyService>();

            //General Services for Master Data
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IVendorService, VendorService>();
            services.AddScoped<IMachineService, MachineService>();

            //HumanResource Services
            services.AddScoped<ILeaveTypeService, LeaveTypeService>();
            services.AddScoped<IEmployeeLeavebalanceService, EmployeeLeavebalanceService>();
            services.AddScoped<IShiftAllocationService, ShiftAllocationService>();
            services.AddScoped<ICandidateService, CandidateService>();
            services.AddScoped<IOfferLetterService, OfferLetterService>();
            services.AddScoped<IAppointmentLetterService, AppointmentLetterService>();


            services.AddScoped<ILeaveApplicationService, LeaveApplicationService>();
            services.AddScoped<IBankService, BankService>();
            services.AddScoped<ICompanyService, CompanyService>();


            services.AddScoped<ITermsAndConditionsService, TermsAndConditionsService>();


            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<ICostCenterService, CostCenterService>();


            // Common services for Master Data

            services.AddScoped<IDashboardService, DashboardService>();

            services.AddScoped<ForeignKeyUsageChecker>();


            services.AddScoped<ICommonService, CommonService>();
            services.AddScoped<ICalculationService, CalculationService>();

            services.AddScoped<IEmployeeService, EmployeeService>();

            services.AddScoped<IAssemblyDefService, AssemblyDefService>();

            services.AddScoped<ILeadService, LeadService>();
            services.AddScoped<IEnquirySalesService, EnquirySalesService>();
            services.AddScoped<IEnqiryFeasibilityService, EnqiryFeasibilityService>();

            services.AddScoped<IMfgQuotationService, MfgQuotationService>();
            services.AddScoped<IMfgPoService, MfgPoService>();
            services.AddScoped<IContractReviewService, ContractReviewService>();

            services.AddScoped<IPerformaInvService, PerformaInvService>();
            services.AddScoped<IMfgDcService, MfgDcService>();

            services.AddScoped<IMfgInvService, MfgInvService>();
            services.AddScoped<IExpInvService, ExpInvService>();


            services.AddScoped<ILabourDcOutgoingService, LabourDcOutgoingService>();
            services.AddScoped<ILabourGRNService, LabourGRNService>();
            services.AddScoped<ILabourSCNService, LabourSCNService>();
            services.AddScoped<ILabourInvoiceService, LabourInvoiceService>();

            services.AddScoped<ICreditNoteService, CreditNoteService>();

            //OutSourcing Service
            services.AddScoped<IMaterialReqService, MaterialReqService>();

            services.AddScoped<IEnquiryPurchaseService, EnquiryPurchaseService>();
            services.AddScoped<IEnquiryPurchaseService, EnquiryPurchaseService>();
            services.AddScoped<IPurchaseQuoteService, PurchaseQuoteService>();
            services.AddScoped<IPurchPoService, PuchPoService>();
            services.AddScoped<PoSelectionService>();

            services.AddScoped<IPurchaseGRNService, PurchaseGRNService>();
            services.AddScoped<IPurchaseSCNService, PurchaseSCNService>();
            services.AddScoped<IPurchaseInvoiceService, PurchaseInvoiceService>();

            services.AddScoped<ISubConDcOutService, SubConDcOutService>();
            services.AddScoped<ISubConGRNService, SubConGRNService>();
            services.AddScoped<ISubConSCNService, SubConSCNService>();
            services.AddScoped<ISubConInvService, SubConInvService>();

            services.AddScoped<IDebitNoteService, DebitNoteService>();

            //Inventory -Stock Service
            services.AddScoped<IStockAddIssPosition, StockAddIssPosition>();
            services.AddScoped<ISCNGenService, SCNGenService>();
            services.AddScoped<IStockManagerService, StockManagerService>();
            services.AddScoped<IMINService, MINService>();

            services.AddScoped<IToolCribIssueService, ToolCribIssueService>();
            services.AddScoped<IToolCribReturnService, ToolCribReturnService>();

            //Planning Service
            services.AddScoped<IMaterialReqAnalysisService, MaterialReqAnalysisService>();
            services.AddScoped<IJobOrderService, JobOrderService>();
            services.AddScoped<IProductionIssueAssyService, ProductionIssueAssyService>();

            services.AddScoped<IRcReleaseService, RcReleaseService>();
            services.AddScoped<IRouteCardService, RouteCardService>();
            services.AddScoped<IProcessLogService, ProcessLogService>();

            services.AddScoped<IEstimateService, EstimateService>(); //Estimation

            services.AddScoped<IProductionReturnAssyService, ProductionReturnAssyService>();
            services.AddScoped<IProductionSCNAssyservice, ProductionSCNAssyService>();

            services.AddScoped<IProductionLogService, ProductionLogService>();

            services.AddScoped<IProductionIssueCompService, ProductionIssueCompService>();
            services.AddScoped<IProductionReturnCompService, ProductionReturnCompService>();
            services.AddScoped<IProductionSCNCompService, ProductionSCNCompService>();

            //Maintainance Services
            services.AddScoped<IMaintenanceScheduleService, MaintenanceScheduleService>();
            services.AddScoped<IMaintenanceProcessService, MaintenanceProcessService>();
            services.AddScoped<IBreakdownMaintenanceService, BreakdownMaintenanceService>();
            services.AddScoped<ICalibrationHistoryAndMaintenanceService, CalibrationHistoryAndMaintenanceService>();

            //Inspection Services
            services.AddScoped<IMasterInspectionService, MasterInspectionService>();
            services.AddScoped<IFinalInspectionService, FinalInspectionService>();
            services.AddScoped<IIncomingInspectionService, IncomingInspectionService>();
            services.AddScoped<IDefectInfoService, DefectInfoService>();

            services.AddScoped<IInspectionSettingsService, InspectionSettingsService>();
            services.AddScoped<IAssemblyRequirementService, AssemblyRequirementService>();

            services.AddScoped<IApprovalService, ApprovalService>();

            //Eway
            services.AddScoped<IEWayDatabaseService, EWayDatabaseService>();
            //EInvoice
            services.AddScoped<IEinvoiceDatabaseService, EinvoiceDatabaseService>();
            //SNS
            services.AddScoped<ICostingService, CostingService>();




            // Report Service
            services.AddScoped<IReportExecutor, ReportExecutor>();
            services.AddScoped<ILabourTrackRepository, LabourTrackRepository>();
            //Purchase Sales Track Report
            services.AddScoped<IPurchaseSalesTrackReportServices, PurchaseSalesTrackReportServices>();

            // Track Reports Service
            services.AddScoped<ILabourTrackReportService, LabourTrackReportService>();
            services.AddScoped<ISalesTrackReportService, SalesTrackReportService>();
            services.AddScoped<IViewTallyDCInOutTrackService, ViewTallyDCInOutTrackService>();
            services.AddScoped<IPOTrackService, POTrackService>();
            services.AddScoped<IJobOrdeAnalysisService, JobOrderAnalysisService>();
            services.AddScoped<IPoPendingServices, PoPendingServices>();
            services.AddScoped<ILabourPendingService, LabourPendingService>();
            services.AddScoped<IProductionPendingService, ProductionPendingService>();

            services.AddScoped<IItemWiseReportService, ItemWiseReportService>();
            services.AddScoped<IDaybookService, DaybookService>();
            services.AddScoped<IToolCribServices, ToolCribServices>();
            services.AddScoped<ITaxDetailsService, TaxDetailsService>();

            //Stock Analysis
            services.AddScoped<IStockLedgerReportService, StockLedgerReportService>();
            services.AddScoped<IStockAnalysisService, StockAnalysisService>();
            services.AddScoped<IRouteCardAnalysisService, RouteCardAnalysisService>();
            services.AddScoped<IItemModificationReportServices, ItemModificationReportServices>();
            services.AddScoped<IRejectionService, RejectionService>();
            services.AddScoped<IRatingService, GetRatingsService>();
            services.AddScoped<IPrPoRatingService, PrPoRatingService>();


            //Human Resource
            services.AddScoped<IAttendanceService, AttendanceService>();
            services.AddScoped<ISalaryService, SalaryService>();
            services.AddScoped<IStaffLoanService, StaffLoanService>();

            //Accounts Service
            services.AddScoped<IGSTITCService, GSTITCService>();
            services.AddScoped<IPaymentsService, PaymentsService>();
            services.AddScoped<IAdvaceAdjustmentService, AdvaceAdjustmentService>();
            services.AddScoped<IFundTransRepository, FundTransRepository>();
            services.AddScoped<IReceiptsService, ReceiptsService>();
            services.AddScoped<ITDSSummaryService, TDSSummaryService>();
            services.AddScoped<IHSNSummaryService, HSNSummaryService>();
            services.AddScoped<ICreditDebitSummaryService, CreditDebitSummaryService>();

            //CashFlow Services
            services.AddScoped<IServiceBillsService, ServiceBillsService>();

            services.AddScoped<IPendingBillsService, PendingBillsService>();
            services.AddScoped<IPaidBillsService, PaidBillsService>();
            services.AddScoped<IPendingStatementsService, PendingStatementsService>();
            services.AddScoped<IProfit_LossAccountService, Profit_LossAccountsService>();
            services.AddScoped<ISummaryAndGraphsService, SummaryAndGraphsService>();

            //Confirmation of accounts
            services.AddScoped<IAccountReportService, AccountReportService>();
            services.AddScoped<IRejectionMasterService, RejectionMasterService>();
            services.AddScoped<IStockIssueRequestService, StockIssueRequestService>();



            //11072026-------------------------------------------------------------------
            services.AddScoped<IAppVersionService, AppVersionService>();

            #endregion

            #region Registered only in the MAUI host before M2-B07 — see KB-003 INV-039

            // These six were present in V.SMART/MauiProgram.cs but missing from
            // V.SMART.Web/Program.cs, so any Blazor screen that injected one threw a DI
            // resolution error. Unioning the two graphs is what a single composition root
            // does by construction. (Correction to INV-039's first pass: IContractReviewService
            // and IRouteCardService were NOT MAUI-only — V.SMART.Web/Program.cs:467 and :518 on
            // master register both. They are registered above with the rest of their modules.)
            services.AddScoped<IRouteCardRepository, RouteCardRepository>();
            services.AddScoped<IRouteCardSubRepository, RouteCardSubRepository>();
            services.AddScoped<IProductionReturnAssyRepository, ProductionReturnAssyRepository>();
            services.AddScoped<IProductionReturnAssySubRepository, ProductionReturnAssySubRepository>();
            services.AddScoped<IProductionSCNAssyRepository, ProductionSCNAssyRepository>();
            services.AddScoped<IProductionSCNAssySubRepository, ProductionSCNAssySubRepository>();

            #endregion

            return services;
        }
    }
}
