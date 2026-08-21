using V.SMART.Shared.Data.Master.Admin_Module;
using V.SMART.Shared.ViewModels.PlanningViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IPlanningService
{
    public interface IApprovalService
    {
        Task<List<ApprovalVM>> GetPendingApprovalsAsync(string type, string level, string userName);
        Task<bool> ApproveAsync(ApprovalVM record, string type, string level, string userName, UserAuthority authority);
        Task<bool> RejectAsync(ApprovalVM record, string type, string level, string reason, string userName);
        Task<bool> BulkApproveAsync(List<ApprovalVM> records, string type, string level, string username);
        Task<bool> BulkRejectAsync(List<ApprovalVM> records, string type, string level, string reason, string username);
    }

}
