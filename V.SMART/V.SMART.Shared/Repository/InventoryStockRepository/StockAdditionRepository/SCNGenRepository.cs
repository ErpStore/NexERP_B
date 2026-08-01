using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Inventory_Stock_.StoreTransferNote;
using V.SMART.Shared.Repository.IRepository.IInventoryStockRepository.IStockAdditionRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.InventoryStockRepository.StockAdditionRepository
{
    public class SCNGenRepository : Repository<SCNGen>, ISCNGenRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;


        public SCNGenRepository(ApplicationDbContext db, ILoggingService loggingService)
            : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
        }

        public async Task<string> GetLastSCNGenNoAsync(string suffix)
        {
            // Safely query the last SCNGenNo using SQL locking hints
            var lastNumberStr = await _db.SCNGen
                .FromSqlRaw(@"
            SELECT TOP 1 * 
            FROM SCNGen WITH (UPDLOCK, ROWLOCK)
            WHERE Suffix = {0}
            ORDER BY TRY_CAST(SCNGenNo AS INT) DESC", suffix)
                .Select(q => q.SCNGenNo)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            // Ensure it's not null or whitespace and is a valid integer
            if (!string.IsNullOrWhiteSpace(lastNumberStr) &&
                int.TryParse(lastNumberStr, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }

            return nextNumber.ToString();
        }


    }
}
