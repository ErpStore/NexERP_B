using V.SMART.Shared.Data;
using V.SMART.Shared.Data.SalesAndLabour.Labour_SCN;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ILabourSCN_Repository;
using V.SMART.Shared.Services;

namespace V.SMART.Shared.Repository.SalesAndLabourRepository.LabourSCN_Repository
{
    internal class LabourSCNSubRepository: Repository<LabourSCNSub>, ILabourSCNSubRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public LabourSCNSubRepository(
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
