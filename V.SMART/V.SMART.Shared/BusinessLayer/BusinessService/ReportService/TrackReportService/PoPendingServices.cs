
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel;

public class PoPendingServices : IPoPendingServices
{
    private readonly IReportExecutor _report;

    public PoPendingServices(IReportExecutor report)
    {
        _report = report;
    }

    
    // CUSTOMER LIST
    public async Task<List<ComboModelVM>> GetCustomerList(int? custId, DateTime? fromDate, DateTime? toDate)
    {
        try
        {
            SqlParameter[] parameters =
            {
             new SqlParameter("@Type", "Sales"),

             new SqlParameter("@Id",
                 custId ?? 0),

             new SqlParameter("@FromDate",
                 fromDate ?? (object)DBNull.Value),

             new SqlParameter("@ToDate",
                 toDate ?? (object)DBNull.Value),

             new SqlParameter("@Mode",
                 "List")
         };

            var result =
                await _report.ExecuteAsync<ComboModelVM>
                (
                    "Sp_GetMfgPOPendingList",
                    parameters
                );

            return result ?? new List<ComboModelVM>();
        }
        catch
        {
            throw;
        }
    }

    // VENDOR LIST
    public async Task<List<ComboModelVM>> GetVendorList(int? vendorId, DateTime? fromDate, DateTime? toDate)
    {
        try
        {
            SqlParameter[] parameters =
            {
                 new SqlParameter("@Type", "Purchase"),

                 new SqlParameter("@Id",
                     vendorId ?? 0),

                 new SqlParameter("@FromDate",
                     fromDate ?? (object)DBNull.Value),

                 new SqlParameter("@ToDate",
                     toDate ?? (object)DBNull.Value),

                 new SqlParameter("@Mode",
                     "List")
           };

            var result =
                await _report.ExecuteAsync<ComboModelVM>
                (
                    "Sp_GetMfgPOPendingList",
                    parameters
                );

            return result ?? new List<ComboModelVM>();
        }
        catch
        {
            throw;
        }
    }

    // PO PENDING DETAILS
    public async Task<List<PoPendingDetailsVM>> GetPoPendingDetails(string type, int id, DateTime? fromDate, DateTime? toDate)
    {
        try
        {
            SqlParameter[] parameters =
            {
                 new SqlParameter("@Type", type),

                 new SqlParameter("@Id",
                     id),

                 new SqlParameter("@FromDate",
                     fromDate ?? (object)DBNull.Value),

                 new SqlParameter("@ToDate",
                     toDate ?? (object)DBNull.Value),

                 new SqlParameter("@Mode",
                     "Details")
            };

            var result =
                await _report.ExecuteAsync<PoPendingDetailsVM>
                (
                    "Sp_GetMfgPOPendingList",
                    parameters
                );

            return result ?? new List<PoPendingDetailsVM>();
        }
        catch
        {
            throw;
        }
    }


}