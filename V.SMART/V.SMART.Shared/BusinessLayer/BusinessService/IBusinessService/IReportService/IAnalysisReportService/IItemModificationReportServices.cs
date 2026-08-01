
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.LabourTrackViewModel;


namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService
{
    public interface IItemModificationReportServices
    {
        Task<List<ItemModificationReportVM>> GetReport(DateTime fromDate,DateTime toDate,string reportType);
    }
}
