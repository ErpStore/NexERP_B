using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using FastReport;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IAccountsService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.AccountsViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.AccountsService
{
    public class SummaryAndGraphsService : ISummaryAndGraphsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        private readonly IReportExecutor _report;

        public SummaryAndGraphsService(IUnitOfWork unitOfWork,ICommonService commonService, IReportExecutor report,CurrentUserService userService,ILoggingService logs,IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _report = report;
            _currentUserService = userService;
            _logs = logs;
            _mapper = mapper;
        }

        public async Task<DataTable> GetPendingBills(string Doctype, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var parameters = new []
                {
                    new SqlParameter("@Doctype", Doctype ?? (object)DBNull.Value),
                    new SqlParameter("@FromDate", fromDate ?? (object)DBNull.Value),
                    new SqlParameter("@ToDate", toDate ?? (object)DBNull.Value)
                };
                var result = await _report.ExecuteDataTableAsync("SP_CustomerMonthSummary",parameters);

                return result ?? new DataTable();
            }
            catch (Exception ex)
            {
                _logs.LogDeveloperError(ex,"Error executing stored procedure SP_CustomerMonthSummary");
                return new DataTable();
            }
        }

    }
}
