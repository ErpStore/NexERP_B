

using Microsoft.Data.SqlClient;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.ViewModels.ReportViewModel.HistoryVM;
using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel;

namespace IQSMART.Shared.BusinessLayer.BusinessService.ReportService.TrackReportService
{
    public class DaybookService : IDaybookService
    {
        private readonly IReportExecutor _report;

        public DaybookService(IReportExecutor report)
        {
            _report = report;
        }

        public async Task<List<DaybookVM>> GetDayBookAsync( DateTime fromDate, DateTime toDate)
        {
            var parameters = new[]
            {
                new SqlParameter("@FromDate", fromDate),
                new SqlParameter("@ToDate", toDate)
            };

            try
            {
                var result = await _report.ExecuteAsync<DaybookVM>(
                    "SP_GetDayBook",
                    parameters);

                return result ?? new List<DaybookVM>();
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<List<CashFlowVM>> GetCashFlowAsync(DateTime fromDate,DateTime toDate)
        {
            var parameters = new[]
            {
                new SqlParameter("@FromDate", fromDate),
                new SqlParameter("@ToDate", toDate)
            };

            var result = await _report.ExecuteAsync<CashFlowVM>(
                "SP_GetCashFlow",
                parameters);

            return result ?? new List<CashFlowVM>();
        }
    }
}