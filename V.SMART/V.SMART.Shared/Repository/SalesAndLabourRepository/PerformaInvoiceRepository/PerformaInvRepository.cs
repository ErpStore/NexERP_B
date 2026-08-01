using V.SMART.Shared.Data;
using V.SMART.Shared.Data.SalesAndLabour.PerformaInvoice;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IItemRepository;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.IPerformaInvoiceRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.SalesAndLabourRepository.PerformaInvoiceRepository
{
    public class PerformaInvRepository : Repository<PerformaInv>, IPerformaInvRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public PerformaInvRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }


        public async Task<string> GetLastPerformaInvNoAsync(string suffix)
        {
            var lastNumberStr = await _db.PerformaInv
                .FromSqlRaw(@"
                SELECT TOP 1 * 
                FROM PerformaInv WITH (UPDLOCK, ROWLOCK) 
                WHERE Suffix = {0} 
                ORDER BY TRY_CAST(InvNo AS INT) DESC", suffix)
                .Select(q => q.InvNo)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (int.TryParse(lastNumberStr, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }

            return nextNumber.ToString();
        }



    }
}
