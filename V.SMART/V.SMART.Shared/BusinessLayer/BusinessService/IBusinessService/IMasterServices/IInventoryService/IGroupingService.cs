using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IInventoryService
{
    public interface IGroupingService
    {
        Task<(List<GroupingVM> groupingVMs, int TotalCount)> 
            SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);

        Task<(bool CanDelete, string Message)> CanDeleteGroupingAsync(int grouppingId);
        Task<bool> DeleteGroupingByGroupingIdAsync(int groupingId);

    }
}
