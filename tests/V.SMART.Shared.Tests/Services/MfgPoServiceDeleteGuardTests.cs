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
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
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

    // =========================================================================
    //  M0-10 / INV-025 - PROVING TEST for the ONE surviving R-08 instance.
    //
    //  Subject: MfgPoService.CanSalesOrderItemCancelCheckAsync
    //  (MfgPoService.cs:590-657), its Contract Review guard at :613-615
    //  (line numbers verified against the file 2026-08-21):
    //
    //      bool hasCR = await _unitOfWork.ContractReviews.GetQueryable()
    //                        .AnyAsync(qs => qs.PoId == subItem.PoId);   // :613
    //      if (hasRc)                                                    // :614  <-- WRONG
    //          return (false, "Cannot cancel this Item as a Contract Review transaction exists.");
    //
    //  `hasRc` is the Route Card boolean computed at :608. The Contract Review
    //  guard therefore never fires on its own evidence: a Sales Order line whose
    //  only downstream document is a Contract Review is reported cancellable.
    //  This is the identical defect M0-09 fixed in CanDeleteSalesOrderAsync:523-526;
    //  it survives here because M0-09's scope was the delete guard alone, and
    //  because this method is not named CanDelete* and does not return
    //  (bool CanDelete, string Message) - it returns (bool CanItemCancel, ...),
    //  so it falls outside the 79-method inventory that defines KB-061 section 2.
    //
    //  OBSERVED OUTPUT, run as an un-skipped [Fact] on branch
    //  migration/M0-10-candelete-guard-audit, 2026-08-21, against unmodified
    //  MfgPoService.cs - quoted verbatim:
    //
    //      [xUnit.net 00:00:04.94]     V.SMART.Shared.Tests.Services.MfgPoServiceDeleteGuardTests.CanSalesOrderItemCancel_WithOnlyContractReview_IsRefused [FAIL]
    //        Failed V.SMART.Shared.Tests.Services.MfgPoServiceDeleteGuardTests.CanSalesOrderItemCancel_WithOnlyContractReview_IsRefused [4 s]
    //        Error Message:
    //         Assert.Equal() Failure: Values differ
    //      Expected: Tuple (False, "Cannot cancel this Item as a Contract Review trans"...)
    //      Actual:   Tuple (True, "Item can be safely Cancell.")
    //
    //      Failed!  - Failed:     1, Passed:    84, Skipped:     0, Total:    85, Duration: 8 s
    //
    //  (xUnit truncates the expected string with a U+00B7 ellipsis; transcribed as
    //   "..." above to keep this file ASCII. The assertion below carries it in full.)
    //
    //  (Baseline immediately before adding this test, same command, same branch:
    //   Passed!  - Failed: 0, Passed: 84, Skipped: 0, Total: 84, Duration: 9 s.)
    //
    //  WHY IT IS Skip-ped RATHER THAN DELETED - M0-10 step 8 permits either.
    //  M0-10 is an investigation and must NOT repair the defect; that is task
    //  M0-10a. A red test is not an acceptable deliverable of an investigation,
    //  so it cannot be left enabled. It is kept skipped rather than deleted
    //  because M0-10a needs precisely this test: change `hasRc` to `hasCR` at
    //  MfgPoService.cs:614, remove the Skip, and it must go green. Deleting it
    //  would force M0-10a to re-derive the seeding requirements already recorded
    //  on SalesOrderDeleteGuardHarness.
    //
    //  DO NOT un-skip this test without also fixing MfgPoService.cs:614.
    //  Recorded in KB-061 sections 3.1 and 7, and in R-60 (KB-060).
    // =========================================================================
    [Fact(Skip = "M0-10 proving test for the surviving R-08 defect at MfgPoService.cs:614. " +
                 "It FAILS by design against current code - M0-10 is an audit and must not repair it. " +
                 "Un-skip as part of task M0-10a, which changes `if (hasRc)` to `if (hasCR)` at :614. " +
                 "Observed failure output is quoted in the comment block above.")]
    public async Task CanSalesOrderItemCancel_WithOnlyContractReview_IsRefused()
    {
        using var harness = new SalesOrderDeleteGuardHarness();

        var po = harness.AddSalesOrder();
        var sub = po.MfgPoSubs.Single();

        // Contract Review on the Sales Order HEADER id - the key :613 joins on.
        // Deliberately NO Route Card, so hasRc is false and the guard at :614
        // cannot fire on the wrong document's evidence.
        harness.AddContractReview(po.PoId);

        var result = await harness.Service.CanSalesOrderItemCancelCheckAsync(
            new MfgPoSubVM { PoId = po.PoId, PoSubId = sub.PoSubId });

        // What the guard's own Message string says it does. Verbatim per BR-SO-001.
        Assert.Equal(
            (false, "Cannot cancel this Item as a Contract Review transaction exists."),
            result);
    }
}
