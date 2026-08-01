
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.CashFlow.ServiceBills;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.CashFlowViewModel.ServiceBillViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchPoVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ICashFlowService
{
    public interface IServiceBillsService
    {
        //--->Common Services
        Task<List<Currency>> GetCurrenciesAsync();

        Task<Companydetails> GetCompanyDetailsAsync();
        Task<VendorVM> GetVendorByIdAsync(int id);
        Task<int> GetDecimalPlacesAsync();
        Task<CustomerVM> GetCustomerById(int custid);
        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);
        Task<Currency?> GetCurrencyByIdAsync(int currId);
        Task<decimal?> GetLatestCurrencyValueAsync(int currId);
        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<IEnumerable<VendorVM>> SearchVendorsAsync(string searchText);
        Task<List<Dictionary<string, object>>> GetPoDetailsByCustId(int custId);


        //--->Developer Defined Services
        Task<IEnumerable<ServiceBillsVM>> GetAllServicesAsync();

        Task<List<Dictionary<string, object>>> GetPoDetailsByVendorcode(int vendorcode);

        Task<(List<ServiceBillsVM> serviceBillsVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
       
        Task<ServiceBillsSubVM> GetServiceSubItemDetailByServiceSubIdAsync(int ServiceSubId);

        Task<bool> DeleteServiceByIdAsync(int ServiceId);

        Task<ServiceBillsVM?> GetServiceByServiceIdAsync(int ServiceId);

        Task<ServiceBills?> GetLastServiceAsync(int custId);

        Task DeleteAndResequenceAsync(ServiceBillsSubVM subitem, ServiceBillsVM service);

        Task<List<ServiceBillsSubVM>> GetServiceSubByServiceIdAsync(int serviceId);

        Task<ServiceBillsVM> UpsertServiceAsync(ServiceBillsVM serviceVM);

        Task<List<MfgPoVM>> GetAllPOsByCustId(int CustId);
    
        Task<List<MfgPoSubVM>> GetPOSubByPoIdAsync(int poId);

        Task<List<PurchPoSubVM>> GetPurchPOSubByPoIdAsync(int poId);

        Task<String> GenerateNewServiceNoAsync();

        Task<bool> CheckDuplicate(String InvNo);

        Task UpdateItemCancelAndAddorRevertAsync(ServiceBillsSubVM subItem, int serviceId, bool isincoming);

        Task UpsertServiceBillsShortCloseAsync(ServiceBillsVM servicebills);

        Task UpdatedCancelStatusAndAddOrRevertQty(ServiceBillsVM servicebills);

        Task<List<ServiceBillsSubVM>> GetDistinctRefPoByServiceIdAsync(int serviceId);
    }
}
