using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISettingsService;
using V.SMART.Shared.Data.Master.MasterScreeenManagement;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;

namespace V.SMART.Shared.BusinessLayer.BusinessService.SettingsService
{
    public class UserThemePreferenceService : IUserThemePreferenceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly IJSRuntime _jsRuntime;

        public UserThemePreferenceService(
            IUnitOfWork unitOfWork,
            ILoggingService loggingService,
            CurrentUserService currentUserService,
            IJSRuntime jsRuntime)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
            _jsRuntime = jsRuntime;
        }

        // =========================================
        // LOAD LOCAL THEME
        // =========================================
        public async Task<bool> LoadLocalThemeAsync()
        {
            try
            {
                var localValue = await _jsRuntime.InvokeAsync<string>(
                        "localStorage.getItem",
                        "theme-mode");

                if (bool.TryParse(localValue, out bool isDark))
                {
                    return isDark;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        // =========================================
        // SYNC THEME FROM DATABASE
        // =========================================
        public async Task SyncThemeFromDatabaseAsync(
            Action<bool> updateTheme)
        {
            try
            {
                var userId = await _currentUserService.GetUserIdAsync();

                if (userId == 0)
                    return;

                var preference = await _unitOfWork.UserThemePreferences
                        .GetQueryable()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(
                            x => x.UserId == userId);

                if (preference == null)
                    return;

                updateTheme?.Invoke(preference.IsDarkMode);

                await _jsRuntime.InvokeVoidAsync(
                    "localStorage.setItem",
                    "theme-mode",
                    preference.IsDarkMode.ToString());
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(
                    ex,
                    "Error while syncing theme preference from database");
            }
        }

        // =========================================
        // SAVE THEME PREFERENCE
        // =========================================
        public async Task SaveThemePreferenceAsync(bool isDarkMode)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync(
                    "localStorage.setItem",
                    "theme-mode",
                    isDarkMode.ToString());

                var userId = await _currentUserService.GetUserIdAsync();

                if (userId == null || userId == 0)
                    return;

                var preference =
                    await _unitOfWork.UserThemePreferences
                        .GetQueryable()
                        .FirstOrDefaultAsync(
                            x => x.UserId == userId);

                if (preference == null)
                {
                    preference = new UserThemePreference
                    {
                        UserId = userId,
                        IsDarkMode = isDarkMode
                    };
                    await _unitOfWork.UserThemePreferences.CreateAsync(preference);
                }
                else
                {
                    preference.IsDarkMode = isDarkMode;
                    await _unitOfWork.UserThemePreferences.UpdateAsync(preference);
                }

                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex,"Error while saving theme preference");
            }
        }
    }

}