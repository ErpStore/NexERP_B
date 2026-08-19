// =============================================================================
//  DELETE-GUARD TESTS - MfgPoService.CanDeleteSalesOrderAsync (task M0-09, R-08)
// -----------------------------------------------------------------------------
//  Subject:
//      V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SalesService/
//      MfgPoService.cs - CanDeleteSalesOrderAsync (line numbers re-verified
//      2026-08-19: method :465-565, Export Invoice guard :499-505, Contract
//      Review guard :523-526).
//
//  BR-SO-001 (KB-030): a Sales Order cannot be deleted once ANY downstream
//  document exists. The returned Message strings are product UX and are asserted
//  VERBATIM below, including their existing grammatical quirks ("a Invoice",
//  "Export - Invoice", "Performa-Invoice"). Do not correct them.
//
//  BR-SO-002 / R-08 (KB-060) - THE DEFECT THESE TESTS PIN:
//      The Export Invoice guard computed `hasExpInvoice` but tested `hasInvoice`;
//      the Contract Review guard computed `hasCR` but tested `hasRc`. Both guards
//      were therefore unreachable.
//
//  OBSERVED PRE-FIX BEHAVIOUR (recorded 2026-08-19, before the two-identifier fix
//  on branch migration/M0-09-delete-guard-fix). Both of the two new tests were run
//  against the unfixed service and FAILED with exactly this:
//
//      Failed ...CanDeleteSalesOrder_WithOnlyExportInvoice_IsRefused
//        Assert.Equal() Failure: Values differ
//        Expected: Tuple (False, "Cannot delete this Sales Order as a Export - Invoi"...)
//        Actual:   Tuple (True, "Sales Order can be safely deleted.")
//
//      Failed ...CanDeleteSalesOrder_WithOnlyContractReview_IsRefused
//        Assert.Equal() Failure: Values differ
//        Expected: Tuple (False, "Cannot delete this Sales Order as a Contract Revie"...)
//        Actual:   Tuple (True, "Sales Order can be safely deleted.")
//
//      (xUnit truncates the expected string in its diff; the assertions below carry
//       it in full. Run summary: Failed: 2, Passed: 4, Total: 6.)
//
//  The two regression tests (Tax Invoice, Route Card) passed BEFORE the fix and
//  pass after it: they pin the guards whose booleans were being tested by mistake,
//  so a future "tidy-up" cannot re-introduce the swap in the other direction.
//
//  BEHAVIOUR CHANGE this fix makes: a Sales Order whose only downstream document is
//  an export invoice, or only a contract review, USED to be reported deletable and
//  is now refused. That is the intent of M0-09.
//
//  SCOPE: this file covers exactly one method. The audit of the other ~40
//  CanDelete...Async implementations is task M0-10 / INV-025 - not done here.
// =============================================================================

using V.SMART.Shared.Tests.Infrastructure;
using Xunit;

namespace V.SMART.Shared.Tests.Services;

public class MfgPoServiceDeleteGuardTests
{
    /// <summary>
    /// Guard 3 (MfgPoService.cs:499-505). Export invoice only - no tax invoice,
    /// no DC. Before the fix this returned (true, "Sales Order can be safely
    /// deleted.") because the guard tested hasInvoice, which was false.
    /// </summary>
    [Fact]
    public async Task CanDeleteSalesOrder_WithOnlyExportInvoice_IsRefused()
    {
        using var harness = new SalesOrderDeleteGuardHarness();

        var po = harness.AddSalesOrder();
        harness.AddExportInvoiceLine(po.MfgPoSubs.Single().PoSubId);

        var result = await harness.Service.CanDeleteSalesOrderAsync(po.PoId);

        Assert.Equal(
            (false, "Cannot delete this Sales Order as a Export - Invoice transaction exists."),
            result);
    }

    /// <summary>
    /// Guard 6 (MfgPoService.cs:523-526). Contract review only - no route card.
    /// Before the fix this returned (true, "Sales Order can be safely deleted.")
    /// because the guard tested hasRc, which was false.
    /// </summary>
    [Fact]
    public async Task CanDeleteSalesOrder_WithOnlyContractReview_IsRefused()
    {
        using var harness = new SalesOrderDeleteGuardHarness();

        var po = harness.AddSalesOrder();
        harness.AddContractReview(po.PoId);

        var result = await harness.Service.CanDeleteSalesOrderAsync(po.PoId);

        Assert.Equal(
            (false, "Cannot delete this Sales Order as a Contract Review transaction exists."),
            result);
    }

    /// <summary>
    /// Regression - guard 2 (MfgPoService.cs:491-497), whose boolean the Export
    /// Invoice guard was wrongly testing. Passes before AND after the fix.
    /// </summary>
    [Fact]
    public async Task CanDeleteSalesOrder_WithTaxInvoice_IsRefused()
    {
        using var harness = new SalesOrderDeleteGuardHarness();

        var po = harness.AddSalesOrder();
        harness.AddTaxInvoiceLine(po.MfgPoSubs.Single().PoSubId);

        var result = await harness.Service.CanDeleteSalesOrderAsync(po.PoId);

        Assert.Equal(
            (false, "Cannot delete this Sales Order as a Invoice transaction exists."),
            result);
    }

    /// <summary>
    /// Regression - guard 5 (MfgPoService.cs:519-521), whose boolean the Contract
    /// Review guard was wrongly testing. Passes before AND after the fix.
    /// </summary>
    [Fact]
    public async Task CanDeleteSalesOrder_WithRouteCard_IsRefused()
    {
        using var harness = new SalesOrderDeleteGuardHarness();

        var po = harness.AddSalesOrder();
        harness.AddRouteCard(po.PoId);

        var result = await harness.Service.CanDeleteSalesOrderAsync(po.PoId);

        Assert.Equal(
            (false, "Cannot delete this Sales Order as a Route-Card transaction exists."),
            result);
    }

    /// <summary>
    /// Pins the permissive missing-order path (MfgPoService.cs:474-475): an unknown
    /// poId is reported deletable, not refused. Unchanged by M0-09; recorded here
    /// because a future refactor might "harden" it without noticing.
    /// </summary>
    [Fact]
    public async Task CanDeleteSalesOrder_WhenOrderDoesNotExist_IsAllowed()
    {
        using var harness = new SalesOrderDeleteGuardHarness();

        var result = await harness.Service.CanDeleteSalesOrderAsync(999999);

        Assert.Equal((true, "Sales Order can be safely deleted."), result);
    }

    /// <summary>
    /// Control: an order with no downstream document at all is deletable. Without
    /// this, the two guard tests above could pass for the wrong reason.
    /// </summary>
    [Fact]
    public async Task CanDeleteSalesOrder_WithNoDownstreamDocuments_IsAllowed()
    {
        using var harness = new SalesOrderDeleteGuardHarness();

        var po = harness.AddSalesOrder();

        var result = await harness.Service.CanDeleteSalesOrderAsync(po.PoId);

        Assert.Equal((true, "Sales Order can be safely deleted."), result);
    }
}
