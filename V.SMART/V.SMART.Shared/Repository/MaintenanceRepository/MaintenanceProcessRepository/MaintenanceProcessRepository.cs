using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Maintenance.Maintenance_Process;
using V.SMART.Shared.Repository.IRepository.IMaintenanceRepository.IMaintenanceProcess;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.MaintenanceRepository.MaintenanceProcessRepository
{
    public class MaintenanceProcessRepository : Repository<MaintenanceProcess>, IMaintenanceProcessRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _loggingService;

        public MaintenanceProcessRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService)
            : base(db, loggingService)
        {
            _db = db;
            _currentUserService = currentUserService;
            _loggingService = loggingService;
        }

    }
}
