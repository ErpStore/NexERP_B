using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IItemRepository;
using V.SMART.Shared.Services;

namespace V.SMART.Shared.Repository.MasterRepository.ItemsRepository
{
    public class UOMRepository : Repository<UOM>, IUOMRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        public UOMRepository(ApplicationDbContext db, ILoggingService loggingService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
        }
    }
}
