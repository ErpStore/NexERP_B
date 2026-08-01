using AutoMapper;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.IMaterialRequisitionService;
using V.SMART.Shared.Data.Inventory_Stock_.StoreTransferNote;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.Data.OutSourcing;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.InventoryViewModel.SCNGenViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using Microsoft.EntityFrameworkCore;
using System.Text;
using V.SMART.Shared.Data.Master.Company_Module;

namespace V.SMART.Shared.BusinessLayer.BusinessService.MasterService.InventoryService
{
    public class AssemblyDefLabourService:IAssemblyDefLabourService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _logs;
        private readonly ICommonService _commonService;
        private readonly ISCNGenService _scnGenService;
        private readonly IMaterialReqService _mreqService;
        public readonly CurrentUserService _userService;
        private readonly IMapper _mapper;
        private readonly ForeignKeyUsageChecker _fkChecker;
        public AssemblyDefLabourService(IUnitOfWork unitOfWork, ILoggingService logs,
            CurrentUserService userService, ICommonService commonService,
            IMaterialReqService materialReqService,
            IMapper mapper, ISCNGenService scnGenService, ForeignKeyUsageChecker foreignKeyUsageChecker)
        {
            _unitOfWork = unitOfWork;
            _logs = logs;
            _userService = userService;
            _scnGenService = scnGenService;
            _commonService = commonService;
            _mreqService = materialReqService;
            _mapper = mapper;
            _fkChecker = foreignKeyUsageChecker;
        }
        public async Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText)
          => await _commonService.SearchItemsAsync(searchText);

        public async Task<IEnumerable<ItemVM>> SearchAssyAndSubAssyItemsAsync(string searchText)
           => await _commonService.SearchAssyAndSubAssyItemsAsync(searchText);
        public async Task<IEnumerable<ItemVM>> SearchOnlyAssyItemsAsync(string searchText)
           => await _commonService.SearchOnlyAssyItemsAsync(searchText);

        public async Task<IEnumerable<ItemVM>> SearchSubAssyAndItemsAsync(string searchText)
            => await _commonService.SearchSubAssyAndItemsAsync(searchText);

        public async Task<ItemVM?> GetItemByItemIdAsync(int? itemId)
            => await _commonService.GetItemByItemIdAsync(itemId);

        public async Task<Companydetails?> GetCompanyDetailsAsync()
    => await _commonService.GetCompanyDetailsAsync();

        public async Task UploadBomAsync(List<BomUploadRowViewModel> bomList)
        {
            if (bomList == null || !bomList.Any())
                return;

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // ===============================
                // STEP 1: ITEM + RM CREATION
                // ===============================
                foreach (var row in bomList)
                {
                    await ProcessBomRowAsync(row);
                }

                // ===============================
                // STEP 2: ASSEMBLY DEFINITIONS
                // ===============================
                await ProcessAssemblyAsync(bomList);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task ProcessBomRowAsync(BomUploadRowViewModel row)
        {
            if (string.IsNullOrWhiteSpace(row.PartNumber))
                return;

            string uom = row.UOM?.Split('-')[0].Trim() ?? "NOS";
            string rmUom = row.RMUOM?.Split('-')[0].Trim() ?? "NOS";
            string shape = row.Shape?.Trim() ?? "";
            string material = row.Material?.Trim() ?? "";
            string grade = row.MaterialGrade?.Trim() ?? "";
            string materialWithGrade = $"{material} {grade}".Trim();

            int? categoryCode = await EnsureCategoryAsync(row.Category);

            string itemCode = builditemcode(
                row.ODLengthComp ?? 0,
                row.IDWidthComp ?? 0,
                row.WallThicknessComp ?? 0,
                row.ThicknessLengthHeightComp ?? 0,
                row.PartNumber.Trim(),
                shape,
                materialWithGrade,
                false);

            string rmItemCode = builditemcode(
                row.ODLength_RM ?? 0,
                row.IDWidth_RM ?? 0,
                row.WallThickness_RM ?? 0,
                row.ThicknessLengthHeight_RM ?? 0,
                row.PartNumber.Trim(),
                shape,
                materialWithGrade,
                true);

            if (string.IsNullOrWhiteSpace(itemCode))
                return;

            if (await _unitOfWork.ItemRepositories
                .AnyAsync(x => x.ItemCode == itemCode || x.ItemCode == rmItemCode))
                return;

            var rm = await EnsureRawMaterialAsync(row);

            await EnsureHSNAsync(row.HSNCode);
            await EnsureHSNAsync(row.SACCode);

            // === Component Item ===
            var componentItem = await EnsureComponentItemAsync(row, itemCode, uom, categoryCode, rm, shape);

            // === RM Item + Mapping ===
            if (!string.IsNullOrWhiteSpace(rmItemCode))
            {
                var rmItem = await EnsureRMItemAsync(row, rmItemCode, rmUom, 1, rm, shape);

                await EnsureCompMasterAsync(componentItem.ItemId, rmItem.ItemId, row);
            }
        }


        private async Task ProcessAssemblyAsync(List<BomUploadRowViewModel> bomList)
        {
            try
            {
                int assyId = 0;
                string assemblyName = string.Empty;

                // ============================
                // STEP 1: Identify Main Assembly
                // ============================
                foreach (var row in bomList)
                {
                    if (row.Category?.Split('-')[0].Trim() == "3"
                        && !string.IsNullOrWhiteSpace(row.PartNumber))
                    {
                        assemblyName = row.PartNumber.Trim();
                        assyId = await _unitOfWork.ItemRepositories.GetItemIdByCodeAsync(assemblyName);
                        break;
                    }
                }

                if (assyId == 0)
                    throw new InvalidOperationException("Main Assembly item not found in BOM.");

                int mainSlNo = await _unitOfWork.AssmblyDefLabs.GetNextSlNoAsync(assyId);

                // ============================
                // STEP 2: Loop BOM Rows
                // ============================
                foreach (var row in bomList)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(row.PartNumber))
                            continue;

                        int mainAssyId = await _unitOfWork.ItemRepositories
                            .GetItemIdByCodeAsync(row.MainAssembly?.Trim());

                        string builtCode = builditemcode(
                            row.ODLengthComp.GetValueOrDefault(),
                            row.IDWidthComp.GetValueOrDefault(),
                            row.WallThicknessComp.GetValueOrDefault(),
                            row.ThicknessLengthHeightComp.GetValueOrDefault(),
                            row.PartNumber.Trim(),
                            row.Shape?.Trim() ?? string.Empty,
                            $"{row.Material?.Trim()} {row.MaterialGrade?.Trim()}",
                            false);

                        int itemId = await _unitOfWork.ItemRepositories.GetItemIdByCodeAsync(builtCode);

                        if (itemId == 0)
                            itemId = await _unitOfWork.ItemRepositories
                                .GetItemIdByCodeAsync(row.PartNumber.Trim());

                        if (itemId == 0)
                            throw new InvalidOperationException($"Item not found for PartNumber: {row.PartNumber}");

                        // ============================
                        // MAIN ASSEMBLY CHILD
                        // ============================
                        if (assyId == mainAssyId)
                        {
                            if (await _unitOfWork.AssmblyDefLabs.ExistsAsync(mainAssyId, itemId))
                                continue;

                            var assy = new AssemblyDefLabour
                            {
                                AssmblyID = mainAssyId,
                                ItemId = itemId,
                                UtilQty = row.Qty.GetValueOrDefault(),
                                SlNo = mainSlNo++,
                                CreatedBy = await _userService.GetUsernameAsync(),
                                CreatedDate = DateTime.Now,
                                Remarks = row.Remarks?.Trim(),
                                BalQty = row.Qty.GetValueOrDefault()


                            };

                            await _unitOfWork.AssmblyDefLabs.CreateAsync(assy);
                            await _unitOfWork.SaveAsync();
                        }
                        // ============================
                        // SUB ASSEMBLY CHILD
                        // ============================
                        else if (mainAssyId == 0)
                        {
                            int subAssyId =
                                await _unitOfWork.ItemRepositories.GetItemIdByCodeAsync(row.SubAssembly1?.Trim());

                            if (subAssyId == 0 && string.IsNullOrWhiteSpace(row.SubAssembly1))
                                subAssyId = await _unitOfWork.ItemRepositories.GetItemIdByCodeAsync(row.SubAssembly2?.Trim());

                            if (subAssyId == 0 && string.IsNullOrWhiteSpace(row.SubAssembly2))
                                subAssyId = await _unitOfWork.ItemRepositories.GetItemIdByCodeAsync(row.SubAssembly3?.Trim());

                            if (subAssyId == 0)
                                continue;

                            if (await _unitOfWork.AssmblyDefLabs.ExistsAsync(subAssyId, itemId))
                                continue;

                            int subSlNo = await _unitOfWork.AssmblyDefLabs.GetNextSlNoAsync(subAssyId);

                            var def = new AssemblyDefLabour
                            {
                                AssmblyID = subAssyId,
                                ItemId = itemId,
                                UtilQty = row.Qty.GetValueOrDefault(),
                                SlNo = subSlNo,
                                CreatedBy = await _userService.GetUsernameAsync(),
                                CreatedDate = DateTime.Now,
                                Remarks = row.Remarks?.Trim(),
                                BalQty = row.Qty.GetValueOrDefault()
                            };

                            await _unitOfWork.AssmblyDefLabs.CreateAsync(def);
                            await _unitOfWork.SaveAsync();
                        }
                    }
                    catch (Exception rowEx)
                    {
                        await _logs.LogDeveloperError(rowEx, $"Error processing BOM row for PartNumber {row.PartNumber}");
                        throw new ApplicationException($"Error processing BOM row. PartNumber: {row.PartNumber}", rowEx);
                    }
                }
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error while processing assembly BOM");
                throw new ApplicationException("Failed to process Assembly BOM.", ex);
            }
        }


        public async Task<IEnumerable<Process>> LoadSelectedProcessAsync(int assyId)
        {
            try
            {
                var result = await _unitOfWork.ProcessFlowCharts.GetQueryable()
                    .Include(pf => pf.Process)
                    .Where(pf => pf.CompItemId == assyId && pf.BOM == true)
                    .Select(pf => new Process
                    {
                        ProcessId = pf.Process.ProcessId,
                        ProcessName = pf.Process.ProcessName
                    })
                    .Distinct()
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error loading selected process for ItemId={assyId}");
                return Enumerable.Empty<Process>();
            }
        }

        private string builditemcode(decimal len, decimal wid, decimal wt, decimal thick, string itemcode, string shape, string mtl, bool isRMcode = false)
        {
            if (isRMcode && len <= 0) return "";

            List<string> dim = new List<string>();
            if (len != 0) dim.Add(len.ToString());
            if (wid != 0) dim.Add(wid.ToString());
            if (thick != 0) dim.Add(thick.ToString());
            if (wt != 0) dim.Add(wt.ToString());

            if (dim.Count == 0) return itemcode;

            if (!string.IsNullOrEmpty(shape))
            {
                if (shape.Contains("Round", StringComparison.OrdinalIgnoreCase))
                {
                    return itemcode + ' ' + mtl + " Ø" + string.Join("x", dim);
                }

                return itemcode + ' ' + shape + ' ' + mtl + ' ' + string.Join("x", dim);
            }

            return itemcode + ' ' + string.Join("x", dim);
        }

        private async Task<int?> EnsureCategoryAsync(string? categoryValue)
        {
            if (string.IsNullOrWhiteSpace(categoryValue))
                return null;

            var parts = categoryValue.Split('-', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                return null;

            string categoryName = parts[1].Trim();
            if (string.IsNullOrWhiteSpace(categoryName))
                return null;

            var existingCategory = await _unitOfWork.Categories
                .FirstOrDefaultAsync(c =>
                    c.CategoryName.ToLower() == categoryName.ToLower());

            if (existingCategory != null)
                return existingCategory.CategoryCode;

            var newCategory = new Category
            {
                CategoryName = categoryName,
                CategoryDesc = string.Empty,
                IsSystemDefined = false
            };

            await _unitOfWork.Categories.CreateAsync(newCategory);

            await _unitOfWork.SaveAsync();
            return newCategory.CategoryCode;
        }

        private async Task<RawMaterial?> EnsureRawMaterialAsync(BomUploadRowViewModel row)
        {
            string material = row.Material?.Trim() ?? "";
            string grade = row.MaterialGrade?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(material))
                return null;

            var rm = await _unitOfWork.RawMaterial.FirstOrDefaultAsync(r =>
                r.RMName.ToLower() == material.ToLower() &&
                r.Grade == grade);

            if (rm != null)
                return rm;

            rm = new RawMaterial
            {
                RMName = material,
                Grade = grade,
                Density = row.Density.GetValueOrDefault()
            };

            await _unitOfWork.RawMaterial.CreateAsync(rm);
            await _unitOfWork.SaveAsync(); // OK inside transaction

            await _logs.LogUserAction(
                await _userService.GetUsernameAsync(),
                _userService.MachineName,
                _userService.IpAddress,
                "Raw Material Creation",
                $"Created Raw Material: {rm.RMName} - {rm.Grade} (Density: {rm.Density})",
                "SaveBOM"
            );

            return rm;
        }


        private async Task EnsureHSNAsync(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return;

            var hsn = await _unitOfWork.HSNMasters
                .FirstOrDefaultAsync(h => h.HSNcode == code);

            if (hsn == null)
            {
                hsn = new HSNMaster
                {
                    HSNCategoryCode = code.Length >= 2 ? code.Substring(0, 2) : code,
                    HSNcode = code,
                    Required = true
                };

                await _unitOfWork.HSNMasters.CreateAsync(hsn);
            }
            else if (!hsn.Required)
            {
                hsn.Required = true;
                await _unitOfWork.HSNMasters.UpdateAsync(hsn);
            }

            await _unitOfWork.SaveAsync(); // keep transaction-safe
        }

        private async Task<Item> EnsureComponentItemAsync(BomUploadRowViewModel row, string itemCode, string uom,
                                    int? categoryCode, RawMaterial? rm, string? shape)
        {
            var existing = await _unitOfWork.ItemRepositories
                .FirstOrDefaultAsync(x => x.ItemCode == itemCode);

            if (existing != null)
                return existing;

            var item = new Item
            {
                ItemCode = itemCode,
                ItemName = row.PartName?.Trim() ?? ".",
                Rate = row.Rate.GetValueOrDefault(),
                ROL = (int?)row.ROL,
                HSNCode = row.HSNCode?.Trim(),
                SACCode = row.SACCode?.Trim(),
                CategoryCode = categoryCode,
                MeasureUnit = uom,
                PurchaseUnit = uom,
                Specification = row.Specification?.Trim(),
                Shape = shape,
                RmId = rm?.RMId,
                BtOtorMfg = row.ItemType?.Trim() == "BOP-Broughtout/Purchase" ? "BtOt" : "Mfg",
                Density = row.Density.GetValueOrDefault(),
                Weight = row.RMWt.GetValueOrDefault(),
                Length = row.ODLengthComp.GetValueOrDefault(),
                Width = row.IDWidthComp.GetValueOrDefault(),
                Thickness = row.ThicknessLengthHeightComp.GetValueOrDefault(),
                WallThickness = row.WallThicknessComp.GetValueOrDefault(),
                CreatedDate = DateTime.Now,
                CreatedBy = await _userService.GetUsernameAsync()
            };

            await _unitOfWork.ItemRepositories.CreateAsync(item);
            await _unitOfWork.SaveAsync();

            await _logs.LogUserAction(await _userService.GetUsernameAsync(), _userService.MachineName,
                _userService.IpAddress, "Item Creation", $"Component Item Created: {itemCode} - {item.ItemName}", "SaveBOM");

            return item;
        }

        private async Task<Item> EnsureRMItemAsync(BomUploadRowViewModel row, string rmItemCode, string rmUOM,
                    int? rmCategoryCode, RawMaterial? rm, string? shape)
        {
            var existing = await _unitOfWork.ItemRepositories
                .FirstOrDefaultAsync(x => x.ItemCode == rmItemCode);

            if (existing != null)
                return existing;

            var rmItem = new Item
            {
                ItemCode = rmItemCode,
                ItemName = row.PartName?.Trim() ?? ".",
                Rate = row.Rate.GetValueOrDefault(),
                ROL = (int?)row.ROL,
                MeasureUnit = rmUOM,
                PurchaseUnit = rmUOM,
                CategoryCode = rmCategoryCode,
                HSNCode = row.HSNCode?.Trim(),
                SACCode = row.SACCode?.Trim(),
                Specification = row.Specification?.Trim(),
                BtOtorMfg = row.ItemType?.Trim() == "BOP-Broughtout/Purchase" ? "BtOt" : "Mfg",
                Shape = shape,
                RmId = rm?.RMId,
                Density = row.Density.GetValueOrDefault(),
                Weight = row.RMWt.GetValueOrDefault(),
                Length = row.ODLength_RM.GetValueOrDefault(),
                Width = row.IDWidth_RM.GetValueOrDefault(),
                Thickness = row.ThicknessLengthHeight_RM.GetValueOrDefault(),
                WallThickness = row.WallThickness_RM.GetValueOrDefault(),
                CreatedDate = DateTime.Now,
                CreatedBy = await _userService.GetUsernameAsync()
            };

            await _unitOfWork.ItemRepositories.CreateAsync(rmItem);
            await _unitOfWork.SaveAsync();

            await _logs.LogUserAction(
                await _userService.GetUsernameAsync(),
                _userService.MachineName,
                _userService.IpAddress,
                "Item Creation",
                $"Raw Material Item Created: {rmItemCode} - {rmItem.ItemName}",
                "SaveBOM");

            return rmItem;
        }

        private async Task EnsureCompMasterAsync(int componentItemId, int rmItemId, BomUploadRowViewModel row)
        {
            bool exists = await _unitOfWork.CompMasters.AnyAsync(x =>
                x.CompItemId == componentItemId &&
                x.RMId == rmItemId);

            if (exists)
                return;

            var compMaster = new CompMaster
            {
                CompItemId = componentItemId,
                RMId = rmItemId,
                Weight = row.RMWt.GetValueOrDefault(),
                LabWORM = false,
                CreatedBy = await _userService.GetUsernameAsync(),
                CreatedDate = DateTime.Now

            };

            await _unitOfWork.CompMasters.CreateAsync(compMaster);
            await _unitOfWork.SaveAsync();

            await _logs.LogUserAction(
                await _userService.GetUsernameAsync(),
                _userService.MachineName,
                _userService.IpAddress,
                "Component Mapping",
                $"Component {componentItemId} mapped to RM {rmItemId} with weight {compMaster.Weight}",
                "SaveBOM");
        }


        public async Task<(decimal UtilQty, decimal BalQty)> GetUtiliyQtyById(int id)
        {
            if (id <= 0)
                return (0, 0);

            var entity = await _unitOfWork.AssmblyDefLabs.GetQueryable()
                                  .Where(a => a.Id == id)
                                  .Select(a => new { a.UtilQty, a.BalQty })
                                  .FirstOrDefaultAsync();

            if (entity == null)
                return (0, 0);

            return (entity.UtilQty, entity.BalQty);
        }

        public async Task<IEnumerable<ItemVM>> SearchBOMAssinedComponentItems(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return Enumerable.Empty<ItemVM>();

            try
            {
                searchText = searchText.Trim().ToLower();

                var items = await _unitOfWork.ProcessFlowCharts.GetQueryable()
                    .Include(p => p.ComponentItem)
                    .Where(p => p.BOM &&
                                !p.ComponentItem.Hide &&
                                (EF.Functions.Like(p.ComponentItem.ItemCode.ToLower(), $"%{searchText}%") ||
                                 EF.Functions.Like(p.ComponentItem.ItemName.ToLower(), $"%{searchText}%")))
                    .GroupBy(p => new { p.CompItemId, p.ComponentItem.ItemCode, p.ComponentItem.ItemName })
                    .Select(g => new ItemVM
                    {
                        ItemId = g.Key.CompItemId,
                        ItemCode = g.Key.ItemCode,
                        ItemName = g.Key.ItemName
                    })
                    .ToListAsync();

                return items;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error SearchBOMAssinedComponentItems items with text='{searchText}'.");
                return Enumerable.Empty<ItemVM>();
            }
        }

        public async Task<IEnumerable<ItemVM>> SearchOnlyComponentItems(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return Enumerable.Empty<ItemVM>();

            try
            {
                searchText = searchText.Trim().ToLower();

                var items = await _unitOfWork.ItemRepositories.GetQueryable()
                    .Where(i => !i.Hide &&
                                i.CategoryCode == 2 &&
                                (EF.Functions.Like(i.ItemCode.ToLower(), $"%{searchText}%") ||
                                 EF.Functions.Like(i.ItemName.ToLower(), $"%{searchText}%")))
                    .GroupBy(i => new
                    {
                        i.ItemId,
                        i.ItemCode,
                        i.ItemName,
                        i.Rate,
                        i.MeasureUnit,
                        i.HSNCode,
                        i.SACCode,
                        i.CGST,
                        i.SGST,
                        i.IGST
                    })
                    .Select(g => new ItemVM
                    {
                        ItemId = g.Key.ItemId,
                        ItemCode = g.Key.ItemCode,
                        ItemName = g.Key.ItemName,
                        Rate = g.Key.Rate,
                        MeasureUnit = g.Key.MeasureUnit,
                        HSNCode = g.Key.HSNCode,
                        SACCode = g.Key.SACCode,
                        CGST = g.Key.CGST,
                        SGST = g.Key.SGST,
                        IGST = g.Key.IGST
                    })
                    .ToListAsync();

                return items;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error SearchOnlyComponentItems items with text='{searchText}'.");
                return Enumerable.Empty<ItemVM>();
            }
        }

        public async Task<List<ItemVM>> GetAllSubAssembliesByAssyIdAsync(int assyId)
            => await _commonService.GetAllSubAssembliesByAssyIdAsync(assyId);


        //BOM Related

        public async Task<List<AssemblyDefLabourVM>> GetAllAssembliesAsync()
        {
            try
            {
                var entities = await _unitOfWork.AssmblyDefLabs.GetQueryable()
                    .Include(a => a.AssemblyItem)
                    .OrderByDescending(a => a.AssmblyID)
                    .ToListAsync();

                var result = _mapper.Map<List<AssemblyDefLabourVM>>(entities);

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "GetAllAssembliesAsync failed.");
                return new List<AssemblyDefLabourVM>();
            }
        }

        public async Task<bool> IsAssyExistsInAssyDefAsync(int assyId)
        {
            try
            {
                return await _unitOfWork.AssmblyDefLabs
                .GetQueryable()
                .AnyAsync(a => a.AssmblyID == assyId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in while IsAssyExistsInAssyDefAsync'.");
                return false;
            }
        }

        public async Task<AssemblyDefLabourVM> GetAssyByAssemblyIdAsync(int assemblyId)
        {
            try
            {
                var entities = await _unitOfWork.AssmblyDefLabs.GetQueryable()
                    .Include(x => x.AssemblyItem).Where(x => x.AssmblyID == assemblyId)
                    .FirstOrDefaultAsync();

                var result = _mapper.Map<AssemblyDefLabourVM>(entities);

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetAssemblyDefByAssemblyIdAsync({assemblyId}) failed.");
                return new AssemblyDefLabourVM();
            }
        }

        public async Task<List<AssemblyDefLabour>> GetAllAsyItemsByAssyId(int assemblyId)
        {
            try
            {
                return await _unitOfWork.AssmblyDefLabs
                    .GetQueryable()
                    .Where(x => x.AssmblyID == assemblyId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetAllAsyItemsByAssyId({assemblyId}) failed.");
                return new List<AssemblyDefLabour>();
            }
        }

        public async Task<List<AssemblyModify>> GetAllAssyModifyTrakByAssyId(int assemblyId)
        {
            try
            {
                return await _unitOfWork.AssemblyModifies
                    .GetQueryable()
                    .Where(x => x.AssyId == assemblyId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetAllAssyModifyTrakByAssyId({assemblyId}) failed.");
                return new List<AssemblyModify>();
            }
        }

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

        public async Task DeleteAndResequenceAsync(AssemblyDefLabourVM assemblyDef)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (assemblyDef.Id > 0)
                {
                    var entity = await _unitOfWork.AssmblyDefLabs.GetAsync(assemblyDef.Id);
                    if (entity == null)
                        throw new InvalidOperationException("Child item not found.");

                    await _unitOfWork.AssmblyDefLabs.DeleteAsync(entity);
                    await _unitOfWork.SaveAsync();

                    var username = await _userService.GetUsernameAsync();
                    await _logs.LogUserAction(
                        username,
                        _userService.MachineName,
                        _userService.IpAddress,
                        "Bill of Material",
                        $"Deleted Item: {assemblyDef.PartItemCode}",
                        $"Assembly Name: {assemblyDef.AssemblyItemCode}"
                    );
                }

                var remaining = await _unitOfWork.AssmblyDefLabs
                    .GetQueryable()
                    .Where(x => x.AssmblyID == assemblyDef.AssmblyID)
                    .OrderBy(x => x.SlNo)
                    .ToListAsync();

                int slno = 1;
                foreach (var item in remaining)
                {
                    item.SlNo = slno++;
                }

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "DeleteAndResequenceAsync failed.");
                throw;
            }
        }

        public async Task UpdateItemCancelAsync(AssemblyDefLabourVM subItem)
        {
            if (subItem == null)
                throw new ArgumentNullException(nameof(subItem), "Subitem cannot be null.");

            if (subItem.Id == 0)
                throw new ArgumentException("Cannot update unsaved subitem.");

            try
            {
                var entity = await _unitOfWork.AssmblyDefLabs
                    .FirstOrDefaultAsync(x => x.Id == subItem.Id);

                if (entity == null)
                    throw new KeyNotFoundException($"Subitem with Child Item {subItem.PartItemCode} not found.");

                entity.Cancel = subItem.Cancel;

                await _unitOfWork.AssmblyDefLabs.UpdateAsync(entity);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in UpdateItemCancelAsync for ItemCode {subItem.PartItemCode} with AssemblyCode {subItem.AssemblyItemCode}");
                throw;
            }
        }

        public async Task UpsertBOMAsync(bool isNew, AssemblyDefLabourVM parentAssyVM, List<AssemblyDefLabourVM> bomRows)
        {
            if (bomRows == null || !bomRows.Any())
                throw new InvalidOperationException("No BOM rows to save.");

            var now = DateTime.Now;
            var currentUser = await _userService.GetUsernameAsync();

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                

                if (isNew)
                {
                    foreach (var row in bomRows)
                    {
                        var entity = _mapper.Map<AssemblyDefLabour>(row);

                        entity.IsComponent = parentAssyVM.IsComponent;
                        entity.AssmblyID = parentAssyVM.AssmblyID ?? 0;
                        entity.CreatedBy = currentUser;
                        entity.CreatedDate = now;

                        await _unitOfWork.AssmblyDefLabs.CreateAsync(entity);
                        await _unitOfWork.SaveAsync();

                    }


                    changes.AppendLine("BOM Created.");
                }
                else
                {
                    var existingBOMs = await _unitOfWork.AssmblyDefLabs.GetQueryable()
                        .Where(x => x.AssmblyID == parentAssyVM.AssmblyID)
                        .ToListAsync();

                    foreach (var row in bomRows)
                    {
                        //if (row.Id > 0)
                        //{
                            var entity = existingBOMs.FirstOrDefault(x => x.ItemId == row.ItemId);

                            ////if (entity == null)
                            ////    continue;

                            //entity.ItemId = row.ItemId ?? 0;
                            //entity.ProcessId = row.ProcessId;
                            //entity.CostId = row.CostId;
                            //entity.UtilQty = row.UtilQty ?? 0;
                            //entity.Cancel = row.Cancel;
                            //entity.Remarks = row.Remarks;
                            //entity.IsLabourMaterial = row.IsLabourMaterial;



                            //entity.ModifiedBy = currentUser;
                            //entity.ModifiedDate = now;
                            if (entity != null)
                            {
                                _mapper.Map(row, entity);
                                entity.ModifiedBy = currentUser;
                                entity.ModifiedDate = now;
                            }
                            else
                            {
                                var newEntity = _mapper.Map<AssemblyDefLabour>(row);
                               
                                newEntity.AssmblyID = parentAssyVM.AssmblyID ?? 0;
                                newEntity.CreatedBy = currentUser;
                                newEntity.CreatedDate = now;
                                await _unitOfWork.AssmblyDefLabs.CreateAsync(newEntity);
                                await _unitOfWork.SaveAsync();
                            }
                        //}
                        //else
                        //{
                        //    var newEntity = _mapper.Map<AssemblyDefLabour>(row);

                        //    newEntity.Id = 0; // Important
                        //    newEntity.AssmblyID = parentAssyVM.AssmblyID ?? 0;
                        //    newEntity.CreatedBy = currentUser;
                        //    newEntity.CreatedDate = now;

                        //    await _unitOfWork.AssmblyDefLabs.CreateAsync(newEntity);
                        //}
                    }
                    changes.AppendLine("BOM Updated.");
                }

              

                await _unitOfWork.SaveAsync();
                var existingBOMss = await _unitOfWork.AssmblyDefLabs.GetQueryable()
                                .Where(x => x.AssmblyID == parentAssyVM.AssmblyID)
                                .ToListAsync();

                int slno = 1;

                foreach (var row in existingBOMss)
                {
                    row.SlNo = slno;
                    slno++;
                }

                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();

                await LogChangesAsync(changes, parentAssyVM.Id == 0 ? "BOM Created" : "BOM Updated");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "Failed to save BOM.");
                throw new InvalidOperationException("Failed to save BOM. Please try again.");
            }
            //try
            //{
            //    var recNo = await _unitOfWork.AssemblyModifies.GetNextRecNoAsync(parentAssyVM.AssmblyID);

            //    if (isNew)
            //    {
            //        foreach (var row in bomRows)
            //        {
            //            var entity = _mapper.Map<AssemblyDefLabour>(row);

            //            entity.IsComponent = parentAssyVM.IsComponent;
            //            entity.AssmblyID = parentAssyVM.AssmblyID ?? 0;
            //            entity.CreatedBy = currentUser;
            //            entity.CreatedDate = now;

            //            await _unitOfWork.AssmblyDefLabs.CreateAsync(entity);
            //            await _unitOfWork.SaveAsync();

            //            await CreateAssemblyModifyIfNotExistsAsync(
            //                recNo,
            //                parentAssyVM.AssmblyID,
            //                row.ItemId,
            //                row.Cancel,
            //                row.ProcessId,
            //                row.Remarks,
            //                row.UtilQty
            //            );
            //        }


            //        changes.AppendLine("BOM Created.");
            //    }
            //    else
            //    {
            //        var existingBOMs = await _unitOfWork.AssmblyDefLabs.GetQueryable()
            //            .Where(x => x.AssmblyID == parentAssyVM.AssmblyID)
            //            .ToListAsync();

            //        var bomItemIds = bomRows.Where(r => r.ItemId.HasValue).Select(r => r.ItemId.Value).ToHashSet();
            //        var toDelete = existingBOMs.Where(e => !bomItemIds.Contains(e.ItemId)).ToList();
            //        if (toDelete.Any())
            //        {
            //            await _unitOfWork.AssmblyDefLabs.DeleteRangeAsync(toDelete);
            //            changes.AppendLine($"{toDelete.Count} BOM items removed.");
            //        }

            //        foreach (var row in bomRows)
            //        {
            //            var entity = existingBOMs.FirstOrDefault(e => e.ItemId == row.ItemId);

            //            if (entity != null)
            //            {
            //                _mapper.Map(row, entity);
            //                entity.ModifiedBy = currentUser;
            //                entity.ModifiedDate = now;
            //            }
            //            else
            //            {
            //                var newEntity = _mapper.Map<AssemblyDefLabour>(row);
            //                newEntity.AssmblyID = parentAssyVM.AssmblyID ?? 0;
            //                newEntity.CreatedBy = currentUser;
            //                newEntity.CreatedDate = now;
            //                await _unitOfWork.AssmblyDefLabs.CreateAsync(newEntity);
            //                await _unitOfWork.SaveAsync();
            //            }

            //            await CreateAssemblyModifyIfNotExistsAsync(
            //                recNo,
            //                parentAssyVM.AssmblyID,
            //                row.ItemId,
            //                row.Cancel,
            //                row.ProcessId,
            //                row.Remarks,
            //                row.UtilQty
            //            );
            //        }

            //        changes.AppendLine("BOM Updated.");
            //    }
            //    await _unitOfWork.SaveAsync();
            //    await transaction.CommitAsync();

            //    await LogChangesAsync(changes, parentAssyVM.Id == 0 ? "BOM Created" : "BOM Updated");
            //}
            //catch (Exception ex)
            //{
            //    await transaction.RollbackAsync();
            //    await _logs.LogDeveloperError(ex, "Failed to save BOM.");
            //    throw new InvalidOperationException("Failed to save BOM. Please try again.");
            //}
        }

        // Logging
        private async Task LogChangesAsync(StringBuilder changes, string action)
        {
            if (changes.Length == 0) return;

            await _logs.LogUserAction(
                UserName: await _userService.GetUsernameAsync(),
                Machine: _userService.MachineName,
                IP_Address: _userService.IpAddress,
                screen: "Bill of Material",
                action: action,
                additionalInfo: changes.ToString()
            );
        }

        private async Task CreateAssemblyModifyIfNotExistsAsync(
            int recNo, int? assyId, int? itemId, bool cancel,
            int? processId, string? remark, decimal? utilityQty)
        {
            if (assyId == null || itemId == null)
                return;

            try
            {
                bool exists = await _unitOfWork.AssemblyModifies.AnyAsync(a =>
                    a.AssyId == assyId &&
                    a.ItemId == itemId &&
                    a.Cancel == cancel &&
                    a.ProcessId == processId &&
                    a.Remark == remark &&
                    a.UtilityQty == (utilityQty ?? 0)
                );

                if (!exists)
                {
                    var modifyHeader = new AssemblyModify
                    {
                        RecNo = recNo,
                        AssyId = assyId,
                        ItemId = itemId,
                        Cancel = cancel,
                        ProcessId = processId,
                        Remark = remark,
                        UtilityQty = utilityQty ?? 0,
                        CreatedBy = await _userService.GetUsernameAsync(),
                        CreatedDate = DateTime.Now
                    };

                    await _unitOfWork.AssemblyModifies.CreateAsync(modifyHeader);
                    await _unitOfWork.SaveAsync();
                }
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to create AssemblyModify for AssyId={assyId}, ItemId={itemId}");
            }
        }

        public async Task<bool> IsNewBOMAsync(int assemblyId)
        {
            try
            {
                bool exists = await _unitOfWork.AssmblyDefLabs.AnyAsync(x => x.AssmblyID == assemblyId);
                return !exists;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "IsNewBOMAsync failed.");
                throw;
            }

        }

        public async Task DeleteAssemblyRelatedDataAsync(int assyId, List<AssemblyDefLabour> allAssemblyRows)
        {
            try
            {
                if (allAssemblyRows == null || allAssemblyRows.Count == 0)
                    return;

                var AssyCode = await _unitOfWork.ItemRepositories
                    .GetQueryable()
                    .Where(i => i.ItemId == assyId)
                    .Select(i => i.ItemCode)
                    .FirstOrDefaultAsync();

                await _unitOfWork.AssmblyDefLabs.DeleteRangeAsync(allAssemblyRows);
                await _unitOfWork.SaveAsync();

                await _logs.LogUserAction(
                       await _userService.GetUsernameAsync(),
                       _userService.MachineName,
                       _userService.IpAddress,
                       "BOM List",
                       $"Deleted assembly: ",
                       $"Assembly Item-Code: {AssyCode}"
                   );
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "DeleteAssemblyRelatedDataAsync failed.");
                throw;
            }
        }

        public async Task DeleteAssemblyModifyTrackRelatedDataAsync(List<AssemblyModify> assyModifyTrack)
        {
            try
            {
                if (assyModifyTrack == null || assyModifyTrack.Count == 0)
                    return;

                await _unitOfWork.AssemblyModifies.DeleteRangeAsync(assyModifyTrack);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "DeleteAssemblyModifyTrackRelatedDataAsync failed.");
                throw;
            }
        }

        public async Task<bool> HasAnyBOMTransactionMadeAsync(int AssyId)
        {
            try
            {
                return await _unitOfWork.MfgPoSubs
                    .GetQueryable()
                    .AnyAsync(qs => qs.ItemId == AssyId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasAnyBOMTransactionMadeAsync for AssemblyId: {AssyId}");
                throw;
            }
        }

        public async Task<List<AssemblyModifyTrackVM>> GetAssemblyModifyTrackByAssyId(int assyId)
        {
            try
            {
                var result = await _unitOfWork.AssemblyModifies.GetQueryable()
                    .Where(am => am.AssyId == assyId)
                    .Join(_unitOfWork.ItemRepositories.GetQueryable(),
                          am => am.ItemId,
                          i => i.ItemId,
                          (am, i) => new { am, i })
                    .GroupJoin(_unitOfWork.Processes.GetQueryable(),
                               temp => temp.am.ProcessId,
                               p => p.ProcessId,
                               (temp, proc) => new { temp.am, temp.i, proc })
                    .SelectMany(x => x.proc.DefaultIfEmpty(),
                                (x, p) => new AssemblyModifyTrackVM
                                {
                                    RecNo = x.am.RecNo,
                                    ItemCode = x.i.ItemCode,
                                    ItemName = x.i.ItemName,
                                    MeasureUnit = x.i.MeasureUnit,
                                    UtilityQty = x.am.UtilityQty,
                                    Cancel = x.am.Cancel ? "Cancelled" : "Active",
                                    ProcessName = p != null ? p.ProcessName : string.Empty,
                                    Remark = x.am.Remark,
                                    CreatedBy = x.am.CreatedBy,
                                    CreatedDate = x.am.CreatedDate
                                })
                    .OrderBy(vm => vm.RecNo)
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetAssemblyModifyTrackByAssyId failed for AssyId {assyId}");
                throw;
            }
        }

        public async Task ConvertBOMToPRAsync(int mainAssyId, List<AssemblyDefLabourVM> bomRows)
        {
            var now = DateTime.Now;
            var currentUser = await _userService.GetUsernameAsync();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var suffix = FinancialYearHelper.GetFinancialYearSuffix(now);
                var prNo = await _unitOfWork.MaterialReqs.GetLastMReqNoAsync(suffix);

                var selectedRows = bomRows
                    .Where(r => r.IsSelected && (r.ReqQty.GetValueOrDefault() > 0))
                    .ToList();

                if (!selectedRows.Any())
                    throw new InvalidOperationException("No selected BOM rows with valid Qty.");


                var prAuthorityExists = await _unitOfWork
                    .UserAuthorities.AnyAsync(x => x.IsPR == true);

                var materialReq = new MaterialReq
                {
                    MReqNo = prNo,
                    Suffix = suffix,
                    MreqDate = now,
                    MreqDateNow = now,
                    NoOfItems = selectedRows.Count,
                    IsPurchOrSubCon = true,
                    IsManual = false,
                    CreatedBy = currentUser,
                    CreatedDate = now,
                    MaterialReqSubs = new List<MaterialReqSub>()
                };

                if (!prAuthorityExists)
                {
                    materialReq.IsAuthorized = true;
                    materialReq.LastApproved = currentUser;
                    materialReq.ApprovedDate = now;
                }
                else
                {
                    materialReq.IsAuthorized = false;
                    materialReq.LastApproved = null;
                    materialReq.ApprovedDate = null;

                    materialReq.IsLevel1 = false;
                    materialReq.IsLevel2 = false;
                    materialReq.IsLevel3 = false;

                    materialReq.Level1Sign = null;
                    materialReq.Level2Sign = null;
                    materialReq.Level3Sign = null;
                }

                materialReq.IsRejected = false;
                materialReq.RejectReason = string.Empty;

                int slNo = 1;

                var itemIds = selectedRows
                    .Where(r => r.ItemId.HasValue)
                    .Select(r => r.ItemId!.Value)
                    .Distinct()
                    .ToList();

                var lastPrices = await _mreqService.GetBulkLastUnitPricesAsync(itemIds, null);

                foreach (var row in selectedRows)
                {
                    decimal unitPrice = 0;

                    if (row.ItemId.HasValue && lastPrices.TryGetValue(row.ItemId.Value, out var price))
                    {
                        unitPrice = price;
                    }

                    if (unitPrice == 0)
                        unitPrice = 1;

                    materialReq.MaterialReqSubs.Add(new MaterialReqSub
                    {
                        SlNo = slNo++,
                        ItemId = row.ItemId!.Value,
                        PurchQty = row.ReqQty!.Value,
                        BalQty = row.ReqQty!.Value,
                        UnitPrice = unitPrice,
                        DueDate = now.AddDays(7),
                        RefBomId = row.Id,
                        MainAssyId = mainAssyId,
                        Remark = row.Remarks
                    });
                }

                await _unitOfWork.MaterialReqs.CreateAsync(materialReq);
                await _unitOfWork.SaveAsync();

                foreach (var row in selectedRows)
                {
                    if (row.Id > 0)
                    {
                        var bomEntity = await _unitOfWork.AssmblyDefLabs.GetAsync(row.Id);
                        if (bomEntity != null)
                        {
                            bomEntity.PRConvertedQty += row.ReqQty!.Value;
                            bomEntity.BalQty -= row.ReqQty.Value;
                            await _unitOfWork.SaveAsync();
                        }
                    }
                    row.ReqQty = 0;
                    row.IsSelected = false;
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "ConvertBOMToPR failed.");

                throw new InvalidOperationException(
                    "Failed to convert BOM to PR. Please try again or contact admin.");
            }
        }

        public async Task<SCNGenVM> AddStockFromBOM(int assemblyId, int? subAssyId, int? subAssyId2, int? subAssyId3, List<AssemblyDefLabourVM> bomRows)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var scn = await CreateSCNGenAsync(assemblyId, subAssyId, subAssyId2, subAssyId3, bomRows);

                var scnVM = new SCNGenVM
                {
                    SCNGenId = 0,
                    IsWithBom = scn.IsWithBom,
                    SCNGenNo = scn.SCNGenNo,
                    Suffix = scn.Suffix,
                    SCNGenDate = scn.SCNGenDate,
                    StoreId = scn.StoreId,
                    AssyId = scn.AssyId,
                    SubAssyId = subAssyId,
                    SubAssyId2 = subAssyId2,
                    SubAssyId3 = subAssyId3,
                    ReqQty = scn.ReqQty,
                    CreatedBy = scn.CreatedBy,
                    CreatedDate = scn.CreatedDate,
                    SCNGenSubVMs = scn.SCNGenSubs
                    .Select((sub) => new SCNGenSubVM
                    {
                        SlNo = sub.SlNo,
                        IsFromBOM = sub.IsFromBOM,
                        RefBomId = sub.RefBomId,
                        ItemId = sub.ItemId,
                        Qty = sub.Qty,
                        UtlQty = sub.UtlQty,
                        UnitPrice = sub.UnitPrice == 0 ? 1 : sub.UnitPrice,
                        Remark = sub.Remark,
                        BatchNo = sub.BatchNo,
                        Make = sub.Make,
                        RackNo = sub.RackNo,
                        CostId = sub.CostId
                    })
                    .ToList()
                };

                var screen = await _unitOfWork.Screens.GetQueryable()
                    .FirstOrDefaultAsync(x => x.ScreenName == "Stock-Add");

                if (screen == null)
                    throw new InvalidOperationException("Screen 'Stock-Add' not found. Cannot determine screen code.");

                int screenCode = screen.ScreenCode;

                var savedSCN = await _scnGenService.UpsertSCNAsync(scnVM, screenCode);

                foreach (var row in bomRows.Where(r => r.IsSelected && r.ReqQty > 0 && r.ItemId.HasValue))
                {
                    var existingItem = await _unitOfWork.AssmblyDefLabs.GetAsync(row.Id);
                    if (existingItem != null)
                    {
                        existingItem.BalQty -= row.ReqQty.GetValueOrDefault();
                        existingItem.StockConvertedQty += row.ReqQty.GetValueOrDefault();
                        await _unitOfWork.AssmblyDefLabs.UpdateAsync(existingItem);
                        await _unitOfWork.SaveAsync();
                    }
                }

                await transaction.CommitAsync();

                return savedSCN;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to generate and save SCN for AssemblyId: {assemblyId}");
                throw new InvalidOperationException("Failed to create SCN. Please try again.");
            }
        }

        private async Task<SCNGen> CreateSCNGenAsync(int assemblyId, int? subAssyId, int? subAssyId2, int? subAssyId3, List<AssemblyDefLabourVM> bomRows)
        {
            string suffix = FinancialYearHelper.GetFinancialYearSuffix(DateTime.Now);
            string scnGenNo = await _scnGenService.GetSCNGenNumberAsync(suffix);
            string currentUser = await _userService.GetUsernameAsync();

            // 🔹 Pre-fetch item prices
            var itemIds = bomRows
                .Where(b => b.ItemId.HasValue && b.IsSelected && b.ReqQty > 0)
                .Select(b => b.ItemId!.Value)
                .Distinct()
                .ToList();

            var unitPrices = await _scnGenService.GetBulkLastUnitPricesAsync(itemIds);

            var scn = new SCNGen
            {
                SCNGenNo = scnGenNo,
                Suffix = suffix,
                SCNGenDate = DateTime.Now,
                StoreId = 1,
                IsWithBom = true,
                AssyId = assemblyId,
                SubAssyId = subAssyId,
                SubAssyId2 = subAssyId2,
                SubAssyId3 = subAssyId3,
                ReqQty = 1,
                NoOfItems = itemIds.Count,
                CreatedBy = currentUser,
                CreatedDate = DateTime.Now
            };

            int slNo = 1;

            foreach (var item in bomRows)
            {
                if (!item.IsSelected || item.ReqQty <= 0 || !item.ItemId.HasValue)
                    continue;

                var sub = new SCNGenSub
                {
                    SCNGenId = scn.SCNGenId,
                    IsFromBOM = true,
                    RefBomId = item.Id,
                    SlNo = slNo++,
                    ItemId = item.ItemId,
                    Qty = item.ReqQty.GetValueOrDefault(),
                    UtlQty = item.UtilQty.GetValueOrDefault(),
                    UnitPrice = unitPrices.TryGetValue(item.ItemId.Value, out var price) ? price : 1m,
                    Remark = item.Remarks ?? string.Empty
                };

                scn.SCNGenSubs.Add(sub);
            }

            return scn;
        }

        public async Task<(List<AssemblyDefLabourVM> AssemblyDefLabourVMs, int TotalCount)>
     SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                IQueryable<AssemblyDefLabour> baseQuery = _unitOfWork.AssmblyDefLabs.GetQueryable();

                // Apply filters (NO INCLUDE HERE)
                if (filters != null && filters.Any())
                {
                    foreach (var filter in filters)
                    {
                        baseQuery = AssyFilterBuilder.ApplyFilter(baseQuery, filter.Key, filter.Value);
                    }
                }

                // STEP 1: Get Latest Id per Assembly (SQL SAFE)
                var latestIdsQuery = baseQuery
                    .GroupBy(x => x.AssmblyID)
                    .Select(g => g.Max(x => x.Id));

                var totalCount = await latestIdsQuery.CountAsync();

                // STEP 2: Paging on ID set
                var pagedIds = await latestIdsQuery
                    .OrderByDescending(id => id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // STEP 3: Fetch actual data WITH Includes
                var list = await _unitOfWork.AssmblyDefLabs
                    .GetQueryable()
                    .Where(x => pagedIds.Contains(x.Id))
                    .Include(x => x.AssemblyItem)
                    .Include(x => x.PartItem)
                    .OrderByDescending(x => x.Id)
                    .ToListAsync();

                var vmList = _mapper.Map<List<AssemblyDefLabourVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync (BOM)");
                throw new InvalidOperationException("Failed to load BOM list.", ex);
            }
        }
        public static class AssyFilterBuilder
        {
            public static IQueryable<AssemblyDefLabour> ApplyFilter(
                IQueryable<AssemblyDefLabour> query,
                string field,
                object value)
            {
                if (value == null)
                    return query;

                var val = value.ToString()?.Trim();

                if (string.IsNullOrWhiteSpace(val))
                    return query;

                switch (field)
                {
                    case "AssemblyItemCode":
                        return query.Where(x =>
                            x.AssemblyItem != null &&
                            x.AssemblyItem.ItemCode != null &&
                            x.AssemblyItem.ItemCode.Contains(val));

                    case "SubAssemblyItemCode":
                        return query.Where(x =>
                            (x.AssemblyItem != null &&
                             x.AssemblyItem.ItemCode != null &&
                             x.AssemblyItem.ItemCode.Contains(val))
                            ||
                            (x.PartItem != null &&
                             x.PartItem.ItemCode != null &&
                             x.PartItem.ItemCode.Contains(val)));

                    case "PartItemCode":
                        return query.Where(x =>
                            x.PartItem != null &&
                            x.PartItem.ItemCode != null &&
                            x.PartItem.ItemCode.Contains(val));

                    case "CreatedBy":
                        return query.Where(x =>
                            x.CreatedBy != null &&
                            x.CreatedBy.Contains(val));

                    case "FromDate":
                        if (DateTime.TryParse(val, out var fromDate))
                        {
                            return query.Where(x =>
                                x.CreatedDate >= fromDate.Date);
                        }
                        break;

                    case "ToDate":
                        if (DateTime.TryParse(val, out var toDate))
                        {
                            var endDate = toDate.Date.AddDays(1).AddTicks(-1);
                            return query.Where(x =>
                                x.CreatedDate <= endDate);
                        }
                        break;
                }

                return query;
            }
        }

        public async Task<(bool CanDelete, string Message)> CanDeleteAssydefAsync(int id)
        {
            try
            {
                var assmblyDef = await _unitOfWork.AssmblyDefLabs
                    .GetQueryable()
                    .Where(s => s.AssmblyID == id).FirstOrDefaultAsync();

                if (assmblyDef == null)
                    return (false, "BOM not found or already removed.");

                var usedIn = await _fkChecker.GetUsageTableAsync<AssmblyDef>(id);

                if (usedIn != null)
                    return (false, $"Cannot delete BOM '{assmblyDef.AssemblyItem.ItemCode}' because it is used in {usedIn} Screen.");

                var hasMfgEnquiry = await _unitOfWork.EnquirySalesSubs.AnyAsync(me => me.ItemId == assmblyDef.AssmblyID);

                if (hasMfgEnquiry)
                    return (false, $"Cannot delete BOM '{assmblyDef.AssemblyItem.ItemCode}' because it is referenced in Manufacturing Enquiries.");

                var hasMfgQuote = await _unitOfWork.MfgQuoteSubs.AnyAsync(mq => mq.ItemId == assmblyDef.AssmblyID);

                if (hasMfgQuote)
                    return (false, $"Cannot delete BOM '{assmblyDef.AssemblyItem.ItemCode}' because it is referenced in Manufacturing Quotes.");

                var hasMfgPo = await _unitOfWork.MfgPoSubs.AnyAsync(mp => mp.ItemId == assmblyDef.AssmblyID);

                if (hasMfgPo)
                    return (false, $"Cannot delete BOM '{assmblyDef.AssemblyItem.ItemCode}' because it is referenced in Manufacturing Purchase Orders.");

                var hasJobcard = await _unitOfWork.JobOrders.AnyAsync(jc => jc.AssyId == assmblyDef.AssmblyID);

                if (hasJobcard)
                    return (false, $"Cannot delete BOM '{assmblyDef.AssemblyItem.ItemCode}' because it is referenced in Job Cards.");

                var StoreTransferNoteCount = await _unitOfWork.SCNGens.AnyAsync(st => st.AssyId == assmblyDef.AssmblyID);

                if (StoreTransferNoteCount)
                    return (false, $"Cannot delete BOM '{assmblyDef.AssemblyItem.ItemCode}' because it is referenced in Store Transfer Notes.");

                var hasMaterialIssueSub = await _unitOfWork.MINs.AnyAsync(mi => mi.AssyId == assmblyDef.AssmblyID);

                if (hasMaterialIssueSub)
                    return (false, $"Cannot delete BOM '{assmblyDef.AssemblyItem.ItemCode}' because it is referenced in Material Issue Notes.");


                return (true, $"BOM '{assmblyDef.AssmblyID}' can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Store delete validation failed: {id}");
                return (false, "Unexpected error occurred while validating Raw Material.");
            }
        }

        public async Task<bool> DeleteBOMAsync(int bomId)
        {
            try
            {
                var AssmblyDefLabs = await _unitOfWork.AssmblyDefLabs
                    .GetQueryable()
                    .Include(x => x.AssemblyItem)
                    .Where(x => x.AssmblyID == bomId)
                    .ToListAsync();

                if (!AssmblyDefLabs.Any())
                    return false;

                // Capture log details BEFORE delete
                var assemblyCode = AssmblyDefLabs.FirstOrDefault()?.AssemblyItem?.ItemCode ?? "";
                var totalRows = AssmblyDefLabs.Count;

                await _unitOfWork.AssmblyDefLabs.DeleteRangeAsync(AssmblyDefLabs);
                await _unitOfWork.SaveAsync();

                await _logs.LogUserAction(
                    UserName: await _userService.GetUsernameAsync(),
                    Machine: _userService.MachineName,
                    IP_Address: _userService.IpAddress,
                    screen: "BOM List",
                    action: $"Deleted BOM for Assembly: {assemblyCode}",
                    additionalInfo: $"Assembly Id: {bomId}, Rows Deleted: {totalRows}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error deleting BOM with Id {bomId}");
                return false;
            }
        }

        //SNS
        public async Task<IEnumerable<ItemVM>> SearchOnlyLabourItems(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return Enumerable.Empty<ItemVM>();

            try
            {
                searchText = searchText.Trim().ToLower();

                var items = await _unitOfWork.ItemRepositories.GetQueryable()
                    .Where(i => !i.Hide &&
                                i.IsLabourItem &&
                                (EF.Functions.Like(i.ItemCode.ToLower(), $"%{searchText}%") ||
                                 EF.Functions.Like(i.ItemName.ToLower(), $"%{searchText}%")))
                    .GroupBy(i => new
                    {
                        i.ItemId,
                        i.ItemCode,
                        i.ItemName,
                        i.Rate,
                        i.MeasureUnit,
                        i.HSNCode,
                        i.SACCode,
                        i.CGST,
                        i.SGST,
                        i.IGST,
                        i.LabourPrice,
                        i.IsLabourItem
                    })
                    .Select(g => new ItemVM
                    {
                        ItemId = g.Key.ItemId,
                        ItemCode = g.Key.ItemCode,
                        ItemName = g.Key.ItemName,
                        Rate = g.Key.Rate,
                        MeasureUnit = g.Key.MeasureUnit,
                        HSNCode = g.Key.HSNCode,
                        SACCode = g.Key.SACCode,
                        CGST = g.Key.CGST,
                        SGST = g.Key.SGST,
                        IGST = g.Key.IGST,
                        LabourPrice = g.Key.LabourPrice,
                        IsLabourItem = g.Key.IsLabourItem
                    })
                    .ToListAsync();

                return items;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error SearchOnlyComponentItems items with text='{searchText}'.");
                return Enumerable.Empty<ItemVM>();
            }
        }


        public async Task<List<AssemblyDefLabourVM>> GetBomDetailsAsync(int assmblyId)
        {
            return await _unitOfWork.AssmblyDefLabs
                .GetQueryable()
                .Include(x => x.PartItem)
                    .ThenInclude(x => x.Category)
                .Include(x => x.Process)
                .Include(x => x.CostCenter)
                .Include(x => x.AssemblyItem)
                .Where(x => x.AssmblyID == assmblyId)
                .Select(x => new AssemblyDefLabourVM
                {
                    Id = x.Id,
                    AssmblyID = x.AssmblyID,

                    ItemId = x.ItemId,
                    PartItemCode = x.PartItem.ItemCode,
                    PartItemName = x.PartItem.ItemName,
                    UOM = x.PartItem.MeasureUnit,
                    HSNCode = x.PartItem.HSNCode,
                    ItemType = x.PartItem.BtOtorMfg,
                    CategoryName = x.PartItem.Category.CategoryName,
                    RackNo = x.PartItem.RackNo,

                    UtilQty = x.UtilQty,
                    BalQty = x.BalQty,
                    

                    CostId = x.CostId,
                    CostCenterName = x.CostCenter.ProjectNo,

                    ProcessId = x.ProcessId,
                    ProcessName = x.Process.ProcessName,

                    UnitPrice = x.UnitPrice,
                    MaterialCost = x.MaterialCost,
                    LaborCost = x.LaborCost,
                    TotalCost = x.TotalCost,
                    AssemblyCost = x.AssemblyCost,

                    Remarks = x.Remarks,
                    Cancel = x.Cancel,
                    IsComponent = x.IsComponent,
                    IsLabourMaterial = x.IsLabourMaterial,

                    CreatedBy = x.CreatedBy,
                    CreatedDate = x.CreatedDate,
                    ModifiedBy = x.ModifiedBy,
                    ModifiedDate = x.ModifiedDate
                })
                .ToListAsync();
        }
        //---sns
        public async Task<int> GetMaxSlNoByAssemblyIdAsync(int assemblyId)
        {
            return await _unitOfWork.AssmblyDefLabs.GetQueryable()
                .Where(a => a.AssmblyID == assemblyId)
                .Select(a => (int?)a.SlNo)
                .MaxAsync() ?? 0;
        }

        public async Task<AssemblyDefLabour> GetByAssemblyAndItemAsync(int assemblyId, int itemId)
        {
            return await _unitOfWork.AssmblyDefLabs
                .FirstOrDefaultAsync(a => a.AssmblyID == assemblyId && a.ItemId == itemId);
        }




    }
}
