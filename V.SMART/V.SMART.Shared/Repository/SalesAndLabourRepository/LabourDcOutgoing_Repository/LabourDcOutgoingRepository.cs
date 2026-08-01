using V.SMART.Shared.Data;
using V.SMART.Shared.Data.SalesAndLabour.LabourDC;
using V.SMART.Shared.Data.SalesAndLabour.LabourGRN;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ILabourDcOutgoing_Repository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;


namespace V.SMART.Shared.Repository.SalesAndLabourRepository.LabourDcOutgoing_Repository
{
    public class LabourDcOutgoingRepository: Repository<LabourDcOutgoing>, ILabourDcOutgoingRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public LabourDcOutgoingRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public async Task<string> GetLastDcNoAsync(string suffix)
        {
            // Safely fetch the latest QuoteNo with locking to prevent concurrency issues
            var lastNumberStr = await _db.LabourDcOutgoing
                .FromSqlRaw(@"
            SELECT TOP 1 * 
            FROM LabourDcOutgoing WITH (UPDLOCK, ROWLOCK)
            WHERE Suffix = {0} and NonReturnDc = 0
            ORDER BY TRY_CAST(DcNo AS INT) DESC", suffix)
                .Select(q => q.DcNo)
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


        public async Task<IEnumerable<LabourDcOutgoing>> GetAllWithItemsAsync()
        {
            return await _db.LabourDcOutgoing
                        .Include(q => q.Customer)
                        .Include(q => q.LabourDcOutgoingSubs)
                        .ToListAsync();
        }

        public async Task<string> GetNextNrDcNoAsync(string suffix)
        {
            var lastNr = await _db.LabourDcOutgoing
                .FromSqlRaw(@"
            SELECT TOP 1 *
            FROM LabourDcOutgoing WITH (UPDLOCK, ROWLOCK)
            WHERE NonReturnDc = 1
              AND DcNo LIKE 'NR%'
            ORDER BY 
              TRY_CAST(SUBSTRING(DcNo, 3, LEN(DcNo)) AS INT) DESC", suffix)
                .Select(x => x.DcNo)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(lastNr) &&
                lastNr.Length > 2 &&
                int.TryParse(lastNr.Substring(2), out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }

            return $"NR{nextNumber}";
        }
    }
}
