using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.DashboadrViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IDashboardService
{
    public interface IDashboardService
    {
        // Summary cards — all 6 cards for a given period/date range
        Task<DashboardSummaryDto> GetSummaryAsync(DashboardFilterDto filter);

        // Tab-specific data
        Task<OverviewDto> GetOverviewAsync(DashboardFilterDto filter);
        Task<InventoryDto> GetInventoryAsync(DashboardFilterDto filter);
        Task<FinanceDto> GetFinanceAsync(DashboardFilterDto filter);
        Task<HrDto> GetHrAsync(DashboardFilterDto filter);
        Task<SalesDto> GetSalesAsync(DashboardFilterDto filter);


        // ── NEW ───────────────────────────────────────────────────────────────
        Task<LabourDto> GetLabourAsync(DashboardFilterDto filter);
        Task<PurchaseDto> GetPurchaseAsync(DashboardFilterDto filter);
        Task<SubcontractDto> GetSubcontractAsync(DashboardFilterDto filter);

        Task<JobCardDto> GetJobCardAsync(DashboardFilterDto filter);

        Task<RouteCardDto> GetRouteCardAsync(DashboardFilterDto filter);
        Task<AnalyticsDto?> GetAnalyticsAsync(DashboardFilterDto filter);

    }
}
