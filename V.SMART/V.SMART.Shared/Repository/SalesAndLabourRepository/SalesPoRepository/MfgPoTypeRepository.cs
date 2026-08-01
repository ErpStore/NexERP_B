using V.SMART.Shared.Data;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ISalesPoRepository;
using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.SalesAndLabourRepository.SalesPoRepository
{
    public class MfgPoTypeRepository : Repository<PoType>, IMfgPoTypeRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public MfgPoTypeRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }





    }
}
