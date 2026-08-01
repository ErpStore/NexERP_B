using V.SMART.Shared.ViewModels;

namespace V.SMART.Shared.Services
{
    public interface IColumnPreferenceService
    {
        Task<List<GridColumn>> LoadPreferencesAsync(string screenName, string userId);
        Task SavePreferencesAsync(string screenName, string userId, List<GridColumn> settings);
    }

}
