using V.SMART.Shared.Data;
using V.SMART.Shared.Data.OutSourcing.PurchasePo;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IPurchOrSubConPoRepository;
using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.OutSourcingRepository.PurchOrSubConPORepository
{
    public class PurchPoSubRepository : Repository<PurchPoSub>, IPurchPoSubRepository       
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;
        private readonly CurrentUserService _currentUserService;
        public PurchPoSubRepository(ApplicationDbContext db, ILoggingService logs, CurrentUserService currentUserService) : base(db, logs)
        {
            _db = db;
            _logs = logs;
            _currentUserService = currentUserService;
        }
    }
}
