using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISettingsService
{
    public interface IUserThemePreferenceService
    {
        Task<bool> LoadLocalThemeAsync();

        Task SyncThemeFromDatabaseAsync(Action<bool> updateTheme);

        Task SaveThemePreferenceAsync(bool isDarkMode);
    }
}
