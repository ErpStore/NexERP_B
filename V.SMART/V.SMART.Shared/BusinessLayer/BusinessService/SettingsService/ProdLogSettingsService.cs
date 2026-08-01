using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISettingsService;
using V.SMART.Shared.Data.GeneralSettingsMaster;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Production.ProductionIssueWOAssy;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.SettingsService
{
    public class ProdLogSettingService : IProdLogSettingsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public ProdLogSettingService(IUnitOfWork unitOfWork, ILoggingService loggingService, CurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public async Task<ProductionLogSetting?> GetProductionLogSettingsAsync(int id)
        {
            try
            {
                return await _unitOfWork.ProductionLogSettings.GetAsync(id);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to get ProductionLogSettings with code {id}");
                throw;
            }
        }

        public async Task<bool> IsDuplicateProdTypeNameAsync(string prodType, int? Id = null)
        {
            try
            {
                return await _unitOfWork.ProductionLogSettings.ExistsByNameAsync(
                  "ProdStopType",
                  prodType?.Trim(),
                  "Id",
                  Id);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error checking duplicate Prouction log Type name");
                throw;
            }
        }


        public async Task<ProductionLogSetting> CreateProdLogSettingsAsync(ProductionLogSetting productionLogSetting)
        {
            try
            {

                await _unitOfWork.ProductionLogSettings.CreateAsync(productionLogSetting);
                await _unitOfWork.SaveAsync();

                await _loggingService.LogUserAction(await _currentUserService.GetUsernameAsync(), Environment.MachineName, _currentUserService.IpAddress, "Production Log Settings", $"Created Prod Stop Type: {productionLogSetting.ProdStopType}", "");
                return productionLogSetting;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to create Production Log Type: {productionLogSetting.ProdStopType}");
                throw;
            }
        }

        public async Task<ProductionLogSetting> UpdateProdLogSettingsAsync(ProductionLogSetting productionLogSetting)
        {
            try
            {

                await _unitOfWork.ProductionLogSettings.UpdateAsync(productionLogSetting); 
                await _unitOfWork.SaveAsync();

                await _loggingService.LogUserAction(await _currentUserService.GetUsernameAsync(), Environment.MachineName, _currentUserService.IpAddress, "Production Log Settings", $"Updated Production log Type: {productionLogSetting.ProdStopType}", "");
                return productionLogSetting;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to update Prouction Stop Type: {productionLogSetting.ProdStopType}");
                throw;
            }
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync(string? keyword = null)
        {
            try
            {
                var categories = await _unitOfWork.Categories.GetAllAsync();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    keyword = keyword.Trim().ToLower();
                    categories = categories.Where(c =>
                      (!string.IsNullOrEmpty(c.CategoryName) && c.CategoryName.ToLower().Contains(keyword)) ||
                      (!string.IsNullOrEmpty(c.CategoryDesc) && c.CategoryDesc.ToLower().Contains(keyword))
                    );
                }

                return categories.OrderByDescending(c => c.CreatedDate).ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error fetching categories");
                return Enumerable.Empty<Category>();
            }
        }


        public async Task<(List<ProductionLogSetting> prodLogSettings, int TotalCount)>
            SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
            Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.ProductionLogSettings.GetQueryable().AsQueryable();

            if (filters != null)
            {
                foreach (var f in filters)
                {
                    query = DynamicWhereBuilder.ApplyFilter(query, f.Key, f.Value);
                }
            }

            var total = await query.CountAsync();

            var list = await query
                .OrderByDescending(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (list, total);
        }

        public static class DynamicWhereBuilder
        {
            public static IQueryable<ProductionLogSetting> ApplyFilter(
                IQueryable<ProductionLogSetting> query, string field, object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                string val = value.ToString().Trim();

                switch (field)
                {
                    case "StopType":
                        return query.Where(x => x.ProdStopType.Contains(value.ToString()));

                    case "Status":
                        return ApplyStatusFilter(query, val);
                  
                }

                return query;
            }

            private static IQueryable<ProductionLogSetting> ApplyStatusFilter(
                IQueryable<ProductionLogSetting> query, string status)
            {
                return status switch
                {
                    "Completed" => query.Where(x => x.IsActive == true),
                    "Pending" => query.Where(x => x.IsActive == false),
                    _ => query
                };
            }
        }


        public async Task<bool> DeleteProductionLogSettingsAsync(int id)
        {
            try
            {
                var prodlogStop = await _unitOfWork.ProductionLogSettings.GetAsync(id);
                if (prodlogStop == null)
                    return false;

                var deleted = await _unitOfWork.ProductionLogSettings.DeleteAsync(id);
                if (deleted)
                {
                    await _unitOfWork.SaveAsync();

                    await _loggingService.LogUserAction(
                      UserName: await _currentUserService.GetUsernameAsync(),
                      Machine: _currentUserService.MachineName,
                      IP_Address: _currentUserService.IpAddress,
                      screen: "Production Log Settings",
                      action: $"Deleted Production Log Type: {prodlogStop.ProdStopType}",
                      additionalInfo: $"Id: {prodlogStop.Id}"
                    );

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error deleting Production Log Stop settings with Id {id}");
                return false;
            }
        }
    }


}
