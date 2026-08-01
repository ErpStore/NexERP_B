using AutoMapper;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISalesService;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.SalesAndLabour.PerformaInvoice;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.PerformaInvoiceVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.SalesService
{
    public class PerformaInvService : IPerformaInvService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;

        public PerformaInvService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            CurrentUserService userService,
            ILoggingService logs,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _logs = logs;
            _mapper = mapper;
        }

        // 🔹 Customers

        public async Task<CustomerVM?> GetCustomerByIdAsync(int custId)
            => await _commonService.GetCustomerByIdAsync(custId);


        public async Task<bool> GetLineIdByCustomerAsync(int custId)
        {
            return await _commonService.GetLineIdByCustomerAsync(custId);
        }

        public async Task<List<CustomerVM>> GetCustomersAsync(int? custId = null)
        {
            if (custId.HasValue && custId.Value > 0)
            {
                var customer = await _commonService.GetCustomerByIdAsync(custId.Value);
                return customer != null ? new List<CustomerVM> { customer } : new List<CustomerVM>();
            }
            return await _commonService.GetAllActiveCustomersAsync();
        }

        public async Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText)
        {
            return await _commonService.SearchCustomersAsync(searchText);
        }

        // 🔹 Items
        public async Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText)
            => await _commonService.SearchItemsAsync(searchText);
        public async Task<ItemVM?> GetItemByItemIdAsync(int? itemId)
            => await _commonService.GetItemByItemIdAsync(itemId);
        //Attachments And corecpondence
        public Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
             => _commonService.GetCorrespondenceAttachmentsCountAsync(refId, refType);

        // 🔹 Currency
        public async Task<List<Currency>> GetCurrenciesAsync()
            => (await _commonService.GetCurrenciesAsync()).ToList();

        public async Task<Currency?> GetCurrencyByIdAsync(int currId)
            => (await _commonService.GetCurrencyByIdAsync(currId));

        // 🔹 Get latest currency rate (from CurrencyToday Service)
        public async Task<decimal?> GetLatestCurrencyValueAsync(int currId)
            => await _commonService.GetLatestCurrencyValueAsync(currId);

        // 🔹 Contacts
        public async Task<List<ContactPerson>> GetContactPersonsAsync(int custId)
            => await _commonService.GetContactPersonsAsync(custId);

        // 🔹 Consignee addresses
        public async Task<List<CustomerIndirect>> GetConsigneeAddressesAsync(int custId)
            => await _commonService.GetConsigneeAddressesAsync(custId);

        // CostCeneter
        public async Task<List<CostCenterVM>> GetCostCenterDetailsByCustId(int custId, HashSet<int> usedCostCenterIds)
            => await _commonService.GetCostCenterDetailsByCustId(custId, usedCostCenterIds);

        public async Task<Companydetails?> GetCompanyDetailsAsync()
            => await _commonService.GetCompanyDetailsAsync();

        //Baanks
        public async Task<List<Banks>> GetAllActiveBanksDetailaAsync()
            => await _commonService.GetAllActiveBanksDetailaAsync();


        public async Task<bool> CheckAssemblyExistsAsync(int itemId)
        {
            try
            {
                if (itemId <= 0)
                    throw new ArgumentException("Invalid ItemId. Must be greater than zero.", nameof(itemId));

                return await _unitOfWork.AssmblyDefs
                    .AnyAsync(a => a.AssmblyID == itemId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error checking Assembly existence for ItemId: {itemId}");
                throw;
            }
        }




        // 🔹 Decimal places
        public async Task<int> GetDecimalPlacesAsync()
            => await _commonService.GetDecimalPlacesAsync();

        // 🔹 PerformaInv operations

        public async Task<(bool CanDelete, string Message)> CanDeletePerfromaInvAsync(int invId)
        {
            try
            {
                var performaInv = await _unitOfWork.PerformaInvs
                                .GetQueryable()
                                .Include(e => e.PerformaInvSubs)
                                .Where(e => e.InvId == invId).FirstOrDefaultAsync();

                if (performaInv == null)
                    return (true, "Performa Invoice can be safely deleted.");

              
                if (performaInv.InvCancl || performaInv.InvCancl)
                    return (false, "Cannot delete this Performa Invoice as it is Cancelled.");

                return (true, "Performa invoice can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeletePerfromaInvAsync for InvId: {invId}");
                throw new Exception("Error checking Performa Invoice delete eligibility", ex);
            }
        }


        public async Task<(List<PerformaInvVM> performaInvVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.PerformaInvs
                    .GetQueryable()
                    .Include(x => x.Customer)
                    .Include(x => x.PerformaInvSubs)
                        .ThenInclude(s => s.Item)
                    .Include(x => x.PerformaInvSubs)
                        .ThenInclude(s => s.MfgPoSub)
                            .ThenInclude(mp => mp.MfgPo)
                    .AsQueryable();

                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        query = PerformaInvFilterBuilder.ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.InvId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<PerformaInvVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync (Per-forma Invoice)");
                throw new InvalidOperationException("Failed to load Performa Invoice list.", ex);
            }
        }

        public static class PerformaInvFilterBuilder
        {
            public static IQueryable<PerformaInv> ApplyFilter(
                IQueryable<PerformaInv> query,
                string field,
                object value)
            {

                //new("InvNo", "Performa Inv No", "text"),
                //new("Customer", "Customer Name", "text"),
                //new("ItemCode", "Item Code", "text"),
                //new("ItemName", "Item Name", "text"),
                //new("PoNo", "Sales Order No", "text"),
                //new("CreatedBy", "Created By", "text"),
                //new("FromDate", "From Date", "date"),
                //new("ToDate", "To Date", "date")

                try
                {
                    if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                        return query;

                    string val = value.ToString()!.Trim();

                    switch (field)
                    {
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

                        case "PoNo":
                            {
                                string part1 = val;
                                string part2 = string.Empty;

                                int slashIndex = val.IndexOf('/');
                                if (slashIndex > -1)
                                {
                                    part1 = val[..slashIndex].Trim();
                                    part2 = val[(slashIndex + 1)..].Trim();
                                }

                                return query.Where(x =>
                                    x.PerformaInvSubs.Any(s =>
                                        s.MfgPoSub != null &&
                                        s.MfgPoSub.MfgPo != null &&
                                        (string.IsNullOrEmpty(part1) ||
                                            s.MfgPoSub.MfgPo.PONo.StartsWith(part1)) &&
                                        (string.IsNullOrEmpty(part2) ||
                                            (x.Suffix != null && x.Suffix.Contains(part2)))
                                    )
                                );
                            }

                        case "Customer":
                            return query.Where(x =>
                                x.Customer != null &&
                                x.Customer.CustName.Contains(val));

                        case "ItemName":
                            return query.Where(x =>
                                x.PerformaInvSubs.Any(s =>
                                    s.Item != null &&
                                    s.Item.ItemName.Contains(val)));

                        case "ItemCode":
                            return query.Where(x =>
                                x.PerformaInvSubs.Any(s =>
                                    s.Item != null &&
                                    s.Item.ItemCode.Contains(val)));

                        case "CreatedBy":
                            return query.Where(x =>
                                x.CreatedBy != null &&
                                x.CreatedBy.Contains(val));

                        case "FromDate":
                            if (DateTime.TryParse(val, out var fromDate))
                                return query.Where(x => x.InvDateNow >= fromDate.Date);
                            return query;

                        case "ToDate":
                            if (DateTime.TryParse(val, out var toDate))
                                return query.Where(x =>
                                    x.InvDateNow <= toDate.Date.AddDays(1).AddTicks(-1));
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

        public async Task<string> GetPrefixFromDb()
        {
            try
            {
                var prefix = await _unitOfWork.PerformaInvs.GetQueryable()
                            .AsNoTracking()
                            .OrderByDescending(q => q.InvId)
                            .Select(q => q.Prefix)
                            .FirstOrDefaultAsync();

                return prefix ?? string.Empty;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in GetPrefixFromDb()");
                return string.Empty;
            }
        }

        public async Task<string> GetPerformaInvNumberAsync(string suffix)
        {
            try
            {
                var lastQuote = await _unitOfWork.PerformaInvs
                    .GetQueryable()
                    .Where(q => q.Suffix == suffix)
                    .OrderByDescending(q => q.InvNo)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastQuote != null)
                {
                    var parts = lastQuote.InvNo.Split('/');
                    if (int.TryParse(parts[0], out int lastNumber))
                        nextNumber = lastNumber + 1;
                }

                return $"{nextNumber}";
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating quotation number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate quotation number.");
            }
        }


        public async Task<IEnumerable<PerformaInvVM>> GetAllInvDetailsAsync()
        {
            try
            {
                var entities = await _unitOfWork.PerformaInvs.GetAllWithIncludeAsync(q => true,
                    q => q.PerformaInvSubs);
                return _mapper.Map<IEnumerable<PerformaInvVM>>(entities);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "GetAllInvDetailsAsync");
                return Enumerable.Empty<PerformaInvVM>();
            }
        }

        public async Task<bool> DeleteInvByInvIdAsync(int invId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var PerformaInvoice = await _unitOfWork.PerformaInvs
                    .GetQueryable()
                    .Include(e => e.PerformaInvSubs)
                    .FirstOrDefaultAsync(e => e.InvId == invId);

                if (PerformaInvoice == null)
                    return false;

                var changes = new StringBuilder();

                foreach (var sub in PerformaInvoice.PerformaInvSubs)
                {
                    if (sub.RefPoSubId > 0)
                    {
                        await AdjustPoBalanceAsync(sub.RefPoSubId, sub.Qty, 0, "Performa-Invoice Deletion");
                    }
                }

                await _unitOfWork.PerformaInvs.DeleteAsync(PerformaInvoice);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Performa-Invoice List",
                    action: $"Deleted Performa-Invoice: {PerformaInvoice.InvNo}",
                    additionalInfo: $"Inv Id: {PerformaInvoice.InvId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Performa Invoice: {invId}");
                throw;
            }
        }
        public async Task<PerformaInvVM?> GetPerformaByInvIdAsync(int invId)
        {
            try
            {
                var entity = await _unitOfWork.PerformaInvs.GetQueryable()
                    .Include(q => q.PerformaInvSubs)
                    .Include(q => q.PerformaInvSubs).ThenInclude(s => s.Item)
                    .Include(q => q.Customer).ThenInclude(c => c.CustomerIndirects)
                    .Include(q => q.Currency)
                    .FirstOrDefaultAsync(q => q.InvId == invId);

                return _mapper.Map<PerformaInvVM?>(entity);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetPerformaByInvIdAsync({invId})");
                return null;
            }
        }
        public async Task<decimal> GetPoItemPerformaBalQtyFromPoSubId(int poSubId)
        {
            try
            {
                return await _unitOfWork.MfgPoSubs.GetQueryable()
                    .Where(e => e.PoSubId == poSubId)
                    .Select(e => e.PerformaBalQty)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching PerformaBalQty for MfgPoSubId: {poSubId}");
                throw new InvalidOperationException("Failed to retrieve Po balance quantity.");
            }
        }

        public async Task<decimal> GetExistingPerformaQtyByInvSubId(int InvSubId)
        {
            try
            {
                return await _unitOfWork.PerformaInvSubs.GetQueryable()
                    .Where(e => e.InvSubId == InvSubId)
                    .Select(e => e.Qty)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Performa Invoice Qty for InvSubId: {InvSubId}");
                throw new InvalidOperationException("Failed to retrieve GetExistingPerformaQtyByInvSubId.");
            }
        }


        public async Task<PerformaInvSubVM?> GetPerformsSubItemDetailByPerformaSubIdAsync(int invSubId)
        {
            try
            {
                return await _unitOfWork.PerformaInvSubs
                    .GetQueryable()
                    .Where(q => q.InvSubId == invSubId)
                    .Select(q => new PerformaInvSubVM
                    {
                        Qty = q.Qty
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Performa sub item detail for InvSubId: {invSubId}");
                throw new InvalidOperationException("Failed to retrieve Po sub-item details.");
            }
        }


        public async Task<PerformaInvVM> UpsertPerformaInvAsync(PerformaInvVM performaInvVM)
        {
            if (performaInvVM == null)
                throw new ArgumentNullException(nameof(performaInvVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                PerformaInv entity;

                if (performaInvVM.InvId == 0)
                {
                    entity = _mapper.Map<PerformaInv>(performaInvVM);

                    // 🔹 Get last number with locking from repository
                    var NextNumber = await _unitOfWork.PerformaInvs.GetLastPerformaInvNoAsync(entity.Suffix);
                    entity.InvNo = NextNumber;

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;

                    entity.PerformaInvSubs = performaInvVM.PerformaInvSubVMs
                        .Select(s => _mapper.Map<PerformaInvSub>(s))
                        .ToList();

                    await _unitOfWork.PerformaInvs.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    foreach (var subVM in performaInvVM.PerformaInvSubVMs)
                    {
                        if (subVM.RefPoSubId > 0)
                            await AdjustPoBalanceAsync(subVM.RefPoSubId, 0, subVM.Qty ?? 0, "Performa-Inv Creation");
                    }

                    changes.AppendLine("Performa Invoice Created.");
                }
                else
                {
                    entity = await _unitOfWork.PerformaInvs.GetQueryable()
                        .Include(q => q.PerformaInvSubs)
                        .FirstOrDefaultAsync(q => q.InvId == performaInvVM.InvId)
                        ?? throw new InvalidOperationException("Performa Invoice not found.");

                    var parentChanges = GetPropertyChanges(entity, performaInvVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(performaInvVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await HandleChildUpdatesAsync(entity, performaInvVM.PerformaInvSubVMs, changes);

                    changes.AppendLine("Performa Invoice Updated.");
                }

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await LogChangesAsync(changes, performaInvVM.InvId == 0 ? "Manufacturing Perform Invoice Created" : "Manufacturing Perform Invoice Updated");

                var savedEntity = await _unitOfWork.PerformaInvs.GetQueryable()
                    .Include(q => q.PerformaInvSubs).ThenInclude(s => s.Item)
                    .Include(q => q.Customer)
                    .Include(q => q.Currency)
                    .FirstOrDefaultAsync(q => q.InvId == entity.InvId);

                return _mapper.Map<PerformaInvVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert Manufacturing PerformInvoice: {performaInvVM.InvNo}");
                throw new InvalidOperationException("Failed to save Manufacturing Performa Invoice. Please try again.");
            }
        }

        private async Task HandleChildUpdatesAsync(PerformaInv existingPerformaInv, List<PerformaInvSubVM> incomingSubVMs, StringBuilder changes)
        {
            var existingSubIds = existingPerformaInv.PerformaInvSubs.Select(s => s.InvSubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.InvSubId).ToHashSet();

            // DELETE removed children
            foreach (var sub in existingPerformaInv.PerformaInvSubs.Where(s => !incomingSubIds.Contains(s.InvSubId)).ToList())
            {
                changes.AppendLine($"Child Deleted - PerformaSubId: {sub.InvSubId}, Item: {sub.Item?.ItemCode}");
                await _unitOfWork.PerformaInvSubs.DeleteAsync(sub.InvSubId);
                await _unitOfWork.SaveAsync();

                if (sub.RefPoSubId > 0)
                    await AdjustPoBalanceAsync(sub.RefPoSubId, sub.Qty, 0, "Performa-Inv Deletion");
            }

            // ADD or UPDATE children
            foreach (var subVM in incomingSubVMs)
            {
                if (subVM.InvSubId == 0)
                {
                    var newSub = _mapper.Map<PerformaInvSub>(subVM);
                    newSub.InvId = existingPerformaInv.InvId;
                    await _unitOfWork.PerformaInvSubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.Qty}");

                    if (subVM.RefPoSubId > 0)
                        await AdjustPoBalanceAsync(subVM.RefPoSubId, 0, subVM.Qty ?? 0, "Performa-Inv Creation");
                }
                else
                {
                    var existingSub = existingPerformaInv.PerformaInvSubs.FirstOrDefault(s => s.InvSubId == subVM.InvSubId);
                    if (existingSub != null)
                    {
                        if (subVM.RefPoSubId > 0)
                            await AdjustPoBalanceAsync(subVM.RefPoSubId, existingSub.Qty, subVM.Qty ?? 0, "Performa-Inv Update");

                        var subChanges = GetPropertyChanges(existingSub, subVM);
                        if (!string.IsNullOrEmpty(subChanges))
                            changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");

                        _mapper.Map(subVM, existingSub);
                    }
                }
            }
        }

        private async Task UpdateCostCenterAsync(int? costCenterId, bool assign, StringBuilder changes)
        {
            if (!costCenterId.HasValue || costCenterId == 0) return;

            var cost = await _unitOfWork.CostCenters.GetAsync(costCenterId.Value);
            if (cost != null && cost.AssToMfgPO != assign)
            {
                cost.AssToMfgPO = assign;
                await _unitOfWork.CostCenters.UpdateAsync(cost);
                await _unitOfWork.SaveAsync();
                changes.AppendLine($"CostCenter Updated - ProjectNo {cost.ProjectNo}: AssToMfgPo set to '{assign}'");
            }
        }

        private async Task AdjustPoBalanceAsync(int? refPoSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refPoSubId.HasValue || refPoSubId == 0) return;

                var poSub = await _unitOfWork.MfgPoSubs.GetAsync(refPoSubId.Value);
                if (poSub == null) return;

                // Rollback old qty first
                if (oldQty > 0)
                    poSub.PerformaBalQty += oldQty;

                // Apply new qty
                if (newQty > poSub.PerformaBalQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Po PerformaBalQty.");

                if (newQty > 0)
                    poSub.PerformaBalQty -= newQty;

                await _unitOfWork.MfgPoSubs.UpdateAsync(poSub);
                await _unitOfWork.SaveAsync();
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustPoBalanceAsync] Validation failed in {context}");
                throw; // rethrow so UI/business logic can show proper error
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustPoBalanceAsync] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to adjust Po balance. Please contact support.");
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
                UserName: await _currentUserService.GetUsernameAsync(),
                Machine: _currentUserService.MachineName,
                IP_Address: _currentUserService.IpAddress,
                screen: "Enquiry Sales",
                action: action,
                additionalInfo: changes.ToString()
            );
        }


        public async Task UpdatedCancelStatusAndAddOrRevertQty(PerformaInvVM performaInvVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingPi = await _unitOfWork.PerformaInvs.GetAsync(performaInvVM.InvId);
                if (existingPi == null)
                    throw new InvalidOperationException("Performa Invoice not found.");

                var subs = await _unitOfWork.PerformaInvSubs
                    .GetQueryable()
                    .Where(s => s.InvId == performaInvVM.InvId)
                    .ToListAsync();

                if (!performaInvVM.InvCancl)
                {
                    foreach (var sub in subs)
                    {
                        await ValidatePoBalanceBeforeRevertAsync(sub);
                    }
                }

                existingPi.InvCancl = performaInvVM.InvCancl;
                existingPi.CancelDate = performaInvVM.CancelDate;
                existingPi.CancelReason = performaInvVM.CancelReason;
                existingPi.CancelBy = performaInvVM.CancelBy;

                await _unitOfWork.PerformaInvs.UpdateAsync(existingPi);
                await _unitOfWork.SaveAsync();

                foreach (var sub in subs)
                {
                    if (existingPi.InvCancl)
                    {
                        if (sub.RefPoSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustPoBalanceAsync(
                                sub.RefPoSubId.Value,
                                sub.Qty,
                                0,
                                $"Performa Invoice Cancelled - {existingPi.InvNo}"
                            );
                        }
                    }
                    else
                    {
                        if (sub.RefPoSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustPoBalanceAsync(
                                sub.RefPoSubId.Value,
                                0,
                                sub.Qty,
                                $"Manufacturing Quotation Reverted - {existingPi.InvNo}"
                            );
                        }
                    }
                }

                await transaction.CommitAsync();
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

        public async Task ValidatePoBalanceBeforeRevertAsync(PerformaInvSub sub)
        {
            if (sub.RefPoSubId.GetValueOrDefault() <= 0)
                return;

            var entity = await _unitOfWork.MfgPoSubs.GetAsync(sub.RefPoSubId.Value);
            if (entity == null)
                throw new InvalidOperationException($"Sales Order not found for RefPoSubId: {sub.RefPoSubId}");

            if (entity.PerformaBalQty < sub.Qty)
            {
                throw new InvalidOperationException(
                    $"Cannot revert because Sales Order balance ({entity.PerformaBalQty}) is less than required quantity ({sub.Qty})."
                );
            }
        }

        public async Task DeleteAndResequenceAsync(PerformaInvSubVM subitem, PerformaInvVM performaVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.InvSubId > 0)
                {
                    var entity = await _unitOfWork.PerformaInvSubs.GetAsync(subitem.InvSubId);
                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");

                    // Restore balance qty
                    if (entity.RefPoSubId > 0)
                    {
                        await AdjustPoBalanceAsync(subitem.RefPoSubId, entity.Qty, 0, "Performa-Inv Deletion");
                    }

                    // Delete from DB
                    await _unitOfWork.PerformaInvSubs.DeleteAsync(entity.InvSubId);
                    await _unitOfWork.SaveAsync();

                    // Log action
                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Performa invoice",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"Performa Invoice Number: {performaVM?.InvNo}"
                    );
                }
                else
                {
                    performaVM.PerformaInvSubVMs.Remove(subitem);
                    return;
                }

                var remaining = await _unitOfWork.PerformaInvSubs
                    .GetQueryable()
                    .Where(x => x.InvId == performaVM.InvId)
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

        public async Task<List<PerformaInvSub>> GetPerformaSubByInvIdAsync(int invId)
        {
            try
            {
                var subs = await _unitOfWork.PerformaInvSubs
                    .GetQueryable()
                    .Where(s => s.InvId == invId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return subs;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Manufacturing Performa Invoice items for InvId: {invId}");
                throw new InvalidOperationException("Failed to retrieve PerformaInvoice sub-items. Please try again.");
            }
        }

        public async Task<List<Dictionary<string, object>>> GetPoDetailsByCustId(int custId)
        {
            try
            {
                var result = await (from p in _unitOfWork.MfgPos.GetQueryable()
                                    join ps in _unitOfWork.MfgPoSubs.GetQueryable()
                                        on p.PoId equals ps.PoId
                                    where p.CustId == custId
                                          && !p.PoTally
                                          && !p.PoCancl
                                          && !p.ShortClose
                                          && !ps.ItemCancel
                                          && ps.PerformaBalQty > 0
                                          && p.MfgORLab == true
                                    select new
                                    {
                                        ps.PoSubId,
                                        p.PONo,
                                        p.Suffix,
                                        p.PODate,
                                        ps.ItemId,
                                        ps.Item.ItemCode,
                                        ps.Item.ItemName,
                                        ps.Item.MeasureUnit,
                                        ps.Item.HSNCode,
                                        ps.Qty,
                                        ps.PerformaBalQty,
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
                    ["PoNo"] = $"{r.PONo}{r.Suffix}",
                    ["PoDate"] = r.PODate,
                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["UOM"] = r.MeasureUnit ?? string.Empty,
                    ["HSNCode"] = r.HSNCode ?? string.Empty,
                    ["Qty"] = r.Qty,
                    ["PerformaBalQty"] = r.PerformaBalQty,
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

        public async Task<int> GetPendingPoCountAsync(int custId)
        {
            return await _unitOfWork.MfgPos
                 .GetQueryable()
                 .Where(po => po.CustId == custId
                              && po.PoTally == false
                              && po.MfgPoSubs.Any(sub => sub.PerformaBalQty > 0))
                 .CountAsync();
        }











        public async Task<string?> GetLastWoNoAsync()
        {
            return await _unitOfWork.MfgPoSubs.GetQueryable()
                .Where(s => !string.IsNullOrEmpty(s.WoNO))
                .OrderByDescending(s => s.PoSubId)
                .Select(s => s.WoNO)
                .FirstOrDefaultAsync();
        }

        public string GenerateNextWoNo(string lastWoNo)
        {
            if (!string.IsNullOrEmpty(lastWoNo))
            {
                int i = lastWoNo.Length - 1;
                while (i >= 0 && char.IsDigit(lastWoNo[i]))
                    i--;

                string prefix = lastWoNo.Substring(0, i + 1);
                string numericPart = lastWoNo.Substring(i + 1);

                if (int.TryParse(numericPart, out int number))
                {
                    int length = numericPart.Length;
                    return prefix + (number + 1).ToString("D" + length);
                }
            }
            return "";
        }
        public async Task<bool> IsDuplicatePoAsync(string poNo, string suffix, int? currentPoId = null, int? custId = null)
        {
            if (string.IsNullOrWhiteSpace(poNo))
                return false;

            try
            {
                return await _unitOfWork.MfgPos
                    .GetQueryable()
                    .AnyAsync(x => x.PONo == poNo
                                && x.Suffix == suffix
                                && x.CustId == custId
                                && (currentPoId == null || x.PoId != currentPoId));
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in IsDuplicatePoAsync for PoNO: {poNo}");
                throw new InvalidOperationException("Failed to check duplicate PO.");
            }
        }



        public async Task<List<PerformaInvSubVM>> GetDistinctRefPoByPerformaInvIdAsync(int invId)
        {
            return await _unitOfWork.PerformaInvSubs
                .GetQueryable()
                .Where(s => s.InvId == invId)
                .GroupBy(s => new { s.RefPoNo, s.RefPoDate })
                .Select(g => new PerformaInvSubVM
                {
                    RefPoNo = g.Key.RefPoNo,
                    RefPoDate = g.Key.RefPoDate
                })
                .ToListAsync();
        }

        private async Task<string?> GetSeriesNoAsync(int poTypeId)
        {
            return await _unitOfWork.PoTypes
                .GetQueryable()
                .Where(p => p.Id == poTypeId)
                .Select(p => p.SeriesNo)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateItemCancelAsync(MfgPoSubVM subItem)
        {
            if (subItem == null)
                throw new ArgumentNullException(nameof(subItem), "Subitem cannot be null.");

            if (subItem.PoSubId == 0)
                throw new ArgumentException("Cannot update unsaved subitem.");

            try
            {
                var entity = await _unitOfWork.MfgPoSubs
                    .FirstOrDefaultAsync(x => x.PoSubId == subItem.PoSubId);

                if (entity == null)
                    throw new KeyNotFoundException($"Subitem with PoSubId {subItem.PoSubId} not found.");

                entity.ItemCancel = subItem.ItemCancel;

                await _unitOfWork.MfgPoSubs.UpdateAsync(entity);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in UpdateItemCancelAsync for ItemCode {subItem.ItemCode}");
                throw;
            }
        }

        public async Task<string> GetNextOANumberAsync(string suffix)
        {
            try
            {
                var lastPo = await _unitOfWork.MfgPos
                    .GetQueryable()
                    .Where(q => !string.IsNullOrEmpty(q.OANo) && q.Suffix == suffix)
                    .OrderByDescending(q => q.PoId)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;

                if (lastPo != null)
                {
                    var parts = lastPo.OANo.Split('/');
                    if (parts.Length > 0 && int.TryParse(parts[0], out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }

                return $"{nextNumber}{suffix}";
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating OA number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate Order Acceptance number.");
            }
        }

        public async Task<string> GetLastOATermsAsync()
        {
            try
            {
                var lastTerms = await _unitOfWork.MfgPos
                    .GetQueryable()
                    .Where(p => !string.IsNullOrEmpty(p.OATerms))
                    .OrderByDescending(p => p.PoId)
                    .Select(p => p.OATerms)
                    .FirstOrDefaultAsync();

                return lastTerms ?? string.Empty;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching last OA Terms");
                return string.Empty;
            }
        }
    }

}
