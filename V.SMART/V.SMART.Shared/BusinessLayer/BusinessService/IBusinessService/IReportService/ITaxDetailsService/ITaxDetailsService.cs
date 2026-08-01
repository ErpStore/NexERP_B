
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.TaxDetailsReportViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITaxDetailsReportService
{
    public interface ITaxDetailsService
    {
        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);
        Task<int> GetScreenCodeByScreenNameAsync(string screenName);
        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);
        Task<List<Customer>> GetSalesCustomersAsync();

        Task<List<TaxDetailsVM>> GetTaxDetailsReportAsync( int? custId, int? vendorCode, string topic, DateTime? fromDate,DateTime? toDate);
        Task<List<Customer>> GetPartiesByTopicAsync(string topic);

        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);

    }
}
