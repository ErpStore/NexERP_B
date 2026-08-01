using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.MasterScreeenManagement;
using V.SMART.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace V.SMART.Shared.Services
{
    public class ColumnPreferenceService : IColumnPreferenceService
    {
        private readonly ApplicationDbContext _db;

        public ColumnPreferenceService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<GridColumn>> LoadPreferencesAsync(string screenName, string userId)
        {
            var pref = await _db.UserPreferences
                .FirstOrDefaultAsync(p => p.ScreenName == screenName && p.UserId == userId);

            if (pref == null) return new List<GridColumn>();

            return JsonSerializer.Deserialize<List<GridColumn>>(pref.PreferenceJson);
        }

        public async Task SavePreferencesAsync(string screenName, string userId, List<GridColumn> settings)
        {
            var json = JsonSerializer.Serialize(settings);

            var existing = await _db.UserPreferences
                .FirstOrDefaultAsync(p => p.ScreenName == screenName && p.UserId == userId);

            if (existing != null)
            {
                existing.PreferenceJson = json;
            }
            else
            {
                _db.UserPreferences.Add(new UserPreference
                {
                    ScreenName = screenName,
                    UserId = userId,
                    PreferenceJson = json
                });
            }

            await _db.SaveChangesAsync();
        }
    }

}
