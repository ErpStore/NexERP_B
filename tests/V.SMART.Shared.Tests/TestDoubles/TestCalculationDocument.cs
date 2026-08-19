using V.SMART.Shared.Utility_Constants;

namespace V.SMART.Shared.Tests.TestDoubles;

/// <summary>
/// Minimal hand-rolled <see cref="ICalculationDocument"/> for the CalculationService
/// characterisation suite (task M0-12-02).
///
/// The interface declares 26 settable members plus two READ-ONLY members -
/// CalculationDocumentSubItem and HasItemWiseTax - at
/// V.SMART/V.SMART.Shared/Utility_Constants/ICalculationDocument.cs:11-40
/// (re-verified 2026-08-19).
///
/// Two deliberate departures from the production ViewModels:
///
///  * HasItemWiseTax is SETTABLE here. All 13 production ViewModels that implement
///    ICalculationDocument derive it as
///    SubItems.Any(i =&gt; LineCGSTRate &gt; 0 || LineSGSTRate &gt; 0 || LineIGSTRate &gt; 0)
///    (e.g. V.SMART/V.SMART.Shared/ViewModels/.../PurchPoVM.cs:246-247), which makes
///    the header-wise branch unreachable whenever any line carries a rate. Pinning
///    both branches of CalculationService.cs:51-84 requires setting the flag directly.
///
///  * SubItems is exposed as a materialised IReadOnlyList. UpdateTotalsAsync
///    enumerates the collection at least six times per call (CalculationService.cs:14,
///    20, 23, 25, 57, 59, and 63 inside the per-line loop), so a lazily evaluated or
///    single-pass IEnumerable would give undefined results. Never hand this double a
///    raw iterator.
///
/// This type lives in the TEST project only. Nothing is added to V.SMART.Shared.
/// (A separate, private, nested TestCalculationDocument exists in SmokeTests.cs; it
/// belongs to M0-12-01 and is left alone.)
/// </summary>
public sealed class TestCalculationDocument : ICalculationDocument
{
    public decimal TotalGrossAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercent { get; set; }
    public bool DiscAmtOrPer { get; set; }
    public decimal TotalBasicAmount { get; set; }
    public decimal FreightCharges { get; set; }
    public decimal PackingCharges { get; set; }
    public decimal PackingPercent { get; set; }
    public bool PackingAmtOrPer { get; set; }
    public decimal InsuranceCharges { get; set; }
    public decimal InsurancePercent { get; set; }
    public bool InsuranceAmtOrPer { get; set; }
    public decimal TotalTaxable { get; set; }
    public decimal CGstRate { get; set; }
    public decimal SGstRate { get; set; }
    public decimal IGstRate { get; set; }
    public decimal TotalCGSTAmount { get; set; }
    public decimal TotalSGSTAmount { get; set; }
    public decimal TotalIGSTAmount { get; set; }
    public decimal TCSAmount { get; set; }
    public decimal TCSPercent { get; set; }
    public bool TCSAmtOrPer { get; set; }
    public decimal OtherCharges { get; set; }

    public bool IsRoundOffEnabled { get; set; }
    public decimal RoundOff { get; set; }
    public decimal GrandTotal { get; set; }

    /// <summary>Settable backing store for the read-only interface member.</summary>
    public IReadOnlyList<TestCalculationSubItem>? SubItems { get; set; }

    /// <summary>Settable backing store for the read-only interface member.</summary>
    public bool ItemWiseTax { get; set; }

    public IEnumerable<ICalculationDocumentSubItem> CalculationDocumentSubItem => SubItems!;

    public bool HasItemWiseTax => ItemWiseTax;

    /// <summary>
    /// Small builder: a document carrying the given lines and nothing else set.
    /// Every unmentioned field stays at its C# default (0 / false), which is what
    /// the arithmetic tests below assume unless they say otherwise.
    /// </summary>
    public static TestCalculationDocument With(params TestCalculationSubItem[] lines) =>
        new() { SubItems = lines };
}
