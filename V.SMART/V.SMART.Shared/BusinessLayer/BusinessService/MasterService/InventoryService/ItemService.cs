


using AutoMapper;
using AutoMapper.QueryableExtensions;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IInventoryService;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.Data.Master.MasterScreeenManagement;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace V.SMART.Shared.BusinessLayer.BusinessService.MasterService.InventoryService
{
    public class ItemService : IItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILoggingService _logs;
        private readonly CurrentUserService _userService;
        private readonly ICommonService _commonService;
        private readonly ForeignKeyUsageChecker _fkChecker;
        private readonly IFileUploadService _fileuploadservice;

        public ItemService(IUnitOfWork unitOfWork, IMapper mapper, ILoggingService loggingService,
                            CurrentUserService userService, ICommonService commonService,
                            ForeignKeyUsageChecker foreignKeyUsageChecker,
                            IFileUploadService fileuploadservice)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logs = loggingService;
            _userService = userService;
            _commonService = commonService;
            _fkChecker = foreignKeyUsageChecker;
            _fileuploadservice = fileuploadservice;
        }

        public async Task<ItemVM?> GetItemByItemIdAsync(int? itemId)
            => await _commonService.GetItemByItemIdAsync(itemId);
        public async Task<List<Category>> GetAllCategoriesAsync()
            => (await _commonService.GetAllActiveCategoriesAsync()).ToList();

        public async Task<List<UOM>> GetAllUOMAsync()
            => await _commonService.GetUOMsAsync();

        public async Task<List<HSNMaster>> GetAllHSNAsync()
            => await _commonService.GetAllHSNAsynce();

        public async Task<List<RawMaterial>> GetAllRMAsync()
            => await _commonService.GetRMAsync();

        public async Task<List<VendorVM>> GetAllActiveVendorsAsync()
            => await _commonService.GetAllActiveVendorsAsync();

        public async Task<List<Process>> GetAllProcessAsync()
            => (await _commonService.GetAllProcessAsync()).ToList();

        public async Task<IEnumerable<Process>> GetAllProcessDetailsAsync()
          => await _commonService.GetAllProcessDetailsAsync();

        public async Task<List<Machine>> GetAllMachineAsync()
            => await _commonService.GetAllMachineAsync();
        public async Task<List<CustomerVM>> GetCustomersAsync()
            => await _commonService.GetAllActiveCustomersAsync();

        public async Task<IEnumerable<ItemVM>> SearchRawMaterialItemsAsync(string searchText)
            => await _commonService.SearchRawMaterialItemsAsync(searchText);

        public async Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText)
        {
            return await _commonService.SearchCustomersAsync(searchText);
        }
        public async Task<IEnumerable<VendorVM>> SearchVenorsAsync(string searchText)
        {
            return await _commonService.SearchVendorsAsync(searchText);
        }

        public Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
           => _commonService.GetCorrespondenceAttachmentsCountAsync(refId, refType);

        public async Task<bool> DeleteItemProductAssign(ItemProductAssign assign)
        {
            try
            {
                await _unitOfWork.ItemProductAssigns.DeleteAsync(assign);
                await _unitOfWork.SaveAsync();

                await _logs.LogUserAction(
                       UserName: await _userService.GetUsernameAsync(),
                       Machine: _userService.MachineName,
                       IP_Address: _userService.IpAddress,
                       screen: "Item Substitution",
                       action: "Delete ItemSubstitution",
                       additionalInfo: $"Deleted Item Substitution Item Code {assign.Item.ItemCode}");

                return true;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in DeleteItemProductAssign");
                return false;
            }
        }

        public async Task<bool> DeleteItemRateAssign(ItemSub sub)
        {
            try
            {
                await _unitOfWork.ItemSubs.DeleteAsync(sub);
                await _unitOfWork.SaveAsync();

                await _logs.LogUserAction(
                       UserName: await _userService.GetUsernameAsync(),
                       Machine: _userService.MachineName,
                       IP_Address: _userService.IpAddress,
                       screen: "Item Rate Assign",
                       action: "Delete Rate Assign",
                       additionalInfo: $"Deleted Item Rate Assign Item Code {sub.Item.ItemCode}");

                return true;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in DeleteItemRateAssign");
                return false;
            }
        }


        public async Task<List<ItemSub>> LoadItemSubAsync(int? itemId)
        {
            try
            {
                if (itemId.HasValue && itemId > 0)
                {
                    var records = await _unitOfWork.ItemSubs.GetQueryable()
                        .Include(x =>x.Customer)
                        .Include(x =>x.Vendor)
                        .Where(x => x.ItemId == itemId).ToListAsync();

                    return records?.ToList() ?? new List<ItemSub>();
                }

                return new List<ItemSub>();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error while loading Item Sub details.");
                throw; 
            }
        }

        public async Task<List<ItemProductAssign>> LoadItemProductAssignAsync(int? itemId)
        {
            try
            {
                if (itemId.HasValue && itemId > 0)
                {
                    var list = await _unitOfWork.ItemProductAssigns.GetQueryable()
                        .Where(x => x.ItemId == itemId).ToListAsync();

                    return list ?? new List<ItemProductAssign> ();
                }

                return new List<ItemProductAssign> ();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error while loading Item Product Assign details.");
                throw; // Do not swallow — let UI decide the notification
            }
        }

        public async Task<ItemVM> GenerateNewRevisionAsync(int itemId)
        {
            if (itemId == 0)
                throw new Exception("Item must be saved before creating a revision.");

            var existingItem = await _unitOfWork.ItemRepositories
                .FirstOrDefaultAsync(x => x.ItemId == itemId);

            if (existingItem == null)
                throw new Exception("Existing item not found.");

            existingItem.Hide = true;

            string baseItemCode = existingItem.ItemCode?.Trim() ?? "";

            baseItemCode = Regex.Replace(baseItemCode, @"\s*R\w+$", "", RegexOptions.IgnoreCase);

            string prefix = "R";
            int nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(existingItem.Rev))
            {
                var match = Regex.Match(existingItem.Rev, @"^([A-Za-z]+)(\d+)$");
                if (match.Success)
                {
                    prefix = match.Groups[1].Value;
                    int.TryParse(match.Groups[2].Value, out int currentNumber);
                    nextNumber = currentNumber + 1;
                }
                else
                {
                    prefix = existingItem.Rev;
                    nextNumber = 1;
                }
            }

            string newRev = $"{prefix}{nextNumber}";

            var newItem = new Item
            {
                ItemCode = $"{baseItemCode} {newRev}",
                ItemName = existingItem.ItemName,
                CategoryCode = existingItem.CategoryCode,
                BtOtorMfg = existingItem.BtOtorMfg,
                Specification = existingItem.Specification,
                DrawingNo = existingItem.DrawingNo,
                ImagePath = existingItem.ImagePath,
                Remarks = existingItem.Remarks,
                Hide = false,
                MeasureUnit = existingItem.MeasureUnit,
                PurchaseUnit = existingItem.PurchaseUnit,
                UnitConvert = existingItem.UnitConvert,
                RackNo = existingItem.RackNo,
                ROL = existingItem.ROL,
                MOL = existingItem.MOL,
                leadTime = existingItem.leadTime,
                PartWeight = existingItem.PartWeight,
                BinLocation = existingItem.BinLocation,
                ConsiderAsRm = existingItem.ConsiderAsRm,
                Make = existingItem.Make,
                RmId = existingItem.RmId,
                Shape = existingItem.Shape,
                Density = existingItem.Density,
                Length = existingItem.Length,
                Width = existingItem.Width,
                Height = existingItem.Height,
                Thickness = existingItem.Thickness,
                WallThickness = existingItem.WallThickness,
                OuterDia = existingItem.OuterDia,
                InnerDia = existingItem.InnerDia,
                Weight = existingItem.Weight,
                Rate = existingItem.Rate,
                AltRate = existingItem.AltRate,
                HSNCode = existingItem.HSNCode,
                SACCode = existingItem.SACCode,
                IsServcS = existingItem.IsServcS,
                IsServcL = existingItem.IsServcL,
                CGST = existingItem.CGST,
                SGST = existingItem.SGST,
                IGST = existingItem.IGST,
                RevItemId = existingItem.ItemId,
                Rev = newRev
            };

            return _mapper.Map<ItemVM>(newItem);
        }

        public async Task<ItemVM> CopyItemAsync(int itemId)
        {
            if (itemId == 0)
                throw new Exception("Please save the item before copying.");

            var existingItem = await _unitOfWork.ItemRepositories
                .FirstOrDefaultAsync(x => x.ItemId == itemId);

            if (existingItem == null)
                throw new Exception("Item not found.");

            string prefix = "copy";
            int nextNumber = 1;
            string baseCode = existingItem.ItemCode;

            if (!string.IsNullOrWhiteSpace(baseCode))
            {
                baseCode = Regex.Replace(
                    baseCode,
                    @"\s*copy(?:\[(\d+)\])?$",
                    "",
                    RegexOptions.IgnoreCase
                );

                var match = Regex.Match(existingItem.ItemCode, @"copy\[(\d+)\]$", RegexOptions.IgnoreCase);

                if (match.Success && int.TryParse(match.Groups[1].Value, out int currentNumber))
                {
                    nextNumber = currentNumber + 1;
                }
                else if (existingItem.ItemCode.EndsWith("copy", StringComparison.OrdinalIgnoreCase))
                {
                    nextNumber = 2;
                }
            }

            string newItemCode = $"{baseCode} {prefix}[{nextNumber}]";

            var newItem = new Item
            {
                ItemCode = newItemCode,
                ItemName = existingItem.ItemName,
                CategoryCode = existingItem.CategoryCode,
                BtOtorMfg = existingItem.BtOtorMfg,
                Specification = existingItem.Specification,
                DrawingNo = existingItem.DrawingNo,
                ImagePath = existingItem.ImagePath,
                Remarks = existingItem.Remarks,
                Hide = false,
                MeasureUnit = existingItem.MeasureUnit,
                PurchaseUnit = existingItem.PurchaseUnit,
                UnitConvert = existingItem.UnitConvert,
                RackNo = existingItem.RackNo,
                ROL = existingItem.ROL,
                MOL = existingItem.MOL,
                leadTime = existingItem.leadTime,
                PartWeight = existingItem.PartWeight,
                BinLocation = existingItem.BinLocation,
                ConsiderAsRm = existingItem.ConsiderAsRm,
                Make = existingItem.Make,
                RmId = existingItem.RmId,
                Shape = existingItem.Shape,
                Density = existingItem.Density,
                Length = existingItem.Length,
                Width = existingItem.Width,
                Height = existingItem.Height,
                Thickness = existingItem.Thickness,
                WallThickness = existingItem.WallThickness,
                OuterDia = existingItem.OuterDia,
                InnerDia = existingItem.InnerDia,
                Weight = existingItem.Weight,
                Rate = existingItem.Rate,
                AltRate = existingItem.AltRate,
                HSNCode = existingItem.HSNCode,
                SACCode = existingItem.SACCode,
                IsServcS = existingItem.IsServcS,
                IsServcL = existingItem.IsServcL,
                CGST = existingItem.CGST,
                SGST = existingItem.SGST,
                IGST = existingItem.IGST
            };

            // Return mapped VM (not saved)
            return _mapper.Map<ItemVM>(newItem);
        }

        public async Task<List<CompMasterVM>> GetCompMastersByItemIdAsync(int itemId)
        {
            try
            {
                var data = await _unitOfWork.CompMasters
                    .GetQueryable()
                    .Where(x => x.CompItemId == itemId)
                    .Include(x => x.ComponentItem)
                    .Include(x => x.RawMaterial)
                    .ToListAsync();

                return _mapper.Map<List<CompMasterVM>>(data);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to get CompMasters for ItemId: {itemId}");
                throw;
            }
        }

        public async Task<int?> GetComIdbyRMIdAsync(int rmId, int CompItemId)
        {
            try
            {
                return await _unitOfWork.CompMasters
                    .GetQueryable()
                    .Where(x => x.RMId == rmId && x.CompItemId == CompItemId)
                    .Select(x => (int?)x.Id)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetComIdbyRMIdAsync for RMId {rmId}");
                throw;
            }
        }



        public async Task<int> GetCompMasterCountAsync(int itemId)
        {
            try
            {
                return await _unitOfWork.CompMasters
                    .GetQueryable()
                    .CountAsync(x => x.CompItemId == itemId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetCompMasterCountAsync for ItemId {itemId}");
                throw;
            }
        }

        public async Task<List<CompMasterVM>> GetPrevProcessFlowRMAsyncByCompItemId(int compItemId)
        {
            try
            {
                var data = await _unitOfWork.CompMasters.GetQueryable()
                    .Include(c => c.RawMaterial)
                    .Where(c => c.CompItemId == compItemId)
                    .Select(c => new CompMasterVM
                    {
                        RMId = c.RawMaterial.ItemId,
                        RMCode = c.RawMaterial.ItemCode
                    })
                    .Distinct()
                    .ToListAsync();

                return data;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error loading Raw Materials for CompItemId={compItemId}");
                return new List<CompMasterVM>();
            }
        }

        public async Task<List<ProcessFlowChartVM>> DeleteAndRecalculateProcessFlowAsync(int processFlowId, int compMasterId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 1. Find row to delete
                var row = await _unitOfWork.ProcessFlowCharts.GetAsync(processFlowId);
                if (row == null)
                {
                    await transaction.RollbackAsync();
                    return null;
                }

                // 2. Delete that row
                await _unitOfWork.ProcessFlowCharts.DeleteAsync(row.SubId);
                await _unitOfWork.SaveAsync();

                // 3. Load all flow rows again
                var processFlows = await _unitOfWork.ProcessFlowCharts.GetQueryable()
                    .Where(p => p.Id == compMasterId)
                    .OrderBy(p => p.SlNo)
                    .ToListAsync();

                if (!processFlows.Any())
                {
                    await transaction.CommitAsync();
                    return new List<ProcessFlowChartVM>();
                }

                // 4. Reset SLNO
                int slNo = 1;
                foreach (var item in processFlows)
                    item.SlNo = slNo++;

                // 5. REAL SubProcess recalculation (Excel logic)
                string lastGroupKey = null;
                int subProcCounter = 0;

                foreach (var item in processFlows)
                {
                    // NEW GROUP → increment SubProcess
                    if (item.GroupKey != lastGroupKey)
                    {
                        subProcCounter++;
                        item.UseSameSubProcess = false;
                    }
                    else
                    {
                        // SAME GROUP → same SubProcess
                        item.UseSameSubProcess = true;
                    }

                    item.SubProcess = subProcCounter;
                    lastGroupKey = item.GroupKey;
                }

                // 6. Update DB
                foreach (var item in processFlows)
                    await _unitOfWork.ProcessFlowCharts.UpdateAsync(item);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                return _mapper.Map<List<ProcessFlowChartVM>>(processFlows);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "Error in DeleteAndRecalculateProcessFlowAsync()");
                return null;
            }
        }

        public async Task<bool> CheckItemExistsAsync(string itemCode)
        {
            try
            {
                return await _unitOfWork.ItemRepositories
                    .GetQueryable()
                    .AnyAsync(x => x.ItemCode.ToLower() == itemCode.ToLower());
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to check Item-Code exists: {itemCode}");
                throw; // rethrow for upper layers
            }
        }
        public async Task<ScreenManagement?> GetWeightConfigAsync()
        {
            try
            {
                return await _unitOfWork.ScreenManagements
                    .GetQueryable()
                    .Where(x => x.ScreenName == "Item" && x.Display == "Material Weight")
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Failed to load Weight Config for Item Screen");
                throw;
            }
        }

        public async Task<CompMasterVM> GetCompMasterByIdAsync(int id)
        {
            var entity = await _unitOfWork.CompMasters.GetQueryable()
                .Include(q => q.ProcessFlowCharts).ThenInclude(s => s.Process)
                .Include(q => q.RawMaterial)
                .Include(q => q.ProcessFlowCharts).ThenInclude(s => s.Machine)
                .Include(q => q.ProcessFlowCharts).ThenInclude(s => s.Vendor)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (entity == null) return new CompMasterVM();

            return _mapper.Map<CompMasterVM>(entity);
        }


        public async Task<bool> DeleteCompMasterWithProcessFlowAsync(int compMasterId)
        {
            try
            {
                if (compMasterId <= 0)
                    return false;

                var processFlows = await _unitOfWork.ProcessFlowCharts.GetQueryable()
                                    .Where(f => f.Id == compMasterId)
                                    .ToListAsync();

                foreach (var flow in processFlows)
                {
                    await _unitOfWork.ProcessFlowCharts.DeleteAsync(flow.Id);
                }

                await _unitOfWork.CompMasters.DeleteAsync(compMasterId);
                await _unitOfWork.SaveAsync();

                return true;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"DeleteCompMasterWithProcessFlowAsync failed for Id: {compMasterId}");
                return false;
            }
        }

        public async Task<ItemProductAssign> UpsertItemSubstitution(ItemProductAssign assign)
        {
            if (assign == null)
                throw new ArgumentNullException(nameof(assign));

            var now = DateTime.Now;
            var currentUser = await _userService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {

                if (assign.Id == 0)
                {
                    assign.CreatedBy = currentUser;
                    assign.CreatedDate = now;

                    await _unitOfWork.ItemProductAssigns.CreateAsync(assign);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine("Item Substituion Created.");
                }
                else
                {
                    var oldentity = await _unitOfWork.ItemProductAssigns.GetQueryable()
                        .FirstOrDefaultAsync(q => q.Id == assign.Id)
                        ?? throw new InvalidOperationException("Item Substitution not found.");

                    var parentChanges = GetPropertyChanges(oldentity, assign);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    assign.ModifiedBy = currentUser;
                    assign.ModifiedDate = now;

                    await _unitOfWork.ItemProductAssigns.UpdateAsync(assign);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine("Item Substituion Updated.");
                }

                await _unitOfWork.SaveAsync();


                await transaction.CommitAsync();

                await LogChangesAsync(changes, assign.Id == 0 ? "Item Substitution Created" : "Item Substitution Updated");

                var savedEntity = await _unitOfWork.ItemProductAssigns.GetQueryable()
                    .FirstOrDefaultAsync(q => q.Id == assign.Id);

                return savedEntity;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert Item Substitution: {assign.Item.ItemCode}");
                throw new InvalidOperationException("Failed to save Item Substitution. Please try again.");
            }
        }
        public async Task<ItemSub> UpsertItemRateAssign(ItemSub sub)
        {
            if (sub == null)
                throw new ArgumentNullException(nameof(sub));

            var now = DateTime.Now;
            var currentUser = await _userService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {

                if (sub.Id == 0)
                {
                    sub.CreatedBy = currentUser;
                    sub.CreatedDate = now;

                    await _unitOfWork.ItemSubs.CreateAsync(sub);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine("Item Rate Assign Created.");
                }
                else
                {
                    var oldentity = await _unitOfWork.ItemSubs.GetQueryable()
                        .FirstOrDefaultAsync(q => q.Id == sub.Id)
                        ?? throw new InvalidOperationException("Item Rate Assign not found.");

                    var parentChanges = GetPropertyChanges(oldentity, sub);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    sub.ModifiedBy = currentUser;
                    sub.ModifiedDate = now;

                    await _unitOfWork.ItemSubs.UpdateAsync(sub);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine("Item Rate Assign Updated.");
                }

                await _unitOfWork.SaveAsync();


                await transaction.CommitAsync();

                await LogChangesAsync(changes, sub.Id == 0 ? "Item Rate Assign Created" : "Item Rate Assign Updated");

                var savedEntity = await _unitOfWork.ItemSubs.GetQueryable()
                    .FirstOrDefaultAsync(q => q.Id == sub.Id);

                return savedEntity;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert Item Rate Assign: {sub.Item.ItemCode}");
                throw new InvalidOperationException("Failed to save Item Rate Assign. Please try again.");
            }
        }

        public async Task<ItemVM> UpsertItem(ItemVM vm)
        {
            if (vm == null)
                throw new ArgumentNullException(nameof(vm));

            var now = DateTime.Now;
            var currentUser = await _userService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                Item entity;

                // ===============================
                // CREATE
                // ===============================
                if (vm.ItemId == 0)
                {
                    entity = _mapper.Map<Item>(vm);

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;

                    await _unitOfWork.ItemRepositories.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine("Item Created.");
                }
                // ===============================
                // UPDATE
                // ===============================
                else
                {
                    entity = await _unitOfWork.ItemRepositories
                        .GetQueryable()
                        .FirstOrDefaultAsync(x => x.ItemId == vm.ItemId)
                        ?? throw new InvalidOperationException("Item not found.");

                    _mapper.Map(vm, entity);

                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await _unitOfWork.ItemRepositories.UpdateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine("Item Updated.");
                }

                // =====================================================
                // HANDLE ITEM CUSTOMER ASSIGN (CLEAR & REINSERT)
                // =====================================================

                // Remove existing assignments
                var existingAssignments = await _unitOfWork
                    .ItemCustomerAssigns
                    .GetQueryable()
                    .Where(x => x.ItemId == entity.ItemId)
                    .ToListAsync();

                foreach (var assign in existingAssignments)
                {
                    await _unitOfWork.ItemCustomerAssigns.DeleteAsync(assign);
                    await _unitOfWork.SaveAsync();
                }

                // Insert selected customers
                if (vm.SelectedCustomers != null && vm.SelectedCustomers.Any())
                {
                    foreach (var customer in vm.SelectedCustomers)
                    {
                        await _unitOfWork
                            .ItemCustomerAssigns
                            .CreateAsync(new ItemCustomerAssign
                            {
                                ItemId = entity.ItemId,
                                CustomerId = customer.CustId
                            });
                    }
                }

                await _unitOfWork.SaveAsync();

                // ===============================
                // COMMIT TRANSACTION
                // ===============================
                await transaction.CommitAsync();

                await LogChangesAsync(changes,vm.ItemId == 0 ? "Item Created" : "Item Updated");

                // ===============================
                // RETURN UPDATED DATA
                // ===============================
                var savedEntity = await _unitOfWork.ItemRepositories
                    .GetQueryable()
                    .Include(x => x.ItemCustomerAssigns)
                        .ThenInclude(x => x.Customer)
                    .FirstOrDefaultAsync(x => x.ItemId == entity.ItemId);

                return _mapper.Map<ItemVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert Item: {vm.ItemCode}");
                throw new InvalidOperationException("Failed to save Item. Please try again.");
            }
        }

        public async Task EnsureSingleDefaultRMAsync(int compItemId)
        {
            try
            {
                var existing = await _unitOfWork.CompMasters
                    .GetQueryable()
                    .Where(c => c.CompItemId == compItemId
                                && c.IsDefaultRM)
                    .ToListAsync();

                if(existing.Any())
                {
                    foreach(var r in existing)
                    {
                        r.IsDefaultRM = false;
                        await _unitOfWork.CompMasters.UpdateAsync(r);
                        await _unitOfWork.SaveAsync();
                    }
                }

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,
                    $"Error in EnsureSingleDefaultRMAsync for CompItemId {compItemId}");
                throw;
            }
        }

        public async Task<(bool Success, List<Factor> Factors)> ToggleGroupingAsync(bool enable)
        {
            try
            {
                if (enable)
                {
                    var factors = await _unitOfWork.Factors.GetAllAsync();
                    await _logs.LogDeveloperInfo("Grouping enabled and factors loaded.", "ToggleGroupingAsync");

                    return (true, factors.ToList());
                }
                else
                {
                    await _logs.LogDeveloperInfo("Grouping disabled and factors cleared.", "ToggleGroupingAsync");
                    return (true, new List<Factor>());
                }
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in ToggleGroupingAsync");
                return (false, new List<Factor>());
            }
        }


        public async Task<List<Grouping>> GetFrcNoByFactorIdAsync(int factorId)
        {
            try
            {
                var result = await _unitOfWork.Groupings.GetQueryable()
                    .Join(
                        _unitOfWork.GroupingSubs.GetQueryable(),
                        g => g.ID,
                        gs => gs.GroupingId,
                        (g, gs) => new { g, gs }
                    )
                    .Where(x => x.gs.Factors == factorId)
                    .Select(x => new Grouping
                    {
                        ID = x.g.ID,
                        Prefix = x.g.Prefix,
                        FRCNo = x.g.FRCNo
                    })
                    .Distinct()
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error loading FRC No for FactorId: {factorId}");
                return new List<Grouping>();
            }
        }

        public async Task<List<ProcessFlowChartVM>> GetAllProcessByGroupingId(int groupingId,int compItemId,int rmId,decimal weight)
        {
            try
            {
                var data = await _unitOfWork.GroupingSubs.GetQueryable()
                    .Where(gs => gs.GroupingId == groupingId)
                    .Select(gs => new
                    {
                        gs.Process.ProcessId,
                        gs.Process.ProcessName
                    })
                    .ToListAsync();

                int slNo = 1;
                int subProcess = 1;

                var result = data.Select(d => new ProcessFlowChartVM
                {
                    SlNo = slNo++,
                    CompItemId = compItemId,
                    RMId = rmId,
                    ProcId = d.ProcessId,
                    ProcessName = d.ProcessName,
                    CompWt = weight,
                    RmWt = weight,

                    GroupKey = Guid.NewGuid().ToString(),
                    SubProcess = subProcess++,

                    IsEditable = true

                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error loading processes for GroupingId: {groupingId}");
                return new List<ProcessFlowChartVM>();
            }
        }

        public async Task<List<ProcessFlowChartVM>> GetProcessFlowChartByCompMasterIdAndRMIdAsync(int compItemId,int rmId)
        {
            try
            {
                var data = await _unitOfWork.ProcessFlowCharts.GetQueryable()
                    .Where(p => p.CompItemId == compItemId && p.RMId == rmId)
                    .OrderBy(p => p.SlNo)
                    .ToListAsync();

                return _mapper.Map<List<ProcessFlowChartVM>>(data);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,
                    $"Error fetching ProcessFlowChart for CompItemId={compItemId}, RMId={rmId}");
                return new List<ProcessFlowChartVM>();
            }
        }



        public async Task<CompMasterVM> UpsertProcessFlow(CompMasterVM compVM)
        {
            if (compVM == null)
                throw new ArgumentNullException(nameof(compVM));

            var now = DateTime.Now;
            var currentUser = await _userService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                CompMaster entity;

                if (compVM.Id == 0)
                {
                    entity = _mapper.Map<CompMaster>(compVM);

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;

                    entity.ProcessFlowCharts = compVM.ProcessFlowCharts.Select(s => _mapper.Map<ProcessFlowChart>(s)).ToList();

                    await _unitOfWork.CompMasters.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine("Raw-Material and Process Allocated to the Component.");
                }
                else
                {
                    entity = await _unitOfWork.CompMasters.GetQueryable()
                        .Include(q => q.ProcessFlowCharts)
                        .FirstOrDefaultAsync(q => q.Id == compVM.Id)
                        ?? throw new InvalidOperationException("Comp Master not found.");

                    var parentChanges = GetPropertyChanges(entity, compVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(compVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await HandleChildUpdatesAsync(entity, compVM.ProcessFlowCharts, changes);

                    changes.AppendLine("Raw-Material and Process Allocated to the Component Updated.");
                }

                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();

                await LogChangesAsync(changes, compVM.Id == 0 ? "Raw-Material and Process Flow Created" : "Raw-Material and Process Flow Updated");

                var savedEntity = await _unitOfWork.CompMasters.GetQueryable()
                    .Include(q => q.ProcessFlowCharts).ThenInclude(s => s.Process)
                    .Include(q => q.RawMaterial)
                    .Include(q => q.ProcessFlowCharts).ThenInclude(s => s.Machine)
                    .FirstOrDefaultAsync(q => q.Id == entity.Id);

                return _mapper.Map<CompMasterVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert Comp Master: {compVM.RMCode}");
                throw new InvalidOperationException("Failed to save Comp master. Please try again.");
            }
        }

        private async Task HandleChildUpdatesAsync(CompMaster existingComponent, List<ProcessFlowChartVM> incomingSubVMs, StringBuilder changes)
        {
            var existingSubIds = existingComponent.ProcessFlowCharts.Select(s => s.SubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.SubId).ToHashSet();

            // DELETE removed children
            foreach (var sub in existingComponent.ProcessFlowCharts.Where(s => !incomingSubIds.Contains(s.SubId)).ToList())
            {
                changes.AppendLine($"Child Deleted - SubId: {sub.SubId}, Process: {sub.Process?.ProcessName}");
                await _unitOfWork.ProcessFlowCharts.DeleteAsync(sub.SubId);
                await _unitOfWork.SaveAsync();
            }

            // ADD or UPDATE children
            foreach (var subVM in incomingSubVMs)
            {
                if (subVM.SubId == 0)
                {
                    var newSub = _mapper.Map<ProcessFlowChart>(subVM);
                    newSub.Id = subVM.Id;
                    await _unitOfWork.ProcessFlowCharts.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Child Added - Process: {subVM.ProcessName}, Unit Price: {subVM.UnitPrice}");

                }
                else
                {
                    var existingSub = existingComponent.ProcessFlowCharts.FirstOrDefault(s => s.SubId == subVM.SubId);
                    if (existingSub != null)
                    {
                        var subChanges = GetPropertyChanges(existingSub, subVM);
                        if (!string.IsNullOrEmpty(subChanges))
                            changes.AppendLine($"Child Updated - Process {subVM.ProcessName}:\n{subChanges}");

                        _mapper.Map(subVM, existingSub);

                        await _unitOfWork.ProcessFlowCharts.UpdateAsync(existingSub);
                        await _unitOfWork.SaveAsync();
                    }
                }
            }
        }

        // Get property changes for logging
        private string GetPropertyChanges<TSource, TTarget>(TSource entity, TTarget vm)
        {
            var sb = new StringBuilder();
            foreach (var prop in typeof(TSource).GetProperties())
            {
                var vmProp = typeof(TTarget).GetProperty(prop.Name);
                if (vmProp == null) continue;

                var oldVal = prop.GetValue(entity)?.ToString() ?? "null";
                var newVal = vmProp.GetValue(vm)?.ToString() ?? "null";

                if (oldVal != newVal)
                    sb.AppendLine($"{prop.Name}: '{oldVal}' → '{newVal}'");
            }
            return sb.ToString();
        }

        // Logging
        private async Task LogChangesAsync(StringBuilder changes, string action)
        {
            if (changes.Length == 0) return;

            await _logs.LogUserAction(
                UserName: await _userService.GetUsernameAsync(),
                Machine: _userService.MachineName,
                IP_Address: _userService.IpAddress,
                screen: "Item",
                action: action,
                additionalInfo: changes.ToString()
            );
        }

        public async Task<(List<ItemVM> Items, int TotalCount)>SearchWithDynamicFilterAsync(
            int pageNumber,
            int pageSize,
            Dictionary<string, object>? filters)
                {
                    var query = _unitOfWork.ItemRepositories
                        .GetQueryable()
                        .AsNoTracking();

                    if (filters != null)
                    {
                        foreach (var filter in filters)
                        {
                            query = DynamicWhereBuilder.ApplyFilter(
                                query,
                                filter.Key,
                                filter.Value
                            );
                        }
                    }

                    var totalCount = await query.CountAsync();

                    var items = await query
                        .OrderByDescending(x => x.ItemId)
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ProjectTo<ItemVM>(_mapper.ConfigurationProvider)
                        .ToListAsync();

                    return (items, totalCount);
        }

        public static class DynamicWhereBuilder
        {
            public static IQueryable<Item> ApplyFilter(
                IQueryable<Item> query,
                string field,
                object value)
            {
                if (value == null) return query;

                var val = value.ToString()?.Trim();
                if (string.IsNullOrEmpty(val)) return query;

                switch (field)
                {
                    case "ItemCode":
                        return query.Where(x =>
                            EF.Functions.Like(x.ItemCode, $"%{val}%" ));

                    case "ItemName":
                        return query.Where(x =>
                            EF.Functions.Like(x.ItemName, $"%{val}%"));

                    case "CategoryName":
                        return query.Where(x =>
                            EF.Functions.Like(x.Category.CategoryName, $"%{val}%"));

                    case "ActiveStatus":
                        return ApplyStatusFilter(query, val);

                    case "CreatedBy":
                        return query.Where(x =>
                            EF.Functions.Like(x.CreatedBy, $"%{val}%"));

                    case "FromDate":
                        if (DateTime.TryParse(val, out var from))
                            return query.Where(x => x.CreatedDate >= from);
                        break;

                    case "ToDate":
                        if (DateTime.TryParse(val, out var to))
                            return query.Where(x => x.CreatedDate <= to);
                        break;

                    case "AssignedCustomer":
                        return query.Where(x =>
                            x.ItemCustomerAssigns.Any(ica =>
                                EF.Functions.Like(ica.Customer.CustName, $"%{val}%")));
                }

                return query;
            }

            private static IQueryable<Item> ApplyStatusFilter(
                IQueryable<Item> query, string status)
            {
                return status switch
                {
                    "Active" => query.Where(x => !x.Hide),
                    "InActive" => query.Where(x => x.Hide),
                    _ => query
                };
            }
        }



        public async Task<ItemVM?> GetItemByItemIdAsync(int itemId)
        {
            try
            {
                var entity = await _unitOfWork.ItemRepositories
                    .GetQueryable()
                    .Where(x => x.ItemId == itemId)
                    .Include(x => x.Category)
                    .Include(x => x.ItemCustomerAssigns)
                        .ThenInclude(x => x.Customer)
                    .FirstOrDefaultAsync();

                if (entity == null)
                    return null;

                var vm = _mapper.Map<ItemVM>(entity);

                vm.SelectedCustomers = entity.ItemCustomerAssigns
                    .Where(x => x.Customer != null)
                    .Select(x => _mapper.Map<CustomerVM>(x.Customer))
                    .ToList();

                return vm;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to get Item by ItemId {itemId}");
                throw;
            }
        }

        public async Task<CustomerVM?> GetCustomerByCustIdAsync(int custId)
        {
            try
            {
                return await _commonService.GetCustomerByIdAsync(custId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to get Customer by CustId {custId}");
                throw;
            }
        }

        public async Task<VendorVM?> GetVendorByVendorIdAsync(int vendorCode)
        {
            try
            {
                return await _commonService.GetVendorByVenerCodeAsync(vendorCode);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to get Vendor by VendorCode {vendorCode}");
                throw;
            }
        }


        public async Task<(bool CanDelete, string Message)> CanDeleteItemAsync(int itemId)
        {
            try
            {
                var item = await _unitOfWork.ItemRepositories
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.ItemId == itemId);

                if (item == null)
                    return (false, "Item not found or already removed.");

                if(item.Hide)
                    return (false, $"Cannot delete Item Code '{item.ItemCode}' because it is already Hidden.");

                var usedIn = await _fkChecker.GetUsageTableAsync<Item>(itemId);

                if (usedIn != null)
                    return (false, $"Cannot delete Item Code '{item.ItemCode}' because it is used in {usedIn} Screen.");

                return (true, $"Item '{item.ItemId}' can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Item delete validation failed: {itemId}");
                return (false, "Unexpected error occurred while validating Item.");
            }
        }



        public async Task DeleteItemAsync(int itemId)
        {
            try
            {
                var item = await _unitOfWork.ItemRepositories.GetAsync(itemId);

                if (item != null)
                {
                    // 🔥 1. Get all related correspondence records
                    var correspondences = await _unitOfWork.Correspondances
                        .GetAllAsync(x => x.ReferenceId == itemId && x.ReferenceType == "Item");

                    // 🔥 2. Delete them first
                    if (correspondences != null && correspondences.Any())
                    {
                        foreach (var correspondence in correspondences)
                        {
                            await _fileuploadservice.DeleteFileAsync(correspondence.FilePath);
                        }
                        await _unitOfWork.Correspondances.DeleteRangeAsync(correspondences);
                    }

                    await _unitOfWork.ItemRepositories.DeleteAsync(item);
                    await _unitOfWork.SaveAsync();
                }

                await _logs.LogUserAction(
                    await _userService.GetUsernameAsync(),
                    _userService.MachineName,
                    _userService.IpAddress,
                    "Item List",
                    "Delete",
                    $"Deleted Item: {item?.ItemCode}"
                );

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to Delete Item ItemId {itemId}");
                throw;
            }
        }

        public async Task<(List<Item> Items, int TotalCount)> GetPagedItemsAsync(
                        int pageNumber, int pageSize, string searchTerm)
        {
            var query = _unitOfWork.ItemRepositories.GetQueryable()
                        .AsNoTracking()
                        .AsQueryable();


            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string term = $"%{searchTerm}%";
                query = query.Where(i =>
                    EF.Functions.Like(i.ItemCode, term) ||
                    EF.Functions.Like(i.ItemName, term));

            }

            int totalCount = await query.CountAsync();

            var items = await query
                    .OrderByDescending(i => i.ItemId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

            return (items, totalCount);
        }


        public async Task<string> GetAssignedCustomer(int itemId)
        {
            try
            {
                var customerNames = await _unitOfWork.ItemRepositories
                    .GetQueryable()
                    .Where(x => x.ItemId == itemId)
                    .SelectMany(x => x.ItemCustomerAssigns)
                    .Select(ica => ica.Customer.CustName)
                    .Distinct()
                    .ToListAsync();

                return string.Join(", ", customerNames);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Failed to get assigned customer for Item.");
                return string.Empty;
            }
        }

        public async Task<byte[]> GetDrawingPath(int itemId)
        {
            try
            {
                var filePath = await _unitOfWork.Correspondances
                    .GetQueryable()
                    .Where(x => x.ReferenceId == itemId
                             && x.DocumentType == "Drawing"
                             && x.ReferenceType == "Item")
                    .OrderBy(x => x.Id) // or CreatedDate
                    .Select(x => x.Image)
                    .FirstOrDefaultAsync();

                return filePath;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Failed to get drawing path for Item.");
                return Array.Empty<byte>();
            }
        }

    }
}
