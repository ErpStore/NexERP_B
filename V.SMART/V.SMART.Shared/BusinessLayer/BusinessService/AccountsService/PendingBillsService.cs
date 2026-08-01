using DocumentFormat.OpenXml.Presentation;
using FastReport;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.LabourTrackViewModel;


namespace V.SMART.Shared.BusinessLayer.BusinessService.ReportService.TrackReportService
{
    public class PendingBillsService : IPendingBillsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IReportExecutor _report;


        public PendingBillsService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            CurrentUserService userService,
            ILoggingService logs,
            IReportExecutor report)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _logs = logs;
            _report = report;

        }

        public async Task<List<PendingBillVM>> GetPendingBills(string? billType,DateTime? fromDate,DateTime? toDate)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@BillType", billType ?? (object)DBNull.Value),
                    new SqlParameter("@FromDate", fromDate ?? (object)DBNull.Value),
                    new SqlParameter("@ToDate", toDate ?? (object)DBNull.Value)
                };

                var result = await _report.ExecuteAsync<PendingBillVM>(
                    "SP_BillPending",
                    parameters
                );

                return result ?? new List<PendingBillVM>();
            }
            catch (Exception ex)
            {
                _logs.LogDeveloperError(
                    ex,
                    "Error executing stored procedure SP_BillPending"
                );

                return new List<PendingBillVM>();
            }
        }

        public async Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
        {
            try
            {
                return await _unitOfWork.Correspondances.GetQueryable()
                    .Where(c => c.ReferenceId == refId && c.ReferenceType == refType)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error counting attachments for RefId={refId}, RefType={refType}.");
                return 0;
            }
        }
    }


}


