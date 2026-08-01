using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.PlanningViewModel.RouteCardViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.AnalysisReportViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.IAnalysisReportService
{
    public interface IRouteCardAnalysisService
    {
        
        Task<List<RouteCardVM>> GetAllRCCustomersAsync(DateTime fromDate,DateTime toDate);
        
        Task<List<RouteCardAnalysisVM>> GetRouteCardAnalysisAsync(DateTime fromDate, DateTime toDate);
        
        Task<List<RouteCardAnalysisDetailsVM>> GetRouteCardAnalysisDetailsAsync(int rcid);

    }

}
