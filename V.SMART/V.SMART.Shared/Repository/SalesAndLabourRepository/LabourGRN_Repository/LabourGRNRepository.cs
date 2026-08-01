using V.SMART.Shared.Data;
using V.SMART.Shared.Data.SalesAndLabour.LabourGRN;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ILabourGRN_Repository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.SalesAndLabourRepository.LabourGRN_Repository
{
    public class LabourGRNRepository: Repository<LabourGRN>, ILabourGRNRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public LabourGRNRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public async Task<string> GetLastGRNNoAsync(string suffix)
        {
            // Safely fetch the latest QuoteNo with locking to prevent concurrency issues
            var lastNumberStr = await _db.LabourGRN
                .FromSqlRaw(@"
            SELECT TOP 1 * 
            FROM LabourGRN WITH (UPDLOCK, ROWLOCK)
            WHERE Suffix = {0}
            ORDER BY TRY_CAST(GRNNo AS INT) DESC", suffix)
                .Select(q => q.GRNNo)
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


        public async Task<IEnumerable<LabourGRN>> GetAllWithItemsAsync()
        {
            return await _db.LabourGRN
                        .Include(q => q.Customer)
                        .Include(q => q.LabourGRNSubs)
                        .ToListAsync();
        }
    }
}
