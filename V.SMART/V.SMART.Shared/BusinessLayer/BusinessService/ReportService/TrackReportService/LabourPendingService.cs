using AutoMapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.ReportViewModel.AccountsVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.ReportService.TrackReportService
{
    public class LabourPendingService : ILabourPendingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        private readonly IReportExecutor _report;

        public LabourPendingService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            CurrentUserService userService,
            ILoggingService logs,
            IMapper mapper,
            IReportExecutor report)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _logs = logs;
            _mapper = mapper;
            _report = report;
        }

        public async Task<bool> IsPOWiseLabDcOutEnabledAsync()
             => await _commonService.GetScreenPermissionsAsync("Labour GRN", "PO Wise Labour DC Outgoing");

        public async Task<List<LabourPendingVM>> GetAccountsAsync(bool? IsPoWiseLabDcOut, bool? IsDetails, string? DocType, DateTime? fromDate, DateTime? toDate, int? custId)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@IsPoWise", IsPoWiseLabDcOut==null? DBNull.Value : IsPoWiseLabDcOut),
                    new SqlParameter("@IsDetails", IsDetails==null? DBNull.Value : IsDetails),
                    new SqlParameter("@DocType", string.IsNullOrWhiteSpace(DocType) ? DBNull.Value : DocType),
                    new SqlParameter("@fromDate", fromDate ?? (object)DBNull.Value),
                    new SqlParameter("@toDate", toDate ?? (object)DBNull.Value),
                    new SqlParameter("@custId", custId.HasValue ? custId.Value : DBNull.Value),
                };

                var result = await _report.ExecuteAsync<LabourPendingVM>("Sp_LabourPendingReport", parameters);
                return result ?? new List<LabourPendingVM>();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error executing stored procedure GetAccountsAsync");
                return new List<LabourPendingVM>();
            }
        }

        public async Task<List<LabourPendingSummaryVM>> GetAccountDetailsAsync(bool? IsPoWiseLabDcOut, bool? IsDetails, string? DocType, DateTime? fromDate, DateTime? toDate, int? custId)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@IsPoWise", IsPoWiseLabDcOut==null? DBNull.Value : IsPoWiseLabDcOut),
                    new SqlParameter("@IsDetails", IsDetails==null? DBNull.Value : IsDetails),
                    new SqlParameter("@DocType", string.IsNullOrWhiteSpace(DocType) ? DBNull.Value : DocType),
                    new SqlParameter("@fromDate", fromDate ?? (object)DBNull.Value),
                    new SqlParameter("@toDate", toDate ?? (object)DBNull.Value),
                    new SqlParameter("@custId", custId.HasValue ? custId.Value : DBNull.Value),
                };

                var result = await _report.ExecuteAsync<LabourPendingSummaryVM>("Sp_LabourPendingReport", parameters);
                return result ?? new List<LabourPendingSummaryVM>();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error executing stored procedure GetAccountsAsync");
                return new List<LabourPendingSummaryVM>();
            }
        }


    }
}
