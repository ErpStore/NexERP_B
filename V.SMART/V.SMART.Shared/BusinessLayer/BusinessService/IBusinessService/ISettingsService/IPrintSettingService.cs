using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.GeneralSettingsMaster;
using V.SMART.Shared.Data.Master.MasterScreeenManagement;
using V.SMART.Shared.ViewModels;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISettingsService
{
    public interface IPrintSettingService
    {
        #region loding screens
        Task<List<Screens?>> GetScreenDetailsAsync();
        Task<IEnumerable<PrintSetting>> GetPrintSettingByScreenName(string screenName);
        Task<IEnumerable<PrintSetting>> UpsertPrintSettingAsync(IEnumerable<PrintSetting> settings);
        Task ToDeleteByIdAsync(int Id);

        Task<List<ScreensVM>> GetScreenPrintSettings();

        Task SaveScreenPrintSettings(List<ScreensVM> list);
        #endregion
    }

}
