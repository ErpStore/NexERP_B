using V.SMART.Shared.Data;
using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using V.SMART.Shared.Data.OutSourcing.PurchaseSCN;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IPurchaseGRN_Repository;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IPurchaseSCN_Repository;
using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.OutSourcingRepository.PurchaseSCN_Repository
{
    public class PurchaseSCNSubRepository : Repository<PurchaseSCNSub>, IPurchaseSCNSubRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public PurchaseSCNSubRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }


        public async Task<List<PurchaseSCNSub>> GetPurchaseSCNSubDataByDcId(int GRNId)
        {
            //return await _db.PurchaseGRNSub
            //    .Where(s => s.GRNId == GRNId)
            //    .ToListAsync();
            return null;
        }
    }
}
