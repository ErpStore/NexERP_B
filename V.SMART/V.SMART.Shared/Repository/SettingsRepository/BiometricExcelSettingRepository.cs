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
    public class BiometricExcelSettingRepository : Repository<BiometricExcelSetting>, IBiometricExcelSettingRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public BiometricExcelSettingRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService)
            : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public async Task<BiometricExcelSetting> GetFirstAsync()
        {
            return await _db.BiometricExcelSetting
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<BiometricExcelSetting?> GetAsync()
        {
            return await _db.BiometricExcelSetting.FirstOrDefaultAsync();
        }

        public async Task CreateAsync(BiometricExcelSetting entity)
        {
            await _db.BiometricExcelSetting.AddAsync(entity);
        }

        public Task UpdateAsync(BiometricExcelSetting entity)
        {
            var trackedEntity = _db.BiometricExcelSetting.Local
                .FirstOrDefault(x => x.Id == entity.Id);

            if (trackedEntity != null)
            {
                _db.Entry(trackedEntity).State = EntityState.Detached;
            }

            _db.BiometricExcelSetting.Update(entity);

            return Task.CompletedTask;
        }
    }
}
