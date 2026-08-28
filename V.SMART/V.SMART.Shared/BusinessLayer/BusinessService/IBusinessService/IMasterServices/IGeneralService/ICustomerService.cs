using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IGeneralService
{
    public interface ICustomerService
    {
        Task<(bool CanDelete, string Message)> CanDeleteCustomerAsync(int custId);
        Task<(List<CustomerVM> customerVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
        Task<bool> DeleteCustomerByCustIdAsync(int custId);

        Task<List<CustomerVM>> GetCustomerListAsync(string topic, DateTime fromDate, DateTime toDate);

        // ------------------------------------------------------------------
        // M2-D02-01 — business logic extracted from CustomerUpsert.razor's @code block.
        // See docs/kb/business-rules/customer-master-rules.md for the triage and the
        // BR-CUST-001..018 rules each member carries.
        // ------------------------------------------------------------------

        /// <summary>Loads a customer with its consignees and contact persons (BR-CUST-017).</summary>
        Task<CustomerVM?> GetCustomerByIdAsync(int custId);

        /// <summary>
        /// Performs a whole create-or-update in one call: duplicate-name check, validation,
        /// opening-balance derivation, audit stamping, child-collection synchronisation,
        /// transaction and user-action log (BR-CUST-001, -010, -011, -012, -013).
        /// </summary>
        Task<(bool Success, string Message, IReadOnlyList<string> Errors, CustomerVM? Customer)> UpsertCustomerAsync(CustomerVM vm);

        /// <summary>
        /// Independently callable validation. Returns the legacy message strings verbatim
        /// (BR-CUST-001, -006, -007, -008, -009, -018). Side effect preserved from the legacy
        /// page: on a matched state it writes <c>vm.StateName</c> (BR-CUST-008).
        /// </summary>
        Task<IReadOnlyList<string>> ValidateCustomerAsync(CustomerVM vm);

        /// <summary>Applies the business-type -> customer-type default (BR-CUST-004).</summary>
        CustomerVM ApplyBusinessTypeDefaults(CustomerVM vm);

        /// <summary>The customer types allowed for a business type (BR-CUST-004).</summary>
        IReadOnlyList<string> GetCustomerTypesForBusinessType(string? busiType);

        /// <summary>Resolves <c>SupTyp</c> for a business type, keeping a still-valid value (BR-CUST-004).</summary>
        string? ResolveSupplyType(string? busiType, string? currentSupTyp);

        /// <summary>Derives PAN from GST (BR-CUST-002).</summary>
        string? DerivePanFromGst(string? gstNo);

        /// <summary>Normalises a customer GST: upper-cased and trimmed (BR-CUST-003).</summary>
        string NormalizeCustomerGst(string? gstNo);

        /// <summary>Normalises a consignee GST: trimmed only, deliberately not upper-cased (BR-CUST-003).</summary>
        string NormalizeConsigneeGst(string? gstNo);

        /// <summary>True when the business type forces GST = "URP" and state 99 (BR-CUST-005).</summary>
        bool IsImportOrExport(string? busiType);

        /// <summary>
        /// True when switching away from Imports/Exports must clear GST, PAN and state
        /// (BR-CUST-005).
        /// </summary>
        bool ShouldClearOnBusinessTypeSwitch(string? currentGstNo);
    }
}
