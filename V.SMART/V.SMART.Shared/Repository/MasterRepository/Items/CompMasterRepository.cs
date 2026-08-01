using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IItems;
using V.SMART.Shared.Services;

namespace V.SMART.Shared.Repository.MasterRepository.Items
{
    public class CompMasterRepository : Repository<CompMaster>, ICompMasterRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public CompMasterRepository(ApplicationDbContext db, 
            ILoggingService loggingService, CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

    }
}
