using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.MasterScreeenManagement;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IMasterSettings;
using V.SMART.Shared.Services;

namespace V.SMART.Shared.Repository.MasterRepository.MasterSettings
{
    public class ScreenRepository : Repository<Screens>, IScreenRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        public ScreenRepository(ApplicationDbContext db, ILoggingService loggingService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
        }
    }
}
