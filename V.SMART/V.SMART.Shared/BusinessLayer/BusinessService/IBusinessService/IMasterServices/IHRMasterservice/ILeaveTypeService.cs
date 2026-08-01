using V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IHRMasterservice
{
    public interface ILeaveTypeService
    {
        Task<(List<LeaveTypeVM> leaveTypeVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
        Task<(bool CanDelete, string Message)> CanDeleteLeaveTypeAsync(int typeId);
        Task<bool> DeleteLeaveTypeByLeaveTypeIdAsync(int typeId);
        Task<LeaveTypeVM> GetByAttendanceCode(string code);
    }
}
