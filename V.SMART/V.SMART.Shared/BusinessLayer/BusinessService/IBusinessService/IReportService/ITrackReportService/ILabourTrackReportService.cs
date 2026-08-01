using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.LabourTrackViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.PoPendingModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService
{
    public interface ILabourTrackReportService
    {

        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);
        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);


        List<string> GetCustomerDocTypes(
        List<LabourTrackSummaryVM> data,
        DateTime? fromDate = null,
        DateTime? toDate = null
        );


        List<LabourTrackSummaryVM> GetItemsByPoId(
           List<LabourTrackSummaryVM> data,
           string docType,
           int poId
       );

        // Interface
       
        Task<decimal> GetTotalInvoiceValue(
                DateTime? from,
                DateTime? to,
                int? selectedCustomerId);
        
        Task<List<string>> GetDocumentNumbersAsync(List<LabourTrackSummaryVM> data, string docType, DateTime? fromDate = null, DateTime? toDate = null);

        Task<List<LabourTrackSummaryVM>> GetLabourTrackSummaryAsync(int? custId, string docType, string docId, int? itemId, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
