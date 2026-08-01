using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISettingsService;
using V.SMART.Shared.Data.GeneralSettingsMaster;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;

namespace V.SMART.Shared.BusinessLayer.BusinessService.SettingsService
{
    public class BiometricExcelSettingService : IBiometricExcelSettingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CurrentUserService _userService;
        private readonly ILoggingService _loggingService;
        private readonly ICommonService _commonService;
        private readonly IHRMasterService _hRMasterService;

        public BiometricExcelSettingService(
            IUnitOfWork unitOfWork,
            CurrentUserService userService,
            ILoggingService loggingService,
            ICommonService commonService)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _loggingService = loggingService;
            _commonService = commonService;
        }

        // ✅ RETURN SINGLE RECORD (NOT LIST)
        public async Task<BiometricExcelSetting> GetBiometricExcelSetAsync()
        {
            try
            {
                return await _unitOfWork.BiometricExcelSettings.GetFirstAsync()
                   ?? new BiometricExcelSetting();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

       
        public async Task SaveBiometricExcelSetsAsync(BiometricExcelSetting biometricExcelSet)
        {
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existing = await _unitOfWork.BiometricExcelSettings.GetFirstAsync();

                if (existing == null)
                {
                    await _unitOfWork.BiometricExcelSettings.CreateAsync(biometricExcelSet);

                    changes.AppendLine("<tr><td colspan='4'><b>New Biometric Excel Setting Created</b></td></tr>");
                }
                else
                {
                    // Always update the existing record
                    biometricExcelSet.Id = existing.Id;

                    var diff = GetPropertyChanges(existing, biometricExcelSet);

                    if (!string.IsNullOrWhiteSpace(diff))
                        changes.Append(diff);

                    await _unitOfWork.BiometricExcelSettings.UpdateAsync(biometricExcelSet);
                }

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await LogChangesAsync(changes, "Biometric Excel Setting Saved");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, "Failed to save Biometric Excel Settings");
                throw;
            }
        }

        public async Task<BiometricExcelSetting?> GetAsync()
        {
            return await _unitOfWork.BiometricExcelSettings.GetAsync();
        }

        // ✅ ALWAYS RETURN SINGLE RECORD
        public async Task<BiometricExcelSetting?> GetLatestAsync()
        {
            return await _unitOfWork.BiometricExcelSettings
                .GetQueryable()
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();
        }

        public async Task SaveAsync(BiometricExcelSetting biometricExcelSetting)
        {
            if (biometricExcelSetting == null)
                throw new ArgumentNullException(nameof(biometricExcelSetting));

            var existing = await GetLatestAsync();

            if (existing == null)
            {
                await _unitOfWork.BiometricExcelSettings.CreateAsync(biometricExcelSetting);
            }
            else
            {
                biometricExcelSetting.Id = existing.Id;
                //setting.CreatedDate = existing.CreatedDate;
                //setting.ModifiedDate = DateTime.Now;

                await _unitOfWork.BiometricExcelSettings.UpdateAsync(biometricExcelSetting);
            }

            await _unitOfWork.SaveAsync();
        }

        public async Task<BiometricExcelSetting?> GetBESByIdAsync(int Id)
        {
            try
            {
                var entity = await _unitOfWork.BiometricExcelSettings
                    .GetQueryable()
                    .FirstOrDefaultAsync(m => m.Id == Id);

                if (entity == null)
                    return null;

                // ✅ RETURN SAVED DATA
                return entity;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"GetBESByIdAsync({Id}) failed.");
                throw;
            }
        }



        // Compare properties and build HTML table for changes
        private string GetPropertyChanges<TSource, TTarget>(TSource oldEntity, TTarget newEntity)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse:collapse;'>");
            sb.AppendLine("<tr style='background-color:#f2f2f2;'><th>Property</th><th>Old Value</th><th>New Value</th></tr>");

            foreach (var prop in typeof(TSource).GetProperties())
            {
                var newProp = typeof(TTarget).GetProperty(prop.Name);
                if (newProp == null) continue;

                var oldVal = prop.GetValue(oldEntity)?.ToString() ?? "null";
                var newVal = newProp.GetValue(newEntity)?.ToString() ?? "null";

                if (oldVal != newVal)
                {
                    sb.AppendLine($"<tr><td>{prop.Name}</td><td>{oldVal}</td><td>{newVal}</td></tr>");
                }
            }

            sb.AppendLine("</table>");
            return sb.ToString();
        }

        private async Task LogChangesAsync(StringBuilder changes, string action)
        {
            if (changes.Length == 0) return;

            await _loggingService.LogUserAction(
                UserName: await _userService.GetUsernameAsync(),
                Machine: _userService.MachineName,
                IP_Address: _userService.IpAddress,
                screen: "HRMaster",
                action: action,
                additionalInfo: changes.ToString()
            );
        }




    }
}
