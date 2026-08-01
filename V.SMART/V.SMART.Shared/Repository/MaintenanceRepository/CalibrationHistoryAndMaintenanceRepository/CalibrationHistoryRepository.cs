using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Maintenance.CalibrationHistoryAndMaintenance;
using V.SMART.Shared.Repository.IRepository.IMaintenanceRepository.ICalibrationHistoryAndMaintenance;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.MaintenanceRepository.CalibrationHistoryAndMaintenanceRepository
{
    public class CalibrationHistoryRepository : Repository<CalibrationHistory>, ICalibrationHistoryRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _loggingService;
        public CalibrationHistoryRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService)
            : base(db, loggingService)
        {
            _db = db;
            _currentUserService = currentUserService;
            _loggingService = loggingService;
        }
    }
}
