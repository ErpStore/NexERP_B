using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.SalesAndLabour.PerformaInvoice;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.PerformaInvoiceVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISalesService
{
    public interface IPerformaInvService
    {
        // 🔹 Customer + Item data reused from Common Service

        Task<bool> GetLineIdByCustomerAsync(int custId);
        Task<List<CustomerVM>> GetCustomersAsync(int? custId = null);
        Task<CustomerVM?> GetCustomerByIdAsync(int custId);
        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);
        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<List<Currency>> GetCurrenciesAsync();
        Task<Currency?> GetCurrencyByIdAsync(int currId);
        Task<decimal?> GetLatestCurrencyValueAsync(int currId);
        Task<int> GetDecimalPlacesAsync();
        Task<List<ContactPerson>> GetContactPersonsAsync(int custId);
        Task<List<CustomerIndirect>> GetConsigneeAddressesAsync(int custId);
        Task<List<CostCenterVM>> GetCostCenterDetailsByCustId(int custId, HashSet<int> usedCostCenterIds);
        Task<Companydetails?> GetCompanyDetailsAsync();
        Task<bool> CheckAssemblyExistsAsync(int itemId);
        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);
        Task<List<Banks>> GetAllActiveBanksDetailaAsync();


        // 🔹 Performa-Invoice specific logic

        Task ValidatePoBalanceBeforeRevertAsync(PerformaInvSub sub);
        Task<(bool CanDelete, string Message)> CanDeletePerfromaInvAsync(int invId);
        Task UpdatedCancelStatusAndAddOrRevertQty(PerformaInvVM performaInvVM);
        Task<(List<PerformaInvVM> performaInvVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
        Task<string> GetPrefixFromDb();
        Task<List<PerformaInvSubVM>> GetDistinctRefPoByPerformaInvIdAsync(int invId);
        Task<string> GetPerformaInvNumberAsync(string suffix);
        Task<IEnumerable<PerformaInvVM>> GetAllInvDetailsAsync();
        Task<bool> DeleteInvByInvIdAsync(int invId);
        Task<PerformaInvVM?> GetPerformaByInvIdAsync(int invId);
        Task<decimal> GetPoItemPerformaBalQtyFromPoSubId(int poSubId);
        Task<decimal> GetExistingPerformaQtyByInvSubId(int InvSubId);
        Task<PerformaInvSubVM?> GetPerformsSubItemDetailByPerformaSubIdAsync(int invSubId);
        Task<PerformaInvVM> UpsertPerformaInvAsync(PerformaInvVM performaInvVM);
        Task DeleteAndResequenceAsync(PerformaInvSubVM subitem, PerformaInvVM performaVM);
        Task<List<PerformaInvSub>> GetPerformaSubByInvIdAsync(int invId);
        Task<List<Dictionary<string, object>>> GetPoDetailsByCustId(int custId);
        Task<int> GetPendingPoCountAsync(int custId);


        Task<bool> IsDuplicatePoAsync(string poNo, string suffix, int? currentPoId = null, int? custId = null);
        Task UpdateItemCancelAsync(MfgPoSubVM subItem);
        Task<string> GetNextOANumberAsync(string suffix);
        Task<string> GetLastOATermsAsync();
    }
}
