using V.SMART.Shared.Data;
using V.SMART.Shared.Data.SalesAndLabour.Labour_SCN;
using V.SMART.Shared.Data.SalesAndLabour.LabourGRN;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ILabourSCN_Repository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.SalesAndLabourRepository.LabourSCN_Repository
{
    public class LabourSCNRepository:Repository<LabourSCN>, ILabourSCNRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public LabourSCNRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }


        public async Task<string> GetLastSCNNoAsync(string suffix)
        {
            // Safely fetch the latest QuoteNo with locking to prevent concurrency issues
            var lastNumberStr = await _db.LabourSCN
                .FromSqlRaw(@"
            SELECT TOP 1 * 
            FROM LabourSCN WITH (UPDLOCK, ROWLOCK)
            WHERE Suffix = {0}
            ORDER BY TRY_CAST(SCNNo AS INT) DESC", suffix)
                .Select(q => q.SCNNo)
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


        public async Task<IEnumerable<LabourSCN>> GetAllWithItemsAsync()
        {
            return await _db.LabourSCN
                        .Include(q => q.Customer)
                        .Include(q => q.LabourSCNSubs)
                        .ToListAsync();
        }
    }
}
