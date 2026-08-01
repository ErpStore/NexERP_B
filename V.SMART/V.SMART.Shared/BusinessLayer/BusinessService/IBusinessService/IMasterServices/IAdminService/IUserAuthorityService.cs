using V.SMART.Shared.ViewModels.MasterViewModel.AdminViewmodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IAdminService
{
    public interface IUserAuthorityService
    {
        Task<(List<UserAuthorityVM> userAuthVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
        Task<bool> DeleteUserAuthByIdAsync(int id);
        Task<List<UserVM>> GetLevelAuthorizedUsersAsync();
        Task<UserAuthorityVM?> GetUserLevelAuthByIdAsync(int id);
        Task<UserAuthorityVM?> GetUserLevelAuthByUserIdAsync(int userId);
        Task<UserAuthorityVM> UpsertUserAuthAsync(UserAuthorityVM vm);
        Task<List<UserAuthorityVM>> GetAllUserAuthoritiesAsync();

    }
}
