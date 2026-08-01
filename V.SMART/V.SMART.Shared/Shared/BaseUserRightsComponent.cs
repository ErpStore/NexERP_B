using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.Json;
using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.Data.Master.Admin_Module;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.MasterViewModel.AdminViewmodel;
namespace V.SMART.Shared.Shared
{
    public abstract class BaseUserRightsComponent : ComponentBase
    {
        [Inject] protected IUnitOfWork _unitOfWork { get; set; }
        [Inject] protected CurrentUserService _userService { get; set; }

        protected List<ColumnPreferenceVM> Columns = new();

        protected abstract string ScreenName { get; }

        protected List<UserRight> userRights = new();

        protected bool CanView => RightsHelper.HasViewRight(userRights, ScreenName);
        protected bool CanCreate => RightsHelper.HasCreateRight(userRights, ScreenName);
        protected bool CanEdit => RightsHelper.HasEditRight(userRights, ScreenName);
        protected bool CanDelete => RightsHelper.HasDeleteRight(userRights, ScreenName);
        protected bool IsHidden => RightsHelper.IsHidden(userRights, ScreenName);

        protected async Task LoadRightsAsync()
        {
            try
            {
                var userId = await _userService.GetUserIdAsync();
                userRights = await _unitOfWork.UserRights.GetUserRightsWithScreensAsync(userId);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load user rights", ex);
            }
        }
        public bool ShowColumn(string field)
        {
            return Columns.FirstOrDefault(x => x.Field == field)?.IsVisible ?? true;
        }

        public async Task LoadColumns()
        {
            try
            {
                var userId = await _userService.GetUsernameAsync();

                if (string.IsNullOrWhiteSpace(userId))
                    return;

                if (string.IsNullOrWhiteSpace(ScreenName))
                    return;

                Columns = GetColumnsByScreen(ScreenName);

                var allData =
              await _unitOfWork.UserColumnPreferences.GetAllAsync();

                var existing =
                    allData.FirstOrDefault(x =>
                        x.UserName == userId &&
                        x.ScreenName == ScreenName);
                if (existing != null)
                {
                    Columns =
                    System.Text.Json.JsonSerializer
                    .Deserialize<List<ColumnPreferenceVM>>
                    (
                        existing.ColumnJson
                    ) ?? Columns;
                }

                StateHasChanged();


            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load LoadColumns()", ex);
            }


        }
        private List<ColumnPreferenceVM> GetColumnsByScreen(string screenName)
        {
            var type = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t =>
                {
                    var attr = t.GetCustomAttribute<ScreenMappingAttribute>();
                    return attr?.ScreenName == screenName;
                });

            if (type == null)
                return new();

            return type.GetProperties()
                .Select(p => new ColumnPreferenceVM
                {
                    Field = p.Name,
                    Title = p.Name,
                    IsVisible = true,
                    IsDate = p.PropertyType == typeof(DateTime)
                          || p.PropertyType == typeof(DateTime?)
                })
                .ToList();
        }
        public async Task SaveColumnPreferences()
        {
            var userName = await _userService.GetUsernameAsync();

            if (string.IsNullOrWhiteSpace(userName))
                return;

            var json = JsonSerializer.Serialize(Columns);

            var existing = await _unitOfWork.UserColumnPreferences
                .GetQueryable()
                .FirstOrDefaultAsync(x =>
                    x.UserName == userName &&
                    x.ScreenName == ScreenName);

            if (existing == null)
            {
                existing = new UserColumnPreference
                {
                    UserName = userName,
                    ScreenName = ScreenName,
                    ColumnJson = json
                };

                await _unitOfWork.UserColumnPreferences.CreateAsync(existing);
            }
            else
            {
                existing.ColumnJson = json;

                _unitOfWork.UserColumnPreferences.UpdateAsync(existing);
            }

            await _unitOfWork.SaveAsync();
        }
    }
}
