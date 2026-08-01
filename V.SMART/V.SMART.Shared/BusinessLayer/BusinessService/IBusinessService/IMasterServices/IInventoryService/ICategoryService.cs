using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IInventoryService
{
    public interface ICategoryService
    {
        Task<(List<CategoryVM> Categories, int TotalCount)>SearchWithDynamicFilterAsync(int pageNumber, int pageSize,Dictionary<string, object>? filters);
        Task<CategoryVM?> GetCategoryAsync(int categoryCode);
        Task<bool> IsDuplicateCategoryNameAsync(string categoryName, int? categoryCode = null);
        Task<CategoryVM> CreateCategoryAsync(CategoryVM category);
        Task<CategoryVM> UpdateCategoryAsync(CategoryVM category);
        //Task<IQueryable<Category>> GetAllCategoriesAsync(string? keyword = null);
        Task<bool> DeleteCategoryAsync(int categoryCode);
        Task<(bool CanDelete, string Message)> CanDeleteCategoryAsync(int categoryId);
    }
}
