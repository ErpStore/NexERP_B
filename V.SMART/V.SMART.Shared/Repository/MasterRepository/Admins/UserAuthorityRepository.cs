using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.Admin_Module;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IAdmins;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace V.SMART.Shared.Repository.MasterRepository.Admins
{
    public class UserAuthorityRepository : Repository<UserAuthority>, IUserAuthorityRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public UserAuthorityRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService)
            : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public async Task<IEnumerable<UserAuthority>> GetAllWithUserAsync()
        {
            return await _db.UserAuthority
                .Include(ua => ua.User)
                .Where(ua => ua.User != null && ua.User.LevelAuthorization == true)
                .ToListAsync();
        }
    }
}
