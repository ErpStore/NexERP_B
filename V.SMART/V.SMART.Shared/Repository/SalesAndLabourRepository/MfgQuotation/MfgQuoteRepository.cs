using V.SMART.Shared.Data;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.IMfgQuotation;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.SalesAndLabourRepository.MfgQuotation
{
    public class MfgQuoteRepository : Repository<MfgQuote>, IMfgQuoteRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public MfgQuoteRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public async Task<string> GetLastQuoteNoAsync(string suffix)
        {
            // Safely fetch the latest QuoteNo with locking to prevent concurrency issues
            var lastNumberStr = await _db.MfgQuote
                .FromSqlRaw(@"
            SELECT TOP 1 * 
            FROM MfgQuote WITH (UPDLOCK, ROWLOCK)
            WHERE Suffix = {0}
            ORDER BY TRY_CAST(QuoteNo AS INT) DESC", suffix)
                .Select(q => q.QuoteNo)
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


        public async Task<IEnumerable<MfgQuote>> GetAllWithItemsAsync()
        {
            return await _db.MfgQuote
                        .Include(q => q.Customer)
                        .Include(q => q.Currency)
                        .Include(q => q.MfgQuoteSub)
                        .ToListAsync();
        }

    }
    
}
