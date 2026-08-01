using V.SMART.Shared.Data;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Inventory_Stock_.StockIssueRequest;
using V.SMART.Shared.Repository.IRepository.IInventoryStockRepository.IStockRequestIssueRepository;

namespace V.SMART.Shared.Repository.InventoryStockRepository.StockRequestIssueRepository
{
    public class StockRequestIssueRepository : Repository<StockIssueRequest>, IStockRequestIssueRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;

        public StockRequestIssueRepository(ApplicationDbContext db, ILoggingService logs) : base(db, logs)
        {
            _db = db;
            _logs = logs;
        }

        public async Task<string> GetLastReqNoAsync(string suffix)
        {
            var lastNumberStr = await _db.StockIssueRequest
                .FromSqlRaw(@"
            SELECT TOP 1 * 
            FROM StockIssueRequest WITH (UPDLOCK, ROWLOCK)
            WHERE Suffix = {0}
            ORDER BY TRY_CAST(IssueNo AS INT) DESC", suffix)
                .Select(q => q.RequestNo)
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
