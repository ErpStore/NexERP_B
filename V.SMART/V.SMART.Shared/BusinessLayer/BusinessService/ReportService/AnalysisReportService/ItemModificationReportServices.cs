
using Microsoft.Data.SqlClient;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.LabourTrackViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.ReportService.TrackReportService
{
    public class ItemModificationReportServices : IItemModificationReportServices
    {
        private readonly IReportExecutor report;

        public ItemModificationReportServices(IReportExecutor report)
        {
            this.report = report;
        }

        public async Task<List<ItemModificationReportVM>> GetReport(DateTime fromDate,DateTime toDate,string reportType)
        {
            var result = await report.ExecuteAsync<ItemModificationReportVM>(
                "Sp_GetItemModificationReport",

                new SqlParameter("@FromDate", fromDate),

                new SqlParameter("@ToDate", toDate),

                new SqlParameter("@ReportType", reportType)
            );

            return result;
        }
    }
}