// =============================================================================
//  CHARACTERISATION SUITE - StockManagerService (task M0-13)
// -----------------------------------------------------------------------------
//  THIS SUITE PINS CURRENT BEHAVIOUR, INCLUDING BEHAVIOUR BELIEVED TO BE
//  DEFECTIVE (R-07 / BR-STK-002 - silent under-allocation).
//
//  A FAILING TEST HERE MEANS BEHAVIOUR CHANGED. It does not mean the test is
//  wrong. If the change was intended, update the test IN THE SAME COMMIT as the
//  production change and record it in:
//      docs/kb/business-rules/business-rule-inventory.md  (KB-030)
//      docs/kb/risks/technical-debt-register.md           (KB-060)
//
//  The R-07 tests below assert that a stock issue of 100 against a batch holding
//  1 SUCCEEDS and silently under-allocates by 99. That assertion is deliberate.
//  Whether to keep or tighten that behaviour is a product decision - task M0-11,
//  open question Q-01 (docs/kb/open-questions.md). DO NOT "fix" the service to
//  make these tests read better.
//
//  Subject under test (read-only, never modified):
//      V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/InventoryService/
//      StockManagerService.cs   (line numbers below re-verified 2026-08-19)
//
//  Harness: StockTestHarness (Infrastructure/StockScenarioBuilder.cs) - option A
//  of the M0-13 spec: Mock<IUnitOfWork> over REAL repositories over a real
//  EF Core InMemory ApplicationDbContext (INV-031, KB-003).
//
// -----------------------------------------------------------------------------
//  STATEMENT -> TEST MAP (the 16 statements of M0-13 "Existing Behavior to
//  Preserve"; BR-STK-001 = 1-12, BR-STK-002 / R-07 = 13-16)
//
//   1  :110-120 existing-issue lookup excludes StoreId; allowMultipleIssue skips it
//         -> S01_ReIssue_MatchesExistingIssueEvenWhenStoreIdDiffers_AndOverwritesIt
//         -> S01_Issue_WhenScreenCodeDiffers_CreatesASecondIssue
//         -> S01_Issue_WhenAllowMultipleIssueIsTrue_CreatesASecondIssueInsteadOfUpdating
//   2  :122-134 update path overwrites StoreId/IssueQty/UnitPrice/BatchNo/RefDate/
//               IssuedBy/CreatedDate, leaves ScreenCode/RefNo/SubItemRefID
//         -> S01_ReIssue_MatchesExistingIssueEvenWhenStoreIdDiffers_AndOverwritesIt
//   3  :136-158 new issue saved at :155 before tracking, so IssueId is populated
//         -> S03_S07_Issue_SingleBatchExactQuantity_ConsumesItAndWritesOneTrack
//   4  :181-199 re-issue reverses all prior tracks first (balances restored, rows
//               deleted, saved at :199)
//         -> S04_ReIssue_WithSmallerQuantity_ReversesPriorTracksBeforeReallocating
//         -> S01_ReIssue_MatchesExistingIssueEvenWhenStoreIdDiffers_AndOverwritesIt
//   5  :203-207 candidates = ItemId + StoreId + RcSubID + BalQty>0, ORDER BY
//               AddDate ascending (FIFO)
//         -> S05_Issue_AcrossThreeBatches_ConsumesOldestAddDateFirst
//         -> S05_Issue_DoesNotConsumeBatchesInAnotherStore
//   6  :204     RcSubID equality including null - route-card stock does not mix
//         -> S06_Issue_WithNullRcSubId_DoesNotConsumeRouteCardBoundBatches
//         -> S06_Issue_WithRouteCardRcSubId_DoesNotConsumeFreeStockBatches
//   7  :212-231 Math.Min(BalQty, remainingQty) per batch, break at :214-215
//         -> S03_S07_Issue_SingleBatchExactQuantity_ConsumesItAndWritesOneTrack
//         -> S07_Issue_PartialQuantity_LeavesRemainderOnTheBatch
//         -> S05_Issue_AcrossThreeBatches_ConsumesOldestAddDateFirst (break)
//   8  :233     one SaveAsync after the loop, not one per batch
//         -> S08_Issue_CommitsAllocationInASingleSaveRegardlessOfBatchCount
//   9  :36-57   AddOrUpdateStockAsync preserves consumed qty; lookup excludes StoreId
//         -> S09_AddOrUpdateStock_OnExistingBatch_PreservesAlreadyConsumedQuantity
//         -> S09_AddOrUpdateStock_WhenNewQuantityIsBelowConsumed_FloorsBalanceAtZero
//         -> S09_AddOrUpdateStock_WhenAllowMultipleAddIsTrue_CreatesASecondBatch
//  10  :255-278 DeleteStockAddAsync refuses when tracked; silent return when absent
//         -> S10_DeleteStockAdd_WhenStockAlreadyIssuedFromBatch_Throws
//         -> S10_DeleteStockAdd_WhenBatchDoesNotExist_ReturnsSilently
//         -> S10_DeleteStockAdd_WhenNoTracksExist_DeletesTheBatch
//  11  :280-314 DeleteStockIssueAsync reverses tracks then deletes; throws if absent
//         -> S11_DeleteStockIssue_RestoresBalancesAndDeletesTheIssue
//         -> S11_DeleteStockIssue_WhenIssueDoesNotExist_Throws
//  12  :162-171 / :235-248 exception translation, exact messages
//         -> S12_Issue_WhenANonInvalidOperationExceptionEscapes_WrapsItWithFailedToIssueStock
//         -> S12_TrackStockUsage_WhenANonInvalidOperationExceptionEscapes_WrapsItWithFailedToProcessStockIssue
//         -> S13_R07_IssueOrUpdateStock_WhenNoBatchHasBalance_ThrowsNoAvailableStockToIssue
//               (proves InvalidOperationException is rethrown UNCHANGED at :162-164)
//  13  :209-210 throws ONLY when the candidate list is empty
//         -> S13_R07_IssueOrUpdateStock_WhenNoBatchHasBalance_ThrowsNoAvailableStockToIssue
//  14  :212-233 no post-loop remainingQty>0 check - returns normally
//         -> S14_R07_IssueOrUpdateStock_WhenBatchesExistButTotalBalanceIsShort_SilentlyUnderAllocatesAndDoesNotThrow
//  15  consequence: IssueQty full, sum(UsedQty) less, ledger silently unbalanced
//         -> S14_... (asserts the numeric drift)
//         -> S15_R07_IssueOrUpdateStock_WhenReIssueIncreasesQuantityBeyondAvailableStock_SilentlyUnderAllocatesOnTheUpdatePathToo
//  16  the asymmetry: 100 against 0 throws; 100 against 1 succeeds, drifts by 99
//         -> S16_R07_IssuingOneHundred_ThrowsAgainstZeroStock_ButSilentlyDriftsByNinetyNineAgainstOneUnit
//
// -----------------------------------------------------------------------------
//  NOT PINNED - recorded as Unknown, deliberately not asserted:
//
//  * FIFO TIE-BREAKING when two StockAdd rows share an identical AddDate.
//    .OrderBy(sa => sa.AddDate) (StockManagerService.cs:206) declares no secondary
//    key. The EF Core InMemory provider evaluates OrderBy as LINQ-to-objects, which
//    is a STABLE sort; SQL Server's ORDER BY guarantees no such thing. Any assertion
//    made here would pin the harness, not the product. Every scenario below
//    therefore uses DISTINCT, explicitly-set AddDate values.
//  * Whether SQL Server agrees with InMemory on `sa.RcSubID == rcSubId` when
//    rcSubId is null (statement 6). Inferred to agree - EF Core applies C#-like null
//    semantics to relational providers - but not verified against SQL Server here.
//  * Decimal rounding at [Precision(18,3)] / (18,4). InMemory stores CLR decimals
//    unrounded; SQL Server would round on save. Every quantity below uses at most
//    3 decimal places so the two providers cannot diverge.
// =============================================================================

