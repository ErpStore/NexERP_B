using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Office;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.IMaterialRequisitionService;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.InventoryViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using V.SMART.Shared.ViewModels.ProductionViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.InventoryService
{
    public class CostingService : ICostingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _logs;
        private readonly ICommonService _commonService;
   
        public readonly CurrentUserService _userService;
        private readonly IMapper _mapper;


        public CostingService(IUnitOfWork unitOfWork, ILoggingService logs,
            CurrentUserService userService, ICommonService commonService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logs = logs;
            _userService = userService;
         
            _commonService = commonService;
      
            _mapper = mapper;
        }
        public async Task<bool> IsAdditionalChargesEnabledAsync()
          => await _commonService.GetScreenPermissionsAsync("BOMLabourCost", "Add Additional Charges");
        public async Task<bool> IsLabourChargesDisplayEnabledAsync()
            => await _commonService.GetScreenPermissionsAsync("BOMLabourCost", "Labour Cost");

        public async Task<bool> IsLabourMaterialValidateEnabledAsync()
        => await _commonService.GetScreenPermissionsAsync("BOM", "Customer Material Validation");

        public async Task<List<AssemblyDefLabourVM>> GetAssemblyDefByAssemblyIdAsync(int assemblyId)
        {
            try
            {
                var entities = await _unitOfWork.AssmblyDefLabs.GetQueryable()
                    .Include(x => x.PartItem)
                    .ThenInclude(i => i.Category)
                    .Where(x => x.AssmblyID == assemblyId)
                    .OrderBy(x => x.SlNo)
                    .ToListAsync();

                var result = _mapper.Map<List<AssemblyDefLabourVM>>(entities);

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetAssemblyDefByAssemblyIdAsync({assemblyId}) failed.");
                return new List<AssemblyDefLabourVM>();
            }
        }
       

        public async Task<List<AssemblyChargeVM>> GetAssemblyChargesByAssemblyIdAsync(int assemblyId)
        {
            try
            {
                var entities = await _unitOfWork.AssemblyCharges.GetQueryable()
                    .Where(x => x.AssmblyID == assemblyId)
                    .OrderBy(x => x.AssemblyExtraChargeId)
                    .ToListAsync();
                
                // Map to ViewModel
                var vmList = entities.Select(x => new AssemblyChargeVM
                {
                    AssemblyExtraChargeId = x.AssemblyExtraChargeId,
                    AssmblyID = x.AssmblyID,
                    Name = x.Name,
                    IsAmount = x.IsAmount,
                    Amount = x.Amount ?? 0m,
                    Percent = x.Percent ?? 0m,
                    CalculatedAmount = x.CalculatedAmount ?? 0m,
                    IsLabour=x.IsLabour,
                    IsDefault=x.IsDefault,
                    
                }).ToList();

                return vmList;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetAssemblyDefByAssemblyIdAsync({assemblyId}) failed.");
                return new List<AssemblyChargeVM>();
            }
        }

        public async Task UpdateBOMOnlyAsync(AssemblyDefLabourVM parentAssyVM, List<AssemblyDefLabourVM> bomRows,  List<AssemblyChargeVM>? assemblyCharges)
        {
            if (bomRows == null || !bomRows.Any())
                throw new InvalidOperationException("No BOM rows to update.");

            if (parentAssyVM?.AssmblyID == null || parentAssyVM.AssmblyID == 0)
                throw new InvalidOperationException("Invalid Assembly ID.");

            var now = DateTime.Now;
            var currentUser = await _userService.GetUsernameAsync();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                int assemblyId = parentAssyVM.AssmblyID.Value;

               
                var existingBOMs = await _unitOfWork.AssmblyDefLabs
                    .GetQueryable()
                    .Where(x => x.AssmblyID == assemblyId)
                    .ToListAsync();

                var existingDict = existingBOMs
                    .Where(x => x.ItemId != 0)
                    .ToDictionary(x => x.ItemId, x => x);

               
                foreach (var row in bomRows)
                {
                    if (!row.ItemId.HasValue)
                        continue;

                    if (!existingDict.TryGetValue(row.ItemId.Value, out var entity))
                        continue; 

                    entity.UnitPrice = row.UnitPrice;
                    entity.MaterialCost = row.MaterialCost;
                    entity.LaborCost = row.LaborCost;
                    entity.TotalCost = row.TotalCost;
                    entity.UtilQty = row.UtilQty ?? 0;
                    entity.IsLabourMaterial = row.IsLabourMaterial;
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;
                }

              
                if (assemblyCharges != null)
                    await SyncAssemblyCharges(assemblyId, assemblyCharges);

               
                decimal bomTotal = bomRows.Sum(x => x.TotalCost??0);
                decimal chargesTotal = assemblyCharges?.Sum(c => c.CalculatedAmount) ?? 0;
                decimal grandTotal = bomTotal + chargesTotal;

                var itemMaster = await _unitOfWork.ItemRepositories
                    .GetQueryable()
                    .FirstOrDefaultAsync(i => i.ItemId == assemblyId);

                if (itemMaster != null)
                {
                    itemMaster.AssemblyPrice = Math.Round(grandTotal, 2);
                    itemMaster.ModifiedBy = currentUser;
                    itemMaster.ModifiedDate = now;
                }

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "Failed to update BOM.");
                throw;
            }
        }

        private async Task SyncAssemblyCharges( int assemblyId, List<AssemblyChargeVM> assemblyCharges)
        {
           
            var dbCharges = await _unitOfWork.AssemblyCharges
                .GetQueryable()
                .Where(x => x.AssmblyID == assemblyId)
                .ToListAsync();

            var existingDict = dbCharges
                .ToDictionary(x => x.AssemblyExtraChargeId);

           
            var incomingIds = assemblyCharges
                .Where(x => x.AssemblyExtraChargeId != 0)
                .Select(x => x.AssemblyExtraChargeId)
                .ToHashSet();

            var toRemove = dbCharges
                .Where(x => !incomingIds.Contains(x.AssemblyExtraChargeId))
                .ToList();

            foreach (var remove in toRemove)
                await _unitOfWork.AssemblyCharges.DeleteAsync(remove);

           
            foreach (var vm in assemblyCharges)
            {
                if (vm.AssemblyExtraChargeId != 0 &&
                    existingDict.TryGetValue(vm.AssemblyExtraChargeId, out var dbCharge))
                {
                    
                    dbCharge.Name = vm.Name;
                    dbCharge.IsAmount = vm.IsAmount;
                    dbCharge.Amount = vm.Amount ?? 0m;
                    dbCharge.Percent = vm.Percent ?? 0m;
                    dbCharge.CalculatedAmount = vm.CalculatedAmount ?? 0m;
                    dbCharge.IsLabour = vm.IsLabour;
                    dbCharge.IsDefault = vm.IsDefault;
                }
                else
                {
                  
                    var newCharge = new AssemblyCharge
                    {
                        AssmblyID = assemblyId,
                        Name = vm.Name,
                        IsAmount = vm.IsAmount,
                        Amount = vm.Amount ?? 0m,
                        Percent = vm.Percent ?? 0m,
                        CalculatedAmount = vm.CalculatedAmount ?? 0m,
                        IsLabour=vm.IsLabour,
                        IsDefault=vm.IsDefault
                        
                    };

                    await _unitOfWork.AssemblyCharges.CreateAsync(newCharge);
                }
            }
        }
        public async Task<List<AssemblyDefLabourVM>> GetAssemblyModifyTrackByAssyId(int assyId)
        {
            if (assyId <= 0)
                return new List<AssemblyDefLabourVM>();

            try
            {
                var result = await _unitOfWork.AssmblyDefLabs.GetQueryable()
                    .Where(am => am.AssmblyID == assyId)
                    .Join(_unitOfWork.ItemRepositories.GetQueryable(),
                          am => am.ItemId,
                          i => i.ItemId,
                          (am, i) => new { am, i })
                    .GroupJoin(_unitOfWork.Processes.GetQueryable(),
                               temp => temp.am.ProcessId,
                               p => p.ProcessId,
                               (temp, proc) => new { temp.am, temp.i, proc })
                    .SelectMany(x => x.proc.DefaultIfEmpty(),
                                (x, p) => new { x.am, x.i, Process = p })
                    .OrderBy(x => x.am.SlNo) // order by DB field
                    .Select(x => new AssemblyDefLabourVM
                    {
                        SlNo = x.am.SlNo,
                        PartItemCode = x.i.ItemCode,
                        PartItemName = x.i.ItemName,
                        UOM = x.i.MeasureUnit,
                        UtilQty = x.am.UtilQty,
                        UnitPrice = x.am.UnitPrice,
                        MaterialCost = x.am.MaterialCost,
                        LaborCost = x.am.LaborCost,
                        TotalCost = x.am.TotalCost,
                        AssemblyCost=x.i.AssemblyPrice,
                        Remarks = x.am.Remarks,
                        CreatedBy = x.am.CreatedBy,
                        CreatedDate = x.am.CreatedDate
                    })
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetAssemblyModifyTrackByAssyId failed for AssyId {assyId}");
                return new List<AssemblyDefLabourVM>(); 
            }
        }
        public async Task<Dictionary<int, decimal>> GetBulkLabourCostsAsync(List<int> itemIds)
        {
            try
            {
                if (itemIds == null || !itemIds.Any())
                    return new Dictionary<int, decimal>();

                var distinctIds = itemIds.Distinct().ToList();

                var data = await _unitOfWork.ItemRepositories
                    .GetQueryable()
                    .Where(i => distinctIds.Contains(i.ItemId))
                    .Select(i => new
                    {
                        i.ItemId,
                        LabourCost= i.AssemblyPrice 
                    })
                    .ToListAsync();

                return data.ToDictionary(x => x.ItemId, x => x.LabourCost);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,
                    $"Error fetching bulk last unit prices for ItemIds: {string.Join(",", itemIds)}");

                throw new InvalidOperationException(
                    "Failed to fetch last unit prices. Please try again.");
            }
        }

       

    }
}
