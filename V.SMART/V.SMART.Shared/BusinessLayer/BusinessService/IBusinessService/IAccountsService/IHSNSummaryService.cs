
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.AccountsReportViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITaxDetailsReportService
{
    public interface IHSNSummaryService
    {
        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);
        Task<int> GetScreenCodeByScreenNameAsync(string screenName);
        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);
        Task<List<Customer>> GetSalesCustomersAsync();

        Task<List<HSNSummaryVM>> GetHSNSummaryReportAsync(string reportTopic, DateTime? fromDate, DateTime? toDate);
    }
}
