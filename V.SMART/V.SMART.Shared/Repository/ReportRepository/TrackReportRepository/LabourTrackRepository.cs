using V.SMART.Shared.Data;
using V.SMART.Shared.Repository.IRepository.IReportRepository.ITrackReportRepository;
using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.ReportRepository.TrackReportRepository
{
    public class LabourTrackRepository : ILabourTrackRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;

        public LabourTrackRepository(ApplicationDbContext db, ILoggingService logs)
        {
            _db = db;
            _logs = logs;
        }


        //public async Task<List<PoPendingVM>> ExportDataAsync(DateTime? fromDate, DateTime? toDate, int? customerId)
        //{
        //    try
        //    {
        //        // Use FromSqlRaw with parameters safely
        //        var parameters = new[]
        //        {
        //            new SqlParameter("@FromDate", fromDate ?? (object)DBNull.Value),
        //            new SqlParameter("@ToDate", toDate ?? (object)DBNull.Value),
        //            new SqlParameter("@CustomerId", customerId ?? (object)DBNull.Value)
        //        };


        //        var result = await _db.PoPendingReports
        //            .FromSqlRaw("EXEC sp_GetPoPendingReport @FromDate, @ToDate, @CustomerId", parameters)
        //            .AsNoTracking() // improve performance for read-only report
        //            .ToListAsync();

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //}
    }
}
