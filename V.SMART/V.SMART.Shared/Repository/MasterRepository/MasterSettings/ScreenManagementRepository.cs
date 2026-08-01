using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.MasterScreeenManagement;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IMasterSettings;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace V.SMART.Shared.Repository.MasterRepository.MasterSettings
{
    public class ScreenManagementRepository : Repository<ScreenManagement>, IScreenManagementRepository
    {
        private readonly ApplicationDbContext _db;
        protected readonly ILoggingService _logs;
        public ScreenManagementRepository(ApplicationDbContext db, ILoggingService loggingService) : base(db, loggingService)
        {
            _db = db;
            _logs = loggingService;
        }

        public async Task<bool> IsFeatureEnabledAsync(string screenName, string displayName)
        {
            return await _db.ScreenManagements.AnyAsync(s =>
                s.ScreenName == screenName &&
                s.Display == displayName &&
                s.Required == true);
        }
    }

}
