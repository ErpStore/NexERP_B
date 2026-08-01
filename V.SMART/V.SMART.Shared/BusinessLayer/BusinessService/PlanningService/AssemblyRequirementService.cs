
using AutoMapper;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IPlanningService;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using V.SMART.Shared.ViewModels.PlanningViewModel.AssyReqAnalysisVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.PlanningService
{
    public class AssemblyRequirementService : IAssemblyRequirementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly IStockManagerService _stockManagerService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;

        public AssemblyRequirementService(
                                    IUnitOfWork unitOfWork,
                                    ICommonService commonService,
                                    IStockManagerService stockManagerService,
                                    CurrentUserService userService,
                                    ILoggingService logs,
                                    IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _stockManagerService = stockManagerService;
            _currentUserService = userService;
            _logs = logs;
            _mapper = mapper;
        }

        // Get All Items By Category Code
        public async Task<List<ItemVM>> GetAllItemsByCategoryCode(int Id)
            => await _commonService.GetAllItemsByCategoryCode(Id);

        // Get Stock For Items
        public async Task<Dictionary<int, decimal>> GetStockForItemsAsync(IEnumerable<int> itemIds, int? storeId)
            => await _stockManagerService.GetStockForItemsAsync(itemIds, storeId);
        
        #region Get All Assemblies
        public async Task<List<AssemblyDefVM>> GetAllAssemblyItemsAsync()
        {
            try 
            { 
                return _mapper.Map<List<AssemblyDefVM>>(
                    await _unitOfWork.AssmblyDefs.GetQueryable()
                        .Include(x => x.AssemblyItem)
                        .Include(x => x.PartItem)
                        .ThenInclude(x => x.Category)
                        .ToListAsync()
                );
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in GetAllAssemblyItemsAsync()" + ex.Message);
                return new List<AssemblyDefVM>();
            }
        }
        #endregion

        #region Get Assemblies Ref to ItemID
        public async Task<List<AssemblyDefVM>> GetAssemblyByItemIdAsync(int itemID)
        {
            try
            {
              var Assemlyitems=await _unitOfWork.AssmblyDefs.GetQueryable()
                                    .Include(x => x.AssemblyItem)
                                    .Include(x => x.PartItem)
                                    .Where(x => x.AssmblyID == itemID)
                                    .ToListAsync();
              return _mapper.Map<List<AssemblyDefVM>>(Assemlyitems);

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in GetAllAssemblyItemsAsync()" + ex.Message);
                return new List<AssemblyDefVM>();
            }
        }
        #endregion

        #region Building the BOM Tree  
        public List<TreeNode> BuildBomTree(int parentAssemblyId, List<AssemblyDefVM> rows, int level)
        {
            try
            {
                return rows
                    .Where(x => x.AssmblyID == parentAssemblyId)   // PARENT
                    .Select(x => new TreeNode
                    {
                        Id = x.ItemId ?? 0,
                        ItemCode = x.PartItemCode,
                        ItemName = x.PartItemName,
                        QtyPerParent = x.UtilQty ?? 0,
                        Level= level,
                        Category = x.CategoryName,
                        UOM = x.UOM,
                        Children = BuildBomTree(x.ItemId.Value, rows, level+1) // CHILD
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logs.LogDeveloperError(ex, "Error in BuildBomTree()" + ex.Message).Wait();
                return new List<TreeNode>();
            }
        }
        #endregion

        #region Get Item Rates by Item IDs
        public async Task<Dictionary<int, decimal>> GetItemRatesByItemIdsAsync(List<int> itemIds)
        {
            try
            {
                return await _unitOfWork.ItemRepositories
                    .GetQueryable()
                    .Where(i => itemIds.Contains(i.ItemId))
                    .Select(i => new
                    {
                        i.ItemId,
                        i.Rate
                    })
                    .ToDictionaryAsync(x => x.ItemId, x => x.Rate);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in GetItemRatesByItemIdsAsync");
                return new Dictionary<int, decimal>();
            }
        }
        #endregion

    }
}
