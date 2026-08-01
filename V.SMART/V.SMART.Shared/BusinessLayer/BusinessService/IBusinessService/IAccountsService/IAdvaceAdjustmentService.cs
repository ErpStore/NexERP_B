
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.AccountsModule;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;

namespace V.SMARTV.Shared.BusinessLayer.BusinessService.IBusinessService.IAccountsService
{
    public  interface IAdvaceAdjustmentService
    {
        Task<IEnumerable<VendorVM>> SearchVendorsAsync(string searchText);
        Task<VendorVM?> GetVendorByIdAsync(int VendorCode);
        Task<AdvaceadjustmentVM> GetAdvaceAdjustsByIdAsync(int id);
        Task<string> GenerateAdvanceAdjustNumberAsync();

        Task<AdvaceadjustmentVM> UpsertAdvanceAdjustAsync(AdvaceadjustmentVM vm, bool BillAdjust);
        Task<(List<AdvaceadjustmentVM> advaceadjustments, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
        Task<bool> DeleteAdvanceAdjustmnetAsync(int paymentId);
        Task<decimal> GetAdvaceAdjustSubAmountByIdAsync(int AdjustSubId);
        
    }

}
