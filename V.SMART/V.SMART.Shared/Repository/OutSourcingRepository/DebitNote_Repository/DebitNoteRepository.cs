
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.OutSourcing.Debit_Note;
using V.SMART.Shared.Repository;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IDebitNote_Repository;
using V.SMART.Shared.Services;

namespace V.SMART.Shared.Repository.OutSourcingRepository.DebitNote_Repository
{
    public class DebitNoteRepository: Repository<DebitNote>, IDebitNoteRepository
    {

        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public DebitNoteRepository(ApplicationDbContext db,ILoggingService loggingService, CurrentUserService currentUserService) : base(db, loggingService) 
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public async Task<string> GetLastDebitNoAsync(string suffix)
        {
            // Safely fetch the latest QuoteNo with locking to prevent concurrency issues
            var lastNumberStr = await _db.DebitNote
                .FromSqlRaw(@"
            SELECT TOP 1 * 
            FROM DebitNote WITH (UPDLOCK, ROWLOCK)
            WHERE Suffix = {0}
            ORDER BY TRY_CAST(DebitNo AS INT) DESC", suffix)
                .Select(q => q.DebitNo)
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
