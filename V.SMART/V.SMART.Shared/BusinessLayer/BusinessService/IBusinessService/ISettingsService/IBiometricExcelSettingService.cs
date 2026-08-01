using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.GeneralSettingsMaster;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISettingsService
{
    public interface IBiometricExcelSettingService
    {
        Task<BiometricExcelSetting> GetBiometricExcelSetAsync();
        Task SaveBiometricExcelSetsAsync(BiometricExcelSetting biometricExcelSet);


        Task<BiometricExcelSetting?> GetAsync();
        Task<BiometricExcelSetting?> GetLatestAsync();
        Task SaveAsync(BiometricExcelSetting biometricExcelSet);
        Task<BiometricExcelSetting?> GetBESByIdAsync(int Id);
    }
}