using V.SMART.Shared.Tests.Infrastructure;
using Xunit;
using static V.SMART.Shared.Tests.Infrastructure.StockLedgerAssertions;

namespace V.SMART.Shared.Tests.Services;

public class StockManagerServiceCharacterisationTests
{
    // Distinct, explicit AddDate values. Never DateTime.Now - see the tie-breaking
    // note in the file header.
    private static readonly DateTime Oldest = new(2026, 1, 1, 8, 0, 0);
    private static readonly DateTime Middle = new(2026, 2, 1, 8, 0, 0);
    private static readonly DateTime Newest = new(2026, 3, 1, 8, 0, 0);
    private static readonly DateTime RefDate = new(2026, 4, 1);

    // ---------------------------------------------------------------------
    //  BR-STK-001 - FIFO consumption, tracked line by line
    // ---------------------------------------------------------------------

    /// <summary>Statements 3 and 7.</summary>
    [Fact]
    public async Task S03_S07_Issue_SingleBatchExactQuantity_ConsumesItAndWritesOneTrack()
    {
        using var h = new StockTestHarness();
        var batch = h.AddBatch(Oldest, addQty: 10m);

        await h.Service.IssueOrUpdateStockAsync(
            h.ItemId, h.StoreId, 10m, 25m, "B1", h.ScreenCode, 7, "ISS-1", RefDate);

        var issue = h.SingleIssue();

        // Statement 3: the issue was saved (:155) before tracking ran, so IssueId is
        // populated and the tracks can reference it.
        Assert.True(issue.IssueId > 0, Dump(h));
        Assert.Equal(10m, issue.IssueQty);
        Assert.Equal(StockTestHarness.UserName, issue.IssuedBy);

        // Statement 7: Math.Min(BalQty, remainingQty) == 10.
        AssertBalance(h, batch.AddId, 0m);
        AssertTracks(h, issue.IssueId, (batch.AddId, 10m));
        AssertLedgerBalanced(h, issue.IssueId);
    }

