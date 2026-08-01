
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.HistoryVM;
using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService
{
    public interface IDaybookService
    {
        Task<List<DaybookVM>> GetDayBookAsync( DateTime fromDate,  DateTime toDate);

        Task<List<CashFlowVM>> GetCashFlowAsync(DateTime fromDate,DateTime toDate);
    }
}
