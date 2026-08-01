using V.SMART.Shared.Data;
using V.SMART.Shared.Data.SalesAndLabour.Credit_Note;
using V.SMART.Shared.Data.SalesAndLabour.SalesInvoice;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ICreditNote_Repository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.SalesAndLabourRepository.CreditNote_Repository
{
    public class CreditNoteRepository : Repository<CreditNote>, ICreditNoteRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public CreditNoteRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public async Task<string> GetLastCreditNoAsync(string suffix)
        {
            // Safely fetch the latest QuoteNo with locking to prevent concurrency issues
            var lastNumberStr = await _db.CreditNote
                .FromSqlRaw(@"
            SELECT TOP 1 * 
            FROM CreditNote WITH (UPDLOCK, ROWLOCK)
            WHERE Suffix = {0}
            ORDER BY TRY_CAST(CreditNo AS INT) DESC", suffix)
                .Select(q => q.CreditNo)
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
