using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IAdminRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using V.SMART.Shared.Data.Master.Admin_Module;

namespace V.SMART.Shared.Repository.MasterRepository.AdminRepository
{
    public class UserRightsRepository : Repository<UserRight>, IUserRightsRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;

        public UserRightsRepository(ApplicationDbContext db, ILoggingService loggingService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
        }


        public async Task<List<UserRight>> GetUserRightsWithScreensAsync(int userId)
        {
            return await _db.Set<UserRight>()
                .Include(r => r.Screens)
                .Where(r => r.UserId == userId)
                .ToListAsync();
        }
    }

    public class UserColumnPreferenceRepository : Repository<UserColumnPreference>, IUserColumnPreferenceRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;

        public UserColumnPreferenceRepository(ApplicationDbContext db, ILoggingService loggingService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
        }

    }
}
