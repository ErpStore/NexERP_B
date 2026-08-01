using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.LabourTrackViewModel;


namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService
{
    public interface ISalesTrackReportService
    {

        Task<List<SalesTrackVM>> ExportDataAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? customerId = null);

        


        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);

        public List<SalesTrackVM> GetPendingPo(
        List<SalesTrackVM> data,
        string docType,
        DateTime? fromDate = null,
        DateTime? toDate = null

       );


        Task<decimal> GetTotalInvoiceValue(
            DateTime? from,
            DateTime? to,
            int? selectedCustomerId);

        List<SalesTrackVM> GetItemsByPoId(
            List<SalesTrackVM> data,
            string docType,
            int poId
        );

       

        List<SalesTrackVM> GetCustomerDocTypes(
          List<SalesTrackVM> data,
          DateTime? fromDate = null,
          DateTime? toDate = null



      );

        List<SalesTrackVM> GetItemsByItemCode(
            List<SalesTrackVM> data,
            string itemCode
        );

    }
}
