using V.SMART.Shared.Data;
using V.SMART.Shared.Data.SalesAndLabour.SalesDC;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ISalesDCRepoditory;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ISalesPoRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.SalesAndLabourRepository.SalesDCRepository
{
    public class MfgDcRepository : Repository<MfgDc>, IMfgDcRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public MfgDcRepository(ApplicationDbContext db, ILoggingService loggingService,
            CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public async Task<string> GetLastDcNoAsync(string suffix)
        {
            var lastNumberStr = await _db.MfgDc
                .FromSqlRaw(@"
                SELECT TOP 1 * 
                FROM MfgDc WITH (UPDLOCK, ROWLOCK) 
                WHERE suffix = {0} 
                ORDER BY TRY_CAST(DcNo AS INT) DESC", suffix)
                .Select(d => d.DcNo)
                .FirstOrDefaultAsync();

            return lastNumberStr ?? "0";
        }

        public async Task<IEnumerable<MfgDc>> GetAllWithItemsAsync()
        {
            try
            {
                return await _db.MfgDc
                        .Include(d => d.Customer)
                        .Include(d => d.MfgDcSubs)
                        .ToListAsync();

            }
            catch (Exception Ex)
            {

                throw;
            }

        }

    }
}
