using AutoMapper;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Pkcs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IPlanningService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Planning.Estimaton;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.PlanningViewModel.EstimationViewModel;
using Microsoft.Data.SqlClient;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using V.SMART.Shared.Data.Master.Inventory;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Vml.Office;

namespace V.SMART.Shared.BusinessLayer.BusinessService.PlanningService
{
    public class EstimateService : IEstimateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly ICommonService _commonService;
        private readonly IMapper _mapper;
        private readonly IExcelTemplateService _excelTemplateService;
        private readonly IReportExecutor _report;

        public EstimateService(IUnitOfWork unitOfWork,
            ILoggingService loggingService, CurrentUserService currentUserService,
            ICommonService commonService, IMapper mapper, IExcelTemplateService excelTemplateService, IReportExecutor report)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
            _commonService = commonService;
            _mapper = mapper;
            _excelTemplateService = excelTemplateService;
            _report = report;
        }
        public async Task<int> GetDecimalPlacesAsync()
            => await _commonService.GetDecimalPlacesAsync();

        public async Task<CustomerVM?> GetCustomerByIdAsync(int custId)
            => await _commonService.GetCustomerByIdAsync(custId);

        public async Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText)
              => await _commonService.SearchCustomersAsync(searchText);
        public async Task<List<ContactPerson>> GetContactPersonsAsync(int custId)
            => await _commonService.GetContactPersonsAsync(custId);

        public async Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText)
            => await _commonService.SearchItemsAsync(searchText);

        public async Task<ItemVM?> GetItemByItemIdAsync(int? itemId)
            => await _commonService.GetItemByItemIdAsync(itemId);
        public async Task<List<CostCenterVM>> GetCostCenterDetailsByCustId(int custId, HashSet<int> usedCostCenterIds)
            => await _commonService.GetCostCenterDetailsByCustId(custId, usedCostCenterIds);

        public async Task<(List<EstimateVM> EstimateVM, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.Estimates
                    .GetQueryable()
                    .Include(x => x.Customer)
                     .Include(x => x.Item)
                    .AsNoTracking();

                
                if (filters != null && filters.Any())
                {
                    foreach (var filter in filters)
                    {
                        query = EstimationFilterBuilder
                            .ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.EstiamateId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
               

                var vmList = _mapper.Map<List<EstimateVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync (Customer)");
                throw new InvalidOperationException("Failed to load customer list.", ex);
            }
        }
        public static class EstimationFilterBuilder
        {
            public static IQueryable<Estimate> ApplyFilter(
                IQueryable<Estimate> query,
                string field,
                object value)
            {
                if (value == null) return query;

                var val = value.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(val))
                    return query;

                switch (field)
                {
                    //case "EstiamateNo":
                    //    return query.Where(x => x.Estimate.EstiamateNo.Contains(val));

                    case "Customer":
                        return query.Where(x => x.Customer.CustName.Contains(val));

                    case "CreatedBy":
                        return query.Where(x =>
                            x.CreatedBy != null &&
                            EF.Functions.Like(x.CreatedBy, $"%{val}%"));



                    default:
                        return query;
                }
            }


        }
      

        public async Task<List<EstimateSubVM>> GetItemProcessList(int itemId, int estimateId)
        {
            var processFlows = await _unitOfWork.ProcessFlowCharts.GetAllAsync();
            var processes = await _unitOfWork.Processes.GetAllAsync();
            var estimateSubs = await _unitOfWork.EstimateSubs.GetAllAsync();
            var estimate = await _unitOfWork.Estimates.GetAllAsync();

            // Check whether Estimate has saved processes
            bool hasEstimate = estimateSubs.Any(x => x.EstiamateId == estimateId);

            if (hasEstimate)
            {
                // Edit Mode
                return (from p in processes
                        join es in estimateSubs
                            on p.ProcessId equals es.ProcessId
                        where es.EstiamateId == estimateId
                        join pf in processFlows
                            on new { ProcId = p.ProcessId, CompItemId = itemId }
                            equals new { ProcId = pf.ProcId, CompItemId = pf.CompItemId }
                            into pfGroup
                        from pf in pfGroup.DefaultIfEmpty()
                        select new EstimateSubVM
                        {
                            EstiamateId = es.EstiamateId,
                            ProcessId = p.ProcessId,
                            ProcessName = p.ProcessName,
                            HrlyRate = Convert.ToDecimal(p.HrlyRate),
                            Hrs = es.Hrs,
                            Amount = es.Amount
                        }).ToList();

              
            }
            else
            {
                // New Mode
                return (processFlows.Any(pf => pf.CompItemId == itemId)
                        ? from pf in processFlows
                          join p in processes
                              on pf.ProcId equals p.ProcessId
                          join es in estimateSubs
                              on p.ProcessId equals es.ProcessId into esGroup
                          from es in esGroup.DefaultIfEmpty()
                          where pf.CompItemId == itemId
                          select new EstimateSubVM
                          {
                              EstiamateId = 0,
                              ProcessId = p.ProcessId,
                              ProcessName = p.ProcessName,
                              HrlyRate = Convert.ToDecimal(p.HrlyRate),
                              Hrs = (decimal)pf.CycleTime.TotalMinutes,
                              Amount = ((decimal)pf.CycleTime.TotalMinutes / 60m) * Convert.ToDecimal(p.HrlyRate)
                          }
                        : from est in estimate
                          where est.ItemId == itemId
                          join es in estimateSubs
                              on est.EstiamateId equals es.EstiamateId
                          join p in processes
                              on es.ProcessId equals p.ProcessId
                          select new EstimateSubVM
                          {
                              EstiamateId = 0,
                              ProcessId = p.ProcessId,
                              ProcessName = p.ProcessName,
                              HrlyRate = es.HrlyRate,//Convert.ToDecimal(p.HrlyRate),
                              Hrs = es.Hrs,
                              Amount = es.Amount
                          }
                       )
                       .DistinctBy(x => x.ProcessName)
                       .ToList();

               
            }
        }
        public async Task<List<Process>> GetAllProcessAsync()
            => (await _commonService.GetAllProcessAsync()).ToList();


        public async Task<List<EstimateVM>> GetItemDetails(int itemId)
        {
            try
            {
                var item = await _unitOfWork.ItemRepositories
                    .FirstOrDefaultAsync(x => x.ItemId == itemId);
                var EstimateItem = await _unitOfWork.Estimates.GetQueryable().Where(x => x.ItemId == itemId).OrderByDescending(x => x.EstiamateId).FirstOrDefaultAsync();

                //var EstimateItem = await _unitOfWork.Estimates.FirstOrDefaultAsync(x => x.ItemId == itemId).OrderByDescending(x => x.EstimateId).FirstOrDefaultAsync(); 

                if (item == null)
                    return new List<EstimateVM>();

                var result = new List<EstimateVM>();

                if (item != null)
                {
                    return new List<EstimateVM>
                        {
                            new EstimateVM
                            {
                                ItemId = item?.ItemId ?? EstimateItem?.ItemId,

                                    Shape = !string.IsNullOrWhiteSpace(item?.Shape)
                                                ? item.Shape
                                                : EstimateItem?.Shape,

                                        Density = item.Density != 0 ? item.Density : EstimateItem?.Density ?? 0,
                                        Length = item.Length != 0 ? item.Length : EstimateItem?.Length ?? 0,
                                        Width = item.Width != 0 ? item.Width : EstimateItem?.Width ?? 0,
                                        Height = item.Height != 0 ? item.Height : EstimateItem?.Height ?? 0,
                                        Thickness = item.Thickness != 0 ? item.Thickness : EstimateItem?.Thickness ?? 0,
                                        WallThickness = item.WallThickness != 0 ? item.WallThickness : EstimateItem?.WallThickness ?? 0,
                                        OuterDia = item.OuterDia != 0 ? item.OuterDia : EstimateItem?.OuterDia ?? 0,
                                        InnerDia = item.InnerDia != 0 ? item.InnerDia : EstimateItem?.InnerDia ?? 0,
                                        Weight = item.Weight != 0 ? item.Weight : EstimateItem?.Weight ?? 0,

                                        //Indirect heads
                                         Overp = EstimateItem?.Overp ?? 0,
                                         Over = EstimateItem?.Over ?? 0,
                                         Marp = EstimateItem?.Marp ?? 0,
                                         Mar = EstimateItem?.Mar ?? 0,
                                         Riskp = EstimateItem?.Riskp ?? 0,
                                         Risk = EstimateItem?.Risk ?? 0,
                                         Packingper = EstimateItem?.Packingper ?? 0,
                                         Packing = EstimateItem?.Packing ?? 0,
                                         Logesticper = EstimateItem?.Logesticper ?? 0,
                                         Logestic = EstimateItem?.Logestic ?? 0,
                                        

                            }
                        };
                }
                else if(EstimateItem != null)
                {
                    return new List<EstimateVM>
                        {
                            new EstimateVM
                            {
                                ItemId = EstimateItem.ItemId,
                                Shape = EstimateItem.Shape,
                                Density = item.Density,
                                Length = EstimateItem.Length,
                                Width = EstimateItem.Width,
                                Height = EstimateItem.Height,
                                Thickness = EstimateItem.Thickness,
                                WallThickness = EstimateItem.WallThickness,
                                OuterDia = EstimateItem.OuterDia,
                                InnerDia = EstimateItem.InnerDia,
                                Weight = EstimateItem.Weight
                            }
                        };
                }

                return result;

            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error getting item details.");
                throw;
            }
        }

        public async Task<List<ItemVM>> GetAllActiveItemsAsync()//All Items
        {
            try
            {

                var Items = await _unitOfWork.ItemRepositories.GetAllAsync();
                return _mapper.Map<List<ItemVM>>(Items);
            }
            catch (Exception ex)
            {

                await _loggingService.LogDeveloperError(ex, "Error fetching active vendors.");
                return new List<ItemVM>();
            }
        }
        public async Task<EstimateVM> Calculate(EstimateVM vm)
        {
            try
            {
                //15-07-2026

                decimal D = vm.Density ?? 0m;
                decimal L = vm.Length ?? 0m;
                decimal W = vm.Width ?? 0m;
                decimal H = vm.Height ?? 0m;
                decimal T = vm.Thickness ?? 0m;
                decimal WT = vm.WallThickness ?? 0m;
                decimal OD = vm.OuterDia ?? 0m;
                decimal ID = vm.InnerDia ?? 0m;

                decimal weight = vm.Weight ?? 0m;
                decimal RmPrice = vm.RmPrice ?? 0m;
                decimal RmTotAmount = 0;

                decimal CastAmt = vm.CastAmt ?? 0m;

                const decimal PI = 3.14159265358979323846M;

                switch (vm.Shape)
                {
                    case "Round":
                        weight = (PI * 0.25M * OD * OD * L * D) / 1_000_000M;
                        break;

                    case "Square Plate":
                        weight = (L * W * T * D) / 1_000_000M; // Corrected formula
                        break;

                    case "Rectangle Plate":
                    case "Sheet":
                        weight = (L * W * T * D) / 1_000_000M;
                        break;

                    case "Round Tube":
                        decimal roundTubeInner = OD - (2 * WT);
                        if (roundTubeInner < 0) roundTubeInner = 0;

                        weight = (PI * 0.25M *
                                 (OD * OD - roundTubeInner * roundTubeInner) *
                                 L * D) / 1_000_000M;
                        break;

                    case "Square Tube":
                        decimal squareTubeInner = W - (2 * WT);
                        if (squareTubeInner < 0) squareTubeInner = 0;

                        weight = (L * (W * W - squareTubeInner * squareTubeInner) * D) / 1_000_000M;
                        break;

                    case "Rectangle Tube":
                        decimal innerWidth = W - (2 * WT);
                        decimal innerHeight = H - (2 * WT);

                        if (innerWidth < 0) innerWidth = 0;
                        if (innerHeight < 0) innerHeight = 0;

                        weight = (L * ((W * H) - (innerWidth * innerHeight)) * D) / 1_000_000M;
                        break;

                    case "L Angle":
                        weight = (L * ((W * WT) + (H * WT)) * D) / 1_000_000M;
                        break;

                    case "Tee":
                    case "Beam":
                        weight = (L * (W * WT + (T * WT)) * D) / 1_000_000M;
                        break;

                    case "Ring":
                        weight = (PI * 0.25M *
                                 (OD * OD - ID * ID) *
                                 L * D) / 1_000_000M;
                        break;

                    case "Hexagonal Plate":
                        weight = (2.598M * OD * OD * T * D) / 1_000_000M;
                        break;

                    case "Octagonal":
                        decimal sqrt2 = 1.41421356237M;
                        weight = ((2 * (1 + sqrt2)) * OD * OD * T * L * D) / 1_000_000M;
                        break;

                    case "U or C Channel":
                    case "I or H Channel":
                        decimal web = (H - 2 * WT) * WT;
                        if (web < 0) web = 0;

                        decimal flanges = 2 * (W * WT);
                        weight = (L * (web + flanges) * D) / 1_000_000M;
                        break;

                    
                }

                //RmWieght
                vm.Weight = Math.Round(weight, 3);

                //RmTotAmount                  RmWieght          RmPrice
                vm.RmTotAmount = Math.Round(((vm.Weight ?? 0) * (vm.RmPrice ?? 0)), 3);

                //TotlCost means RawMaterial Cost and ProcessCost
                decimal totalCost = ((vm.RmTotAmount ?? 0) + (vm.Conv ?? 0));
                vm.Totalcost = totalCost;

                //OTHER HEADS -  (O)
                //Overp% = Over
                decimal overp = ((vm.Totalcost ?? 0) * (vm.Overp ?? 0)) / 100;
                vm.Over = overp;

                //Margin %
                decimal margin = ((vm.Totalcost ?? 0) * (vm.Marp ?? 0)) / 100;
                vm.Mar = margin;

                //Risk Factor %
                decimal riskfactor = ((vm.Totalcost ?? 0) * (vm.Riskp ?? 0)) / 100;
                vm.Risk = riskfactor;

                //Packing %
                decimal Packing = ((vm.Totalcost ?? 0) * (vm.Packingper ?? 0)) / 100;
                vm.Packing = Packing;

                //Logistics % 
                decimal Logistic = ((vm.Totalcost ?? 0) * (vm.Logesticper ?? 0)) / 100;
                vm.Logestic = Logistic;

                ////Total Grand Total 
                // decimal GrandTotal = (vm.Over ?? 0 + vm.Mar ?? 0 + vm.Risk ?? 0 + vm.Packing ?? 0 + vm.Logestic ?? 0 + vm.Totalcost ?? 0);
                decimal GrandTotal = (overp + margin + riskfactor + Packing + Logistic + totalCost + CastAmt);
                vm.Grand = Math.Round(GrandTotal, 3);

                return vm;



                //

               // //FirstLine    RM ThickNess.         RM Width            RM Len.              Density
               // decimal tot = (vm.RmthSQR ?? 0) * (vm.RmwdSQR ?? 0) * (vm.RmlenSQR ?? 0) * (vm.DenSQR ?? 0) / 1000000;
               // vm.RmkgSQR = Math.Round(tot, 2);//firstLine Rawmaterial Kg
               // //firstLine RmCost              RM Kg              RM rate
               // vm.RmcostSQR = Math.Round(((vm.RmkgSQR ?? 0) * (vm.RmrateSQR ?? 0)), 3);

               // //SecondLine
               // //tot1 = (((3.14 * Rmdia * Rmdia) / 4) * Rm_len * Density) / 1000000;

               // //SecondLine                         //RM Dia             //RM Dia            RM Len.           Rm Density
               // decimal Secondlietot = (((3.14m * (vm.RmdiaRND ?? 0) * (vm.RmdiaRND ?? 0)) / 4 * (vm.RmlenRND ?? 0) * (vm.DensityRND ?? 0)) / 1000000);
               // vm.RmkgRND = Math.Round(Secondlietot, 2);//SecondLine Rawmaterial Kg
               // //SecondLine RmCost         RM Kg              RM Rate
               // vm.RmcostRND = Math.Round(((vm.RmkgRND ?? 0) * (vm.RmrateRND ?? 0)), 3);


               // //Both RawMaterial Cost
               // decimal bothrawMaterilCost = (vm.RmcostSQR ?? 0) + (vm.RmcostRND ?? 0);
               // vm.totalRmcost = bothrawMaterilCost;

               // //SubTot(); //Important for sub part Calculation Grid

               // //TotlCost means RawMaterial Cost and ProcessCost
               // decimal totalCost = ((vm.totalRmcost ?? 0) + (vm.Conv ?? 0));
               // vm.Totalcost = totalCost;

               // //OTHER HEADS -  (O)
               // //Overp% = Over
               // decimal overp = ((vm.Totalcost ?? 0) * (vm.Overp ?? 0)) / 100;
               // vm.Over = overp;

               // //Margin %
               // decimal margin = ((vm.Totalcost ?? 0) * (vm.Marp ?? 0)) / 100;
               // vm.Mar = margin;

               // //Risk Factor %
               // decimal riskfactor = ((vm.Totalcost ?? 0) * (vm.Riskp ?? 0)) / 100;
               // vm.Risk = riskfactor;

               // //Packing %
               // decimal Packing = ((vm.Totalcost ?? 0) * (vm.Packingper ?? 0)) / 100;
               // vm.Packing = Packing;

               // //Logistics % 
               // decimal Logistic = ((vm.Totalcost ?? 0) * (vm.Logesticper ?? 0)) / 100;
               // vm.Logestic = Logistic;

               // ////Total Grand Total 
               //// decimal GrandTotal = (vm.Over ?? 0 + vm.Mar ?? 0 + vm.Risk ?? 0 + vm.Packing ?? 0 + vm.Logestic ?? 0 + vm.Totalcost ?? 0);
               // decimal GrandTotal = (overp + margin + riskfactor + Packing + Logistic  + totalCost);
               // vm.Grand = Math.Round(GrandTotal, 3);

               // return vm;
            }
            catch (Exception ex)
            {

                await _loggingService.LogDeveloperError(ex, $"Error Calulating Estimate");
                throw new InvalidOperationException("Failed to Calulating Estimate ");
            }
        }
        

        public async Task<string> GetEstimateNumberAsync(string suffix)
        {
            try
            {
                var lastEstimateNo = await _unitOfWork.Estimates
                    .GetQueryable()
                    .Where(q => q.Suffix == suffix)
                    .OrderByDescending(q => q.EstiamateId) // Better than EstiamateNo since EstiamateNo is string
                    .Select(q => q.EstiamateNo)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;

                if (!string.IsNullOrEmpty(lastEstimateNo))
                {
                    var parts = lastEstimateNo.Split('/');

                    if (int.TryParse(parts[0], out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }

                return nextNumber.ToString();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex,
                    $"Error generating Estimate number for suffix: {suffix}");

                throw new InvalidOperationException("Failed to generate Estimate number.");
            }
        }

        public async Task<EstimateVM> UpsertEstimateAsync(EstimateVM estimateVM)
        {
            if (estimateVM == null)
                throw new ArgumentNullException(nameof(estimateVM));
            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                Estimate entity;

                if (estimateVM.EstiamateId == 0)
                {
                    entity = _mapper.Map<Estimate>(estimateVM);

                    // 🔹 Get last number with locking from repository
                    var NextNumber = await _unitOfWork.Estimates.GetLastEstimateNoAsync(entity.Suffix);
                    entity.EstiamateNo = NextNumber;

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.EstimateSub = estimateVM.EstimateSubVM.Select(s => _mapper.Map<EstimateSub>(s)).ToList();

                    await _unitOfWork.Estimates.CreateAsync(entity);
                    var enqSub = await _unitOfWork.EnquirySalesSubs.GetQueryable().FirstOrDefaultAsync(i => i.EnquirySubId == entity.RefEnqSubId);

                    if (enqSub != null)
                    {
                        enqSub.IsEstiItemTally = true;
                    }
                    await _unitOfWork.SaveAsync();
                    

                }
                else
                {
                    entity = await _unitOfWork.Estimates.GetQueryable()
                        .Include(q => q.EstimateSub)
                        .FirstOrDefaultAsync(q => q.EstiamateId == estimateVM.EstiamateId)
                        ?? throw new InvalidOperationException("Estimation not found.");

                    var parentChanges = GetPropertyChanges(entity, estimateVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(estimateVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    // await SetSalesQuoteAuthorizationStatusAsync(entity, currentUser);

                    await HandleChildUpdatesAsync(entity, estimateVM.EstimateSubVM, changes);

                    changes.AppendLine("Estimation Updated.");

                }
                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();

                await LogChangesAsync(changes, estimateVM.EstiamateId == 0 ? "Estimation Created" : "Estimation Updated");

                var savedEntity = await _unitOfWork.Estimates.GetQueryable()
                    .Include(q => q.EstimateSub)
                    .Include(q => q.Customer)
                    .FirstOrDefaultAsync(q => q.EstiamateId == entity.EstiamateId);

                return _mapper.Map<EstimateVM>(savedEntity!);
            }
            catch (Exception ex)
            {

                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, $"Failed to upsert Estimation: {estimateVM.EstiamateNo}");
                throw new InvalidOperationException("Failed to save Estimation. Please try again.");
            }


        }
        private async Task HandleChildUpdatesAsync(Estimate existingEstimate, List<EstimateSubVM> incomingSubVMs, StringBuilder changes)
        {
            try
            {

                var existingSubIds = existingEstimate.EstimateSub.Select(s => s.EstiamateSubId).ToHashSet();
                var incomingSubIds = incomingSubVMs.Select(s => s.EstiamateSubId).ToHashSet();

                // DELETE removed children
                foreach (var sub in existingEstimate.EstimateSub.Where(s => !incomingSubIds.Contains(s.EstiamateSubId)).ToList())
                {
                    changes.AppendLine($"Child Deleted - EstiamateSubId: {sub.EstiamateSubId}, Process: {sub.Process?.ProcessName}");
                    await _unitOfWork.EstimateSubs.DeleteAsync(sub.EstiamateSubId);
                    await _unitOfWork.SaveAsync();

                    
                }

                // ADD or UPDATE children
                foreach (var subVM in incomingSubVMs)
                {
                    if (subVM.EstiamateSubId == 0)
                    {
                        var newSub = _mapper.Map<EstimateSub>(subVM);
                        newSub.EstiamateId = existingEstimate.EstiamateId;
                        await _unitOfWork.EstimateSubs.CreateAsync(newSub);
                        await _unitOfWork.SaveAsync();

                        changes.AppendLine($"Child Added - Process: {subVM.ProcessId}, Qty: {subVM.HrlyRate}");

                    }
                    else
                    {
                        var existingSub = existingEstimate.EstimateSub.FirstOrDefault(s => s.EstiamateSubId == subVM.EstiamateSubId);
                        if (existingSub != null)
                        {
                            



                            var subChanges = GetPropertyChanges(existingSub, subVM);
                            if (!string.IsNullOrEmpty(subChanges))
                                changes.AppendLine($"Child Updated - ItemCode {subVM.ProcessId}:\n{subChanges}");

                            _mapper.Map(subVM, existingSub);
                        }
                    }
                }
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "[HandleChildUpdatesAsync - Sales Quotation] Failed to update Quotation children");
                throw new InvalidOperationException("Failed to update Quotation details. Please contact support.");
            }
        }


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

        private async Task LogChangesAsync(StringBuilder changes, string action)
        {
            if (changes.Length == 0) return;

            await _loggingService.LogUserAction(
                UserName: await _currentUserService.GetUsernameAsync(),
                Machine: _currentUserService.MachineName,
                IP_Address: _currentUserService.IpAddress,
                screen: "Estimation",
                action: action,
                additionalInfo: changes.ToString()
            );
        }
        public async Task<(bool CanDelete, string Message)> CanDeleteEstimateAsync(int EstiamateId)
        {
            try
            {
                var Estimate = await _unitOfWork.Estimates
                                .GetQueryable()
                                .Include(e => e.EstimateSub)
                                .Where(e => e.EstiamateId == EstiamateId).FirstOrDefaultAsync();

                if (Estimate == null)
                    return (true, "Estmation can be safely deleted.");

                var EstimateSubIds = Estimate.EstimateSub
                    .Select(es => es.EstiamateSubId)
                    .ToList();

                

                return (true, "Estimation can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error in CanDeleteEstimateAsync for EstiamateId: {EstiamateId}");
                throw new Exception("Error checking Sales Estimaion delete eligibility", ex);
            }
        }
        public async Task<EstimateVM?> GetEstimationByEstimateIdAsync(int EstimateId)
        {
            try
            {
                var entity = await _unitOfWork.Estimates.GetQueryable()
                    .Include(q => q.EstimateSub).ThenInclude(i => i.Process)
                    .Include(q => q.EnquirySalesSub)
                    .Include(q => q.Customer)
                    .Include(x => x.Item)
                    .FirstOrDefaultAsync(q => q.EstiamateId == EstimateId);
                return _mapper.Map<EstimateVM?>(entity);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"GetEstimateAsync({EstimateId})");
                return null;
            }
        }
        public async Task<bool> DeleteEstimationByEstimateIdAsync(int EstiamateId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Get the quotation with its sub-items
                var Estimation = await _unitOfWork.Estimates
                    .GetQueryable()
                    .Include(e => e.EstimateSub)
                    .FirstOrDefaultAsync(e => e.EstiamateId == EstiamateId);

                if (Estimation == null)
                {
                    return false;
                }

                var changes = new StringBuilder();

                if (Estimation.RefEnqSubId > 0)
                {
                    await AdjustEnquiryBalanceAsync(Estimation.RefEnqSubId, "Estimation Deletion");
                }

                // Delete the quotation
                await _unitOfWork.Estimates.DeleteAsync(Estimation);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Log the user action
                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Estimation List",
                    action: $"Deleted Estimation: {Estimation.EstiamateNo}",
                    additionalInfo: $"Estimation Id: {Estimation.EstiamateId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, $"Failed to delete Estimation: {EstiamateId}");
                throw;
            }
        }
        private async Task AdjustEnquiryBalanceAsync(int? refEnqSubId,string context)
        {
            try
            {
                if (!refEnqSubId.HasValue || refEnqSubId == 0) return;

                var enqSub = await _unitOfWork.EnquirySalesSubs.GetAsync(refEnqSubId.Value);
                if (enqSub == null) return;

               // var enqSub = await _unitOfWork.EnquirySalesSubs.GetQueryable().FirstOrDefaultAsync(i => i.EnquirySubId == entity.RefEnqSubId);

                if (enqSub != null)
                {
                    enqSub.IsEstiItemTally = false;
                }

                await _unitOfWork.EnquirySalesSubs.UpdateAsync(enqSub);
                await _unitOfWork.SaveAsync();

                var totalBalQty = await _unitOfWork.EnquirySalesSubs
                    .GetQueryable()
                    .Where(e => e.EnquiryId == enqSub.EnquiryId && !e.ItemCancel)
                    .SumAsync(e => e.BalQty);


                var Enquiry = await _unitOfWork.EnquirySaless.GetAsync(enqSub.EnquiryId);
                if (Enquiry != null)
                {
                    Enquiry.EnquiryTally = (totalBalQty == 0);
                    await _unitOfWork.EnquirySaless.UpdateAsync(Enquiry);
                    await _unitOfWork.SaveAsync();
                }
            }
            catch (InvalidOperationException ex)
            {
                await _loggingService.LogDeveloperError(ex, $"[AdjustEstiamtionAsync] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"[AdjustEstiamtion] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to Adjust Estiamtion. Please contact support.");
            }
        }

        public async Task<List<Dictionary<string, object>>> GetEnquiryDetailsByCustId(int custId)//, bool mfgOrLab
        {
            try
            {
                var result = await (from e in _unitOfWork.EnquirySaless.GetQueryable()
                                    join es in _unitOfWork.EnquirySalesSubs.GetQueryable()
                                        on e.EnquiryId equals es.EnquiryId
                                    where e.CustId == custId && !e.EnquiryTally && !e.Cancel && !e.ShortClose && es.BalQty > 0 && !es.ItemCancel && !es.IsEstiItemTally  /*&& e.MfgORLab == mfgOrLab*/
                                    select new
                                    {
                                        es.EnquirySubId,
                                        es.EnquiryId,
                                        e.EnquiryNo,
                                        e.Suffix,
                                        e.EnquiryDate,
                                        es.ItemId,
                                        es.Item.ItemCode,
                                        es.Item.ItemName,
                                        es.Item.HSNCode,
                                        es.Item.MeasureUnit,
                                        es.Qty,
                                        es.BalQty,
                                        es.UnitPrice,
                                        CostCenterId = es.CostCenterId == 0 ? (int?)null : es.CostCenterId,
                                        es.CostCenter.ProjectNo
                                    }).ToListAsync();

                return result.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,
                    ["RefEnqSubId"] = r.EnquirySubId,
                    ["EnquiryId"] = r.EnquiryId,
                    ["EnquiryNo"] = $"{r.EnquiryNo}{r.Suffix}",
                    ["EnquiryDate"] = r.EnquiryDate,
                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["UOM"] = r.MeasureUnit ?? string.Empty,
                    ["HSNCode"] = r.HSNCode ?? string.Empty,
                    ["Qty"] = r.Qty,
                    ["BalQty"] = r.BalQty,
                    ["Rate"] = r.UnitPrice,
                    ["CostCenterId"] = r.CostCenterId ?? (int?)null,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,
                }).ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching enquiry details for CustId: {custId}");
                throw new InvalidOperationException("Failed to retrieve enquiry details. Please try again.");
            }
        }

        public async Task<List<ItemVM>> GetItemByEnquirySubId(int EnquiryId)
        {
            var items = await _unitOfWork.EnquirySaless.GetQueryable()
                .Where(x => x.EnquiryId == EnquiryId)
                .SelectMany(x => x.EnquirySalesSub.Where(s => !s.IsEstiItemTally))
                .Select(s => new ItemVM
                {
                    ItemId = s.ItemId,
                    ItemCode = s.Item.ItemCode,
                    ItemName = s.Item.ItemName
                })
                .ToListAsync();

            return items;
        }
        
        public async Task<int> GetEnqSubId(int enquiryId, int itemId)
        {
            return await _unitOfWork.EnquirySalesSubs.GetQueryable()
                .Where(i => i.EnquiryId == enquiryId && i.ItemId == itemId)
                .Select(i => i.EnquirySubId)
                .FirstOrDefaultAsync();
        }

        public async Task<string?> GetEnquiryNo(int enquirySubId)
        {
            return await _unitOfWork.EnquirySaless.GetQueryable()
                .Where(e => e.EnquirySalesSub.Any(s => s.EnquirySubId == enquirySubId))
                .Select(e => e.EnquiryNo)
                .FirstOrDefaultAsync();
        }

        public async Task<Decimal> ProcessRate(int SelectedProcessId)
        {
            return await _unitOfWork.Processes.GetQueryable()
                    .Where(i => i.ProcessId == SelectedProcessId)
                     .Select(i => (decimal)i.HrlyRate)
                    .FirstOrDefaultAsync();
        }
        public async Task<bool> IsProcessFlowExists(int itemId)
        {
            try
            {
                return await _unitOfWork.ProcessFlowCharts
                    .AnyAsync(x => x.CompItemId == itemId);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error checking Process Flow.");
                throw;
            }
        }
    }
}

