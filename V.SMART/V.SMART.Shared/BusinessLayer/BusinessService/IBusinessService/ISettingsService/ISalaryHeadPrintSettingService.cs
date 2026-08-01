using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.GeneralSettingsMaster;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISettingsService
{
    public interface ISalaryHeadPrintSettingService
    {
      
        Task UpsertSalaryPrintHeadSettingAsync(List<SalaryHeadPrintSetting> settings);
        Task<List<SalaryHeadPrintSetting>> GetAllSalaryPrintHeadSettingsAsync();
        Task<List<SalaryHeadPrintSetting>> GetAllAsync();
    }

}
