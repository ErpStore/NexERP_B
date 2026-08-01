
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.AccountsModule;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.ViewModels.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IAccountsService
{
    public interface IReceiptsService
    {
        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);
        Task<CustomerVM?> GetCustomerByIdAsync(int Custid);
        Task<ReceiptsVM?> GetReceiptsByIdAsync(int poId);
        Task<string> GenerateReceiptsNumberAsync();
        Task<List<BillsVM>> GetBillsByCustomerAsync(int vendorCode, string billType);
        Task<List<PartyVM>> GetCustomerByIncomeTypeAsync(string type, string paymenttypename);

        Task<ReceiptsVM> UpsertReceiptsAsync(ReceiptsVM PaymentsVMs, bool BillAdjust);
        Task<IEnumerable<ReceiptsVM>> GetAllReceiptsAsync();
        Task<(List<ReceiptsVM> ReceiptsVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
        Task UpdatePendingBalanceAsync(string receiptType, List<ReceiptsSubVM> newItems, List<ReceiptsSub> oldItems);
        Task<bool> DeletePaymentAsync(int paymentId);
        Task<bool> DeleteReceiptSubidAsync(int paymentId);

        Task<List<PartyVM>> SearchPartysAsync(string searchText);
        Task<decimal> GetAdvanceAmountPaid(string type, string paymentType, int PartyCode);

        Task<decimal> GetReceiptsSubAmountByIdAsync(int receiptsSubId);


        ///////////////adavacnce//////////////
        Task<List<PartyVM>> GetVendorsByPurchaseTypeAdvacnceAsync(string type, string paymentType);
        Task<List<PartyVM>> GetCustomerByReceiptTypeAdvacnceAsync(string type, string paymentType);
        Task<Companydetails?> GetCompanyDetailsAsync();

    }
}
