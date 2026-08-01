using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IMasterSettings;
using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.MasterRepository.MasterSettings
{
        public class InspectionSettingsRepository : Repository<InspectionSettings>,IInspectionSettingsRepository
        {
            private readonly ApplicationDbContext _db;
            private readonly ILoggingService _loggingService;
            private readonly CurrentUserService _currentUserService;
            public InspectionSettingsRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService) : base(db, loggingService)
            {
                _db = db;
                _loggingService = loggingService;
                _currentUserService = currentUserService;
            }
        }
}
