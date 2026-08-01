using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IAdminService
{
    public interface IUserRightService
    {
        Task SyncRightsForUserAsync(int userId);
    }
}
