using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IAccountsService
{
    public interface IIncomeService
    {
        Task<(List<IncomeVM> incomeVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
        Task<(bool CanDelete, string Message)> CanDeleteIncomeAsync(int id);

        Task<bool> DeleteIncomeByIncomeIdAsync(int incomeCode);

    }
}
