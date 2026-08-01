using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.LabourTrackViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.PoPendingModel;

using V.SMART.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

using V.SMART.Shared.Repository.IRepository.IReportRepository.ITrackReportRepository;
using DocumentFormat.OpenXml.Bibliography;

namespace V.SMART.Shared.BusinessLayer.BusinessService.ReportService.TrackReportService
{
    public class LabourTrackReportService : ILabourTrackReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly ILabourTrackRepository _labourTrackRepository;
        private readonly IReportExecutor _report;
        private decimal totalInvoiceValue;

        public LabourTrackReportService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            CurrentUserService userService,
            ILoggingService logs,
            ILabourTrackRepository labourTrackRepository, IReportExecutor report)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _logs = logs;
            _labourTrackRepository = labourTrackRepository; _report = report;

        }




        public async Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText)
        {
            return await _commonService.SearchCustomersAsync(searchText);
        }

        public async Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText)
            => await _commonService.SearchItemsAsync(searchText);




     
        // Service
        public async Task<decimal> GetTotalInvoiceValue(
            DateTime? from,
            DateTime? to,
            int? selectedCustomerId)
        {
            try
            {
                var query = _unitOfWork.LabInvs.GetQueryable();

                // Filter by Date range if both dates are provided
                if (from.HasValue && to.HasValue)
                {
                    query = query.Where(i =>
                        i.LabInvDate >= from.Value &&
                        i.LabInvDate <= to.Value);
                }

                if (selectedCustomerId.HasValue && selectedCustomerId.Value > 0)
                {
                     totalInvoiceValue = await query
                         .Where(i => i.CustId == selectedCustomerId.Value)
                         .Select(i => (decimal?)i.GrandTotal)
                         .SumAsync() ?? 0m;
                }
                else
                {
                    // All customers total
                     totalInvoiceValue = await query
                        .Select(i => (decimal?)i.GrandTotal)
                        .SumAsync() ?? 0m;
                }

                return totalInvoiceValue;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in GetTotalInvoiceValue");
                return 0m;
            }
        }

        public async Task<List<string>> GetDocumentNumbersAsync(
        List<LabourTrackSummaryVM> data,
        string docType,
        DateTime? fromDate,
        DateTime? toDate)
        {
            try
            {
                if (data == null || !data.Any())
                    return new List<string>();

                // ✅ Common Date Filter
                bool IsInDateRange(DateTime? date) =>
                    (!fromDate.HasValue || (date.HasValue && date.Value.Date >= fromDate.Value.Date)) &&
                    (!toDate.HasValue || (date.HasValue && date.Value.Date <= toDate.Value.Date));

                var query = data.AsEnumerable();

                return docType switch
                {
                    "Labour DC" => query
                        .Where(x => !string.IsNullOrEmpty(x.DCNo) && IsInDateRange(x.DCDate))
                        .Select(x => x.DCNo)
                        .ToList(),

                    "Labour GRN" => query
                        .Where(x => !string.IsNullOrEmpty(x.GRNNo) && IsInDateRange(x.GRNDate))
                        .Select(x => x.GRNNo)
                        .ToList(),

                    "Labour SCN" => query
                        .Where(x => !string.IsNullOrEmpty(x.SCNNo) && IsInDateRange(x.SCNDate))
                        .Select(x => x.SCNNo)
                        .ToList(),

                    "Labour PO" => query
                        .Where(x => !string.IsNullOrEmpty(x.PONo) && IsInDateRange(x.PODate))
                        .Select(x => x.PONo)
                        .ToList(),

                    "Labour Invoice" => query
                        .Where(x => !string.IsNullOrEmpty(x.InvNo) && IsInDateRange(x.InvDate))
                        .Select(x => x.InvNo)
                        .ToList(),

                    // ✅ fallback → apply only date filter if needed
                    _ => query
                        .Where(x => IsInDateRange(x.PODate)) // or remove this line if not needed
                        .Select(x => x.PONo)
                        .Where(x => !string.IsNullOrEmpty(x))
                        .ToList()
                };
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in GetDocumentNumbersAsync");
                return new List<string>();
            }
        }



        public List<string> GetCustomerDocTypes(
        List<LabourTrackSummaryVM> data,
        DateTime? fromDate,
        DateTime? toDate)
        {
            if (data == null || !data.Any())
                return new List<string>();

            // ✅ Date filter logic
            bool IsInDateRange(DateTime? date) =>
                (!fromDate.HasValue || (date.HasValue && date.Value.Date >= fromDate.Value.Date)) &&
                (!toDate.HasValue || (date.HasValue && date.Value.Date <= toDate.Value.Date));

            var filtered = data.Where(x =>
                IsInDateRange(x.PODate) ||
                IsInDateRange(x.GRNDate) ||
                IsInDateRange(x.SCNDate) ||
                IsInDateRange(x.DCDate) ||
                IsInDateRange(x.InvDate)
            );

            var docTypes = new List<string>();

            if (filtered.Any(x => !string.IsNullOrEmpty(x.PONo)))
                docTypes.Add("Labour PO");

            if (filtered.Any(x => !string.IsNullOrEmpty(x.GRNNo)))
                docTypes.Add("Labour GRN");

            if (filtered.Any(x => !string.IsNullOrEmpty(x.SCNNo)))
                docTypes.Add("Labour SCN");

            if (filtered.Any(x => !string.IsNullOrEmpty(x.DCNo)))
                docTypes.Add("Labour DC");

            if (filtered.Any(x => !string.IsNullOrEmpty(x.InvNo)))
                docTypes.Add("Labour Invoice");

            return docTypes;
        }

        public List<LabourTrackSummaryVM> GetItemsByPoId(List<LabourTrackSummaryVM> allDocs, string docType, int poId)
        {

            var filtered = docType switch
            {
                "Labour PO" => allDocs.Where(x => x.PoId == poId),
                "Labour GRN" => allDocs.Where(x => x.GRNId == poId),
                "Labour SCN" => allDocs.Where(x => x.SCNId == poId),
                "Labour DC" => allDocs.Where(x => x.DCId == poId),
                "Labour Invoice" => allDocs.Where(x => x.InvId == poId),
                _ => Enumerable.Empty<LabourTrackSummaryVM>()
            };

            return filtered
                .GroupBy(x => x.ItemCode)
                .Select(g => g.First())
                .ToList();
        }


    


        public async Task<List<LabourTrackSummaryVM>> GetLabourTrackSummaryAsync(
         int? custId,
         string? docType,
         string? docNo,
         int? itemId,
         DateTime? fromDate,
         DateTime? toDate)
        {
            try
            {

               


                var parameters = new[]
                {
            new SqlParameter("@CustId", custId.HasValue ? custId.Value : DBNull.Value),
            new SqlParameter("@DocType", string.IsNullOrWhiteSpace(docType) ? DBNull.Value : docType),
            new SqlParameter("@DocNo", string.IsNullOrWhiteSpace(docNo) ? DBNull.Value : docNo),
            new SqlParameter("@ItemId", itemId.HasValue ? itemId.Value : DBNull.Value),
            new SqlParameter("@FromDate", fromDate ?? (object)DBNull.Value),
            new SqlParameter("@ToDate", toDate ?? (object)DBNull.Value)
        };

                var result = await _report.ExecuteAsync<LabourTrackSummaryVM>(
                                         "Sp_Labour_Track",
                                           parameters 
                                     );
                return result ?? new List<LabourTrackSummaryVM>();
            }
            catch (Exception ex)
            {
                _logs.LogDeveloperError(ex, "Error executing stored procedure Sp_Labour_Track");
                return new List<LabourTrackSummaryVM>();
            }
        }
    }
}
