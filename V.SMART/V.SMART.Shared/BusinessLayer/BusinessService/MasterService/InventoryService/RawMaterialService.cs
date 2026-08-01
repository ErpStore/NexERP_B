using AutoMapper;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
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
    public class RawMaterialService : IRawMaterialService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ForeignKeyUsageChecker _fkChecker;

        public RawMaterialService(
            IUnitOfWork unitOfWork,
            ILoggingService loggingService,
            CurrentUserService currentUserService,
            IMapper mapper,
            ForeignKeyUsageChecker fkChecker)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _fkChecker = fkChecker;
        }

        public async Task<(List<RawMaterialVM> rawMaterialVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.RawMaterial
                    .GetQueryable()
                    .AsQueryable();

                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        query = RmFilterBuilder.ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.RMId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<RawMaterialVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync (Raw- Material)");
                throw new InvalidOperationException("Failed to load Material list.", ex);
            }
        }

        public static class RmFilterBuilder
        {
            public static IQueryable<RawMaterial> ApplyFilter(
                IQueryable<RawMaterial> query,
                string field,
                object value)
            {
                try
                {
                    if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                        return query;

                    string val = value.ToString()!.Trim();

                    switch (field)
                    {
                        case "RmName":
                            return query.Where(x => x.RMName.Contains(val));

                        case "CreatedBy":
                            return query.Where(x =>
                                x.CreatedBy != null &&
                                x.CreatedBy.Contains(val));

                        case "FromDate":
                            if (DateTime.TryParse(val, out var fromDate))
                                return query.Where(x => x.CreatedDate >= fromDate.Date);
                            return query;

                        case "ToDate":
                            if (DateTime.TryParse(val, out var toDate))
                                return query.Where(x =>
                                    x.CreatedDate <= toDate.Date.AddDays(1).AddTicks(-1));
                            return query;
                    }

                    return query;
                }
                catch
                {
                    return query;
                }
            }
        }

        public async Task<(bool CanDelete, string Message)> CanDeleteRMAsync(int rmId)
        {
            try
            {
                var store = await _unitOfWork.RawMaterial
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.RMId == rmId);

                if (store == null)
                    return (false, "Raw Material not found or already removed.");

                var usedIn = await _fkChecker.GetUsageTableAsync<RawMaterial>(rmId);

                if (usedIn != null)
                    return (false, $"Cannot delete Raw-Material '{store.RMName}' because it is used in {usedIn} Screen.");

                return (true, $"Material '{store.RMId}' can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Store delete validation failed: {rmId}");
                return (false, "Unexpected error occurred while validating Raw Material.");
            }
        }


        public async Task<bool> DeleteRmAsync(int rmId)
        {
            try
            {
                var store = await _unitOfWork.RawMaterial.GetAsync(rmId);

                if (store == null)
                    return false;

                await _unitOfWork.RawMaterial.DeleteAsync(rmId);
                await _unitOfWork.SaveAsync();

                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Raw Material List",
                    action: $"Deleted Raw Material: {store.RMName}",
                    additionalInfo: $"Material Name: {store.RMName}"
                );
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error deleting Raw Material with Id {rmId}");
                return false;
            }
        }

        public async Task<RawMaterialVM?> GetRawMaterialByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return null;

                var entity = await _unitOfWork.RawMaterial.GetAsync(id);

                if (entity == null)
                    return null;

                var vm = _mapper.Map<RawMaterialVM>(entity);

                return vm;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex,"Error occurred while fetching RawMaterial in Service");
                return null;
            }
        }

        public async Task<bool> IsDuplicateRawMaterialAsync(string rmName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rmName))
                    return false;

                bool isDuplicate = await _unitOfWork.RawMaterial.GetQueryable().AnyAsync(x=> x.RMName == rmName);
                return isDuplicate;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex,"Error occurred while checking duplicate RawMaterial");
                return false;
            }
        }


        public async Task<RawMaterialVM> UpsertRMAsync(RawMaterialVM vm)
        {
            if (vm == null)
                throw new ArgumentNullException(nameof(vm));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                RawMaterial entity;

                // ----------------- CREATE NEW USER Auth -----------------
                if (vm.RMId == 0)
                {

                    entity = _mapper.Map<RawMaterial>(vm);
                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;

                    await _unitOfWork.RawMaterial.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();
                    changes.AppendLine("Raw Material Created.");
                }
                else
                {
                    entity = await _unitOfWork.RawMaterial.GetQueryable()
                        .FirstOrDefaultAsync(u => u.RMId == vm.RMId)
                        ?? throw new InvalidOperationException("Raw Material not found for update.");

                    var propertyChanges = GetPropertyChanges(entity, vm);
                    if (!string.IsNullOrEmpty(propertyChanges))
                        changes.AppendLine(propertyChanges);

                    _mapper.Map(vm, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await _unitOfWork.RawMaterial.UpdateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine("Raw Material Updated.");
                }

                await transaction.CommitAsync();

                await LogChangesAsync(changes, vm.RMId == 0 ? "Raw Material Created" : "Raw Material Updated");

                var savedEntity = await _unitOfWork.RawMaterial.GetQueryable()
                    .FirstOrDefaultAsync(u => u.RMId == entity.RMId);

                return _mapper.Map<RawMaterialVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, $"Failed to upsert RawMaterial: {vm.RMName}");
                throw new InvalidOperationException("Failed to save Raw Material. Please try again.");
            }
        }

        private string GetPropertyChanges(RawMaterial entity, RawMaterialVM vm)
        {
            var sb = new StringBuilder();

            foreach (var prop in typeof(RawMaterial).GetProperties())
            {
                var vmProp = typeof(RawMaterialVM).GetProperty(prop.Name);
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
                screen: "Material Master",
                action: action,
                additionalInfo: changes.ToString()
            );
        }



    }
}
