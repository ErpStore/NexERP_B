using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Inventory_Stock_.MaterialIssueNote;
using V.SMART.Shared.Repository.IRepository.IInventoryStockRepository.IMaterialIssueNoteRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.InventoryStockRepository.MaterialIssueNoteRepository
{
    public class MaterialIssNoteRepository : Repository<MaterialIssNote>, IMaterialIssueNoteRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;

        public MaterialIssNoteRepository(ApplicationDbContext db, ILoggingService logs) : base(db, logs)
        {
            _db = db;
            _logs = logs;
        }

        public async Task<string> GetLastMINNoAsync(string suffix)
        {
            var lastNumberStr = await _db.MaterialIssNote
                .FromSqlRaw(@"
            SELECT TOP 1 * 
            FROM MaterialIssNote WITH (UPDLOCK, ROWLOCK)
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
