

using Microsoft.Data.SqlClient;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.ReportService.TrackReportService
{
    public class ToolCribServices : IToolCribServices
    {
        private readonly IReportExecutor _report;

        public ToolCribServices(IReportExecutor report)
        {
            _report = report;
        }

        public async Task<List<ToolCribVM>> GetReport(
            DateTime? fromDate,
            DateTime? toDate,
            string? type)
        {
            var parameters = new[]
            {
                new SqlParameter(
                    "@FromDate",
                    fromDate ?? (object)DBNull.Value),

                new SqlParameter(
                    "@ToDate",
                    toDate ?? (object)DBNull.Value),

                new SqlParameter(
                    "@Type",
                    string.IsNullOrWhiteSpace(type)
                        ? DBNull.Value
                        : type)
            };

            try
            {
                var result = await _report.ExecuteAsync<ToolCribVM>(
                    "sp_GetToolCribIssueReport",
                    parameters);

                return result ?? new List<ToolCribVM>();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}