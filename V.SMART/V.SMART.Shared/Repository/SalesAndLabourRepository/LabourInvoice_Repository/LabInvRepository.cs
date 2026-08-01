using V.SMART.Shared.Data;
using V.SMART.Shared.Data.SalesAndLabour_Module;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.SalesAndLabourRepository
{
    public class LabInvRepository : Repository<LabInv>, ILabInvRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public LabInvRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public async Task<string> GetLastLabInvoiceNoAsync(string suffix)
        {
            if (string.IsNullOrWhiteSpace(suffix))
                return "1";

            var lastNumberStr = await _db.LabInv
                .FromSqlRaw(@"
            SELECT TOP 1 LabInvNo
            FROM LabInv WITH (UPDLOCK, ROWLOCK)
            WHERE Suffix = {0}
            ORDER BY TRY_CAST(LabInvNo AS INT) DESC",
                    suffix)
                .Select(x => x.LabInvNo)
                .FirstOrDefaultAsync();

            return int.TryParse(lastNumberStr, out int lastNo)
                ? (lastNo + 1).ToString()
                : "1";
        }



        public async Task<IEnumerable<LabInv>> GetAllWithItemsAsync()
        {
            return await _db.LabInv
                        .Include(q => q.Customer)
                        .Include(q => q.Currency)
                        .Include(q => q.LabInvSubs)
                        .ToListAsync();
        }

    }
    
}
