using DocumentFormat.OpenXml.VariantTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.AccountsVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService
{
    public interface ILabourPendingService
    {
        Task<bool> IsPOWiseLabDcOutEnabledAsync();

        Task<List<LabourPendingVM>> GetAccountsAsync(bool? IsPoWiseLabDcOut, bool? Type, string? ExpenseType, DateTime? fromDate, DateTime? toDate, int? custId);

        Task<List<LabourPendingSummaryVM>> GetAccountDetailsAsync(bool? IsPoWiseLabDcOut, bool? IsDetails, string? DocType, DateTime? fromDate, DateTime? toDate, int? custId);
    }
}
