using AutoMapper;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IGeneralService;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using static MudBlazor.Icons;

namespace V.SMART.Shared.BusinessLayer.BusinessService.MasterService.GeneralService
{
    public class CustomerService : ICustomerService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILoggingService _logs;
        private readonly CurrentUserService _userService;
        private readonly ICommonService _commonService;
        private readonly ForeignKeyUsageChecker _fkChecker;

        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper, ILoggingService loggingService,
                            CurrentUserService userService, ICommonService commonService,
                            ForeignKeyUsageChecker foreignKeyUsageChecker)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logs = loggingService;
            _userService = userService;
            _commonService = commonService;
            _fkChecker = foreignKeyUsageChecker;
        }


        public async Task<(List<CustomerVM> customerVMs, int TotalCount)>SearchWithDynamicFilterAsync(int pageNumber,int pageSize,Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.Customers
                    .GetQueryable()
                    .Include(x => x.Currency)
                    .AsNoTracking();

                // Apply Filters
                if (filters != null && filters.Any())
                {
                    foreach (var filter in filters)
                    {
                        query = CustomerFilterBuilder
                            .ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.CustId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<CustomerVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,"Error in SearchWithDynamicFilterAsync (Customer)");
                throw new InvalidOperationException("Failed to load customer list.", ex);
            }
        }

        public static class CustomerFilterBuilder
        {
            public static IQueryable<Customer> ApplyFilter(
                IQueryable<Customer> query,
                string field,
                object value)
            {
                if (value == null) return query;

                var val = value.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(val))
                    return query;

                switch (field)
                {
                    case "Customer":
                        return query.Where(x =>
                            x.CustName != null &&
                            EF.Functions.Like(x.CustName, $"%{val}%"));

                    case "CreatedBy":
                        return query.Where(x =>
                            x.CreatedBy != null &&
                            EF.Functions.Like(x.CreatedBy, $"%{val}%"));

                    case "FromDate":
                        if (DateTime.TryParse(val, out var fromDate))
                            return query.Where(x =>
                                x.CreatedDate >= fromDate.Date);
                        return query;

                    case "ToDate":
                        if (DateTime.TryParse(val, out var toDate))
                            return query.Where(x =>
                                x.CreatedDate <= toDate.Date
                                    .AddDays(1)
                                    .AddTicks(-1));
                        return query;

                    case "Status":
                        return ApplyStatusFilter(query, val);

                    default:
                        return query;
                }
            }

            private static IQueryable<Customer> ApplyStatusFilter(
                IQueryable<Customer> query,
                string status)
            {
                return status switch
                {
                    "Active" => query.Where(x => !x.Inactive),
                    "In Active" => query.Where(x => x.Inactive),
                    _ => query
                };
            }
        }


        public async Task<(bool CanDelete, string Message)> CanDeleteCustomerAsync(int custId)
        {
            try
            {
                var customer = await _unitOfWork.Customers
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.CustId == custId);

                if (customer == null)
                    return (false, "Customer not found or already removed.");

                var usedIn = await _fkChecker.GetUsageTableAsync<Customer>(custId);

                if (usedIn != null)
                    return (false, $"Cannot delete Customer '{customer.CustName}' because it is used in {usedIn} Screen.");

                if (customer.Inactive)
                    return (false, "Customer is inactive. You cannot delete this record.");

                return (true, $"Customer '{customer.CustId}' can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Customer delete validation failed: {custId}");
                return (false, "Unexpected error occurred while validating Customer.");
            }
        }


        public async Task<bool> DeleteCustomerByCustIdAsync(int custId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Get the customer with its sub-items
                var customer = await _unitOfWork.Customers
                    .GetQueryable()
                    .FirstOrDefaultAsync(e => e.CustId == custId);

                if (customer == null)
                {
                    return false;
                }

                var changes = new StringBuilder();

                // Delete the customer
                await _unitOfWork.Customers.DeleteAsync(customer);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Log the user action
                await _logs.LogUserAction(
                    UserName: await _userService.GetUsernameAsync(),
                    Machine: _userService.MachineName,
                    IP_Address: _userService.IpAddress,
                    screen: "Customer List",
                    action: $"Deleted Customer: {customer.CustName}",
                    additionalInfo: $"Customer Id: {customer.CustId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Customer: {custId}");
                throw;
            }
        }


        public async Task<List<CustomerVM>> GetCustomerListAsync(string topic, DateTime fromDate, DateTime toDate)
        {
            try
            {
                IQueryable<Customer> query;

                // ================= SALES + LABOUR =================
                if (topic == "Sales")
                {
                    query =
                    (
                        from c in _unitOfWork.Customers.GetQueryable().AsNoTracking()
                        join si in _unitOfWork.MfgInvs.GetQueryable().AsNoTracking()
                            on c.CustId equals si.CustId
                        where si.InvDate >= fromDate
                              && si.InvDate < toDate.AddDays(1)
                        select c
                    )
                    .Union
                    (
                        from c in _unitOfWork.Customers.GetQueryable().AsNoTracking()
                        join li in _unitOfWork.LabInvs.GetQueryable().AsNoTracking()
                            on c.CustId equals li.CustId
                        where li.LabInvDate >= fromDate
                              && li.LabInvDate < toDate.AddDays(1)
                        select c
                    );
                }

                // ================= EXPORTS =================
                else if (topic == "Exports")
                {
                    query =
                    (
                        from c in _unitOfWork.Customers.GetQueryable().AsNoTracking()
                        join ei in _unitOfWork.ExpInvs.GetQueryable().AsNoTracking()
                            on c.CustId equals ei.CustId
                        where ei.ExpInvDate >= fromDate
                              && ei.ExpInvDate < toDate.AddDays(1)
                        select c
                    );
                }
                else
                {
                    return new List<CustomerVM>();
                }

                var customers = await query
                    .Distinct()
                    .OrderBy(x => x.CustName)
                    .ToListAsync();

                return _mapper.Map<List<CustomerVM>>(customers);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error loading customer list");
                throw;
            }
        }


        // ==================================================================================
        // M2-D02-01 — business logic extracted, unchanged, from CustomerUpsert.razor's @code.
        //
        // Every message string, regex, branch and ordering below is a verbatim move from the
        // Razor page. The triage that justifies each move, and the BR-CUST-001..018 rules the
        // members carry, are in docs/kb/business-rules/customer-master-rules.md.
        //
        // The pure parts are `static` so they can be characterised by unit tests without a
        // DbContext, a mapper or a user service; the instance members simply delegate.
        // ==================================================================================

        /// <summary>GST value forced for Imports/Exports customers (BR-CUST-005).</summary>
        public const string UrpGstNo = "URP";

        /// <summary>The "URP" state's <c>StateCode</c> (BR-CUST-005).</summary>
        public const int UrpStateCode = 99;

        /// <summary>BR-CUST-006 — CustomerUpsert.razor:1163 (pre-extraction).</summary>
        public const string GstPattern = @"^(URP|[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1})$";

        /// <summary>BR-CUST-007 — CustomerUpsert.razor:1148/:1173 (pre-extraction).</summary>
        public const string PanPattern = @"^[A-Z]{5}[0-9]{4}[A-Z]{1}$";

        /// <summary>BR-CUST-004 — customer types for Local / InterState.</summary>
        public static readonly IReadOnlyList<string> DomesticCustomerTypes = new[] { "B2B", "SEZWP", "SEZWOP" };

        /// <summary>BR-CUST-004 — customer types for Imports / Exports.</summary>
        public static readonly IReadOnlyList<string> OverseasCustomerTypes = new[] { "SEZWP", "SEZWOP", "EXPWP", "EXPWOP" };

        // ---------------------------------------------------------------- derivation helpers

        /// <summary>
        /// BR-CUST-002. Verbatim from CustomerUpsert.razor:729-736 (consignee) and :753-761
        /// (customer). The customer handler used <c>string.IsNullOrEmpty</c> and the consignee
        /// handler <c>string.IsNullOrWhiteSpace</c>; the two differ only for an all-whitespace
        /// 15-character GST, which the customer path cannot reach because it trims first.
        /// </summary>
        public static string DerivePan(string? gstNo)
        {
            if (!string.IsNullOrWhiteSpace(gstNo) && gstNo.Length == 15)
            {
                return gstNo.Substring(2, 10);
            }

            return string.Empty;
        }

        /// <summary>BR-CUST-003 — CustomerUpsert.razor:750.</summary>
        public static string NormalizeCustomerGstValue(string? gstNo) => (gstNo ?? string.Empty).ToUpper().Trim();

        /// <summary>
        /// BR-CUST-003 — CustomerUpsert.razor:727. Trimmed only, deliberately **not**
        /// upper-cased. The asymmetry is preserved, not fixed (see Q-106).
        /// </summary>
        public static string NormalizeConsigneeGstValue(string? gstNo) => (gstNo ?? string.Empty).Trim();

        /// <summary>BR-CUST-005 — CustomerUpsert.razor:1234 and :1145 use this same test.</summary>
        public static bool IsImportOrExportBusinessType(string? busiType) =>
            busiType == "Imports" || busiType == "Exports";

        /// <summary>BR-CUST-005 — the asymmetric clear-on-switch-back, CustomerUpsert.razor:1245.</summary>
        public static bool ShouldClearOnBusinessTypeSwitchCore(string? currentGstNo) =>
            string.IsNullOrWhiteSpace(currentGstNo)
            || currentGstNo.Length < 15
            || currentGstNo.Equals(UrpGstNo, StringComparison.OrdinalIgnoreCase);

        /// <summary>BR-CUST-004 — CustomerUpsert.razor:850-877.</summary>
        public static IReadOnlyList<string> GetCustomerTypes(string? busiType)
        {
            if (string.IsNullOrEmpty(busiType))
            {
                return Array.Empty<string>();
            }

            switch (busiType)
            {
                case "Local":
                case "InterState":
                    return DomesticCustomerTypes;

                case "Imports":
                case "Exports":
                    return OverseasCustomerTypes;

                default:
                    return Array.Empty<string>();
            }
        }

        /// <summary>
        /// BR-CUST-004 — CustomerUpsert.razor:850-877. An empty business type leaves the current
        /// value untouched (the legacy method returned before the switch); an unrecognised
        /// business type clears it to null.
        /// </summary>
        public static string? ResolveSupplyTypeCore(string? busiType, string? currentSupTyp)
        {
            if (string.IsNullOrEmpty(busiType))
            {
                return currentSupTyp;
            }

            switch (busiType)
            {
                case "Local":
                case "InterState":
                    return string.IsNullOrEmpty(currentSupTyp) || !DomesticCustomerTypes.Contains(currentSupTyp)
                        ? "B2B"
                        : currentSupTyp;

                case "Imports":
                case "Exports":
                    return string.IsNullOrEmpty(currentSupTyp) || !OverseasCustomerTypes.Contains(currentSupTyp)
                        ? "SEZWP"
                        : currentSupTyp;

                default:
                    return null;
            }
        }

        /// <summary>
        /// BR-CUST-010 — CustomerUpsert.razor:981-990. Applied on create **and** update, so an
        /// edit re-stamps the pending balance and its date. Preserved, not fixed (see Q-107).
        /// </summary>
        public static void ApplyOpeningBalance(Customer customer)
        {
            if (customer.OpenBal.HasValue)
            {
                customer.OpenBalPndg = customer.OpenBal;
                customer.OpenBalDate = DateTime.Now;
            }
            else
            {
                customer.OpenBalPndg = null;
                customer.OpenBalDate = null;
            }
        }

        // ---------------------------------------------------------------- child-collection diff

        /// <summary>
        /// BR-CUST-012 — CustomerUpsert.razor:1000/:1007/:1047/:1073. Rows with a blank
        /// <c>AltCustName</c> are silently discarded. Preserved, not fixed (see Q-108).
        /// </summary>
        public static IEnumerable<CustomerIndirectVM> PersistableIndirects(IEnumerable<CustomerIndirectVM>? indirects) =>
            (indirects ?? Enumerable.Empty<CustomerIndirectVM>())
                .Where(i => !string.IsNullOrWhiteSpace(i.AltCustName));

        /// <summary>BR-CUST-012 — the contact-person equivalent.</summary>
        public static IEnumerable<ContactPersonVM> PersistableContacts(IEnumerable<ContactPersonVM>? contacts) =>
            (contacts ?? Enumerable.Empty<ContactPersonVM>())
                .Where(c => !string.IsNullOrWhiteSpace(c.ContactPersonName));

        /// <summary>
        /// BR-CUST-013 — CustomerUpsert.razor:1028/:1035-1042. The retained set is built from
        /// **ids**, not names, so an existing row whose name was blanked is retained rather than
        /// deleted. Preserved verbatim (see Q-108).
        /// </summary>
        public static List<int> IndirectIdsToDelete(
            IEnumerable<CustomerIndirect>? original,
            IEnumerable<CustomerIndirectVM>? current)
        {
            var currentIndirectIds = (current ?? Enumerable.Empty<CustomerIndirectVM>())
                .Where(i => i.AltCustId != 0)
                .Select(i => i.AltCustId)
                .ToHashSet();

            return (original ?? Enumerable.Empty<CustomerIndirect>())
                .Where(o => !currentIndirectIds.Contains(o.AltCustId))
                .Select(o => o.AltCustId)
                .ToList();
        }

        /// <summary>BR-CUST-013 — the contact-person equivalent, CustomerUpsert.razor:1029/:1064.</summary>
        public static List<int> ContactIdsToDelete(
            IEnumerable<ContactPerson>? original,
            IEnumerable<ContactPersonVM>? current)
        {
            var currentContactIds = (current ?? Enumerable.Empty<ContactPersonVM>())
                .Where(c => c.Id != 0)
                .Select(c => c.Id)
                .ToHashSet();

            return (original ?? Enumerable.Empty<ContactPerson>())
                .Where(o => !currentContactIds.Contains(o.Id))
                .Select(o => o.Id)
                .ToList();
        }

        // ---------------------------------------------------------------- validation

        /// <summary>
        /// BR-CUST-006 through BR-CUST-009 and BR-CUST-018 — the whole of the legacy
        /// <c>ValidateCustomer</c> (CustomerUpsert.razor:1127-1213), message strings verbatim.
        /// The <c>customer.StateName</c> write at :1185 is a side effect of the legacy method and
        /// is preserved here (BR-CUST-008).
        /// </summary>
        public static List<string> ValidateCustomerFields(
            CustomerVM customer,
            IReadOnlyList<State> states,
            IEnumerable<CustomerIndirectVM>? indirects)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(customer.CustName))
            {
                errors.Add("Customer Name is required.");
            }
            if (string.IsNullOrWhiteSpace(customer.BusiType))
            {
                errors.Add("Business Type is required.");
            }

            string? busiType = customer.BusiType?.Trim();
            if (IsImportOrExportBusinessType(busiType))
            {
                if (!string.Equals(customer.GSTNo, UrpGstNo, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add("For Imports/Exports, GST No must be 'URP'.");
                }
                // For Imports/Exports, PAN is not strictly tied to GST, but if it exists, it should be valid
                if (!string.IsNullOrWhiteSpace(customer.PANNo) && !Regex.IsMatch(customer.PANNo, PanPattern))
                {
                    errors.Add("Invalid PAN No format for Imports/Exports.");
                }
            }
            else // Local, InterState
            {
                if (string.IsNullOrWhiteSpace(customer.GSTNo))
                {
                    errors.Add("Please enter GST No.");
                }
                else if (customer.GSTNo.Length != 15)
                {
                    errors.Add("GST No must be 15 characters.");
                }
                else if (!Regex.IsMatch(customer.GSTNo, GstPattern))
                {
                    errors.Add("Invalid GST No format.");
                }

                if (string.IsNullOrWhiteSpace(customer.PANNo))
                {
                    errors.Add("Please enter PAN No.");
                }
                else if (!Regex.IsMatch(customer.PANNo, PanPattern))
                {
                    errors.Add("Invalid PAN No format.");
                }
            }

            var matchedState = states?.FirstOrDefault(s => s.StateCode == customer.StateId);
            if (customer.StateId == 0 || matchedState == null)
            {
                errors.Add("Please select a valid State.");
            }
            else
            {
                customer.StateName = matchedState.StateName;
            }

            foreach (var indirect in PersistableIndirects(indirects))
            {
                if (string.IsNullOrWhiteSpace(indirect.GSTNo))
                {
                    errors.Add($"Consignee '{indirect.AltCustName}': Please enter GST No.");
                }
                else if (indirect.GSTNo.Length != 15)
                {
                    errors.Add($"Consignee '{indirect.AltCustName}': GST No must be 15 characters.");
                }
                else if (!Regex.IsMatch(indirect.GSTNo, GstPattern))
                {
                    errors.Add($"Consignee '{indirect.AltCustName}': Invalid GST No format.");
                }

                if (string.IsNullOrWhiteSpace(indirect.PANNo))
                {
                    errors.Add($"Consignee '{indirect.AltCustName}': Please enter PAN No.");
                }
                else if (!Regex.IsMatch(indirect.PANNo, PanPattern))
                {
                    errors.Add($"Consignee '{indirect.AltCustName}': Invalid PAN No format.");
                }
            }

            return errors;
        }

        // ---------------------------------------------------------------- ICustomerService members

        public string? DerivePanFromGst(string? gstNo) => DerivePan(gstNo);

        public string NormalizeCustomerGst(string? gstNo) => NormalizeCustomerGstValue(gstNo);

        public string NormalizeConsigneeGst(string? gstNo) => NormalizeConsigneeGstValue(gstNo);

        public bool IsImportOrExport(string? busiType) => IsImportOrExportBusinessType(busiType);

        public bool ShouldClearOnBusinessTypeSwitch(string? currentGstNo) =>
            ShouldClearOnBusinessTypeSwitchCore(currentGstNo);

        public IReadOnlyList<string> GetCustomerTypesForBusinessType(string? busiType) => GetCustomerTypes(busiType);

        public string? ResolveSupplyType(string? busiType, string? currentSupTyp) =>
            ResolveSupplyTypeCore(busiType, currentSupTyp);

        public CustomerVM ApplyBusinessTypeDefaults(CustomerVM vm)
        {
            if (vm == null)
            {
                return vm!;
            }

            vm.SupTyp = ResolveSupplyTypeCore(vm.BusiType, vm.SupTyp);
            return vm;
        }

        public async Task<CustomerVM?> GetCustomerByIdAsync(int custId)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetCustomerWithAllRelatedDataAsync(custId);

                return customer == null ? null : _mapper.Map<CustomerVM>(customer);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetCustomerByIdAsync (Customer): {custId}");
                throw;
            }
        }

        /// <summary>
        /// BR-CUST-001 first, exactly as the legacy page did: on a duplicate name the save aborts
        /// with that message alone and the field validations never run
        /// (CustomerUpsert.razor:960-977).
        /// </summary>
        public async Task<IReadOnlyList<string>> ValidateCustomerAsync(CustomerVM vm)
        {
            bool isDuplicate = await _unitOfWork.Customers.ExistsByNameAsync(
                "CustName",
                vm.CustName?.Trim(),
                "CustId",
                vm.CustId == 0 ? null : vm.CustId);

            if (isDuplicate)
            {
                return new List<string> { "Customer name already exists." };
            }

            var states = (await _unitOfWork.States.GetAllAsync())?.ToList() ?? new List<State>();

            return ValidateCustomerFields(vm, states, vm.CustomerIndirectVMs);
        }

        /// <summary>
        /// The legacy <c>UpsertCustomer</c> (CustomerUpsert.razor:954-1125) minus its three
        /// presentation concerns — the toasts, the NavigationGuard JS interop and the
        /// navigation — which stay in the page. The transaction, the per-row SaveAsync sequence
        /// (deliberately not optimised), the audit stamping, the LogUserAction entry with screen
        /// "Customer" and the rollback-on-exception all move here unchanged.
        /// </summary>
        public async Task<(bool Success, string Message, IReadOnlyList<string> Errors, CustomerVM? Customer)> UpsertCustomerAsync(CustomerVM vm)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var errors = await ValidateCustomerAsync(vm);
                if (errors.Count > 0)
                {
                    return (false, string.Empty, errors, null);
                }

                var currentUsername = await _userService.GetUsernameAsync();

                if (vm.CustId == 0)
                {
                    var customer = _mapper.Map<Customer>(vm);

                    // The children are inserted row by row below, exactly as the legacy page did.
                    // Leaving them on the navigation property would make EF insert them again as
                    // part of the parent's INSERT.
                    customer.CustomerIndirects = new List<CustomerIndirect>();
                    customer.ContactPersons = new List<ContactPerson>();

                    ApplyOpeningBalance(customer);

                    customer.CreatedBy = currentUsername;
                    customer.CreatedDate = DateTime.Now;
                    // BR-CUST-011: create never stamps Modified*. CustomerMapping sets
                    // ModifiedDate = DateTime.Now unconditionally, so it is cleared back here.
                    customer.ModifiedBy = null;
                    customer.ModifiedDate = null;

                    await _unitOfWork.Customers.CreateAsync(customer);
                    await _unitOfWork.SaveAsync();

                    foreach (var indirectVm in PersistableIndirects(vm.CustomerIndirectVMs))
                    {
                        var indirect = _mapper.Map<CustomerIndirect>(indirectVm);
                        indirect.CustId = customer.CustId;
                        await _unitOfWork.CustomerIndirects.CreateAsync(indirect);
                        await _unitOfWork.SaveAsync();
                    }

                    foreach (var contactVm in PersistableContacts(vm.ContactPersonVMs))
                    {
                        var contact = _mapper.Map<ContactPerson>(contactVm);
                        contact.CustId = customer.CustId;
                        await _unitOfWork.ContactPersons.CreateAsync(contact);
                        await _unitOfWork.SaveAsync();
                    }

                    await _unitOfWork.SaveAsync();
                    await transaction.CommitAsync();

                    await _logs.LogUserAction(
                        UserName: currentUsername,
                        Machine: _userService.MachineName,
                        IP_Address: _userService.IpAddress,
                        screen: "Customer",
                        action: $"Customer Created: {customer.CustName}",
                        additionalInfo: $"CustomerName: {customer.CustName}");

                    return (true, "Customer Created Successfully", Array.Empty<string>(), _mapper.Map<CustomerVM>(customer));
                }
                else
                {
                    var existing = await _unitOfWork.Customers.GetCustomerWithAllRelatedDataAsync(vm.CustId);
                    if (existing == null)
                    {
                        // Not reachable from the Blazor page, which only edits a customer it has
                        // already loaded. Needed because the service is now addressed by id.
                        return (false, "Customer not found or already removed.", Array.Empty<string>(), null);
                    }

                    // The originally persisted children, captured before mapping: the id-set diff
                    // (BR-CUST-013) is taken against these, and they are the tracked instances.
                    var originalIndirects = existing.CustomerIndirects?.ToList() ?? new List<CustomerIndirect>();
                    var originalContacts = existing.ContactPersons?.ToList() ?? new List<ContactPerson>();
                    var createdBy = existing.CreatedBy;
                    var createdDate = existing.CreatedDate;

                    _mapper.Map(vm, existing);

                    // Restore the tracked child collections that the mapping replaced, and the
                    // create-audit fields, which an update never re-writes (BR-CUST-011).
                    existing.CustomerIndirects = originalIndirects;
                    existing.ContactPersons = originalContacts;
                    existing.CreatedBy = createdBy;
                    existing.CreatedDate = createdDate;

                    ApplyOpeningBalance(existing);

                    existing.ModifiedBy = currentUsername;
                    existing.ModifiedDate = DateTime.Now;

                    await _unitOfWork.Customers.UpdateAsync(existing);

                    foreach (var altCustId in IndirectIdsToDelete(originalIndirects, vm.CustomerIndirectVMs))
                    {
                        await _unitOfWork.CustomerIndirects.DeleteAsync(altCustId);
                        await _unitOfWork.SaveAsync();
                    }

                    foreach (var indirectVm in PersistableIndirects(vm.CustomerIndirectVMs))
                    {
                        if (indirectVm.AltCustId == 0)
                        {
                            var indirect = _mapper.Map<CustomerIndirect>(indirectVm);
                            indirect.CustId = existing.CustId;
                            await _unitOfWork.CustomerIndirects.CreateAsync(indirect);
                            await _unitOfWork.SaveAsync();
                        }
                        else
                        {
                            var tracked = originalIndirects.FirstOrDefault(o => o.AltCustId == indirectVm.AltCustId);
                            if (tracked != null)
                            {
                                _mapper.Map(indirectVm, tracked);
                                await _unitOfWork.CustomerIndirects.UpdateAsync(tracked);
                            }
                            else
                            {
                                await _unitOfWork.CustomerIndirects.UpdateAsync(_mapper.Map<CustomerIndirect>(indirectVm));
                            }

                            await _unitOfWork.SaveAsync();
                        }
                    }

                    foreach (var contactId in ContactIdsToDelete(originalContacts, vm.ContactPersonVMs))
                    {
                        await _unitOfWork.ContactPersons.DeleteAsync(contactId);
                        await _unitOfWork.SaveAsync();
                    }

                    foreach (var contactVm in PersistableContacts(vm.ContactPersonVMs))
                    {
                        if (contactVm.Id == 0)
                        {
                            var contact = _mapper.Map<ContactPerson>(contactVm);
                            contact.CustId = existing.CustId;
                            await _unitOfWork.ContactPersons.CreateAsync(contact);
                            await _unitOfWork.SaveAsync();
                        }
                        else
                        {
                            var tracked = originalContacts.FirstOrDefault(o => o.Id == contactVm.Id);
                            if (tracked != null)
                            {
                                _mapper.Map(contactVm, tracked);
                                await _unitOfWork.ContactPersons.UpdateAsync(tracked);
                            }
                            else
                            {
                                await _unitOfWork.ContactPersons.UpdateAsync(_mapper.Map<ContactPerson>(contactVm));
                            }

                            await _unitOfWork.SaveAsync();
                        }
                    }

                    await _unitOfWork.SaveAsync();
                    await transaction.CommitAsync();

                    await _logs.LogUserAction(
                        UserName: currentUsername,
                        Machine: _userService.MachineName,
                        IP_Address: _userService.IpAddress,
                        screen: "Customer",
                        action: $"Customer Updated: {existing.CustName}",
                        additionalInfo: $"CustomerName: {existing.CustName}");

                    return (true, "Customer Updated Successfully", Array.Empty<string>(), _mapper.Map<CustomerVM>(existing));
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "An error occurred in UpsertCustomer()");
                return (false, "An error occurred while saving the customer.", Array.Empty<string>(), null);
            }
        }

    }
}
