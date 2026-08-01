using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.MasterScreeenManagement;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IMasterSettings;
using V.SMART.Shared.Services;

namespace V.SMART.Shared.Repository.MasterRepository.MasterSettings
{
    public class UserPreferenceRepository : Repository<UserPreference>, IUserPreferenceRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        public UserPreferenceRepository(ApplicationDbContext db, ILoggingService loggingService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
        }
}
}
