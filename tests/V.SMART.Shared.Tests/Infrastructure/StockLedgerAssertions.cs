using System.Text;
using Xunit;

namespace V.SMART.Shared.Tests.Infrastructure;

/// <summary>
/// Ledger-wide assertions for the StockManagerService characterisation suite.
///
/// The failure mode under investigation (R-07 / BR-STK-002) is a MISMATCH between
/// StockIssue.IssueQty, the StockAdd.BalQty values and the StockIssueTrack rows.
/// A single-sided assertion cannot see it, and a bare "Assert.Equal(9, 10)" failure
/// message cannot be diagnosed. Every helper here therefore attaches
/// <see cref="Dump"/> - the whole ledger - to its failure message.
/// </summary>
public static class StockLedgerAssertions
{
    /// <summary>Renders every StockAdd, StockIssue and StockIssueTrack row.</summary>
    public static string Dump(StockTestHarness harness)
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("---- LEDGER ----");
        sb.AppendLine("StockAdd:");
        foreach (var a in harness.AllBatches())
        {
            sb.AppendLine(
                $"  AddId={a.AddId} AddDate={a.AddDate:O} ItemId={a.ItemId} StoreId={a.StoreId} " +
                $"RcSubID={(a.RcSubID?.ToString() ?? "null")} AddQty={a.AddQty} BalQty={a.BalQty} RefNo={a.RefNo}");
        }

        sb.AppendLine("StockIssue:");
        foreach (var i in harness.AllIssues())
        {
            sb.AppendLine(
                $"  IssueId={i.IssueId} ItemId={i.ItemId} StoreId={i.StoreId} ScreenCode={i.ScreenCode} " +
                $"SubItemRefID={(i.SubItemRefID?.ToString() ?? "null")} RefNo={i.RefNo} IssueQty={i.IssueQty} IssuedBy={i.IssuedBy}");
        }

        sb.AppendLine("StockIssueTrack:");
        foreach (var t in harness.AllTracks())
        {
            sb.AppendLine(
                $"  IssueTrackId={t.IssueTrackId} IssueId={(t.IssueId?.ToString() ?? "null")} " +
                $"AddId={(t.AddId?.ToString() ?? "null")} UsedQty={t.UsedQty}");
        }

        sb.AppendLine($"SaveAsync calls: {harness.SaveCount}");
        sb.AppendLine("----------------");
        return sb.ToString();
    }

    public static void AssertBalance(StockTestHarness harness, int addId, decimal expected)
    {
        var actual = harness.BalanceOf(addId);
        Assert.True(
            expected == actual,
            $"Expected StockAdd {addId} BalQty {expected} but found {actual}.{Dump(harness)}");
    }

    public static void AssertNoTracks(StockTestHarness harness, int issueId)
    {
        var tracks = harness.TracksFor(issueId);
        Assert.True(
            tracks.Count == 0,
            $"Expected no StockIssueTrack rows for issue {issueId} but found {tracks.Count}.{Dump(harness)}");
    }

    /// <summary>
    /// Asserts the exact sequence of (AddId, UsedQty) allocations for an issue, in
    /// ALLOCATION order (ascending IssueTrackId - see StockTestHarness.TracksFor).
    /// For a FIFO scenario this sequence is the proof of consumption order.
    /// </summary>
    public static void AssertTracks(
        StockTestHarness harness,
        int issueId,
        params (int AddId, decimal UsedQty)[] expected)
    {
        var actual = harness.TracksFor(issueId)
            .Select(t => (AddId: t.AddId!.Value, t.UsedQty))
            .ToArray();

        Assert.True(
            expected.Length == actual.Length,
            $"Expected {expected.Length} StockIssueTrack rows for issue {issueId} but found {actual.Length}.{Dump(harness)}");

        for (var i = 0; i < expected.Length; i++)
        {
            Assert.True(
                expected[i].AddId == actual[i].AddId && expected[i].UsedQty == actual[i].UsedQty,
                $"Track[{i}] expected (AddId={expected[i].AddId}, UsedQty={expected[i].UsedQty}) " +
                $"but found (AddId={actual[i].AddId}, UsedQty={actual[i].UsedQty}).{Dump(harness)}");
        }
    }

    /// <summary>
    /// The balance test that R-07 fails: IssueQty must equal the sum of the tracked
    /// quantities. Used deliberately in BOTH directions - satisfied on the healthy
    /// paths, and violated by a known, asserted amount on the R-07 paths.
    /// </summary>
    public static decimal LedgerDrift(StockTestHarness harness, int issueId)
    {
        var issue = harness.AllIssues().Single(i => i.IssueId == issueId);
        return issue.IssueQty - harness.TrackedQtyFor(issueId);
    }

    public static void AssertLedgerBalanced(StockTestHarness harness, int issueId)
    {
        var drift = LedgerDrift(harness, issueId);
        Assert.True(
            drift == 0m,
            $"Expected IssueQty to equal the sum of StockIssueTrack.UsedQty for issue {issueId}, " +
            $"but the ledger drifts by {drift}.{Dump(harness)}");
    }

    /// <summary>
    /// Asserts the ledger is out of balance by exactly <paramref name="expectedDrift"/>.
    /// This is not an aspiration - it pins R-07 / BR-STK-002 as it behaves TODAY.
    /// </summary>
    public static void AssertLedgerDriftsBy(StockTestHarness harness, int issueId, decimal expectedDrift)
    {
        var drift = LedgerDrift(harness, issueId);
        Assert.True(
            drift == expectedDrift,
            $"Expected issue {issueId} to under-allocate by exactly {expectedDrift} (R-07), " +
            $"but the drift is {drift}.{Dump(harness)}");
    }
}
