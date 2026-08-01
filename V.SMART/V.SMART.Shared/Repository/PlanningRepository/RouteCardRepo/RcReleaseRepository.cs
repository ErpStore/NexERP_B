using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.Repository.IRepository.IPlanningRepository.IRouteCardRepo;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.PlanningRepository.RouteCardRepo
{
    public class RcReleaseRepository : Repository<RouteCardRelease>, IRcReleaseRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;
        public RcReleaseRepository(ApplicationDbContext db, ILoggingService logs) : base(db, logs)
        {
            _db = db;
            _logs = logs;
        }
        
        public async Task<string> GetLastRcReleaseNoAsync(string suffix)
        {
            // Safely fetch the latest QuoteNo with locking to prevent concurrency issues
            var lastNumberStr = await _db.RouteCardRelease
                .FromSqlRaw(@"SELECT TOP 1 * 
                 FROM RouteCardRelease WITH (UPDLOCK, ROWLOCK)
                 WHERE Suffix = {0}
                 ORDER BY TRY_CAST(RcReleaseNo AS INT) DESC", suffix)
                .Select(q => q.RcReleaseNo)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            // Ensure null safety before parsing
            if (!string.IsNullOrWhiteSpace(lastNumberStr) &&
                int.TryParse(lastNumberStr, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }

            return nextNumber.ToString();
        }




    }
}