    /// <summary>Statement 7 - partial consumption leaves the remainder.</summary>
    [Fact]
    public async Task S07_Issue_PartialQuantity_LeavesRemainderOnTheBatch()
    {
        using var h = new StockTestHarness();
        var batch = h.AddBatch(Oldest, addQty: 10m);

        await h.Service.IssueOrUpdateStockAsync(
            h.ItemId, h.StoreId, 4.250m, 25m, null, h.ScreenCode, 7, "ISS-1", RefDate);

        var issue = h.SingleIssue();
        Assert.Equal(4.250m, issue.IssueQty);
        AssertBalance(h, batch.AddId, 5.750m);
        AssertTracks(h, issue.IssueId, (batch.AddId, 4.250m));
        AssertLedgerBalanced(h, issue.IssueId);
    }

    /// <summary>
    /// Statement 5 - FIFO. Three batches with distinct AddDate values, inserted
    /// NEWEST FIRST so that insertion order (AddId) is the reverse of AddDate order:
    /// consuming the oldest first can then only be the effect of
    /// .OrderBy(sa => sa.AddDate) at :206, not of insertion order.
    /// Also statement 7's break at :214-215 - the third batch is never touched.
    /// </summary>
    [Fact]
    public async Task S05_Issue_AcrossThreeBatches_ConsumesOldestAddDateFirst()
    {
        using var h = new StockTestHarness();
        var newest = h.AddBatch(Newest, addQty: 10m);
        var middle = h.AddBatch(Middle, addQty: 10m);
        var oldest = h.AddBatch(Oldest, addQty: 10m);

        await h.Service.IssueOrUpdateStockAsync(
            h.ItemId, h.StoreId, 15m, 25m, null, h.ScreenCode, 7, "ISS-1", RefDate);

        var issue = h.SingleIssue();

        AssertTracks(h, issue.IssueId, (oldest.AddId, 10m), (middle.AddId, 5m));
        AssertBalance(h, oldest.AddId, 0m);
        AssertBalance(h, middle.AddId, 5m);
        AssertBalance(h, newest.AddId, 10m); // untouched - the loop broke first
        AssertLedgerBalanced(h, issue.IssueId);
    }

    /// <summary>Statement 5 - StoreId discriminates.</summary>
    [Fact]
    public async Task S05_Issue_DoesNotConsumeBatchesInAnotherStore()
    {
        using var h = new StockTestHarness();
        var elsewhere = h.AddBatch(Oldest, addQty: 10m, storeId: h.OtherStoreId);
        var here = h.AddBatch(Middle, addQty: 10m, storeId: h.StoreId);

        await h.Service.IssueOrUpdateStockAsync(
            h.ItemId, h.StoreId, 6m, 25m, null, h.ScreenCode, 7, "ISS-1", RefDate);

        var issue = h.SingleIssue();
        AssertBalance(h, elsewhere.AddId, 10m); // other store untouched
        AssertBalance(h, here.AddId, 4m);
        AssertTracks(h, issue.IssueId, (here.AddId, 6m));
        AssertLedgerBalanced(h, issue.IssueId);
    }

    /// <summary>
    /// Statement 6 - a free-stock request (rcSubId null) does not consume
    /// route-card-bound batches, even though the bound batch is OLDER and would win
    /// on FIFO order if RcSubID were ignored.
    /// </summary>
    [Fact]
    public async Task S06_Issue_WithNullRcSubId_DoesNotConsumeRouteCardBoundBatches()
    {
        using var h = new StockTestHarness();
        var bound = h.AddBatch(Oldest, addQty: 10m, rcSubId: 77);
        var free = h.AddBatch(Middle, addQty: 10m, rcSubId: null);

        await h.Service.IssueOrUpdateStockAsync(
            h.ItemId, h.StoreId, 6m, 25m, null, h.ScreenCode, 7, "ISS-1", RefDate, rcSubId: null);

        var issue = h.SingleIssue();
        AssertBalance(h, bound.AddId, 10m);
        AssertBalance(h, free.AddId, 4m);
        AssertTracks(h, issue.IssueId, (free.AddId, 6m));
        AssertLedgerBalanced(h, issue.IssueId);
    }

    /// <summary>Statement 6, the other direction.</summary>
    [Fact]
    public async Task S06_Issue_WithRouteCardRcSubId_DoesNotConsumeFreeStockBatches()
    {
        using var h = new StockTestHarness();
        var free = h.AddBatch(Oldest, addQty: 10m, rcSubId: null);
        var bound = h.AddBatch(Middle, addQty: 10m, rcSubId: 77);
        var otherCard = h.AddBatch(Oldest, addQty: 10m, rcSubId: 88);

        await h.Service.IssueOrUpdateStockAsync(
            h.ItemId, h.StoreId, 6m, 25m, null, h.ScreenCode, 7, "ISS-1", RefDate, rcSubId: 77);

        var issue = h.SingleIssue();
        AssertBalance(h, free.AddId, 10m);
        AssertBalance(h, otherCard.AddId, 10m);
        AssertBalance(h, bound.AddId, 4m);
        AssertTracks(h, issue.IssueId, (bound.AddId, 6m));
        AssertLedgerBalanced(h, issue.IssueId);
    }

