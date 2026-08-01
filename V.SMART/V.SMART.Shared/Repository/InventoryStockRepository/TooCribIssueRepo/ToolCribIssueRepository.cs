using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Inventory_Stock_.ToolCrib;
using V.SMART.Shared.Repository.IRepository.IInventoryStockRepository.IToolCribIssueRepo;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.InventoryStockRepository.TooCribIssueRepo
{
    public class ToolCribIssueRepository : Repository<ToolCribIssue>, IToolCribIssueRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public ToolCribIssueRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }


        public async Task<string> GetLastToolCribIssueNoAsync(string suffix)
        {
            var lastNumberStr = await _db.ToolCribIssue
                .FromSqlRaw(@"
                SELECT TOP 1 * 
                FROM ToolCribIssue WITH (UPDLOCK, ROWLOCK)
                WHERE Suffix = {0}
                ORDER BY TRY_CAST(TCIssueNo AS INT) DESC", suffix)
                .Select(q => q.TCIssueNo)
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
