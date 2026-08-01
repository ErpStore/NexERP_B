using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IGeneralService
{
    public interface ICustomerService
    {
        Task<(bool CanDelete, string Message)> CanDeleteCustomerAsync(int custId);
        Task<(List<CustomerVM> customerVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
        Task<bool> DeleteCustomerByCustIdAsync(int custId);

        Task<List<CustomerVM>> GetCustomerListAsync(string topic, DateTime fromDate, DateTime toDate);
    }
}
