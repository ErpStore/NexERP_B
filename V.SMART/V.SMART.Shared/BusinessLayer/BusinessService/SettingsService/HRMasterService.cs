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
    public class HRMasterService : IHRMasterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CurrentUserService _userService;
        private readonly ILoggingService _loggingService;
        private readonly ICommonService _commonService;
        private readonly IHRMasterService _hRMasterService;

        public HRMasterService(
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
        public async Task<HRMasterSetting> GetHRMSettingAsync()
        {
            return await _unitOfWork.HRMasterSettings.GetFirstAsync()
                   ?? new HRMasterSetting();
        }

        // ✅ SAVE (ONLY ONE RECORD EVER)
        public async Task SaveHRSettingsAsync(HRMasterSetting hrMaster)
        {
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 🔥 ALWAYS FETCH EXISTING RECORD
                var existing = await _unitOfWork.HRMasterSettings.GetFirstAsync();

                if (existing == null)
                {
                    // ✅ FIRST TIME INSERT
                    await _unitOfWork.HRMasterSettings.CreateAsync(hrMaster);

                    changes.AppendLine("<tr><td colspan='4'><b>New HR Master Created</b></td></tr>");
                }
                else
                {
                    // ✅ FORCE UPDATE SAME RECORD
                    hrMaster.Id = existing.Id;

                    var diff = GetPropertyChanges(existing, hrMaster);

                    if (!string.IsNullOrWhiteSpace(diff))
                        changes.Append(diff);

                    await _unitOfWork.HRMasterSettings.UpdateAsync(hrMaster);
                }

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await LogChangesAsync(changes, "HR Master Setting Saved");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, "Failed to save HR Master Settings");
                throw new InvalidOperationException("Failed to save HR Master Settings.");
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
