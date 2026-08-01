using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.AccountsVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.IAccountReportService
{
    public interface IAccountReportService
    {
        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);
        Task<IEnumerable<VendorVM>> SearchVendorsAsync(string searchText);
        Task<List<ConfirmationOfAccountsVM>> GetAccountsAsync(string? Type, string? ExpenseType, DateTime? fromDate, DateTime? toDate, int? custId);

        Task<List<KeyValuePair<int, string>>> GetAllIncomeList();

        Task<List<KeyValuePair<int, string>>> GetAllExpenseList();

        Task<decimal> GetCustOpeningBalance(int custId, DateTime fromDate);

        Task<decimal> GetVendorOpeningBalance(int vendorId, DateTime fromDate);

        Task<decimal> GetCustSalesOpeningBalance(int custId, DateTime fromDate);

        Task<decimal> GetSubcontractOpeningBalance(int subcontractId, DateTime fromDate);

        Task<List<CustomerVM>> GetAllCustomersAsync(string type, DateTime fromdate, DateTime todate);

        Task<List<VendorVM>> GetAllVendorsAsync(string type, DateTime fromdate, DateTime todate);
    }
}