    /// <summary>
    /// Statements 1, 2 and 4 together. The second call names a DIFFERENT StoreId but
    /// the same ItemId / SubItemRefID / RefNo / ScreenCode, and it still updates the
    /// existing issue - proof that StoreId is not part of the match key (:114-119).
    /// The prior track in the first store is reversed (balance restored, row deleted)
    /// before re-allocation happens in the second store.
    /// This is also the quantity-DECREASE case: 6 issued, then 4.
    /// </summary>
    [Fact]
    public async Task S01_ReIssue_MatchesExistingIssueEvenWhenStoreIdDiffers_AndOverwritesIt()
    {
        using var h = new StockTestHarness();
        var inStoreA = h.AddBatch(Oldest, addQty: 10m, storeId: h.StoreId);
        var inStoreB = h.AddBatch(Middle, addQty: 10m, storeId: h.OtherStoreId);

        await h.Service.IssueOrUpdateStockAsync(
            h.ItemId, h.StoreId, 6m, 25m, "FIRST", h.ScreenCode, 7, "ISS-1", RefDate);

        var firstIssueId = h.SingleIssue().IssueId;
        AssertBalance(h, inStoreA.AddId, 4m);

        var newRefDate = new DateTime(2026, 5, 5);
        await h.Service.IssueOrUpdateStockAsync(
            h.ItemId, h.OtherStoreId, 4m, 99m, "SECOND", h.ScreenCode, 7, "ISS-1", newRefDate);

        // Statement 1: still exactly one StockIssue - the lookup matched.
        var issue = Assert.Single(h.AllIssues());
        Assert.Equal(firstIssueId, issue.IssueId);

        // Statement 2: overwritten fields...
        Assert.Equal(h.OtherStoreId, issue.StoreId);
        Assert.Equal(4m, issue.IssueQty);
        Assert.Equal(99m, issue.UnitPrice);
        Assert.Equal("SECOND", issue.BatchNo);
        Assert.Equal(newRefDate, issue.RefDate);
        // ...and fields the update path never touches.
        Assert.Equal(h.ScreenCode, issue.ScreenCode);
        Assert.Equal("ISS-1", issue.RefNo);
        Assert.Equal(7, issue.SubItemRefID);

        // Statement 4: the store-A allocation was reversed before re-allocating.
        AssertBalance(h, inStoreA.AddId, 10m);
        AssertBalance(h, inStoreB.AddId, 6m);
        AssertTracks(h, issue.IssueId, (inStoreB.AddId, 4m));
        AssertLedgerBalanced(h, issue.IssueId);
    }

    /// <summary>
    /// Statement 4 in isolation, same store, quantity DECREASING from 8 to 3: the
    /// original 8 is added back to BalQty and the track row deleted, then 3 is
    /// allocated afresh. The batch must end at 7, not at 2 (which is what a
    /// non-reversing implementation would leave).
    /// </summary>
    [Fact]
    public async Task S04_ReIssue_WithSmallerQuantity_ReversesPriorTracksBeforeReallocating()
    {
        using var h = new StockTestHarness();
        var batch = h.AddBatch(Oldest, addQty: 10m);

        await h.Service.IssueOrUpdateStockAsync(
            h.ItemId, h.StoreId, 8m, 25m, null, h.ScreenCode, 7, "ISS-1", RefDate);
        var issueId = h.SingleIssue().IssueId;
        AssertBalance(h, batch.AddId, 2m);
        var trackIdBefore = h.TracksFor(issueId).Single().IssueTrackId;

        await h.Service.IssueOrUpdateStockAsync(
            h.ItemId, h.StoreId, 3m, 25m, null, h.ScreenCode, 7, "ISS-1", RefDate);

        Assert.Single(h.AllIssues());
        AssertBalance(h, batch.AddId, 7m);

        var tracks = h.TracksFor(issueId);
        AssertTracks(h, issueId, (batch.AddId, 3m));
        // The original row was deleted, not edited: a new IssueTrackId.
        Assert.NotEqual(trackIdBefore, tracks.Single().IssueTrackId);
        AssertLedgerBalanced(h, issueId);
    }

    /// <summary>Statement 1 - allowMultipleIssue skips the lookup entirely.</summary>
    [Fact]
    public async Task S01_Issue_WhenAllowMultipleIssueIsTrue_CreatesASecondIssueInsteadOfUpdating()
    {
        using var h = new StockTestHarness();
        var batch = h.AddBatch(Oldest, addQty: 20m);

        await h.Service.IssueOrUpdateStockAsync(
            h.ItemId, h.StoreId, 5m, 25m, null, h.ScreenCode, 7, "ISS-1", RefDate);
        await h.Service.IssueOrUpdateStockAsync(
            h.ItemId, h.StoreId, 5m, 25m, null, h.ScreenCode, 7, "ISS-1", RefDate,
            rcSubId: null, allowMultipleIssue: true);

        var issues = h.AllIssues();
        Assert.Equal(2, issues.Count);
        AssertBalance(h, batch.AddId, 10m);
        AssertTracks(h, issues[0].IssueId, (batch.AddId, 5m));
        AssertTracks(h, issues[1].IssueId, (batch.AddId, 5m));
        AssertLedgerBalanced(h, issues[0].IssueId);
        AssertLedgerBalanced(h, issues[1].IssueId);
    }

