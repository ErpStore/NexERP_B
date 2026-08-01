using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Planning.AssyJobOrder;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.Repository.IRepository.IPlanningRepository.IJobOrderRepo;
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
    public class RouteCardRepository : Repository<RouteCard>, IRouteCardRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;
        public RouteCardRepository(ApplicationDbContext db, ILoggingService logs) : base(db, logs)
        {
            _db = db;
            _logs = logs;
        }

        public async Task<string> GetLastRcNoAsync(string suffix)
        {
            var lastNumberStr = await _db.RouteCard
                .FromSqlRaw(@"
            SELECT TOP 1 * 
            FROM RouteCard WITH (UPDLOCK, ROWLOCK)
            WHERE Suffix = {0}
            ORDER BY TRY_CAST(RCNo AS INT) DESC", suffix)
                .Select(q => q.RCNo)
                .FirstOrDefaultAsync();
            int nextNumber = 1;
            if (!string.IsNullOrWhiteSpace(lastNumberStr) &&
                int.TryParse(lastNumberStr, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
            return nextNumber.ToString("D3");
        }
    }
}
