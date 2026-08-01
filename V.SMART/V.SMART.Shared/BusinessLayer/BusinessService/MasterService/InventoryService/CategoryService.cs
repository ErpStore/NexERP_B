using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
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
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public CategoryService(
            IUnitOfWork unitOfWork,
            ILoggingService loggingService,
            CurrentUserService currentUserService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }
        public async Task<CategoryVM?> GetCategoryAsync(int categoryCode)
        {
            try
            {
                var entity = await _unitOfWork.Categories.GetAsync(categoryCode);

                if (entity == null)
                    return null;

                return _mapper.Map<CategoryVM>(entity);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to get category with code {categoryCode}");
                throw;
            }
        }


        public async Task<bool> IsDuplicateCategoryNameAsync(string categoryName, int? categoryCode = null)
        {
            try
            {
                return await _unitOfWork.Categories.ExistsByNameAsync(
                  "CategoryName",
                  categoryName?.Trim(),
                  "CategoryCode",
                  categoryCode);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error checking duplicate category name");
                throw;
            }
        }
        public async Task<CategoryVM> CreateCategoryAsync(CategoryVM vm)
        {
            try
            {
                if (await IsDuplicateCategoryNameAsync(vm.CategoryName))
                {
                    await _loggingService.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Category",
                        $"Failed to create duplicate category: {vm.CategoryName}", "");

                    throw new InvalidOperationException("Category name already exists.");
                }

                vm.CategoryName = vm.CategoryName?.Trim();
                vm.CategoryDesc = vm.CategoryDesc?.Trim();

                var entity = _mapper.Map<Category>(vm);

                entity.CreatedBy = await _currentUserService.GetUsernameAsync();
                entity.CreatedDate = DateTime.Now;

                await _unitOfWork.Categories.CreateAsync(entity);
                await _unitOfWork.SaveAsync();

                await _loggingService.LogUserAction(
                    entity.CreatedBy,
                    Environment.MachineName,
                    _currentUserService.IpAddress,
                    "Category",
                    $"Created category: {entity.CategoryName}", "");

                return _mapper.Map<CategoryVM>(entity);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to create category: {vm.CategoryName}");
                throw;
            }
        }

        public async Task<CategoryVM> UpdateCategoryAsync(CategoryVM vm)
        {
            try
            {
                var entity = await _unitOfWork.Categories.GetAsync(vm.CategoryCode);

                if (entity == null)
                    throw new Exception("Category not found");

                if (entity.IsSystemDefined)
                    throw new InvalidOperationException("Cannot modify a system-defined category.");

                if (await IsDuplicateCategoryNameAsync(vm.CategoryName, vm.CategoryCode))
                    throw new InvalidOperationException("Category name already exists.");

                vm.CategoryName = vm.CategoryName?.Trim();
                vm.CategoryDesc = vm.CategoryDesc?.Trim();

                // 🔥 THIS LINE FIXES EVERYTHING
                _mapper.Map(vm, entity);

                entity.ModifiedBy = await _currentUserService.GetUsernameAsync();
                entity.ModifiedDate = DateTime.Now;

                await _unitOfWork.SaveAsync();

                await _loggingService.LogUserAction(
                    entity.ModifiedBy,
                    Environment.MachineName,
                    _currentUserService.IpAddress,
                    "Category",
                    $"Updated category: {entity.CategoryName}", "");

                return _mapper.Map<CategoryVM>(entity);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to update category: {vm.CategoryName}");
                throw;
            }
        }


        public async Task<(List<CategoryVM> Categories, int TotalCount)>
            SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
            Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.Categories.GetQueryable();

            if (filters != null)
            {
                foreach (var f in filters)
                {
                    query = DynamicWhereBuilder.ApplyFilter(query, f.Key, f.Value);
                }
            }

            var total = await query.CountAsync();

            var entities = await query
                .OrderByDescending(x => x.CategoryCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 🔥 Map Entity List -> VM List
            var list = _mapper.Map<List<CategoryVM>>(entities);

            return (list, total);
        }


        public static class DynamicWhereBuilder
        {
            public static IQueryable<Category> ApplyFilter(
                IQueryable<Category> query, string field, object value)
            {
                if (value == null) return query;

                switch (field)
                {
                    case "CategoryName":
                        return query.Where(x => x.CategoryName.Contains(value.ToString()));

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


        public async Task<(bool CanDelete, string Message)> CanDeleteCategoryAsync(int categoryId)
        {
            try
            {
                var category = await _unitOfWork.Categories
                    .GetQueryable()
                    .FirstOrDefaultAsync(c => c.CategoryCode == categoryId);

                if (category == null)
                    return (false, "Category not found or already deleted.");

                bool hasItem = await _unitOfWork.ItemRepositories
                    .GetQueryable()
                    .AnyAsync(i => i.CategoryCode == categoryId);

                if (hasItem)
                    return (false, "Cannot delete this category because it is already assigned to an item.");

                if (category.IsSystemDefined)
                    return (false, "System-defined categories cannot be deleted.");

                return (true, "Category can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex,$"Error while validating delete eligibility for CategoryId: {categoryId}");
                return (false, "An error occurred while validating the category.");
            }
        }


        public async Task<bool> DeleteCategoryAsync(int categoryCode)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetAsync(categoryCode);

                if (category == null || category.IsSystemDefined)
                    return false;

                await _unitOfWork.Categories.DeleteAsync(category);
                await _unitOfWork.SaveAsync();

                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "CategoryList",
                    action: $"Deleted category: {category.CategoryName}",
                    additionalInfo: $"CategoryCode: {category.CategoryCode}"
                );
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error deleting category with code {categoryCode}");
                return false;
            }
        }
    }
}
