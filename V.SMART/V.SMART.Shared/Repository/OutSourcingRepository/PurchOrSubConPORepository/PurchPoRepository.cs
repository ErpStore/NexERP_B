using V.SMART.Shared.Data;
using V.SMART.Shared.Data.OutSourcing.PurchasePo;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IPurchOrSubConPoRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.OutSourcingRepository.PurchOrSubConPORepository
{
    public class PurchPoRepository : Repository <PurchPo> , IPurchPoRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;
        private readonly CurrentUserService _currentUserService;

        public PurchPoRepository(ApplicationDbContext db, ILoggingService logs, CurrentUserService currentUserService) : base(db, logs)
        {
            _db = db;
            _logs = logs;
            _currentUserService = currentUserService;
        }

        public async Task<string> GetLastPONoAsync(string suffix)
        {
            // Safely fetch the latest PONo with locking to prevent concurrency issues
            var lastNumberStr = await _db.PurchPo
                .FromSqlRaw(@"SELECT TOP 1 * 
                    FROM PurchPo WITH (UPDLOCK, ROWLOCK)
                    WHERE Suffix = {0}
                    ORDER BY TRY_CAST(PONo AS INT) DESC", suffix)
                .Select(q => q.PONo)
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
