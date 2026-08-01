using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Inventory_Stock_.InterStoreTransfer;
using V.SMART.Shared.Repository.IRepository.IInventoryStockRepository.IStoreInterTransferRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.InventoryViewModel.StoreInterTransferViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.InventoryStockRepository.StoreInterTransferRepository
{
    public class StoreInterTransRepository : Repository<StoreInterTrans>, IStoreInterTransRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;


        public StoreInterTransRepository(ApplicationDbContext db, ILoggingService loggingService)
            : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
        }

        public async Task<string> GetLastISTNoAsync(string suffix)
        {
            // Safely query the last ISTNo using SQL locking hints
            var lastNumberStr = await _db.StoreInterTrans
                .FromSqlRaw(@"SELECT TOP 1 * 
            FROM StoreInterTrans WITH (UPDLOCK, ROWLOCK)
            WHERE Suffix = {0}
            ORDER BY TRY_CAST(ISTNo AS INT) DESC", suffix)
                .Select(q => q.ISTNo)
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

        public async Task<List<StoreInterTrans>> InstantSearchInterStoreTransAsync(StoreInterTransVM search)
        {
            try
            {
                IQueryable<StoreInterTrans> query = _db.StoreInterTrans
                    .AsNoTracking()
                    .Include(i => i.StoreAdd)
                    .Include(i => i.StoreIssue);

                // ------------------------- 
                //   EXACT MATCH FILTERS
                // -------------------------

                // IST No (exact match, but supports multiple words)
                if (!string.IsNullOrWhiteSpace(search.ISTNo))
                {
                    var term = search.ISTNo.Trim();

                    query = query.Where(i =>
                        EF.Functions.Like(i.ISTNo.ToString(), "%" + term + "%")
                    );
                }

                // Store FROM (Store Issue Name)
                if (!string.IsNullOrWhiteSpace(search.StoreIssName))
                    query = query.Where(i => i.StoreIssue.StoreName == search.StoreIssName);

                // Store TO (Store Add Name)
                if (!string.IsNullOrWhiteSpace(search.StoreAddName))
                    query = query.Where(i => i.StoreAdd.StoreName == search.StoreAddName);

                // Store IDs (exact match)
                if (search.StoreIdFrom.HasValue)
                    query = query.Where(i => i.StoreIssueId == search.StoreIdFrom.Value);

                if (search.StoreIdTo.HasValue)
                    query = query.Where(i => i.StoreAddId == search.StoreIdTo.Value);

                // Main Remark
                if (!string.IsNullOrWhiteSpace(search.MainRemark))
                    query = query.Where(i => i.MainRemark == search.MainRemark);

                // CreatedBy
                if (!string.IsNullOrWhiteSpace(search.CreatedBy))
                    query = query.Where(i => i.CreatedBy == search.CreatedBy);

                // ModifiedBy
                if (!string.IsNullOrWhiteSpace(search.ModifiedBy))
                    query = query.Where(i => i.ModifiedBy == search.ModifiedBy);

                // Date (exact)
                if (search.ISTDate != default(DateTime))
                    query = query.Where(i => i.ISTDate.Date == search.ISTDate.Date);

                // Return EXACT RECORDS ONLY
                return await query
                    .OrderByDescending(i => i.ISTDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in StoreInterTrans search");
                return new List<StoreInterTrans>();
            }
        }





    }
}