    /// <summary>
    /// Statement 1 - ScreenCode IS part of the match key: the same item, sub-item and
    /// RefNo under a different screen creates a second issue.
    /// </summary>
    [Fact]
    public async Task S01_Issue_WhenScreenCodeDiffers_CreatesASecondIssue()
    {
        using var h = new StockTestHarness();
        var batch = h.AddBatch(Oldest, addQty: 20m);

        await h.Service.IssueOrUpdateStockAsync(
            h.ItemId, h.StoreId, 5m, 25m, null, h.ScreenCode, 7, "ISS-1", RefDate);
        await h.Service.IssueOrUpdateStockAsync(
            h.ItemId, h.StoreId, 5m, 25m, null, h.OtherScreenCode, 7, "ISS-1", RefDate);

        var issues = h.AllIssues();
        Assert.Equal(2, issues.Count);
        Assert.Equal(h.ScreenCode, issues[0].ScreenCode);
        Assert.Equal(h.OtherScreenCode, issues[1].ScreenCode);
        AssertBalance(h, batch.AddId, 10m);
    }

    /// <summary>
    /// Statement 8 - allocation is committed by ONE SaveAsync after the loop (:233),
    /// not one per batch. The create path makes exactly four SaveAsync calls
    /// (:155 create, :199 reversal, :233 allocation, :160 outer) and that count does
    /// not grow with the number of batches consumed.
    /// </summary>
    [Fact]
    public async Task S08_Issue_CommitsAllocationInASingleSaveRegardlessOfBatchCount()
    {
        using var oneBatch = new StockTestHarness();
        oneBatch.AddBatch(Oldest, addQty: 30m);
        await oneBatch.Service.IssueOrUpdateStockAsync(
            oneBatch.ItemId, oneBatch.StoreId, 30m, 25m, null, oneBatch.ScreenCode, 7, "ISS-1", RefDate);

        using var threeBatches = new StockTestHarness();
        threeBatches.AddBatch(Oldest, addQty: 10m);
        threeBatches.AddBatch(Middle, addQty: 10m);
        threeBatches.AddBatch(Newest, addQty: 10m);
        await threeBatches.Service.IssueOrUpdateStockAsync(
            threeBatches.ItemId, threeBatches.StoreId, 30m, 25m, null, threeBatches.ScreenCode, 7, "ISS-1", RefDate);

        Assert.Equal(3, threeBatches.TracksFor(threeBatches.SingleIssue().IssueId).Count);
        Assert.Equal(4, oneBatch.SaveCount);
        Assert.Equal(oneBatch.SaveCount, threeBatches.SaveCount);
    }

    /// <summary>
    /// Statement 9 - AddOrUpdateStockAsync on an existing batch: usedQty is
    /// AddQty - BalQty and the new BalQty is Math.Max(qty - usedQty, 0) (:53-57).
    /// The second call also names a different StoreId and still updates the same
    /// batch, proving StoreId is not part of the lookup key (:40-45).
    /// </summary>
    [Fact]
    public async Task S09_AddOrUpdateStock_OnExistingBatch_PreservesAlreadyConsumedQuantity()
    {
        using var h = new StockTestHarness();

        await h.Service.AddOrUpdateStockAsync(
            h.ItemId, h.StoreId, 10m, 25m, "B1", h.ScreenCode, 3, "GRN-1", RefDate, "first");

        var addId = Assert.Single(h.AllBatches()).AddId;

        await h.Service.IssueOrUpdateStockAsync(
            h.ItemId, h.StoreId, 4m, 25m, null, h.ScreenCode, 7, "ISS-1", RefDate);
        AssertBalance(h, addId, 6m); // usedQty is now 4

        await h.Service.AddOrUpdateStockAsync(
            h.ItemId, h.OtherStoreId, 8m, 30m, "B2", h.ScreenCode, 3, "GRN-1", RefDate, "revised");

        var batch = Assert.Single(h.AllBatches());
        Assert.Equal(addId, batch.AddId);
        Assert.Equal(8m, batch.AddQty);
        Assert.Equal(4m, batch.BalQty); // Math.Max(8 - 4, 0)
        Assert.Equal(h.OtherStoreId, batch.StoreId);
        Assert.Equal(30m, batch.UnitPrice);
        Assert.Equal("B2", batch.BatchNo);
    }

