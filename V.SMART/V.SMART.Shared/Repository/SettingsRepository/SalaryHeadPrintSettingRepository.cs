using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.GeneralSettingsMaster;
using V.SMART.Shared.Repository.IRepository.ISettingsRepository;
using V.SMART.Shared.Services;

namespace V.SMART.Shared.Repository.SettingsRepository
{
    public class SalaryHeadPrintSettingRepository : Repository<SalaryHeadPrintSetting>, ISalaryHeadPrintSettingRepository
    {

        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;


        public SalaryHeadPrintSettingRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService)
            : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }


        public async Task<List<SalaryHeadPrintSetting>> GetAllAsync()
        {
            return await _db.SalaryHeadPrintsSetting
                .AsNoTracking()
                .ToListAsync();
        }


        public async Task SaveAllAsync(List<SalaryHeadPrintSetting> settings)
        {
            _db.ChangeTracker.Clear();

            _db.SalaryHeadPrintsSetting.UpdateRange(settings);

            await _db.SaveChangesAsync();
        }

    }
}
