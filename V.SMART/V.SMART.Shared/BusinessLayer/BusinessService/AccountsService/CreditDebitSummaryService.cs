
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITaxDetailsReportService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.AccountsReportViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.ReportService.TaxDeatilsReportService
{
    public class CreditDebitSummaryService : ICreditDebitSummaryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;

        private readonly IReportExecutor _report;


        public CreditDebitSummaryService(
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

        public async Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText)
        {
            return await _commonService.SearchCustomersAsync(searchText);
        }

        //screens

        public async Task<int> GetScreenCodeByScreenNameAsync(string screenName)
                 => await _commonService.GetScreenCodeByScreenNameAsync(screenName);
        public async Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText)
            => await _commonService.SearchItemsAsync(searchText);

        public async Task<List<Customer>> GetSalesCustomersAsync()
        {
            return await (
                from mi in _unitOfWork.MfgInvs.GetQueryable()
                join c in _unitOfWork.Customers.GetQueryable()
                    on mi.CustId equals c.CustId

                select c
            )
            .Distinct()
            .OrderBy(x => x.CustName)
            .ToListAsync();
        }

        public async Task<List<CreditNoteDebitNoteSummaryVM>> GetCreditDebitSummaryReportAsync(string reportTopic, DateTime? fromDate, DateTime? toDate)
        {

            try
            {
                var result = await _report.ExecuteAsync<CreditNoteDebitNoteSummaryVM>(
                    "Sp_CreditDebitNoteSummaryReport",


                    new SqlParameter("@ReportType", reportTopic),

                    new SqlParameter("@FromDate", fromDate),

                    new SqlParameter("@ToDate", toDate)

                );

                return result.ToList();
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
