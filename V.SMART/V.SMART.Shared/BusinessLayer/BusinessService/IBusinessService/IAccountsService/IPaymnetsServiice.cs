

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
    public interface IPaymentsService
    {
        Task<IEnumerable<VendorVM>> SearchVendorsAsync(string searchText);
        Task<VendorVM?> GetVendorByIdAsync(int VendorCode);
        Task<Companydetails?> GetCompanyDetailsAsync();
        Task<PaymentsVM?> GetPaymentsByIdAsync(int poId);
        Task<string> GeneratePaymentNumberAsync();
        Task<List<BillsVM>> GetBillsByVendorAsync(int vendorCode, string billType);
        Task<List<PartyVM>> GetVendorsByPurchaseTypeAsync(string type,string paymenttypename);

        Task<PaymentsVM> UpsertPaymentsAsync(PaymentsVM PaymentsVMs,bool BillAdjust);
        Task<IEnumerable<PaymentsVM>> GetAllPaymentsAsync();
        Task<(List<PaymentsVM> payments, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
        Task UpdatePendingBalanceAsync(string expenseType, List<PaymentSubVM> newItems, List<PaymentsSub> oldItems);
        Task<bool> DeletePaymentAsync(int paymentId);
        Task<bool> DeletePaymentSubidAsync(int paymentId);

        Task<List<PartyVM>> SearchPartysAsync(string searchText);
        Task<decimal> GetAdvanceAmountPaid(string type, string paymentType, int PartyCode);

        Task<decimal> GetPaymentSubAmountByIdAsync(int paymentSubId);


        ///////////////adavacnce//////////////
        Task<List<PartyVM>> GetVendorsByPurchaseTypeAdvacnceAsync(string type, string paymentType);
        // Task<List<Bank>> GetBanksAsync();

        ///////////////

    }
}
