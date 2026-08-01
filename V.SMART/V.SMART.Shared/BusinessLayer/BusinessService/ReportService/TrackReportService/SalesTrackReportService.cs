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
    public class SalesTrackReportService : ISalesTrackReportService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private object _context;
        private decimal totalInvoiceValue;
        private readonly IReportExecutor _report;
        // private readonly IMapper _mapper;

        public SalesTrackReportService(
            IUnitOfWork unitOfWork,
            CurrentUserService userService,
            ICommonService commonService,
            ILoggingService logs, IReportExecutor report)
        {

            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _unitOfWork = unitOfWork;
            _logs = logs;
            _report = report;
        }



        public async Task<List<SalesTrackVM>> ExportDataAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? customerId = null)
        {
            try
            {
                var parameters = new[]
                {
                new SqlParameter("@FromDate",
                fromDate.HasValue ? fromDate.Value : DBNull.Value),

                new SqlParameter("@ToDate",
                toDate.HasValue ? toDate.Value : DBNull.Value),

                new SqlParameter("@CustomerId",
                customerId.HasValue ? customerId.Value : DBNull.Value)
            };

                var result = await _report.ExecuteAsync<SalesTrackVM>("sp_Sales_Track", parameters);

                return result ?? new List<SalesTrackVM>();
            }
            catch (Exception ex)
            {
                if (_logs != null)
                {
                    await _logs.LogDeveloperError(
                        ex,
                        "Error in ExportDataAsync() - PoPendingService");
                }

                return new List<SalesTrackVM>();
            }
        }

        public async Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText)
        {
            return await _commonService.SearchCustomersAsync(searchText);
        }

        public List<SalesTrackVM> GetCustomerDocTypes(
        List<SalesTrackVM> data,
        DateTime? fromDate = null,
        DateTime? toDate = null)
        {
            if (data == null || !data.Any())
                return new List<SalesTrackVM>();

            // ✅ Date filter logic
            bool IsInDateRange(DateTime? date) =>
                (!fromDate.HasValue || (date.HasValue && date.Value.Date >= fromDate.Value.Date)) &&
                (!toDate.HasValue || (date.HasValue && date.Value.Date <= toDate.Value.Date));

            // ✅ Apply date filtering FIRST
            var filtered = data.Where(x =>
                IsInDateRange(x.EnqDate) ||
                IsInDateRange(x.QtDate) ||
                IsInDateRange(x.PODate) ||
                IsInDateRange(x.MreqDate) ||
                IsInDateRange(x.JobOrderDate) ||
                IsInDateRange(x.RouteCardDate) ||
                IsInDateRange(x.DcDate) ||
                IsInDateRange(x.InvoiceDate)
            );

            var docTypes = new List<string>();

            if (filtered.Any(x => !string.IsNullOrEmpty(x.EnqNo)))
                docTypes.Add("Enquiry");

            if (filtered.Any(x => !string.IsNullOrEmpty(x.QtNo)))
                docTypes.Add("Quotation");

            if (filtered.Any(x => !string.IsNullOrEmpty(x.PONo)))
                docTypes.Add("PO");

            if (filtered.Any(x => !string.IsNullOrEmpty(x.MReqNo)))
                docTypes.Add("MaterialReq");

            if (filtered.Any(x => !string.IsNullOrEmpty(x.JobOrderNo)))
                docTypes.Add("JobOrder");

            if (filtered.Any(x => !string.IsNullOrEmpty(x.RouteCardNo)))
                docTypes.Add("RouteCard");

            if (filtered.Any(x => !string.IsNullOrEmpty(x.DcNo)))
                docTypes.Add("DC");

            if (filtered.Any(x => !string.IsNullOrEmpty(x.InvNo)))
                docTypes.Add("Invoice");

            // ✅ Fix Distinct issue (use string, then map)
            return docTypes
                .Distinct()
                .Select(x => new SalesTrackVM { DocType = x })
                .ToList();
        }


        public List<SalesTrackVM> GetPendingPo(
         List<SalesTrackVM> data,
         string docType,
         DateTime? fromDate,
         DateTime? toDate)
        {
            if (data == null || !data.Any())
                return new List<SalesTrackVM>();

            // ✅ Common Date Filter
            bool IsInDateRange(DateTime? date) =>
                (!fromDate.HasValue || (date.HasValue && date.Value.Date >= fromDate.Value.Date)) &&
                (!toDate.HasValue || (date.HasValue && date.Value.Date <= toDate.Value.Date));

            var query = data.AsEnumerable();

            // ✅ DocType filter + Date filter
            return docType switch
            {
                "Enquiry" => query
                    .Where(x => !string.IsNullOrEmpty(x.EnqNo) && IsInDateRange(x.EnqDate))
                    .ToList(),

                "Quotation" => query
                    .Where(x => !string.IsNullOrEmpty(x.QtNo) && IsInDateRange(x.QtDate))
                    .ToList(),

                "PO" => query
                    .Where(x => !string.IsNullOrEmpty(x.PONo) && IsInDateRange(x.PODate))
                    .ToList(),

                "MaterialReq" => query
                    .Where(x => !string.IsNullOrEmpty(x.MReqNo) && IsInDateRange(x.MreqDate))
                    .ToList(),

                "JobOrder" => query
                    .Where(x => !string.IsNullOrEmpty(x.JobOrderNo) && IsInDateRange(x.JobOrderDate))
                    .ToList(),

                "RouteCard" => query
                    .Where(x => !string.IsNullOrEmpty(x.RouteCardNo) && IsInDateRange(x.RouteCardDate))
                    .ToList(),

                "DC" => query
                    .Where(x => !string.IsNullOrEmpty(x.DcNo) && IsInDateRange(x.DcDate))
                    .ToList(),

                "Invoice" => query
                    .Where(x => !string.IsNullOrEmpty(x.InvNo) && IsInDateRange(x.InvoiceDate))
                    .ToList(),

                // ✅ fallback → only date filter
                _ => query.Where(x => IsInDateRange(x.PODate)).ToList()
            };
        }




        // Service
        public async Task<decimal> GetTotalInvoiceValue(
            DateTime? from,
            DateTime? to,
            int? selectedCustomerId)
        {
            try
            {
                var query = _unitOfWork.MfgInvs.GetQueryable();

                // Filter by Date range if both dates are provided
                if (from.HasValue && to.HasValue)
                {
                    query = query.Where(i =>
                        i.InvDate >= from.Value &&
                        i.InvDate <= to.Value);
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
        public List<SalesTrackVM> GetItemsByPoId(List<SalesTrackVM> allDocs, string docType, int poId)
        {

            var filtered = docType switch
            {
                "Enquiry" => allDocs.Where(x => x.EnquiryId == poId),
                "Quotation" => allDocs.Where(x => x.QuoteId == poId),
                "PO" => allDocs.Where(x => x.PoId == poId),
                "MaterialReq" => allDocs.Where(x => x.MreqId == poId),
                "DC" => allDocs.Where(x => x.DcId == poId),
                "RouteCard" => allDocs.Where(x => x.RCId == poId),
                "JobOrder" => allDocs.Where(x => x.JobId == poId),
                "Invoice" => allDocs.Where(x => x.InvId == poId),
                _ => Enumerable.Empty<SalesTrackVM>()
            };

            return filtered
                .GroupBy(x => x.Itemcode)
                .Select(g => g.First())
                .ToList();
        }

        // ================================
        // 6️⃣ Get Items by ItemCode (In-Memory)
        // ================================
        public List<SalesTrackVM> GetItemsByItemCode(List<SalesTrackVM> allDocs, string itemCode)
        {
            return allDocs
                .Where(x => x.Itemcode == itemCode)
                .GroupBy(x => x.Itemcode)
                .Select(g => g.First())
                .ToList();
        }
    }
}

