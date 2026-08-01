using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Production.DailyProductionLog;
using V.SMART.Shared.Data.Production.ProductionReturnGrnAssy;
using V.SMART.Shared.Repository.IRepository.IProductionRepository.IProductionLogRepo;
using V.SMART.Shared.Repository.IRepository.IProductionRepository.IProductionReturnAssyRepo;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.ProductionRepository.ProductionLogRepo
{
    public class ProductionLogRepository : Repository<ProductionLog>, IProductionLogRepository
    {

        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;
        public ProductionLogRepository(ApplicationDbContext db, ILoggingService logs) : base(db, logs)
        {
            _db = db;
            _logs = logs;
        }

        public async Task<string> GetLastLogNoAsync(string suffix)
        {
            var lastNumberStr = await _db.ProductionLog
                .FromSqlRaw(@"SELECT TOP 1 * 
            FROM ProductionLog WITH (UPDLOCK, ROWLOCK)
            WHERE Suffix = {0}
            ORDER BY TRY_CAST(LogNo AS INT) DESC", suffix)
                .Select(q => q.LogNo)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(lastNumberStr) &&
                int.TryParse(lastNumberStr, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }

            return nextNumber.ToString();
        }
    }
}
