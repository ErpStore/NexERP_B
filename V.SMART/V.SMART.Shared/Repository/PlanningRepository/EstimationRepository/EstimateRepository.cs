using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Planning.Estimaton;
using V.SMART.Shared.Repository.IRepository.IPlanningRepository.IEstimatationRepository;
using V.SMART.Shared.Services;

namespace V.SMART.Shared.Repository.PlanningRepository.EstimationRepository
{
    public class EstimateRepository : Repository<Estimate>,IEstimateRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;
        private readonly CurrentUserService _currentUserService;
        public EstimateRepository(ApplicationDbContext db, ILoggingService logs, CurrentUserService currentUserService) : base(db, logs)
        {
            _db = db;
            _logs = logs;
            _currentUserService = currentUserService;
        }
        public async Task<string> GetLastEstimateNoAsync(string suffix)
        {
            // Safely fetch the latest QuoteNo with locking to prevent concurrency issues
            var lastNumberStr = await _db.Estimate
                .FromSqlRaw(@"
            SELECT TOP 1 * 
            FROM Estimate WITH (UPDLOCK, ROWLOCK)
            WHERE Suffix = {0}
            ORDER BY TRY_CAST(EstiamateNo AS INT) DESC", suffix)
                .Select(q => q.EstiamateNo)
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
