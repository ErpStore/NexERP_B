

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
using V.SMART.Shared.DependencyInjection;
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

            // =========================
            // Domain graph — M2-B07
            // =========================
            // Every repository, UnitOfWork, business service, MasterDbContext, the
            // tenant-resolved ApplicationDbContext and AutoMapper now come from the single
            // shared composition root in V.SMART.Shared
            // (V.SMART.Shared/DependencyInjection/ServiceCollectionExtensions.cs), which all
            // three hosts call. This replaced ~230 registrations that used to be duplicated
            // here and in V.SMART.Web/Program.cs, and had already drifted (KB-060 R-26).
            //
            // The MasterDb connection string is still read from the environment variable
            // ConnectionStrings__MasterDb first, with a configuration fall-back, because
            // MauiAppBuilder.Configuration has no environment-variable provider by default
            // (M0-03-02, docs/CONFIGURATION.md). That precedence moved into the extension
            // unchanged, and this call stays ahead of the AddJsonFile below exactly as the
            // inline block it replaced did.
            builder.Services.AddVSmartDomain(builder.Configuration);

            // =========================
            // Host UI libraries
            // =========================
            builder.Services.AddMudServices();
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddApexCharts();
            builder.Services.AddAuthorizationCore();

            // HttpClient with timeout
            builder.Services.AddScoped<HttpClient>(sp =>
            {
                return new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(30)
                };
            });

            // =========================
            // Host platform seam — a different implementation in every host, so deliberately
            // NOT in AddVSmartDomain().
            // =========================
            builder.Services.AddScoped<IPathProvider, DesktopPathProvider>();
            builder.Services.AddScoped<IFileUploadService, MauiFileUploadService>();
            // M2-B07 note: Singleton here, Scoped in V.SMART.Web. The divergence is reported,
            // not silently changed — see KB-060 R-26 and the M2-B07 report.
            builder.Services.AddSingleton<IFileOpener, DesktopFileOpener>();

            // =========================
            // Host UI state
            // =========================
            builder.Services.AddScoped<ThemeStateService>();
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
            builder.Services.AddScoped<IColumnPreferenceService, ColumnPreferenceService>();
            builder.Services.AddScoped<IExcelTemplateService, ExcelTemplateService>();
            builder.Services.AddSingleton<SessionTimeoutService>();

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

