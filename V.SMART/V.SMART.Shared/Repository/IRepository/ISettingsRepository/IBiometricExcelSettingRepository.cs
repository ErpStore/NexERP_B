using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.GeneralSettingsMaster;

namespace V.SMART.Shared.Repository.IRepository.ISettingsRepository
{
    public interface IBiometricExcelSettingRepository : IRepository<BiometricExcelSetting>
    {
        Task<BiometricExcelSetting> GetFirstAsync();
        Task<BiometricExcelSetting?> GetAsync();
        Task CreateAsync(BiometricExcelSetting entity);
        Task UpdateAsync(BiometricExcelSetting entity);
    }
}
