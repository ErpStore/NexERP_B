using AutoMapper;
using DocumentFormat.OpenXml.InkML;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IInventoryService;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.MasterService.InventoryService
{
    public class StoreService: IStoreService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ForeignKeyUsageChecker _fkChecker;

        public StoreService(
            IUnitOfWork unitOfWork,
            ILoggingService loggingService,
            CurrentUserService currentUserService,
            IMapper mapper  ,
            ForeignKeyUsageChecker fkChecker)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _fkChecker = fkChecker;
        }

        public async Task<(List<StoreVM> storeVMs, int TotalCount)>
            SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
            Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.Stores.GetQueryable();

            if (filters != null)
            {
                foreach (var f in filters)
                {
                    query = DynamicWhereBuilder.ApplyFilter(query, f.Key, f.Value);
                }
            }

            var total = await query.CountAsync();

            var entities = await query
                .OrderByDescending(x => x.StoreId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 🔥 Map Entity List -> VM List
            var list = _mapper.Map<List<StoreVM>>(entities);

            return (list, total);
        }


        public static class DynamicWhereBuilder
        {
            public static IQueryable<Store> ApplyFilter(
                IQueryable<Store> query, string field, object value)
            {
                if (value == null) return query;

                switch (field)
                {
                    case "StoreName":
                        return query.Where(x => x.StoreName.Contains(value.ToString()));

                    case "CreatedBy":
                        return query.Where(x => x.CreatedBy.Contains(value.ToString()));

                    case "FromDate":
                        return query.Where(x => x.CreatedDate >= DateTime.Parse(value.ToString()));

                    case "ToDate":
                        return query.Where(x => x.CreatedDate <= DateTime.Parse(value.ToString()));
                }

                return query;
            }
        }

        public async Task<(bool CanDelete, string Message)> CanDeleteStoreAsync(int storeId)
        {
            try
            {
                var store = await _unitOfWork.Stores
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.StoreId == storeId);

                if (store == null)
                    return (false, "Store not found or already removed.");

                if (store.IsSystemDefined)
                    return (false, $"'{store.StoreName}' is a system-defined store and cannot be deleted.");

                var usedIn = await _fkChecker.GetUsageTableAsync<Store>(storeId);

                if (usedIn != null)
                    return (false, $"Cannot delete store '{store.StoreName}' because it is used in {usedIn}.");

                return (true, $"Store '{store.StoreName}' can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Store delete validation failed: {storeId}");
                return (false, "Unexpected error occurred while validating store.");
            }
        }

        public async Task<bool> DeleteStoreAsync(int storeId)
        {
            try
            {
                var store = await _unitOfWork.Stores.GetAsync(storeId);

                if (store == null || store.IsSystemDefined)
                    return false;

                await _unitOfWork.Stores.DeleteAsync(store);
                await _unitOfWork.SaveAsync();

                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Store List",
                    action: $"Deleted Store: {store.StoreName}",
                    additionalInfo: $"StoreName: {store.StoreName}"
                );
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error deleting Store with Id {storeId}");
                return false;
            }
        }

        public async Task<StoreVM?> GetStoreDataByStoreId(int storeId)
        {
            try
            {
                if (storeId <= 0)
                    return null;

                var entity = await _unitOfWork.Stores.GetQueryable()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.StoreId == storeId);

                return _mapper.Map<StoreVM>(entity);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex,
                    $"Error while fetching Store data for StoreId : {storeId}");

                throw new Exception("Unable to load store details. Please contact administrator.");
            }
        }

        public async Task<ServiceResult> UpsertStoreAsync(StoreVM vm)
        {
            try
            {
                if (vm.IsSystemDefined)
                    return ServiceResult.Fail("You cannot modify a system-defined Store.");

                if (!vm.CanIssue && !vm.CanAdd)
                    return ServiceResult.Fail("Please enable either 'Can Issue', 'Can Add', or both.");

                bool isDuplicate = await _unitOfWork.Stores.ExistsByNameAsync(
                    "StoreName",
                    vm.StoreName,
                    "StoreId",
                    vm.StoreId == 0 ? null : vm.StoreId);

                if (isDuplicate)
                    return ServiceResult.Fail("Store name already exists.");


                bool isNew = vm.StoreId == 0;

                await UpsertStoresAsync(vm);

                return ServiceResult.Ok(isNew
                    ? "Store created successfully"
                    : "Store updated successfully");
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Store Upsert Failed");
                return ServiceResult.Fail("Unable to save store. Contact administrator.");
            }
        }

        public async Task<StoreVM> UpsertStoresAsync(StoreVM vm)
        {
            if (vm == null)
                throw new ArgumentNullException(nameof(vm));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                Store entity;

                // ----------------- CREATE NEW USER Auth -----------------
                if (vm.StoreId == 0)
                {

                    entity = _mapper.Map<Store>(vm);
                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;

                    await _unitOfWork.Stores.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();
                    changes.AppendLine("Store Created.");
                }
                else
                {
                    entity = await _unitOfWork.Stores.GetQueryable()
                        .FirstOrDefaultAsync(u => u.StoreId == vm.StoreId)
                        ?? throw new InvalidOperationException("Store not found for update.");

                    var propertyChanges = GetPropertyChanges(entity, vm);
                    if (!string.IsNullOrEmpty(propertyChanges))
                        changes.AppendLine(propertyChanges);

                    _mapper.Map(vm, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await _unitOfWork.Stores.UpdateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine("Store Updated.");
                }

                await transaction.CommitAsync();

                await LogChangesAsync(changes, vm.StoreId == 0 ? "Store Created" : "Store Updated");

                var savedEntity = await _unitOfWork.Stores.GetQueryable()
                    .FirstOrDefaultAsync(u => u.StoreId == entity.StoreId);

                return _mapper.Map<StoreVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, $"Failed to upsert Store: {vm.StoreName}");
                throw new InvalidOperationException("Failed to save Store. Please try again.");
            }
        }

        private string GetPropertyChanges(Store entity, StoreVM vm)
        {
            var sb = new StringBuilder();

            foreach (var prop in typeof(Store).GetProperties())
            {
                var vmProp = typeof(StoreVM).GetProperty(prop.Name);
                if (vmProp == null) continue;

                var oldVal = prop.GetValue(entity)?.ToString() ?? "null";
                var newVal = vmProp.GetValue(vm)?.ToString() ?? "null";

                if (oldVal != newVal)
                    sb.AppendLine($"{prop.Name}: '{oldVal}' → '{newVal}'");
            }
            return sb.ToString();
        }

        private async Task LogChangesAsync(StringBuilder changes, string action)
        {
            if (changes.Length == 0) return;

            await _loggingService.LogUserAction(
                UserName: await _currentUserService.GetUsernameAsync(),
                Machine: _currentUserService.MachineName,
                IP_Address: _currentUserService.IpAddress,
                screen: "Store Master",
                action: action,
                additionalInfo: changes.ToString()
            );
        }

        public class ServiceResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = "";

            public static ServiceResult Fail(string message)
                => new() { Success = false, Message = message };

            public static ServiceResult Ok(string message)
                => new() { Success = true, Message = message };
        }


    }
}
