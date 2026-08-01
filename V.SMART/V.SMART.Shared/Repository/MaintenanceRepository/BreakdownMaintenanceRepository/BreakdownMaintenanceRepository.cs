using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Maintenance.BreakdownMaintenance;
using V.SMART.Shared.Data.Maintenance.BreakdownMaintenance;
using V.SMART.Shared.Repository.IRepository.IMaintenanceRepository.IBreakdownMaintenance;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.MaintenanceRepository.BreakdownMaintenanceRepository
{
    public class BreakdownMaintenanceRepository : Repository<BreakdownMaintenance>, IBreakdownMaintenanceRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _loggingService;
        public BreakdownMaintenanceRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService)
            : base(db, loggingService)
        {
            _db = db;
            _currentUserService = currentUserService;
            _loggingService = loggingService;
        }

    }
}
