using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Production.ProductionComponent;
using V.SMART.Shared.Data.Production.ProductionIssueWOAssy;
using V.SMART.Shared.Repository.IRepository.IProductionRepository.IProductionCompRepo;
using V.SMART.Shared.Repository.IRepository.IProductionRepository.IProductionIssueWOAssyRepo;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.ProductionRepository.ProuctionCompRepo
{
    public class ProductionIssueCompRepository : Repository<ProductionIssueComp>, IProductionIssueCompRepository

    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;
        public ProductionIssueCompRepository(ApplicationDbContext db, ILoggingService logs) : base(db, logs)
        {
            _db = db;
            _logs = logs;
        }

        public async Task<string> GetLastIssueNoAsync(string suffix)
        {
            var lastNumberStr = await _db.ProductionIssueComp
                .FromSqlRaw(@"
            SELECT TOP 1 * 
            FROM ProductionIssueComp WITH (UPDLOCK, ROWLOCK)
            WHERE Suffix = {0}
            ORDER BY TRY_CAST(IssueNo AS INT) DESC", suffix)
                .Select(q => q.IssueNo)
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
