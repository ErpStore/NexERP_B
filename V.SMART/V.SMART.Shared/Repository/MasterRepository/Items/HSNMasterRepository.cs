using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IItemRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace V.SMART.Shared.Repository.MasterRepository.ItemsRepository
{
    public class HSNMasterRepository : Repository<HSNMaster>, IHSNMasterRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        public HSNMasterRepository(ApplicationDbContext db, ILoggingService loggingService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
        }

        public void AddRange(IEnumerable<HSNMaster> entities)
        {
            _db.HSNMaster.AddRange(entities);
        }
        public async Task<List<string>> GetExistingHSNCodesAsync(List<string> hsnCodes)
        {
            return await _db.Set<HSNMaster>()
                .Where(h => hsnCodes.Contains(h.HSNcode))
                .Select(h => h.HSNcode)
                .ToListAsync();
        }

    }
}
