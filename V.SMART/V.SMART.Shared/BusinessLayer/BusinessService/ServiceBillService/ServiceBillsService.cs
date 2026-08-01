using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;


using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ICashFlowService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.Data.CashFlow.ServiceBills;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.CashFlowViewModel.ServiceBillViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchPoVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.CashFlowService
{
    public class ServiceBillsService : IServiceBillsService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly IStockManagerService _stockManagerService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;

        public ServiceBillsService(
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

        // 🔹 Currency
        public async Task<List<Currency>> GetCurrenciesAsync()
            => (await _commonService.GetCurrenciesAsync()).ToList();

        // 🔹 Company Details
        public async Task<Companydetails> GetCompanyDetailsAsync()
            => await _commonService.GetCompanyDetailsAsync();

     
        // 🔹 Get Decimal Places Async
        public async Task<int> GetDecimalPlacesAsync()
            => await _commonService.GetDecimalPlacesAsync();

        // 🔹Get Customer By Id
        public async Task<CustomerVM> GetCustomerById(int custid)
            => await _commonService.GetCustomerByIdAsync(custid);

        // 🔹Get All Customers
        public async Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText)
            => await _commonService.SearchCustomersAsync(searchText);
        

        //🔹Get Currency By Id Async
        public async Task<Currency?> GetCurrencyByIdAsync(int currId)
             => await _commonService.GetCurrencyByIdAsync(currId);

        //🔹Get Latest Currency Value Async
        public async Task<decimal?> GetLatestCurrencyValueAsync(int currId)
           => await _commonService.GetLatestCurrencyValueAsync(currId);

        //🔹Search Items Async
        public async Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText)
            => await _commonService.SearchItemsAsync(searchText);

        //🔹Get Item By ItemId Async
        public async Task<ItemVM?> GetItemByItemIdAsync(int? itemId)
            => await _commonService.GetItemByItemIdAsync(itemId);

        //🔹Get All Active Vendors Async
        public async Task<IEnumerable<VendorVM>> SearchVendorsAsync(string searchText)
          => await _commonService.SearchVendorsAsync(searchText);

        #region Get Po Details By CustId
        public async Task<List<Dictionary<string, object>>> GetPoDetailsByCustId(int custId)
        {
            try
            {
                var result = await (from p in _unitOfWork.MfgPos.GetQueryable()
                                    join ps in _unitOfWork.MfgPoSubs.GetQueryable()
                                        on p.PoId equals ps.PoId
                                    where p.CustId == custId
                                          && !p.PoTally && p.IsAuthorized == true
                                          && !p.PoCancl && ps.BalQty > 0 && p.IsServiceBill == true
                                    select new
                                    {
                                        ps.PoSubId,
                                        p.IsOpenPO,
                                        p.PONo,
                                        p.Suffix,
                                        p.PODate,
                                        ps.LineNo,
                                        ps.ItemId,
                                        ps.Item.ItemCode,
                                        ps.Item.ItemName,
                                        ps.Item.MeasureUnit,
                                        ps.Item.HSNCode,
                                        ps.Qty,
                                        ps.BalQty,
                                        ps.UnitPrice,
                                        ps.DueDate,
                                        CostCenterId = ps.CostId == 0 ? (int?)null : ps.CostId,
                                        ps.CostCenter.ProjectNo,
                                        p.MainRemark
                                    }).ToListAsync();

                return result.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,
                    ["PoSubId"] = r.PoSubId,
                    ["IsOpenPo"] = r.IsOpenPO ? "Yes" : "No",
                    ["PoNo"] = r.PONo,
                    ["Suffix"] = r.Suffix,
                    ["PoDate"] = r.PODate,
                    ["LineId"] = r.LineNo,
                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["UOM"] = r.MeasureUnit ?? string.Empty,
                    ["HSNCode"] = r.HSNCode ?? string.Empty,
                    ["Qty"] = r.Qty,
                    ["BalQty"] = r.BalQty,
                    ["UnitPrice"] = r.UnitPrice,
                    ["PoDuedate"] = r.DueDate,
                    ["CostCenterId"] = r.CostCenterId,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,
                    ["Remark"] = r.MainRemark ?? string.Empty,
                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching PO details for CustId: {custId}");
                throw new InvalidOperationException("Failed to retrieve PO details. Please try again.");
            }
        }
        #endregion

        #region Get Po Details By vendor code
        public async Task<List<Dictionary<string, object>>> GetPoDetailsByVendorcode(int vendorcode)
        {
            try
            {
                var result = await (from p in _unitOfWork.PurchPos.GetQueryable()
                                    join ps in _unitOfWork.PurchPoSubs.GetQueryable()
                                        on p.PoId equals ps.PoId
                                    where p.VendorCode == vendorcode
                                          && !p.PoTally
                                          && !p.PoCancl && ps.BalQty>0 && p.IsServiceBill == true
                                    select new
                                    {
                                        ps.PoSubId,
                                        p.IsOpenPO,
                                        p.PONo,
                                        p.Suffix,
                                        p.PODate,
                                        ps.ItemId,
                                        ps.Item.ItemCode,
                                        ps.Item.ItemName,
                                        ps.Item.MeasureUnit,
                                        ps.Item.HSNCode,
                                        ps.Qty,
                                        ps.BalQty,
                                        ps.UnitPrice,
                                        ps.DueDate,
                                        CostCenterId = ps.CostId == 0 ? (int?)null : ps.CostId,
                                        ps.CostCenter.ProjectNo,
                                        p.MainRemark
                                    }).ToListAsync();

                return result.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,
                    ["PoSubId"] = r.PoSubId,
                    ["IsOpenPo"] = r.IsOpenPO ? "Yes" : "No",
                    ["PoNo"] = r.PONo,
                    ["Suffix"] = r.Suffix,
                    ["PoDate"] = r.PODate,
                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["UOM"] = r.MeasureUnit ?? string.Empty,
                    ["HSNCode"] = r.HSNCode ?? string.Empty,
                    ["Qty"] = r.Qty,
                    ["BalQty"] = r.BalQty,
                    ["UnitPrice"] = r.UnitPrice,
                    ["PoDuedate"] = r.DueDate,
                    ["CostCenterId"] = r.CostCenterId,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,
                    ["Remark"] = r.MainRemark ?? string.Empty,
                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching PO details for vendorcode : {vendorcode}");
                throw new InvalidOperationException("Failed to retrieve PO details. Please try again.");
            }
        }
        #endregion

        #region Get All ServicesAsync
        public async Task<IEnumerable<ServiceBillsVM>> GetAllServicesAsync()
        {
            try
            {
                var bills = await _unitOfWork.ServiceBills
                           .GetQueryable().AsNoTracking().ToListAsync();

                var billIds = bills.Select(b => b.ServiceId).ToList();

                var billSubs = await _unitOfWork.ServiceBillsSubs
                    .GetQueryable()
                    .Where(s => billIds.Contains(s.ServiceId))
                    .AsNoTracking()
                    .ToListAsync();

                var billVMs = _mapper.Map<List<ServiceBillsVM>>(bills);

                foreach (var billVM in billVMs)
                {
                    billVM.ServiceBillsSub = billSubs
                        .Where(s => s.ServiceId == billVM.ServiceId)
                        .Select(s => _mapper.Map<ServiceBillsSubVM>(s))
                        .ToList();
                }

                return billVMs;

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "GetAllQuoteAsync");
                return Enumerable.Empty<ServiceBillsVM>();
            }
        }
        #endregion

        #region SearchWithDynamicFilterAsync
        public async Task<(List<ServiceBillsVM> serviceBillsVMs, int TotalCount)>
                      SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.ServiceBills
                    .GetQueryable()
                    .Include(s => s.ServiceBillSub)   // 🔥 Direct include instead of separate query
                    .AsQueryable();

                // ✅ Dynamic Filters
                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        query = ServiceBillFilterBuilder.ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                // ✅ Total count BEFORE paging
                var totalCount = await query.CountAsync();

                // ✅ Server-side paging
                var bills = await query
                    .OrderByDescending(s => s.ServiceId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .AsNoTracking()
                    .ToListAsync();

                var billVMs = _mapper.Map<List<ServiceBillsVM>>(bills);

                return (billVMs, totalCount);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync (ServiceBills)");
                throw new InvalidOperationException("Failed to load service bill list.", ex);
            }
        }
        public static class ServiceBillFilterBuilder
        {
            public static IQueryable<ServiceBills> ApplyFilter(
                IQueryable<ServiceBills> query,
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
                        case "InOrOut":
                            {
                                if(val== "Service Incoming")
                                {
                                    return query.Where(i=>i.IncomingServics==true);
                                }
                                else
                                {
                                    return query.Where(i => i.IncomingServics == false);
                                }
                            }

                        case "InvNo":
                            {
                                string part1 = val;
                                string part2 = string.Empty;

                                int slashIndex = val.IndexOf('/');
                                if (slashIndex > -1)
                                {
                                    part1 = val[..slashIndex].Trim();
                                    part2 = val[(slashIndex + 1)..].Trim();
                                }

                                return query.Where(x => (string.IsNullOrEmpty(part1) || x.InvNo.StartsWith(part1)) &&
                                    (string.IsNullOrEmpty(part2) || (x.Suffix != null && x.Suffix.Contains(part2)))
                                );
                            }

                        case "DCNO":
                            {
                                string part1 = val;
                                string part2 = string.Empty;

                                int slashIndex = val.IndexOf('/');
                                if (slashIndex > -1)
                                {
                                    part1 = val[..slashIndex].Trim();
                                    part2 = val[(slashIndex + 1)..].Trim();
                                }
                                return query.Where(x => (string.IsNullOrEmpty(part1) || x.DcNo.StartsWith(part1))
                                );
                            }

                        //case "Customer":
                        //    return query.Where(x =>
                        //        x.Customer != null &&
                        //        x.Customer.CustName.Contains(val));

                        //case "Vendor":
                        //    return query.Where(x =>
                        //        x.Customer != null &&
                        //        x.Customer.CustName.Contains(val));


                        case "CreatedBy":
                            return query.Where(x =>
                                x.CreatedBy != null &&
                                x.CreatedBy.Contains(val));

                        case "FromDate":
                            if (DateTime.TryParse(val, out var fromDate))
                                return query.Where(x => x.InvDate >= fromDate.Date);
                            return query;

                        case "ToDate":
                            if (DateTime.TryParse(val, out var toDate))
                                return query.Where(x =>
                                    x.InvDate <= toDate.Date.AddDays(1).AddTicks(-1));
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

        #region Delete Service By ServiceId By Async
        public async Task<bool> DeleteServiceByIdAsync(int serviceId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 1️⃣ Load Service Bill with sub-items
                var service = await _unitOfWork.ServiceBills
                    .GetQueryable()
                    .Include(e => e.ServiceBillSub)
                    .FirstOrDefaultAsync(e => e.ServiceId == serviceId);

                if (service == null)
                    return false;

                var changes = new StringBuilder();

                // 2️⃣ Reverse balances BEFORE delete
                foreach (var sub in service.ServiceBillSub)
                {

                    if (service.IncomingServics == false)
                    {
                        if (sub.RefMfgPoSubId > 0)
                        {
                            await AdjustServicePoSubBalanceAsync(sub.RefMfgPoSubId, oldQty: sub.Qty, newQty: 0, context: "Service Bill Deletion");
                        }
                    }
                    else
                    {
                        if (sub.RefPurchPoSubId > 0)
                        {
                            await AdjustServicePurchPoSubBalanceAsync(sub.RefMfgPoSubId, oldQty: sub.Qty, newQty: 0, context: "Service Bill Deletion");
                        }
                    }

                    changes.AppendLine($"Reverted PO Sub Id {sub.RefMfgPoSubId}, Qty {sub.Qty}");
                }
                

                // 3️⃣ Delete entity (tracked)
                await _unitOfWork.ServiceBills.DeleteAsync(service);

                // 4️⃣ Save & commit
                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // 5️⃣ Audit log
                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "ServiceBills List",
                    action: $"Deleted Service Bill: {service.ServiceId}",
                    additionalInfo: $"Service Bill Id: {service.ServiceId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Service Bill: {serviceId}");
                throw;
            }
        }
        #endregion

        #region Get ServiceBillsSubItem Detail By ServiceBillsSubId Async
        public async Task<ServiceBillsSubVM> GetServiceSubItemDetailByServiceSubIdAsync(int ServiceSubId)
        {
            try
            {
                var result = await _unitOfWork.ServiceBillsSubs
                              .GetQueryable()
                              .Where(q => q.ServiceBillSubId == ServiceSubId)
                              .Select(q => new ServiceBillsSubVM
                              {
                                  Qty = q.Qty,
                                  BalQty = q.BalQty
                              })
                              .FirstOrDefaultAsync();

                if (result == null)
                {
                    // handle not found case
                    return new ServiceBillsSubVM
                    {
                        Qty = 0,
                        BalQty = 0
                    };
                }

                return result;

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching quote sub item detail for QuoteSubId: {ServiceSubId}");
                throw new InvalidOperationException("Failed to retrieve quote sub-item details.");
            }
        }
        #endregion

        #region Get Service By ServiceId Async
        public async Task<ServiceBillsVM?> GetServiceByServiceIdAsync(int ServiceId)
        {
            try
            {
                var entity = await _unitOfWork.ServiceBills.GetQueryable()
                    .Include(q => q.ServiceBillSub)
                    .Include(q => q.ServiceBillSub).ThenInclude(s => s.Item)
                    .Include(q => q.Currency)
                    .FirstOrDefaultAsync(q => q.ServiceId == ServiceId);

                return _mapper.Map<ServiceBillsVM?>(entity);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetQuotationAsync({ServiceId})");
                return null;
            }
        }
        #endregion

        #region Get Last Service Bill By Customer Id Async
        public async Task<ServiceBills?> GetLastServiceAsync(int custId)
        {
            try
            {
                return await _unitOfWork.ServiceBills.GetLatestAsync(
                    q => q.CustVendorId == custId,
                    q => q.ServiceId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetLastServiceAsync for CustId: {custId}");
                throw new InvalidOperationException("Failed to retrieve last Service Bills. Please try again.");
            }
        }
        #endregion

        #region Delete And Resequence Async
        public async Task DeleteAndResequenceAsync(ServiceBillsSubVM subitem, ServiceBillsVM service)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.ServiceId > 0) // persisted subitem
                {
                    var entity = await _unitOfWork.ServiceBillsSubs.GetAsync(subitem.ServiceBillSubId);
                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");

                   // Restore balance qty
                    if (entity.ItemCancel)
                    {
                        await AdjustServicePoSubBalanceAsync(subitem.ServiceId, entity.Qty, 0, "Service Bill Deletion");
                    }
                    // Delete from DB
                    await _unitOfWork.ServiceBillsSubs.DeleteAsync(entity.ServiceBillSubId);
                    await _unitOfWork.SaveAsync();

                    // Log action
                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Mfg Quote",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"ServiceBills No: {service?.InvNo}"
                    );
                }
                else
                {
                    // Not yet persisted → just remove from VM
                    service.ServiceBillsSub.Remove(subitem);
                    return;
                }

                // Resequence persisted subitems
                var remaining = await _unitOfWork.MfgQuoteSubs
                    .GetQueryable()
                    .Where(x => x.QuoteId == service.ServiceId)
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
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        #endregion

        #region Get Service Sub By Service Id Async
        public async Task<List<ServiceBillsSubVM>> GetServiceSubByServiceIdAsync(int serviceId)
        {
            try
            {
                var subs = await _unitOfWork.ServiceBillsSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Where(s => s.ServiceId == serviceId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return _mapper.Map<List<ServiceBillsSubVM>>(subs);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching ServiceSub items for QuoteId: {serviceId}");
                throw new InvalidOperationException("Failed to retrieve ServiceBills sub-items. Please try again.");
            }
        }
        #endregion

        #region Upsert Service Bill Async
        public async Task<ServiceBillsVM> UpsertServiceAsync(ServiceBillsVM serviceVM)
        {
            if (serviceVM == null)
                throw new ArgumentNullException(nameof(serviceVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                ServiceBills entity;

                if (serviceVM.ServiceId == 0)
                {
                    entity = _mapper.Map<ServiceBills>(serviceVM);

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.Balance= serviceVM.GrandTotal;
                    entity.ServiceBillSub = serviceVM.ServiceBillsSub.Select(s => _mapper.Map<ServiceBillsSub>(s)).ToList();
                    await _unitOfWork.ServiceBills.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    // ✅ BALANCE UPDATE
                    foreach (var subVM in serviceVM.ServiceBillsSub)
                    {

                        if (serviceVM.IncomingServics == false)
                        {
                            if (subVM.RefMfgPoSubId > 0)
                            {
                                await AdjustServicePoSubBalanceAsync(subVM.RefMfgPoSubId, 0, subVM.Qty ?? 0, "Service Bill Creation");
                            }
                        }
                        else
                        {
                            if (subVM.RefPurchPoSubId > 0)
                            {
                                await AdjustServicePurchPoSubBalanceAsync(subVM.RefPurchPoSubId, 0, subVM.Qty ?? 0, "Service Bill Creation");
                            }
                        }
                    }
                    
                    changes.AppendLine("ServiceBills Created.");

                }
                else
                {
                    entity = await _unitOfWork.ServiceBills.GetQueryable()
                        .Include(q => q.ServiceBillSub)
                        .FirstOrDefaultAsync(q => q.ServiceId == serviceVM.ServiceId)
                        ?? throw new InvalidOperationException("ServiceBills not found.");

                    foreach (var newSub in serviceVM.ServiceBillsSub)
                    {
                        var oldSub = entity.ServiceBillSub
                            .FirstOrDefault(s => s.ServiceBillSubId == newSub.ServiceBillSubId);

                        var oldQty = oldSub?.Qty ?? 0;
                        var newQty = newSub.Qty ?? 0;
                        if (serviceVM.IncomingServics == false)
                        {
                            if (newSub.RefMfgPoSubId > 0 && oldQty != newQty)
                            {

                                await AdjustServicePoSubBalanceAsync(newSub.RefMfgPoSubId, oldQty, newQty, "Service Bill Update");
                            }
                        }
                        else
                        {
                            if (newSub.RefPurchPoSubId > 0 && oldQty != newQty)
                            {
                                await AdjustServicePurchPoSubBalanceAsync(newSub.RefPurchPoSubId, oldQty, newQty, "Service Bill Update");
                            }
                        }
                    }

                    var parentChanges = GetPropertyChanges(entity, serviceVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(serviceVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await HandleChildUpdatesAsync(entity, serviceVM.ServiceBillsSub, changes);

                    changes.AppendLine("ServiceBills Updated.");
                }

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await LogChangesAsync(changes, serviceVM.ServiceId == 0 ? "ServiceBill Created" : "ServiceBill Updated");

                var savedEntity = await _unitOfWork.ServiceBills.GetQueryable()
                    .Include(q => q.ServiceBillSub).ThenInclude(s => s.Item)
                    .Include(q => q.Currency)
                    .Include(q => q.ServiceBillSub)
                    .FirstOrDefaultAsync(q => q.ServiceId == entity.ServiceId);

                return _mapper.Map<ServiceBillsVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert ServiceBills: {serviceVM.ServiceId}");
                throw new InvalidOperationException("Failed to save ServiceBills. Please try again.");
            }
        } 
        #endregion

        #region Storing the ServiceBill in Sub Table
        private async Task HandleChildUpdatesAsync(ServiceBills existingService, List<ServiceBillsSubVM> incomingSubVMs, StringBuilder changes)
        {
            var existingSubIds = existingService.ServiceBillSub.Select(s => s.ServiceBillSubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.ServiceBillSubId).ToHashSet();

            // DELETE removed children
            foreach (var sub in existingService.ServiceBillSub.Where(s => !incomingSubIds.Contains(s.ServiceBillSubId)).ToList())
            {
                changes.AppendLine($"Child Deleted - ServiceBillSubId: {sub.ServiceBillSubId}, Item: {sub.Item?.ItemCode}");
                await _unitOfWork.ServiceBillsSubs.DeleteAsync(sub.ServiceBillSubId);
                await _unitOfWork.SaveAsync();
            }

            // ADD or UPDATE children
            foreach (var subVM in incomingSubVMs)
            {
                if (subVM.ServiceBillSubId == 0)
                {
                    var newSub = _mapper.Map<ServiceBillsSub>(subVM);
                    newSub.ServiceId = existingService.ServiceId;
                    await _unitOfWork.ServiceBillsSubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.Qty}");
                }
                else
                {
                    var existingSub = existingService.ServiceBillSub.FirstOrDefault(s => s.ServiceBillSubId == subVM.ServiceBillSubId);
                    if (existingSub != null)
                    {
                        var subChanges = GetPropertyChanges(existingSub, subVM);
                        if (!string.IsNullOrEmpty(subChanges))
                            changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");

                        _mapper.Map(subVM, existingSub);
                    }
                }
            }
        }
        #endregion

        #region Adjusting the Service PoSub Balance
        private async Task AdjustServicePoSubBalanceAsync(int? refPoSubId,decimal oldQty,decimal newQty,string context)
        {
            if (!refPoSubId.HasValue || refPoSubId == 0) return;

            try
            {
                var poSub = await _unitOfWork.MfgPoSubs.GetAsync(refPoSubId.Value);
                if (poSub == null) return;

                // restore old qty
                if (oldQty > 0)
                    poSub.BalQty += oldQty;

                // validation
                if (newQty > poSub.BalQty)
                    throw new InvalidOperationException($"{context}: Qty exceeds available balance.");

                // consume new qty
                if (newQty > 0)
                    poSub.BalQty -= newQty;

                await _unitOfWork.MfgPoSubs.UpdateAsync(poSub);
                await _unitOfWork.SaveAsync();

                // ✅ Calculate total BalQty for the parent PO
                var totalBalQty = await _unitOfWork.MfgPoSubs
                    .GetQueryable()
                    .Where(e => e.PoId == poSub.PoId)
                    .SumAsync(e => e.BalQty);

                var po = await _unitOfWork.MfgPos.GetAsync(poSub.PoId);
                if (po != null)
                {
                    po.PoTally = (totalBalQty == 0); // Tally only if all BalQty consumed
                    await _unitOfWork.MfgPos.UpdateAsync(po);
                    await _unitOfWork.SaveAsync();
                }

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustServicePoSubBalance] {context}");
                throw;
            }
        }
        private async Task AdjustServicePurchPoSubBalanceAsync(int? refPoSubId, decimal oldQty, decimal newQty, string context)
        {
            if (!refPoSubId.HasValue || refPoSubId == 0) return;

            try
            {
                var poSub = await _unitOfWork.PurchPoSubs.GetAsync(refPoSubId.Value);
                if (poSub == null) return;

                // restore old qty
                if (oldQty > 0)
                    poSub.BalQty += oldQty;

                // validation
                if (newQty > poSub.BalQty)
                    throw new InvalidOperationException($"{context}: Qty exceeds available balance.");

                // consume new qty
                if (newQty > 0)
                    poSub.BalQty -= newQty;

                await _unitOfWork.PurchPoSubs.UpdateAsync(poSub);
                await _unitOfWork.SaveAsync();

                // ✅ Calculate total BalQty for the parent PO
                var totalBalQty = await _unitOfWork.PurchPoSubs
                    .GetQueryable()
                    .Where(e => e.PoId == poSub.PoId)
                    .SumAsync(e => e.BalQty);

                var po = await _unitOfWork.PurchPos.GetAsync(poSub.PoId);
                if (po != null)
                {
                    po.PoTally = (totalBalQty == 0); // Tally only if all BalQty consumed
                    await _unitOfWork.PurchPos.UpdateAsync(po);
                    await _unitOfWork.SaveAsync();
                }

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustServicePoSubBalance] {context}");
                throw;
            }
        }
        #endregion

        #region Get PODetails from mfgposub
        public async Task<List<MfgPoSubVM>> GetPOSubByPoIdAsync(int poId)
        {
            try
            {
                var subs = await _unitOfWork.MfgPoSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Include(s => s.CostCenter)
                    .Where(s => s.PoId == poId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return _mapper.Map<List<MfgPoSubVM>>(subs);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Manufacturing PO items for PoId: {poId}");
                throw new InvalidOperationException("Failed to retrieve Sales Order sub-items. Please try again.");
            }
        }
        #endregion

        #region Get PODetails from PurchPOSub
        public async Task<List<PurchPoSubVM>> GetPurchPOSubByPoIdAsync(int poId)
        {
            try
            {
                var subs = await _unitOfWork.PurchPoSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Include(s => s.CostCenter)
                    .Where(s => s.PoId == poId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return _mapper.Map<List<PurchPoSubVM>>(subs);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching GetPurchPOSubByPoIdAsync");
                throw new InvalidOperationException("Failed to Get Purch POSub By PoId Async. Please try again.");
            }
        }
        #endregion

        #region Get Property Changes
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
        #endregion

        #region Logging Changes Async
        // Logging
        private async Task LogChangesAsync(StringBuilder changes, string action)
        {
            if (changes.Length == 0) return;

            await _logs.LogUserAction(
                UserName: await _currentUserService.GetUsernameAsync(),
                Machine: _currentUserService.MachineName,
                IP_Address: _currentUserService.IpAddress,
                screen: "ServiceBills",
                action: action,
                additionalInfo: changes.ToString()
            );
        }
        #endregion

        #region Getting MfgLabPOS By Customer Id
        public async Task<List<MfgPoVM>> GetAllPOsByCustId(int CustId)
        {
            try
            {
                var mfgPosList = await _unitOfWork.MfgPos.GetQueryable()
                                .Where(i => i.CustId == CustId)
                                .ToListAsync();
                var mfgPosVMs = _mapper.Map<List<MfgPoVM>>(mfgPosList);
                return mfgPosVMs;

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error occured in GetAllPOsByCustId");
                return new List<MfgPoVM>();
            }
        }
        #endregion

        #region Generating new Service Bill Number
        public async Task<String> GenerateNewServiceNoAsync()
        {
            try
            {

                var latestService = "";

                latestService = await _unitOfWork.ServiceBills.GetQueryable()
                                                           .OrderByDescending(i => i.ServiceId)
                                                           .Select(i => i.InvNo)
                                                           .FirstOrDefaultAsync();
                if (latestService != null)
                {
                    var serialNo = int.Parse(latestService);   
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

        #region Checking Duplicate Service Number
        public async Task<bool> CheckDuplicate(String InvNo)
        {
            try
            {
                var duplicateService = await _unitOfWork.ServiceBills.GetQueryable()
                                                            .Where(i => i.InvNo == InvNo)
                                                            .FirstOrDefaultAsync();
                if (duplicateService != null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in CheckDuplicate: ");
                return false;
            }
        }
        #endregion

        #region Update ItemCancel And Add or Revert Async
        public async Task UpdateItemCancelAndAddorRevertAsync(ServiceBillsSubVM subItem, int serviceId,bool isincoming)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (isincoming == false)
                {
                    if (subItem.RefMfgPoSubId > 0)
                    {
                        if (subItem.ItemCancel)
                        {
                            await AdjustServicePoSubBalanceAsync(subItem.RefMfgPoSubId, oldQty: subItem.Qty ?? 0, newQty: 0, context: "Service Bill Cancelled");
                        }
                        else
                        {
                            await AdjustServicePoSubBalanceAsync(subItem.RefMfgPoSubId, oldQty:0 , newQty: subItem.Qty ?? 0, context: "Service Bill Cancelled");
                        }
                    }
                }
                else
                {
                    if (subItem.RefPurchPoSubId > 0)
                    {
                        if (subItem.ItemCancel)
                        {
                            await AdjustServicePurchPoSubBalanceAsync(subItem.RefPurchPoSubId, oldQty: subItem.Qty ?? 0, newQty: 0, context: "Service Bill Cancelled");
                        }
                        else
                        {
                            await AdjustServicePurchPoSubBalanceAsync(subItem.RefPurchPoSubId, oldQty: 0, newQty: subItem.Qty ?? 0, context: "Service Bill Cancelled");
                        }
                    }
                }

                var subEntity = await _unitOfWork.ServiceBillsSubs.GetQueryable().Where
                                (x => x.ServiceBillSubId == subItem.ServiceBillSubId).FirstOrDefaultAsync();

                if (subEntity == null)
                    throw new KeyNotFoundException($"Subitem with QuoteSubId {subItem.ServiceBillSubId} not found.");

                subEntity.ItemCancel = subItem.ItemCancel;
                subEntity.ItemCancelReason = subItem.ItemCancelReason;
                await _unitOfWork.ServiceBillsSubs.UpdateAsync(subEntity);
                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpdateItemCancelAndAddorRevertAsync] Validation issue");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Error in UpdateItemCancelAndAddorRevertAsync for ItemCode {subItem.ItemCode}");
                throw new InvalidOperationException("Failed to update Item cancel/revert status. Please contact support.");
            }
        }
        #endregion

        #region to update the Short Cancel Status
        public async Task UpsertServiceBillsShortCloseAsync(ServiceBillsVM servicebills)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var servicePo = await _unitOfWork.ServiceBills.GetAsync(servicebills.ServiceId);
                if (servicePo == null)
                    throw new InvalidOperationException("Service Bills not found.");

                servicePo.ShortClose = servicebills.ShortClose;

                await _unitOfWork.ServiceBills.UpdateAsync(servicePo);
                await _unitOfWork.SaveAsync();

                //await UpdatePoTallyStatusAsync(servicebills.PoId);

                await transaction.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpsertServiceBillsShortCloseAsync] Validation issue");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpsertSalesQuoteShortCloseAsync] Unexpected error");
                throw new InvalidOperationException("Failed to update short-Close/re-open status. Please contact support.");
            }
        }
        #endregion

        #region to Update the Cancel Status and Add or Revert the Qty
        public async Task UpdatedCancelStatusAndAddOrRevertQty(ServiceBillsVM servicebills)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingservice = await _unitOfWork.ServiceBills.GetAsync(servicebills.ServiceId);
                if (existingservice == null)
                    throw new InvalidOperationException("Service Bill not found.");

                existingservice.IsCancel = servicebills.IsCancel;
                existingservice.CancelReason = servicebills.CancelReason;
                await _unitOfWork.ServiceBills.UpdateAsync(existingservice);
                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();
                if (servicebills.IsCancel == true)
                {
                    // ✅ BALANCE UPDATE
                    foreach (var subVM in servicebills.ServiceBillsSub)
                    {

                        if (servicebills.IncomingServics == false)
                        {
                            if (subVM.RefMfgPoSubId > 0)
                            {
                                await AdjustServicePoSubBalanceAsync(subVM.RefMfgPoSubId, oldQty: subVM.Qty ?? 0, newQty: 0, context: "Service Bill Cancelled");
                               
                            }
                        }
                        else
                        {
                            if (subVM.RefPurchPoSubId > 0)
                            {
                                await AdjustServicePurchPoSubBalanceAsync(subVM.RefPurchPoSubId, oldQty: subVM.Qty ?? 0, newQty: 0, context: "Service Bill Cancelled");
                               
                            }
                        }
                    }
                }
                else
                {
                    foreach (var sub in servicebills.ServiceBillsSub)
                    {

                        if (servicebills.IncomingServics == false)
                        {
                            if (sub.RefMfgPoSubId > 0)
                            {
                                await AdjustServicePoSubBalanceAsync(sub.RefMfgPoSubId, 0, sub.Qty ?? 0, "Service Bill Reverted");
                            }
                        }
                        else
                        {
                            if (sub.RefPurchPoSubId > 0)
                            {
                                await AdjustServicePurchPoSubBalanceAsync(sub.RefPurchPoSubId, 0, sub.Qty ?? 0, "Service Bill Reverted");
                            }
                        }
                    }
                }

            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpdatedCancelStatusAndAddOrRevertQty] Validation issue");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpdatedCancelStatusAndAddOrRevertQty] Unexpected error");
                throw new InvalidOperationException("Failed to update cancel/revert status. Please contact support.");
            }
        }
        #endregion

        #region Get Reference PO Number By PO Sub Id Async  
        public async Task<List<ServiceBillsSubVM>> GetDistinctRefPoByServiceIdAsync(int serviceId)
        {
            try
            {
                return await _unitOfWork.ServiceBillsSubs
                    .GetQueryable()
                    .Where(s => s.ServiceId == serviceId)
                    .GroupBy(s => new { s.RefPoNo, s.RefPoDate })
                    .Select(g => new ServiceBillsSubVM
                    {
                        RefPoNo = g.Key.RefPoNo,
                        RefPoDate = g.Key.RefPoDate
                    })
                    .ToListAsync();
            }
            catch(Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching distinct Ref PO for ServiceId: {serviceId}");
                return new List<ServiceBillsSubVM>();
            }
        }
        #endregion

        // 🔹 Get Vendor By Id
        public async Task<VendorVM> GetVendorByIdAsync(int id)
        {
            try
            {
                var vendor = await _commonService.GetVendorByVenerCodeAsync(id);

                if (vendor == null)
                    throw new Exception($"Vendor not found for Id {id}");

                return vendor;
            }
            catch(Exception ex) 
            {
                await _logs.LogDeveloperError(ex, $"Error Occured While fetching Vedor by VendorCode");
                return new VendorVM();
            }
        }


    }
}
