using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionLogViewModel;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProuctionCompViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IPlanningService
{
    public interface IProcessLogService
    {
        Task<List<ProductionLogVM>> GetDailyProductionLogsAsync(int rcSubId);
        Task<List<ProductionIssueCompVM>> GetProductionIssueLogsAsync(int rcSubId);
        Task<List<ProductionReturnCompVM>> GetProductionReturnLogsAsync(int rcSubId);
        Task<List<ProductionSCNCompVM>> GetProductionSCNLogsAsync(int rcSubId);
    }
}
