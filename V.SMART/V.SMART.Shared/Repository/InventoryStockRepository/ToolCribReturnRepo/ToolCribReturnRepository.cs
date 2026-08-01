using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Inventory_Stock_.ToolCrib;
using V.SMART.Shared.Repository.IRepository.IInventoryStockRepository.IToolCribReturnRepo;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.InventoryStockRepository.ToolCribReturnRepo
{
    public class ToolCribReturnRepository : Repository<ToolCribReturn>, IToolCribReturnRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public ToolCribReturnRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public async Task<string> GetLastReturnNoAsync(string suffix)
        {
            try
            {
                var lastNumberStr = await _db.ToolCribReturns
                    .FromSqlRaw(@"
                        SELECT TOP 1 * 
                        FROM ToolCribReturns WITH (UPDLOCK, ROWLOCK)
                        WHERE Suffix = {0}
                        ORDER BY TRY_CAST(LEFT(TCReturnNo, CHARINDEX('/', TCReturnNo + '/') - 1) AS INT) DESC", suffix)
                    .Select(r => r.TCReturnNo)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (!string.IsNullOrWhiteSpace(lastNumberStr))
                {
                    var parts = lastNumberStr.Split('/');
                    if (int.TryParse(parts[0], out int lastNumber))
                        nextNumber = lastNumber + 1;
                }

                return $"{nextNumber}";
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error generating DC number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate DC number.");
            }
        }

        //public async Task<string> GetLastDcNoAsync(string suffix)
        //{
        //    var lastReturn = await _db.ToolCribReturns
        //        .Where(q => q.Suffix == suffix)
        //        .OrderByDescending(q => q.DcNo)
        //        .FirstOrDefaultAsync();

        //    int nextNumber = 1;
        //    if (lastReturn != null)
        //    {
        //        var parts = lastReturn.DcNo.Split('/');
        //        if (int.TryParse(parts[0], out int lastNumber))
        //            nextNumber = lastNumber + 1;
        //    }

        //    return $"{nextNumber}/{suffix}";
        //}

        //public async Task<string> GetToolCribReturnNumberAsync(string suffix)
        //{
        //    var lastReturn = await _db.ToolCribReturns
        //        .Where(q => q.Suffix == suffix)
        //        .OrderByDescending(q => q.DcNo)
        //        .FirstOrDefaultAsync();

        //    int nextNumber = 1;
        //    if (lastReturn != null)
        //    {
        //        var parts = lastReturn.DcNo.Split('/');
        //        if (int.TryParse(parts[0], out int lastNumber))
        //            nextNumber = lastNumber + 1;
        //    }

        //    return $"{nextNumber}";
        //}
    }

}
