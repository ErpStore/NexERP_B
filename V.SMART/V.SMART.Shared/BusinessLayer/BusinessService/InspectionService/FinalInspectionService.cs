using AutoMapper;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using ExcelDataReader.Log.Logger;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInspectionService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISalesService;
using V.SMART.Shared.Data.Inspection.FinalInspection;
using V.SMART.Shared.Data.Inspection.MasterInspection;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.Data.OutSourcing.PurchasePo;
using V.SMART.Shared.Data.SalesAndLabour.SalesDC;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.InspectionViewModel.FinalInspectionVM;
using V.SMART.Shared.ViewModels.InspectionViewModel.MasterInspectionVM;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesDCVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.InspectionService
{
    public class FinalInspectionService : IFinalInspectionService
    {
        #region objects and constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        private readonly IMfgDcService _mfgDcService;

        public FinalInspectionService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            CurrentUserService userService,
            ILoggingService logs,
            IMapper mapper,
            IMfgDcService mfgDcService)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _logs = logs;
            _mapper = mapper;
            _mfgDcService = mfgDcService;
        }
        #endregion

        // Get All Instruments
        public async Task<List<ItemVM>> GetAllInstrumentsAsync()
            => await _commonService.GetAllActiveItemsAsync();

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

        #region  Get All Get Inspection RowVMs Async
        public async Task<List<InspectionRowVM>> GetInspectionRowVMsAsync(int ItemId)
        {
            try
            {
                var Dimensions = await _commonService.GetInspectionRowVMsAsync(ItemId, 0);
                if (Dimensions == null || (Dimensions.Count == 0))
                {
                    Dimensions = await _commonService.GetFinalInspectionRowVMsAsync(ItemId);
                }
                return Dimensions;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "GetInspectionRowVMsAsync");
                return new List<InspectionRowVM>();
            }
        }
        #endregion

        #region Get All DcDetails Async
        public async Task<IEnumerable<MfgDcVM>> GetAllDcDetailsAsync()
        {
            try
            {
                var entities = await _unitOfWork.MfgDcs.GetQueryable()
                    .Where(dc => dc.MfgDcSubs.Any(sub => sub.InspectId == null))
                    .Select(dc => new MfgDc
                    {
                        DcId = dc.DcId,
                        DcNo = dc.DcNo,
                        Customer = dc.Customer,
                        DcDate = dc.DcDate,
                        CustId=dc.CustId,
                        // 🔥 ONLY subs without inspection
                        MfgDcSubs = dc.MfgDcSubs
                            .Where(sub => sub.InspectId == null)
                            .Select(sub => new MfgDcSub
                            {
                                DcSubId = sub.DcSubId,
                                DcId=sub.DcId,
                                Item = sub.Item,
                                ItemId = sub.ItemId,
                                RefPoNo = sub.RefPoNo,
                                Qty = sub.Qty,
                                InspectId = sub.InspectId
                            }).ToList()
                    })
                    .ToListAsync();

                return _mapper.Map<IEnumerable<MfgDcVM>>(entities);

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "GetAllDcDetailsAsync");
                return Enumerable.Empty<MfgDcVM>();
            }
        }
        #endregion

        #region Generating new Inspection Number
        public async Task<String> GenerateNewInspectionNoAsync(String Screen)
        {
            try
            {

                var latestInspection = "";
                if (Screen == "Random")
                {
                     latestInspection = await _unitOfWork.FinalInspections.GetQueryable()
                                                            .OrderByDescending(i => i.Id)
                                                            .Where(i=>i.IsRandom==true)
                                                            .Select(i => i.InspectNo)
                                                            .FirstOrDefaultAsync();
                }
                else
                {
                    latestInspection = await _unitOfWork.FinalInspections.GetQueryable()
                                                            .OrderByDescending(i => i.Id)
                                                            .Where(i => i.IsRandom == false)
                                                            .Select(i => i.InspectNo)
                                                            .FirstOrDefaultAsync();
                }
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

        #region Saving Final-Inspection
        public async Task<FinalInspectionVM> UpsertInspectAsync(FinalInspectionVM inspectVM)
        {
            if (inspectVM == null)
                throw new ArgumentNullException(nameof(inspectVM));

            //It is Required When the same DcNo and same ItemId
            var DcSubId= inspectVM.RefMfgDcSubID;
            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();

            try
            {
                FinalInspection entity;

                if (inspectVM.Id == 0)
                {
                    entity = _mapper.Map<FinalInspection>(inspectVM);
                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;

                    await _unitOfWork.FinalInspections.CreateAsync(entity);
                }
                else
                {
                    entity = await _unitOfWork.FinalInspections
                        .GetQueryable()
                        .FirstOrDefaultAsync(x => x.Id == inspectVM.Id)
                        ?? throw new InvalidOperationException("Inspection not found.");

                    _mapper.Map(inspectVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;
                }

                //Updating Dimensions in Reference FinalInspection table

                var refEntity = await _unitOfWork.FinalInspectionRefs
                    .GetQueryable()
                    .FirstOrDefaultAsync(x => x.ItemID == inspectVM.ItemId);

                if (refEntity == null)
                {
                    refEntity = new FinalInspectionRef
                    {
                        ItemID = inspectVM.ItemId!.Value,
                        InsRefData = inspectVM.InspectData,
                        TotRows = (short)inspectVM.TotRows
                    };

                    await _unitOfWork.FinalInspectionRefs.CreateAsync(refEntity);
                }
                else
                {
                    refEntity.InsRefData = inspectVM.InspectData;
                    refEntity.TotRows = (short)inspectVM.TotRows;
                }

                await _unitOfWork.SaveAsync();

                var savedEntity = await _unitOfWork.FinalInspections
                    .GetQueryable()
                    .Include(x => x.Item)
                    .Include(x => x.Customer)
                    .FirstAsync(x => x.Id == entity.Id);

                //Updating the Inspection No. in MfgDc
                if (savedEntity.DcType == "Manufacturing")
                {
                    //Joining the table mfgdc and mfgdcsub to fetch mfgdcsub based on dc no.
                    var mfgdcSub = await _unitOfWork.MfgDcSubs.GetQueryable()
                                           .Where(i => i.DcSubId == DcSubId).FirstOrDefaultAsync();
                    if (mfgdcSub != null)
                    {
                        mfgdcSub.InspectId = savedEntity.Id;
                        await _unitOfWork.SaveAsync();
                    }
                }

                return _mapper.Map<FinalInspectionVM>(savedEntity);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Failed to save inspection");
                throw new InvalidOperationException("Failed to save inspection. Please try again.");
            }
        }

        #endregion

        #region Get Inspection By Id
        public async Task<FinalInspectionVM?> GetInspectionByIdAsync(int inspectionId)
        {
            try
            {
                var inspectionEntity = await _unitOfWork.FinalInspections
                    .GetQueryable()
                    .Include(x => x.Customer)
                    .Include(x => x.Item)
                    .FirstOrDefaultAsync(x => x.Id == inspectionId);

                if (inspectionEntity == null)
                {
                    return null;
                }
                var inspectionVM = _mapper.Map<FinalInspectionVM>(inspectionEntity);
                return inspectionVM;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetInspectionByIdAsync for ID : {inspectionId}");
                return null;
            }
        }
        #endregion

        #region Delete Inspection By Id
        public async Task<bool> DeleteInspectionByIdAsync(int inspectionId)
        {
            try
            {
                var inspectionEntity = await _unitOfWork.FinalInspections
                  .GetQueryable()
                  .FirstOrDefaultAsync(x => x.Id == inspectionId);

                if (inspectionEntity == null)
                    return false;

                // STEP 1: Find all child records referencing this inspection
                var dcSubs = await _unitOfWork.MfgDcSubs
                    .GetQueryable()
                    .Where(p => p.InspectId == inspectionId)
                    .ToListAsync();

                // STEP 2: Remove the FK reference (unlink)
                foreach (var sub in dcSubs)
                {
                    sub.InspectId = null;
                }

                // Save FK updates FIRST
                await _unitOfWork.SaveAsync();

                // STEP 3: Now delete the inspection safely
                await _unitOfWork.FinalInspections.DeleteAsync(inspectionEntity);
                await _unitOfWork.SaveAsync();

                return true;

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in DeleteInspectionByIdAsync for ID : {inspectionId} in Final Inspection");
                return false;
            }
        }
        #endregion

        #region To Check Duplicate Inspection
        public async Task<bool> CheckDuplicateRandom(string inspNo,string screen,string suffix)
        {
            try
            {
                inspNo = inspNo?.Trim();
                screen = screen?.Trim();

                if (string.IsNullOrWhiteSpace(inspNo))
                    return false;

                var query = _unitOfWork.FinalInspections.GetQueryable()
                    .Where(i => i.InspectNo != null);

                if (screen.Equals("Individual", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(i =>
                        i.InspectNo.Trim() == inspNo && i.Suffix== suffix &&
                        i.IsRandom == false);
                }
                else
                {
                    query = query.Where(i =>
                        i.InspectNo.Trim() == inspNo && i.Suffix == suffix &&
                        i.IsRandom == true);
                }
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CheckDuplicateRandom() in Final Inspection Service");
                return false;
            }
        }
        #endregion

        #region Instant Seach
        public async Task<(List<FinalInspectionVM> FinalInspectionVMs, int TotalCount)>
         SearchFinalInspectionAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.FinalInspections
                    .GetQueryable()
                    .Include(x => x.Customer)
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

                var vmList = _mapper.Map<List<FinalInspectionVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,
                    "Error in SearchFinalInspectionAsync");
                throw new InvalidOperationException("Failed to load final inspection list.", ex);
            }
        }

        public static class FinalInspectionFilterBuilder
        {
            public static IQueryable<FinalInspection> ApplyFilter(
                IQueryable<FinalInspection> query,
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
                        case "ScreenType":
                            return ScreenTypeFilter(query, val);

                        case "DcType":
                            return InspectionTypeFilter(query, val);

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
                                (string.IsNullOrEmpty(part2) || (x.Suffix != null && x.Suffix.Contains(part2)))
                            );
                        

                        case "Customer":
                            return query.Where(x =>
                                x.Customer != null &&
                                x.Customer.CustName.Contains(val));

                        case "ItemName":
                            return query.Where(x =>
                                x.Item != null &&
                                x.Item.ItemName.Contains(val));

                        case "ItemCode":
                            return query.Where(x =>
                                x.Item != null &&
                                x.Item.ItemCode.Contains(val));

                        case "DCNo":
                            return query.Where(x =>
                             x.DCNo != null &&
                             x.DCNo.Contains(val));

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

        private static IQueryable<FinalInspection> InspectionTypeFilter(
              IQueryable<FinalInspection> query, string status)
        {
            return status switch
            {
                "Random Inspection" => query.Where(x => x.IsRandom == true),
                "Individual Inspection" => query.Where(x => x.IsRandom == false),
                _ => query
            };
        }

        private static IQueryable<FinalInspection> ScreenTypeFilter(
           IQueryable<FinalInspection> query, string status)
        {
            return status switch
            {
                "Manufacturing" => query.Where(x => x.DcType == "Manufacturing"),
                "Labour" => query.Where(x => x.DcType == "Labour"),
                _ => query
            };
        }
        #endregion

        #region Clearing the All Values in FinalInspection
        public async Task<FinalInspectionVM> ClearInspection(FinalInspectionVM FinalInspectionVM)
        {
            try
            {
                FinalInspectionVM.DCNo = string.Empty;
                FinalInspectionVM.PONo = String.Empty;
                FinalInspectionVM.CustomerName = string.Empty;
                FinalInspectionVM.CustId = null;
                FinalInspectionVM.ItemId = null;
                FinalInspectionVM.ItemCode = string.Empty;
                FinalInspectionVM.ItemName = string.Empty;
                FinalInspectionVM.CostCenter = string.Empty;
                FinalInspectionVM.Qty = null;
                FinalInspectionVM.AcceptQty = null;
                FinalInspectionVM.RejQty = null;
                FinalInspectionVM.ReWork = null;
                FinalInspectionVM.SheetSlNo = 0;
                FinalInspectionVM.SheetName = string.Empty;
                FinalInspectionVM.ItemUOM = string.Empty;
                return FinalInspectionVM;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error Loading in ClearInspection() in FinalInspection Service");
                return new FinalInspectionVM();
            }
        }
        #endregion

    }
}
