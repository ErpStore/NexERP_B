using V.SMART.Shared.Utility_Constants;

namespace V.SMART.Shared.Tests.TestDoubles;

/// <summary>
/// Minimal hand-rolled <see cref="ICalculationDocumentSubItem"/> for the
/// CalculationService characterisation suite (task M0-12-02).
///
/// The interface is declared entirely GET-ONLY at
/// V.SMART/V.SMART.Shared/Utility_Constants/ICalculationDocumentSubItem.cs:12-25
/// (re-verified 2026-08-19). This double makes every member settable so a test can
/// arrange a line, and so the suite can prove that CalculationService never WRITES
/// LineCGSTAmount / LineSGSTAmount / LineIGSTAmount (statement 13).
///
/// Note that LineGross is an INPUT here, not a derived value: CalculationService
/// reads it directly (CalculationService.cs:20) and never computes it from
/// Qty x UnitPrice. Several tests set Qty and UnitPrice inconsistently with
/// LineGross on purpose, to pin that.
/// </summary>
public sealed class TestCalculationSubItem : ICalculationDocumentSubItem
{
    public decimal Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineDiscountPercent { get; set; }
    public decimal LineDiscountAmount { get; set; }
    public decimal LineCGSTRate { get; set; }
    public decimal LineSGSTRate { get; set; }
    public decimal LineIGSTRate { get; set; }

    public decimal LineGross { get; set; }
    public decimal LineBasicAmount { get; set; }
    public decimal LineCGSTAmount { get; set; }
    public decimal LineSGSTAmount { get; set; }
    public decimal LineIGSTAmount { get; set; }

    /// <summary>
    /// Small builder so each test states only the fields it cares about.
    /// </summary>
    public static TestCalculationSubItem Line(
        decimal gross,
        decimal lineDiscount = 0m,
        decimal cgstRate = 0m,
        decimal sgstRate = 0m,
        decimal igstRate = 0m) => new()
        {
            LineGross = gross,
            LineDiscountAmount = lineDiscount,
            LineCGSTRate = cgstRate,
            LineSGSTRate = sgstRate,
            LineIGSTRate = igstRate,
        };
}
