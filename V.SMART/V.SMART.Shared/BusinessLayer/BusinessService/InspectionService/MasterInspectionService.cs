using AutoMapper;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInspectionService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISalesService;
using V.SMART.Shared.Data.Inspection.FinalInspection;
using V.SMART.Shared.Data.Inspection.MasterInspection;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.InspectionViewModel.FinalInspectionVM;
using V.SMART.Shared.ViewModels.InspectionViewModel.MasterInspectionVM;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesDCVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static MudBlazor.Icons;

namespace V.SMART.Shared.BusinessLayer.BusinessService.InspectionService
{
    public class MasterInspectionService : IMasterInspectionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        
        public MasterInspectionService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            CurrentUserService userService,
            ILoggingService logs,
            IMapper mapper,
            ILoggingService js)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _logs = logs;
            _mapper = mapper;
        }

        // Get All Instruments
        public async Task<List<ItemVM>> GetAllInstrumentsAsync()
            => await _commonService.GetAllItemsByCategoryCode(2);

        // Get All Get No Of Samples
        public async Task<int> GetNoOfSamples()
            => await _commonService.GetNoOfSamples();

        // Get All Get All Active Customers
        public async Task<List<CustomerVM>> GetAllActiveCustomersAsync()
            => await _commonService.GetAllActiveCustomersAsync();

        // Get All Active Defects
        public async Task<List<DefectInfo>> GetAllActiveDefectsAsync()
            => await _commonService.GetAllActiveDefectsAsync();

        // Get All All Cost Center Details
        public async Task<List<CostCenterVM>> GetAllCostCenterDetails()
            => await _commonService.GetAllCostCenterDetails();

        // Get All Components
        public async Task<List<ItemVM>> GetAllComponentsAsync()
            => await _commonService.GetAllItemsByCategoryCode(2);

        // Get All Staff
        public async Task<List<Staff>> GetAllStaffAsync()
            => await _commonService.GetAllStaffAsync();

        // Get All Get Inspection RowVMs Async
        public async Task<List<InspectionRowVM>> GetInspectionRowVMsAsync(int ItemId, int ProcessId)
            => await _commonService.GetInspectionRowVMsAsync(ItemId, ProcessId);

       public async Task<IEnumerable<Process>> GetAllProcessAsync()
            =>await _commonService.GetAllProcessAsync();

        // 🔹 Items
        public async Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText)
            => await _commonService.SearchItemsAsync(searchText);

        #region Get Inspection By Id Async
        public async Task<MasterInspectionVM?> GetInspectionByIdAsync(int inspectionId)
        {
            try
            {
                var inspectionEntity = await _unitOfWork.MasterInspections
                    .GetQueryable()
                    .Include(x => x.Process)
                    .Include(x => x.Item)
                    .FirstOrDefaultAsync(x => x.Id == inspectionId);

                if (inspectionEntity == null)
                {
                    return null;
                }
                var inspectionVM = _mapper.Map<MasterInspectionVM>(inspectionEntity);
                return inspectionVM;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetInspectionByIdAsync for ID : {inspectionId}");
                return null;
            }
        }
        #endregion

        #region Generate New InspectionNo Async
        public async Task<String> GenerateNewInspectionNoAsync()
        {
            try
            {
                var latestInspection = "";
               
                    latestInspection = await _unitOfWork.MasterInspections.GetQueryable()
                                                           .OrderByDescending(i => i.Id)
                                                           .Select(i => i.InspectNo)
                                                           .FirstOrDefaultAsync();
                if (latestInspection != null)
                {
                    var serialNo = int.Parse(latestInspection);   
                    serialNo++;
                    return serialNo.ToString();
                }
                else
                {
                    return "1";
                }
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in GenerateNewInspectionNoAsync: ");
                return ""; // Default to 1 if error occurs
            }
        }
        #endregion

        #region Upsert Inspect Async
        public async Task<MasterInspectionVM> UpsertInspectAsync(MasterInspectionVM inspectVM)
        {
            if (inspectVM == null)
                throw new ArgumentNullException(nameof(inspectVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                MasterInspection entity;

                // ================= CREATE =================
                if (inspectVM.Id == 0)
                {
                    entity = _mapper.Map<MasterInspection>(inspectVM);

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;

                    await _unitOfWork.MasterInspections.CreateAsync(entity);
                }
                // ================= UPDATE =================
                else
                {
                    entity = await _unitOfWork.MasterInspections
                        .GetQueryable()
                        .FirstOrDefaultAsync(x => x.Id == inspectVM.Id)
                        ?? throw new InvalidOperationException("Inspection not found.");

                    // Map VM → Entity (FK IDs will update)
                    _mapper.Map(inspectVM, entity);

                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;
                }

                await _unitOfWork.SaveAsync();

                // ================= INSPECTION REF =================
                var refEntity = await _unitOfWork.InspectionRefs
                    .GetQueryable()
                    .FirstOrDefaultAsync(x => x.ItemID == inspectVM.ItemId && x.ProcessId== inspectVM.processId);

                if (refEntity == null)
                {
                    // CREATE
                    refEntity = new InspectionRef
                    {
                        ItemID = inspectVM.ItemId!.Value,
                        InsRefData = inspectVM.InspectData,
                        TotRows = (short)inspectVM.TotRows,
                        ProcessId= inspectVM.processId??0,
                    };

                    await _unitOfWork.InspectionRefs.CreateAsync(refEntity);
                }
                else
                {
                    // UPDATE
                    refEntity.InsRefData = inspectVM.InspectData;
                    refEntity.TotRows = (short)inspectVM.TotRows;
                }

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Reload with navigation properties
                var savedEntity = await _unitOfWork.MasterInspections
                    .GetQueryable()
                    .Include(x => x.Item)
                    .Include(x => x.Process)
                    .FirstAsync(x => x.Id == entity.Id);

                return _mapper.Map<MasterInspectionVM>(savedEntity);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "Failed to save inspection");
                throw new InvalidOperationException("Failed to save inspection. Please try again.");
            }
        }
        #endregion

        #region Delete Inspection By IdAsync
        public async Task<bool> DeleteInspectionByIdAsync(int inspectionId)
        {
            try
            {
                var inspectionEntity = await _unitOfWork.MasterInspections
                    .GetQueryable()
                    .FirstOrDefaultAsync(x => x.Id == inspectionId);
                if (inspectionEntity == null)
                {
                    return false; // Not found
                }
               
                await _unitOfWork.MasterInspections.DeleteAsync(inspectionEntity);

                //Deleting the Related Dimensions in InspectionRef

                var InspRef=await _unitOfWork.InspectionRefs.GetQueryable()
                           .Where(x=>x.ItemID== inspectionEntity.ItemId && x.ProcessId== inspectionEntity.ProcessId)
                           .FirstOrDefaultAsync();
                if (InspRef != null)
                {
                    await _unitOfWork.InspectionRefs.DeleteAsync(InspRef);
                }
                await _unitOfWork.SaveAsync();
                 return true;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in DeleteInspectionByIdAsync for ID : {inspectionId}");
                return false;
            }
        }
        #endregion

        #region Instant Seach
        public async Task<(List<MasterInspectionVM> FinalInspectionVMs, int TotalCount)>
         SearchFinalInspectionAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.MasterInspections
                    .GetQueryable()
                    .Include(x => x.Item)
                    .AsQueryable();

                // Apply dynamic filters
                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        query = FinalInspectionFilterBuilder.ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.Id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<MasterInspectionVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,
                    "Error in SearchFinalInspectionAsync");
                throw new InvalidOperationException("Failed to load Master inspection list.", ex);
            }
        }

        public static class FinalInspectionFilterBuilder
        {
            public static IQueryable<MasterInspection> ApplyFilter(
                IQueryable<MasterInspection> query,
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
                        case "InspectNo":
                            string part1 = val;
                            string part2 = string.Empty;

                            int slashIndex = val.IndexOf('/');
                            if (slashIndex > -1)
                            {
                                part1 = val[..slashIndex].Trim();
                                part2 = val[(slashIndex + 1)..].Trim();
                            }

                            return query.Where(x => (string.IsNullOrEmpty(part1) || x.InspectNo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || (x != null && x.Suffix.Contains(part2)))
                            );

                        case "ItemName":
                            return query.Where(x =>
                                x.Item != null &&
                                x.Item.ItemName.Contains(val));

                        case "ItemCode":
                            return query.Where(x =>
                                x.Item != null &&
                                x.Item.ItemCode.Contains(val));

                        case "CreatedBy":
                            return query.Where(x =>
                                x.CreatedBy != null &&
                                x.CreatedBy.Contains(val));

                        case "FromDate":
                            if (DateTime.TryParse(val, out var fromDate))
                                return query.Where(x => x.InspectDate >= fromDate.Date);
                            return query;

                        case "ToDate":
                            if (DateTime.TryParse(val, out var toDate))
                                return query.Where(x =>
                                    x.InspectDate <= toDate.Date.AddDays(1).AddTicks(-1));
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
        #endregion

    }
}