    /// <summary>Statement 9 - the Math.Max floor at zero.</summary>
    [Fact]
    public async Task S09_AddOrUpdateStock_WhenNewQuantityIsBelowConsumed_FloorsBalanceAtZero()
    {
        using var h = new StockTestHarness();

        await h.Service.AddOrUpdateStockAsync(
            h.ItemId, h.StoreId, 10m, 25m, null, h.ScreenCode, 3, "GRN-1", RefDate, null);
        var addId = Assert.Single(h.AllBatches()).AddId;

        await h.Service.IssueOrUpdateStockAsync(
            h.ItemId, h.StoreId, 7m, 25m, null, h.ScreenCode, 7, "ISS-1", RefDate);
        AssertBalance(h, addId, 3m); // usedQty is now 7

        // 2 - 7 would be negative; Math.Max floors it. Note the ledger is now
        // inconsistent by construction (7 tracked against an AddQty of 2) - that is
        // current behaviour, pinned, not endorsed.
        await h.Service.AddOrUpdateStockAsync(
            h.ItemId, h.StoreId, 2m, 25m, null, h.ScreenCode, 3, "GRN-1", RefDate, null);

        var batch = Assert.Single(h.AllBatches());
        Assert.Equal(2m, batch.AddQty);
        Assert.Equal(0m, batch.BalQty);
    }

    /// <summary>Statement 9 - allowMultipleAdd skips the lookup.</summary>
    [Fact]
    public async Task S09_AddOrUpdateStock_WhenAllowMultipleAddIsTrue_CreatesASecondBatch()
    {
        using var h = new StockTestHarness();

        await h.Service.AddOrUpdateStockAsync(
            h.ItemId, h.StoreId, 10m, 25m, null, h.ScreenCode, 3, "GRN-1", RefDate, null);
        await h.Service.AddOrUpdateStockAsync(
            h.ItemId, h.StoreId, 10m, 25m, null, h.ScreenCode, 3, "GRN-1", RefDate, null,
            RcSubId: null, allowMultipleAdd: true);

        Assert.Equal(2, h.AllBatches().Count);
    }

    /// <summary>Statement 10 - refusal, with the exact user-facing message.</summary>
    [Fact]
    public async Task S10_DeleteStockAdd_WhenStockAlreadyIssuedFromBatch_Throws()
    {
        using var h = new StockTestHarness();
        var batch = h.AddBatch(Oldest, addQty: 10m);
        await h.Service.IssueOrUpdateStockAsync(
            h.ItemId, h.StoreId, 4m, 25m, null, h.ScreenCode, 7, "ISS-1", RefDate);

        var ex = await Assert.ThrowsAsync<Exception>(
            () => h.Service.DeleteStockAddAsync(batch.AddId));

        Assert.Equal("Cannot delete. Stock already issued from this batch.", ex.Message);
        Assert.Single(h.AllBatches());
        AssertBalance(h, batch.AddId, 6m);
        Assert.Contains(h.Logs.DeveloperErrors, e => e.Source == "Error in DeleteStockAddAsync");
    }

    /// <summary>Statement 10 - unknown id returns silently (:261-262), no throw.</summary>
    [Fact]
    public async Task S10_DeleteStockAdd_WhenBatchDoesNotExist_ReturnsSilently()
    {
        using var h = new StockTestHarness();
        h.AddBatch(Oldest, addQty: 10m);

        await h.Service.DeleteStockAddAsync(987654);

        Assert.Single(h.AllBatches());
        Assert.Empty(h.Logs.DeveloperErrors);
    }

    /// <summary>Statement 10 - the untracked batch is deleted.</summary>
    [Fact]
    public async Task S10_DeleteStockAdd_WhenNoTracksExist_DeletesTheBatch()
    {
        using var h = new StockTestHarness();
        var batch = h.AddBatch(Oldest, addQty: 10m);

        await h.Service.DeleteStockAddAsync(batch.AddId);

        Assert.Empty(h.AllBatches());
    }

    /// <summary>Statement 11.</summary>
    [Fact]
    public async Task S11_DeleteStockIssue_RestoresBalancesAndDeletesTheIssue()
    {
        using var h = new StockTestHarness();
        var oldest = h.AddBatch(Oldest, addQty: 10m);
        var middle = h.AddBatch(Middle, addQty: 10m);

        await h.Service.IssueOrUpdateStockAsync(
            h.ItemId, h.StoreId, 14m, 25m, null, h.ScreenCode, 7, "ISS-1", RefDate);
        var issueId = h.SingleIssue().IssueId;
        AssertBalance(h, oldest.AddId, 0m);
        AssertBalance(h, middle.AddId, 6m);

        await h.Service.DeleteStockIssueAsync(issueId);

        Assert.Empty(h.AllIssues());
        AssertNoTracks(h, issueId);
        AssertBalance(h, oldest.AddId, 10m);
        AssertBalance(h, middle.AddId, 10m);
    }

    /// <summary>Statement 11 - the exact user-facing message.</summary>
    [Fact]
    public async Task S11_DeleteStockIssue_WhenIssueDoesNotExist_Throws()
    {
        using var h = new StockTestHarness();

        var ex = await Assert.ThrowsAsync<Exception>(
            () => h.Service.DeleteStockIssueAsync(987654));

        Assert.Equal("Stock Issue not found.", ex.Message);
        Assert.Contains(h.Logs.DeveloperErrors, e => e.Source == "Error in DeleteStockIssueAsync");
    }

