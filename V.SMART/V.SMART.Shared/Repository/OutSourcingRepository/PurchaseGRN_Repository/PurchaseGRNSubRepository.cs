using V.SMART.Shared.Data;
using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using V.SMART.Shared.Data.SalesAndLabour.SalesDC;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IPurchaseGRN_Repository;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ISalesDCRepoditory;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.OutSourcingRepository.PurchaseGRN_Repository
{
    public class PurchaseGRNSubRepository : Repository<PurchaseGRNSub>, IPurchaseGRNSubRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public PurchaseGRNSubRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }


        public async Task<List<PurchaseGRNSub>> GetPurchaseGrnSubDataByDcId(int GRNId)
        {
            return await _db.PurchaseGRNSub
                .Where(s => s.GRNId == GRNId)
                .ToListAsync();
        }
    }
}
