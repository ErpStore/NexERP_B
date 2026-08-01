using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISettingsService;
using V.SMART.Shared.Data.GeneralSettingsMaster;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.SalesAndLabour.SalesDC;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesDCVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.SettingsService
{
    public class StoreMapService : IStoreMappingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CurrentUserService _userService;
        private readonly ILoggingService _loggingService;
        private readonly ICommonService _commonService;

        public StoreMapService(
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

        public async Task<IEnumerable<Store>> GetAllActiveStoresAsync()
        {
            return await _commonService.GetAllActiveStoresAsync();
        }

        public async Task<IEnumerable<Store>> GetAllStoreIssueAsync()
        {
            return await _commonService.GetAllIssueStoresAsync();
        }

        public async Task<List<StoreMap>> GetAllScreenStoreMapAsync()
        {
            return await _unitOfWork.StoreMaps.GetQueryable()
                .Include(sm => sm.Store)
                .OrderBy(sm => sm.SlNo)
                .ToListAsync();
        }


        public async Task SaveScreenStoreMapsAsync(IEnumerable<StoreMap> storeMaps)
        {
            if (storeMaps == null || !storeMaps.Any())
                throw new ArgumentException("No store maps to save.", nameof(storeMaps));

            var changes = new StringBuilder();
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                foreach (var storeMap in storeMaps)
                {
                    if (storeMap.Id == 0)
                    {
                        // New entry
                        await _unitOfWork.StoreMaps.CreateAsync(storeMap);
                        changes.AppendLine("<tr><td colspan='4'><b>New Store Map Created</b></td></tr>");
                    }
                    else
                    {
                        // Compare with existing
                        var existing = await _unitOfWork.StoreMaps.GetAsync(storeMap.Id);
                        if (existing == null)
                            throw new InvalidOperationException($"Store Map with Id {storeMap.Id} not found.");

                        var diff = GetPropertyChanges(existing, storeMap);
                        if (!string.IsNullOrWhiteSpace(diff))
                        {
                            changes.Append(diff);
                        }

                        await _unitOfWork.StoreMaps.UpdateAsync(storeMap);
                    }
                }

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Log once with all changes
                await LogChangesAsync(changes, "Store Maps Saved");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, "Failed to save store maps batch");
                throw new InvalidOperationException("Failed to save Store Maps. Please try again.");
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
                screen: "Store Map",
                action: action,
                additionalInfo: changes.ToString()
            );
        }




    }
}