    /// <summary>
    /// Statement 12 - a non-InvalidOperationException escaping IssueOrUpdateStockAsync
    /// is logged and wrapped with the exact user-facing message (:166-171). The
    /// failure is injected at the FIRST SaveAsync, which is the create-path save at
    /// :155, i.e. before TrackStockUsageAsync is entered.
    /// </summary>
    [Fact]
    public async Task S12_Issue_WhenANonInvalidOperationExceptionEscapes_WrapsItWithFailedToIssueStock()
    {
        using var h = new StockTestHarness();
        h.AddBatch(Oldest, addQty: 10m);
        var boom = new ApplicationException("save exploded");
        h.SaveFailure = call => call == 1 ? boom : null;

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => h.Service.IssueOrUpdateStockAsync(
                h.ItemId, h.StoreId, 5m, 25m, null, h.ScreenCode, 7, "ISS-1", RefDate));

        Assert.Equal("Failed to issue stock. Please contact support.", ex.Message);
        Assert.Same(boom, ex.InnerException);
        Assert.Contains(h.Logs.DeveloperErrors, e => e.Source == "Error in IssueOrUpdateStockAsync");
    }

    /// <summary>
    /// Statement 12 - the same for TrackStockUsageAsync (:239-247), whose message
    /// differs. The failure is injected at the SECOND SaveAsync, which is the
    /// reversal save at :199, inside TrackStockUsageAsync. The resulting
    /// InvalidOperationException then passes back out through
    /// IssueOrUpdateStockAsync's catch at :162-164 UNCHANGED - the message is
    /// TrackStockUsageAsync's, not "Failed to issue stock".
    /// </summary>
    [Fact]
    public async Task S12_TrackStockUsage_WhenANonInvalidOperationExceptionEscapes_WrapsItWithFailedToProcessStockIssue()
    {
        using var h = new StockTestHarness();
        h.AddBatch(Oldest, addQty: 10m);
        var boom = new ApplicationException("save exploded");
        h.SaveFailure = call => call == 2 ? boom : null;

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => h.Service.IssueOrUpdateStockAsync(
                h.ItemId, h.StoreId, 5m, 25m, null, h.ScreenCode, 7, "ISS-1", RefDate));

        Assert.Equal("Failed to process stock issue. Please contact support.", ex.Message);
        Assert.Same(boom, ex.InnerException);
        Assert.Contains(h.Logs.DeveloperErrors, e => e.Source!.StartsWith("TrackStockUsageAsync failed"));
        Assert.DoesNotContain(h.Logs.DeveloperErrors, e => e.Source == "Error in IssueOrUpdateStockAsync");
    }

    // ---------------------------------------------------------------------
    //  BR-STK-002 / R-07 - THE DEFECT THIS SUITE PINS, AND DOES NOT FIX.
    //
    //  Read these four tests as a pair of opposites. Asking for stock when there
    //  is NONE is refused loudly. Asking for more stock than exists is accepted
    //  silently, and the difference simply disappears from the ledger.
    // ---------------------------------------------------------------------

    /// <summary>
    /// Statement 13. When NO batch has BalQty &gt; 0 the service refuses, with the
    /// exact user-facing message. Also statement 12: the InvalidOperationException
    /// travels out of IssueOrUpdateStockAsync unchanged (:162-164) rather than being
    /// re-wrapped, and nothing is logged as a developer error.
    ///
    /// Note the side effect this pins: the StockIssue row has ALREADY been created
    /// and committed at :154-155 before tracking runs, so a refused issue leaves an
    /// orphan StockIssue row for the full quantity with no tracks at all.
    /// </summary>
    [Fact]
    public async Task S13_R07_IssueOrUpdateStock_WhenNoBatchHasBalance_ThrowsNoAvailableStockToIssue()
    {
        using var h = new StockTestHarness();
        var exhausted = h.AddBatch(Oldest, addQty: 5m, balQty: 0m);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => h.Service.IssueOrUpdateStockAsync(
                h.ItemId, h.StoreId, 100m, 25m, null, h.ScreenCode, 7, "ISS-1", RefDate));

        Assert.Equal("No available stock to issue.", ex.Message);
        Assert.Null(ex.InnerException);
        Assert.DoesNotContain(h.Logs.DeveloperErrors, e => e.Source == "Error in IssueOrUpdateStockAsync");

        AssertBalance(h, exhausted.AddId, 0m);
        var orphan = h.SingleIssue();
        Assert.Equal(100m, orphan.IssueQty);
        AssertNoTracks(h, orphan.IssueId);
    }

    /// <summary>
    /// Statements 14 and 15 - R-07.
    ///
    /// PLAIN ENGLISH: 100 units are requested. Only 30 exist. The system records an
    /// issue of 100 units, takes the 30 that exist, and reports SUCCESS. The missing
    /// 70 units are not allocated, not flagged and not reported anywhere. The stock
    /// ledger is left 70 units out of balance.
    ///
    /// This test passing is NOT approval of that behaviour. It is a photograph of it.
    /// The decision to keep or change it is task M0-11 / open question Q-01.
    /// </summary>
    [Fact]
    public async Task S14_R07_IssueOrUpdateStock_WhenBatchesExistButTotalBalanceIsShort_SilentlyUnderAllocatesAndDoesNotThrow()
    {
        using var h = new StockTestHarness();
        var oldest = h.AddBatch(Oldest, addQty: 20m);
        var middle = h.AddBatch(Middle, addQty: 10m);

        // No exception. That is the defect.
        await h.Service.IssueOrUpdateStockAsync(
            h.ItemId, h.StoreId, 100m, 25m, null, h.ScreenCode, 7, "ISS-1", RefDate);

        var issue = h.SingleIssue();

        // The issue claims the FULL requested quantity...
        Assert.Equal(100m, issue.IssueQty);
        // ...while only what existed was actually taken from batches.
        Assert.Equal(30m, h.TrackedQtyFor(issue.IssueId));
        AssertTracks(h, issue.IssueId, (oldest.AddId, 20m), (middle.AddId, 10m));
        AssertBalance(h, oldest.AddId, 0m);
        AssertBalance(h, middle.AddId, 0m);

        // The drift is a number in the test, not an inference: 100 - 30 = 70.
        Assert.Equal(70m, issue.IssueQty - h.TrackedQtyFor(issue.IssueId));
        AssertLedgerDriftsBy(h, issue.IssueId, 70m);
    }

    /// <summary>
    /// Statement 15 - the same silent drift on the UPDATE path, not only on create.
    /// An issue of 3 is re-issued as 100 against a batch of 5: the original 3 is
    /// reversed, all 5 are taken, and the issue records 100. Drift 95.
    /// </summary>
    [Fact]
    public async Task S15_R07_IssueOrUpdateStock_WhenReIssueIncreasesQuantityBeyondAvailableStock_SilentlyUnderAllocatesOnTheUpdatePathToo()
    {
        using var h = new StockTestHarness();
        var batch = h.AddBatch(Oldest, addQty: 5m);

        await h.Service.IssueOrUpdateStockAsync(
            h.ItemId, h.StoreId, 3m, 25m, null, h.ScreenCode, 7, "ISS-1", RefDate);
        var issueId = h.SingleIssue().IssueId;
        AssertLedgerBalanced(h, issueId);

        // Same match key, so this UPDATES the existing issue (:122-134).
        await h.Service.IssueOrUpdateStockAsync(
            h.ItemId, h.StoreId, 100m, 25m, null, h.ScreenCode, 7, "ISS-1", RefDate);

        var issue = Assert.Single(h.AllIssues());
        Assert.Equal(issueId, issue.IssueId);
        Assert.Equal(100m, issue.IssueQty);
        Assert.Equal(5m, h.TrackedQtyFor(issueId));
        AssertTracks(h, issueId, (batch.AddId, 5m));
        AssertBalance(h, batch.AddId, 0m);
        AssertLedgerDriftsBy(h, issueId, 95m);
    }

    /// <summary>
    /// Statement 16 - THE ASYMMETRY, and the single most important test in this
    /// suite. Both halves are here on purpose so the contrast cannot be missed.
    ///
    /// PLAIN ENGLISH:
    ///   Ask for 100 units when the store holds ZERO -> the system refuses:
    ///       "No available stock to issue."
    ///   Ask for 100 units when the store holds ONE  -> the system agrees, records
    ///       an issue of 100, takes the 1 unit that exists, and loses the other 99
    ///       with no error, no warning and no record.
    ///
    /// One unit of stock is the entire difference between a hard failure and a
    /// silent 99-unit hole in the ledger. That is what task M0-11 / Q-01 asks a
    /// product owner to decide about.
    /// </summary>
    [Fact]
    public async Task S16_R07_IssuingOneHundred_ThrowsAgainstZeroStock_ButSilentlyDriftsByNinetyNineAgainstOneUnit()
    {
        // Half one: 100 requested, 0 available -> throws.
        using (var zero = new StockTestHarness())
        {
            zero.AddBatch(Oldest, addQty: 5m, balQty: 0m);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => zero.Service.IssueOrUpdateStockAsync(
                    zero.ItemId, zero.StoreId, 100m, 25m, null, zero.ScreenCode, 7, "ISS-1", RefDate));

            Assert.Equal("No available stock to issue.", ex.Message);
        }

        // Half two: 100 requested, 1 available -> succeeds, drifts by 99.
        using var one = new StockTestHarness();
        var batch = one.AddBatch(Oldest, addQty: 1m);

        await one.Service.IssueOrUpdateStockAsync(
            one.ItemId, one.StoreId, 100m, 25m, null, one.ScreenCode, 7, "ISS-1", RefDate);

        var issue = one.SingleIssue();
        Assert.Equal(100m, issue.IssueQty);
        Assert.Equal(1m, one.TrackedQtyFor(issue.IssueId));
        AssertTracks(one, issue.IssueId, (batch.AddId, 1m));
        AssertBalance(one, batch.AddId, 0m);
        Assert.Equal(99m, issue.IssueQty - one.TrackedQtyFor(issue.IssueId));
        AssertLedgerDriftsBy(one, issue.IssueId, 99m);
    }
}
