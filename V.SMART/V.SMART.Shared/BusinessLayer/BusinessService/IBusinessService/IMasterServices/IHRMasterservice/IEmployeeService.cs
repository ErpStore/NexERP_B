using V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IHRMasterservice
{
    public interface IEmployeeService
    {
        Task<(List<StaffVM> staffVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber,int pageSize,Dictionary<string, object>? filters);
        Task<(bool Success, string Message)> DeleteEmployeeAsync(int staffId);
        Task<(bool CanDelete, string Message)> CanDeleteEmployeeAsync(int staffId);
    }
     
}
