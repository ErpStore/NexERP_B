using V.SMART.Shared.Data.Enum;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.ViewModels.MasterViewModel.AdminViewmodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IAdminService
{
    public interface IUserService
    {

        Task<bool> IsProductionLogEnabledAsync(int userId);
        Task<List<State>> GetStatesAsync();

        Task<UserVM> UpsertUserAsync(UserVM vm);
        Task<(List<UserVM> userVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
        Task<(bool CanDelete, string Message)> CanDeleteUserAsync(int userId);

        Task<bool> DeleteUserbyUserIdAsync(int userId);
        Task<UserRole> GetCurrentUserRoleAsync(ClaimsPrincipal user);
        Task<UserVM?> GetUserForEditAsync(int userId);
        Task<UserVM> GetNewUserAsync();
        Task<List<Staff>> GetAvailableStaffAsync();
        Task<bool> IsAssignStatesFeatureEnabledAsync();

        Task<byte[]> PrintQrCards(int UserId);

        Task UpdateUserDeviceAsync(int userId);
    }
}
