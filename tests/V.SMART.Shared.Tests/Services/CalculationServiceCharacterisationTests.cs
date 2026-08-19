// =============================================================================
//  CHARACTERISATION SUITE - CalculationService (task M0-12-02)
// -----------------------------------------------------------------------------
//  THIS SUITE PINS CURRENT BEHAVIOUR, INCLUDING BEHAVIOUR THAT IS SURPRISING OR
//  ARGUABLY WRONG. It is not a correctness suite. Every expected value below was
//  derived by hand from the source first, then confirmed by execution.
//
//  A FAILING TEST HERE MEANS BEHAVIOUR CHANGED. It does not mean the test is
//  wrong. When one fails: investigate what changed. If the change was intended,
//  update the test IN THE SAME COMMIT as the production change, and record the
//  new behaviour in:
//      docs/kb/business-rules/business-rule-inventory.md  (KB-030, BR-CALC-001/002)
//      docs/kb/risks/technical-debt-register.md           (KB-060, R-05 / R-15)
//  Do NOT "fix" the service to make a test read better.
//
//  Subject under test (read-only, NEVER modified by this task):
//      V.SMART/V.SMART.Shared/Services/CalculationService.cs
//      class :10, UpdateTotalsAsync :12-114, file is 117 lines.
//      All line numbers below re-verified against source on 2026-08-19.
//
//  CalculationService is a pure unit - no constructor, no fields, no
//  dependencies (CalculationService.cs:10-12). This suite needs no database, no
//  fixture and no mocks; it uses the two hand-rolled doubles in
//  tests/V.SMART.Shared.Tests/TestDoubles/.
//
//  Why it matters: BR-CALC-001's disposition is "preserve verbatim - do not port
//  to TypeScript". Every quotation, sales order, invoice and purchase order in
//  every tenant is priced here, and BR-CALC-001's migration note requires the
//  same method to be called from a second host (POST /api/documents/calculate,
//  M2 scope). These numbers are the parity baseline that endpoint must match.
//
// -----------------------------------------------------------------------------
//  STATEMENT -> TEST MAP
//  (statements 1-19 = BR-CALC-001, statements 20-21 = BR-CALC-002; the numbering
//   is the "Existing Behavior to Preserve" table of
//   docs/kb/execution/tasks/M0-12-02.md)
//
//   1  :14-15  null document / null collection / empty collection -> silent return
//         -> S01_NullDocument_SilentlyReturnsWithoutComputing
//         -> S01_NullSubItemCollection_SilentlyReturnsWithoutComputing_AndMutatesNothing
//         -> S01_EmptySubItemCollection_SilentlyReturnsWithoutComputing_AndMutatesNothing
//   2  :20     TotalGrossAmount = sum(LineGross); LineGross is read, never derived
//         -> S02_TotalGrossAmount_IsSumOfLineGross_AndQtyTimesUnitPriceIsIgnored
//   3  :23-26  any line discount overwrites header DiscountAmount; DiscAmtOrPer
//              and DiscountPercent are ignored entirely
//         -> S03_WhenAnyLineHasDiscount_HeaderDiscountAmountIsOverwritten_AndDiscAmtOrPerIsIgnored
//   4  :29-31  no line discount: DiscAmtOrPer true keeps the caller's amount;
//              false recomputes it from DiscountPercent
//         -> S04_WhenDiscAmtOrPerIsTrue_CallersFixedDiscountAmountSurvives
//         -> S04_WhenDiscAmtOrPerIsFalse_DiscountAmountIsRecomputedFromPercent
//   5  :35     TotalBasicAmount = gross - discount, with NO floor at zero
//         -> S05_WhenDiscountExceedsGross_TotalBasicAmountGoesNegative_WithNoFloorAtZero
//   6  :38-42  Packing/Insurance recomputed only when the AmtOrPer flag is FALSE
//         -> S06_WhenPackingAndInsuranceAmtOrPerAreFalse_ChargesAreRecomputedFromPercent
//         -> S06_WhenPackingAndInsuranceAmtOrPerAreTrue_CallersChargesSurvive
//   7  FreightCharges is NEVER computed - no FreightPercent, no freight branch
//         -> S07_FreightCharges_IsNeverComputed_AndAlwaysKeepsTheCallersValue
//   8  :45-48  TotalTaxable = basic + freight + packing + insurance
//         -> S08_TotalTaxable_IsBasicPlusFreightPlusPackingPlusInsurance
//   9  :51-78 (reset :53-55) item-wise branch resets the three header totals to 0
//         -> S09_ItemWiseBranch_ResetsHeaderTaxTotalsToZeroBeforeAccumulating
//         -> S09_ItemWiseBranch_WithThreeLinesAtDifferentRates_AllocatesFreightByGrossProportion
//  10  :57,:61 proportion = LineGross / sum(LineGross), and 0 when the sum is 0
//         -> S10_ItemWiseBranch_WhenEveryLineGrossIsZero_ProportionGuardYieldsZeroTax
//         -> S09_ItemWiseBranch_WithThreeLinesAtDifferentRates_... (the non-zero case)
//  11  :63-65  a FIXED header discount (DiscAmtOrPer == true) does NOT reduce the
//              item-wise taxable base - tax is charged on the UNDISCOUNTED amount
//         -> S11_ItemWiseBranch_WhenHeaderDiscountIsAFixedAmount_ItDoesNotReduceTheTaxableBase
//         -> S11_ItemWiseBranch_WhenHeaderDiscountIsAPercentage_ItIsSpreadAcrossLinesByProportion
//  12  :67-72  per-line base = gross - lineDiscount - proportionalDiscount
//              + proportion * (freight + packing + insurance)
//         -> S12_ItemWiseBranch_PerLineTaxableBase_SubtractsLineDiscountAndAddsProportionalCharges
//  13  :74-76  header totals accumulate; the per-line LineCGSTAmount /
//              LineSGSTAmount / LineIGSTAmount are NEVER WRITTEN
//         -> S13_ItemWiseBranch_PerLineTaxAmountsAreNeverWritten_OnlyHeaderTotalsAccumulate
//  14  :81-83  header-wise branch uses CGstRate/SGstRate/IGstRate; per-line rates
//              are ignored entirely
//         -> S14_HeaderWiseBranch_UsesHeaderRates_AndPerLineRatesAreIgnored
//  15  :87-95  TCS base is TAX-INCLUSIVE; OtherCharges is NOT in the TCS base
//         -> S15_TcsPercent_IsAppliedToATaxInclusiveBase_AndOtherChargesAreExcluded
//         -> S15_WhenTcsAmtOrPerIsTrue_CallersFixedTcsAmountSurvives
//  16  :99     preRoundGrandTotal = grandBeforeTCS + TCSAmount + OtherCharges
//         -> S16_PreRoundGrandTotal_IsGrandBeforeTcsPlusTcsPlusOtherCharges
//  17  :101-111 round-off: AwayFromZero at :103; RoundOff is SIGNED; when disabled
//              RoundOff is 0 and the total is not rounded
//         -> S17_WhenRoundOffEnabled_MidpointRoundsAwayFromZero_NotBankers
//         -> S17_WhenRoundOffEnabled_AndAnOddIntegerMidpoint_StillRoundsAwayFromZero
//         -> S17_WhenRoundingDown_RoundOffIsNegative
//         -> S17_WhenRoundOffDisabled_RoundOffIsZero_AndGrandTotalIsNotRounded
//  18  NO INTERMEDIATE ROUNDING - exactly one Math.Round in the method, at :103
//         -> S18_NoIntermediateRounding_FullDecimalPrecisionSurvivesToTheGrandTotal
//  19  :113    async in signature only; the method completes synchronously
//         -> S19_UpdateTotalsAsync_CompletesSynchronously_DespiteTheAsyncSignature
//  20  CommonConstants.cs:11,:18,:25-26 - the fixed rate lists and the R-15
//      zero-coercion of an unlisted rate
//         -> CommonConstantsGstRateTests (companion file)
//  21  CalculationService does NOT call GetIGST/GetGST and does NOT validate rates
//         -> S21_R15_AnUnlistedGstRate_IsAppliedWithoutValidationOrCoercionToZero
//
// -----------------------------------------------------------------------------
//  NOT PINNED - deliberately not asserted:
//
//  * DECIMAL SCALE of the computed totals. xUnit's Assert.Equal(decimal, decimal)
//    uses decimal.Equals, which is numerically equal across scales (18.000m equals
//    18m). Every assertion below therefore pins the VALUE, not the trailing
//    precision. A change that altered only scale would pass. Pinning scale needs
//    ToString(CultureInfo.InvariantCulture) or decimal.GetBits; whether scale is
//    part of the contract the React client (M2-C10) and the future
//    POST /api/documents/calculate endpoint must honour is recorded as an open
//    question in docs/kb/open-questions.md.
//  * Whether the fixed-header-discount behaviour of statement 11 is intended
//    product behaviour or a defect. That is a product decision, not something
//    source can answer - see docs/kb/open-questions.md.
//  * Idempotence under repeated invocation. The Blazor UI calls this on nearly
//    every keystroke; reading the method suggests it is idempotent, but that is
//    not one of the 21 statements this task was scoped to pin.
// =============================================================================

using V.SMART.Shared.Services;
using V.SMART.Shared.Tests.TestDoubles;
using V.SMART.Shared.Utility_Constants;
using Xunit;

namespace V.SMART.Shared.Tests.Services;

public class CalculationServiceCharacterisationTests
{
    // The service is stateless (CalculationService.cs:10-12), so a fresh instance
    // per test costs nothing and keeps the suite free of shared mutable state.
    private static CalculationService Service() => new();

    // -------------------------------------------------------------------------
    //  Statement 1 - the three silent returns (:14-15)
    // -------------------------------------------------------------------------

    /// <summary>Statement 1. A null document does not throw.</summary>
    [Fact]
    public async Task S01_NullDocument_SilentlyReturnsWithoutComputing()
    {
        // CalculationService.cs:14 short-circuits on document == null. No
        // ArgumentNullException, no logging, nothing.
        await Service().UpdateTotalsAsync(null!);
    }

    /// <summary>Statement 1. A null sub-item collection leaves every field untouched.</summary>
    [Fact]
    public async Task S01_NullSubItemCollection_SilentlyReturnsWithoutComputing_AndMutatesNothing()
    {
        var doc = PreLoadedDocument();
        doc.SubItems = null;

        await Service().UpdateTotalsAsync(doc);

        AssertNothingWasMutated(doc);
    }

    /// <summary>Statement 1. An EMPTY sub-item collection also leaves every field untouched.</summary>
    [Fact]
    public async Task S01_EmptySubItemCollection_SilentlyReturnsWithoutComputing_AndMutatesNothing()
    {
        var doc = PreLoadedDocument();
        doc.SubItems = Array.Empty<TestCalculationSubItem>();

        await Service().UpdateTotalsAsync(doc);

        AssertNothingWasMutated(doc);
    }

    /// <summary>
    /// A document whose OUTPUT fields already carry non-zero sentinel values. If the
    /// early return at :14-15 ever became a "zero everything out" path, every one of
    /// these assertions would fail.
    /// </summary>
    private static TestCalculationDocument PreLoadedDocument() => new()
    {
        TotalGrossAmount = 111.11m,
        DiscountAmount = 22.22m,
        TotalBasicAmount = 88.89m,
        PackingCharges = 33.33m,
        InsuranceCharges = 44.44m,
        TotalTaxable = 166.66m,
        TotalCGSTAmount = 55.55m,
        TotalSGSTAmount = 66.66m,
        TotalIGSTAmount = 77.77m,
        TCSAmount = 88.88m,
        RoundOff = 99.99m,
        GrandTotal = 123.45m,
    };

    private static void AssertNothingWasMutated(TestCalculationDocument doc)
    {
        Assert.Equal(111.11m, doc.TotalGrossAmount);
        Assert.Equal(22.22m, doc.DiscountAmount);
        Assert.Equal(88.89m, doc.TotalBasicAmount);
        Assert.Equal(33.33m, doc.PackingCharges);
        Assert.Equal(44.44m, doc.InsuranceCharges);
        Assert.Equal(166.66m, doc.TotalTaxable);
        Assert.Equal(55.55m, doc.TotalCGSTAmount);
        Assert.Equal(66.66m, doc.TotalSGSTAmount);
        Assert.Equal(77.77m, doc.TotalIGSTAmount);
        Assert.Equal(88.88m, doc.TCSAmount);
        Assert.Equal(99.99m, doc.RoundOff);
        Assert.Equal(123.45m, doc.GrandTotal);
    }

    // -------------------------------------------------------------------------
    //  Statement 2 - gross (:20)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Statement 2. LineGross is a read-only INPUT to the engine
    /// (ICalculationDocumentSubItem.cs:21). The lines below carry Qty x UnitPrice
    /// values that contradict LineGross on purpose: 3 x 7 = 21 and 2 x 4 = 8, total
    /// 29 - which is NOT what comes out.
    /// </summary>
    [Fact]
    public async Task S02_TotalGrossAmount_IsSumOfLineGross_AndQtyTimesUnitPriceIsIgnored()
    {
        var doc = TestCalculationDocument.With(
            new TestCalculationSubItem { Qty = 3m, UnitPrice = 7m, LineGross = 100m },
            new TestCalculationSubItem { Qty = 2m, UnitPrice = 4m, LineGross = 50m });

        await Service().UpdateTotalsAsync(doc);

        Assert.Equal(150m, doc.TotalGrossAmount);
        Assert.NotEqual(29m, doc.TotalGrossAmount);
    }

    // -------------------------------------------------------------------------
    //  Statements 3 and 4 - discount (:23-31)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Statement 3. One line carrying a discount is enough: the caller's header
    /// DiscountAmount (999) is discarded, DiscAmtOrPer (true) is ignored, and
    /// DiscountPercent (50) is ignored. The result is the SUM of line discounts.
    /// </summary>
    [Fact]
    public async Task S03_WhenAnyLineHasDiscount_HeaderDiscountAmountIsOverwritten_AndDiscAmtOrPerIsIgnored()
    {
        var doc = TestCalculationDocument.With(
            TestCalculationSubItem.Line(gross: 100m, lineDiscount: 10m),
            TestCalculationSubItem.Line(gross: 200m, lineDiscount: 5m));
        doc.DiscountAmount = 999m;
        doc.DiscAmtOrPer = true;
        doc.DiscountPercent = 50m;

        await Service().UpdateTotalsAsync(doc);

        Assert.Equal(300m, doc.TotalGrossAmount);
        Assert.Equal(15m, doc.DiscountAmount);   // 10 + 5, not 999 and not 150
        Assert.Equal(285m, doc.TotalBasicAmount);
    }

    /// <summary>Statement 4, true branch (:30).</summary>
    [Fact]
    public async Task S04_WhenDiscAmtOrPerIsTrue_CallersFixedDiscountAmountSurvives()
    {
        var doc = TestCalculationDocument.With(
            TestCalculationSubItem.Line(gross: 100m),
            TestCalculationSubItem.Line(gross: 200m));
        doc.DiscAmtOrPer = true;
        doc.DiscountAmount = 40m;
        doc.DiscountPercent = 50m;   // ignored on this branch

        await Service().UpdateTotalsAsync(doc);

        Assert.Equal(40m, doc.DiscountAmount);
        Assert.Equal(260m, doc.TotalBasicAmount);
    }

    /// <summary>Statement 4, false branch (:31). 300 x 10 / 100 = 30.</summary>
    [Fact]
    public async Task S04_WhenDiscAmtOrPerIsFalse_DiscountAmountIsRecomputedFromPercent()
    {
        var doc = TestCalculationDocument.With(
            TestCalculationSubItem.Line(gross: 100m),
            TestCalculationSubItem.Line(gross: 200m));
        doc.DiscAmtOrPer = false;
        doc.DiscountAmount = 999m;   // discarded
        doc.DiscountPercent = 10m;

        await Service().UpdateTotalsAsync(doc);

        Assert.Equal(30m, doc.DiscountAmount);
        Assert.Equal(270m, doc.TotalBasicAmount);
    }

    // -------------------------------------------------------------------------
    //  Statement 5 - subtotal (:35)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Statement 5. There is no Math.Max(0, ...) at :35. A discount larger than gross
    /// produces a NEGATIVE basic amount, which then flows into the taxable value and
    /// the grand total unchallenged.
    /// </summary>
    [Fact]
    public async Task S05_WhenDiscountExceedsGross_TotalBasicAmountGoesNegative_WithNoFloorAtZero()
    {
        var doc = TestCalculationDocument.With(TestCalculationSubItem.Line(gross: 100m));
        doc.DiscAmtOrPer = true;
        doc.DiscountAmount = 150m;

        await Service().UpdateTotalsAsync(doc);

        Assert.Equal(-50m, doc.TotalBasicAmount);
        Assert.Equal(-50m, doc.TotalTaxable);
        Assert.Equal(-50m, doc.GrandTotal);
    }

    // -------------------------------------------------------------------------
    //  Statements 6, 7 and 8 - packing, insurance, freight, taxable (:38-48)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Statement 6, false branch (:39, :42). Basic = 300; packing = 300 x 10 / 100 = 30;
    /// insurance = 300 x 5 / 100 = 15. Both caller-supplied charge amounts are discarded.
    /// </summary>
    [Fact]
    public async Task S06_WhenPackingAndInsuranceAmtOrPerAreFalse_ChargesAreRecomputedFromPercent()
    {
        var doc = TestCalculationDocument.With(TestCalculationSubItem.Line(gross: 300m));
        doc.PackingAmtOrPer = false;
        doc.PackingPercent = 10m;
        doc.PackingCharges = 777m;      // discarded
        doc.InsuranceAmtOrPer = false;
        doc.InsurancePercent = 5m;
        doc.InsuranceCharges = 888m;    // discarded

        await Service().UpdateTotalsAsync(doc);

        Assert.Equal(30m, doc.PackingCharges);
        Assert.Equal(15m, doc.InsuranceCharges);
        Assert.Equal(345m, doc.TotalTaxable);
    }

    /// <summary>Statement 6, true branch (:38, :41 - the assignment is skipped).</summary>
    [Fact]
    public async Task S06_WhenPackingAndInsuranceAmtOrPerAreTrue_CallersChargesSurvive()
    {
        var doc = TestCalculationDocument.With(TestCalculationSubItem.Line(gross: 300m));
        doc.PackingAmtOrPer = true;
        doc.PackingPercent = 10m;       // ignored
        doc.PackingCharges = 77m;
        doc.InsuranceAmtOrPer = true;
        doc.InsurancePercent = 5m;      // ignored
        doc.InsuranceCharges = 33m;

        await Service().UpdateTotalsAsync(doc);

        Assert.Equal(77m, doc.PackingCharges);
        Assert.Equal(33m, doc.InsuranceCharges);
        Assert.Equal(410m, doc.TotalTaxable);
    }

    /// <summary>
    /// Statement 7. There is no FreightPercent on ICalculationDocument and no freight
    /// branch in the method - freight is only READ, at :46. The caller's value passes
    /// through untouched.
    /// </summary>
    [Fact]
    public async Task S07_FreightCharges_IsNeverComputed_AndAlwaysKeepsTheCallersValue()
    {
        var doc = TestCalculationDocument.With(TestCalculationSubItem.Line(gross: 300m));
        doc.FreightCharges = 123.45m;

        await Service().UpdateTotalsAsync(doc);

        Assert.Equal(123.45m, doc.FreightCharges);
        Assert.Equal(423.45m, doc.TotalTaxable);
    }

    /// <summary>Statement 8 (:45-48). 300 - 0 + 100 + 30 + 15 = 445.</summary>
    [Fact]
    public async Task S08_TotalTaxable_IsBasicPlusFreightPlusPackingPlusInsurance()
    {
        var doc = TestCalculationDocument.With(TestCalculationSubItem.Line(gross: 300m));
        doc.FreightCharges = 100m;
        doc.PackingPercent = 10m;       // -> 30
        doc.InsurancePercent = 5m;      // -> 15

        await Service().UpdateTotalsAsync(doc);

        Assert.Equal(300m, doc.TotalBasicAmount);
        Assert.Equal(445m, doc.TotalTaxable);
    }

    // -------------------------------------------------------------------------
    //  Statements 9 to 13 - the ITEM-WISE branch (:51-78)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Statement 9, the reset at :53-55. Every line carries a zero rate, so nothing is
    /// accumulated - and the caller's pre-set header tax totals are still wiped to 0.
    /// </summary>
    [Fact]
    public async Task S09_ItemWiseBranch_ResetsHeaderTaxTotalsToZeroBeforeAccumulating()
    {
        var doc = TestCalculationDocument.With(TestCalculationSubItem.Line(gross: 1000m));
        doc.ItemWiseTax = true;
        doc.TotalCGSTAmount = 999m;
        doc.TotalSGSTAmount = 888m;
        doc.TotalIGSTAmount = 777m;

        await Service().UpdateTotalsAsync(doc);

        Assert.Equal(0m, doc.TotalCGSTAmount);
        Assert.Equal(0m, doc.TotalSGSTAmount);
        Assert.Equal(0m, doc.TotalIGSTAmount);
    }

    /// <summary>
    /// Statements 9, 10 and 12 together - the branch genuinely exercised with THREE
    /// lines at THREE different rate shapes.
    ///
    /// Gross 500 / 300 / 200 = 1000, freight 200, no discounts.
    ///   proportions 0.5 / 0.3 / 0.2
    ///   bases       500+100=600, 300+60=360, 200+40=240
    ///   L1 CGST 2.5% -> 15,    SGST 2.5% -> 15
    ///   L2 CGST 6%   -> 21.6,  SGST 6%   -> 21.6
    ///   L3 IGST 18%  -> 43.2
    ///   totals CGST 36.6, SGST 36.6, IGST 43.2
    ///   taxable 1200, grand 1200 + 116.4 = 1316.4
    /// </summary>
    [Fact]
    public async Task S09_ItemWiseBranch_WithThreeLinesAtDifferentRates_AllocatesFreightByGrossProportion()
    {
        var doc = TestCalculationDocument.With(
            TestCalculationSubItem.Line(gross: 500m, cgstRate: 2.5m, sgstRate: 2.5m),
            TestCalculationSubItem.Line(gross: 300m, cgstRate: 6m, sgstRate: 6m),
            TestCalculationSubItem.Line(gross: 200m, igstRate: 18m));
        doc.ItemWiseTax = true;
        doc.FreightCharges = 200m;

        await Service().UpdateTotalsAsync(doc);

        Assert.Equal(1000m, doc.TotalGrossAmount);
        Assert.Equal(1200m, doc.TotalTaxable);
        Assert.Equal(36.6m, doc.TotalCGSTAmount);
        Assert.Equal(36.6m, doc.TotalSGSTAmount);
        Assert.Equal(43.2m, doc.TotalIGSTAmount);
        Assert.Equal(1316.4m, doc.GrandTotal);
    }

    /// <summary>
    /// Statement 10, the divide-by-zero guard at :61. Every LineGross is 0, so
    /// totalLineGross is 0 and proportion is forced to 0 rather than throwing.
    ///
    /// The consequence worth noticing: freight of 100 IS in TotalTaxable but is NOT in
    /// any per-line taxable base, so it attracts no tax at all even though both lines
    /// carry an 18% rate.
    /// </summary>
    [Fact]
    public async Task S10_ItemWiseBranch_WhenEveryLineGrossIsZero_ProportionGuardYieldsZeroTax()
    {
        var doc = TestCalculationDocument.With(
            TestCalculationSubItem.Line(gross: 0m, igstRate: 18m),
            TestCalculationSubItem.Line(gross: 0m, igstRate: 18m));
        doc.ItemWiseTax = true;
        doc.FreightCharges = 100m;

        await Service().UpdateTotalsAsync(doc);

        Assert.Equal(0m, doc.TotalGrossAmount);
        Assert.Equal(0m, doc.TotalBasicAmount);
        Assert.Equal(100m, doc.TotalTaxable);
        Assert.Equal(0m, doc.TotalIGSTAmount);
        Assert.Equal(100m, doc.GrandTotal);
    }

    /// <summary>
    /// Statement 11 - THE SURPRISING ONE, AND IT IS PINNED DELIBERATELY.
    ///
    /// proportionalDiscount at :63-65 is 0 whenever DiscAmtOrPer is true. So a fixed
    /// header discount of 100 on a gross of 1000 reduces TotalBasicAmount and
    /// TotalTaxable to 900, but the item-wise tax is still charged on the FULL 1000:
    ///   CGST = 600 x 9% + 400 x 9% = 54 + 36 = 90
    /// Tax computed on the discounted 900 would be 81. The 90 is what the code does.
    ///
    /// Whether that is intended is a product question, not a source question - see
    /// docs/kb/open-questions.md. Do not "fix" this to make the numbers agree.
    /// </summary>
    [Fact]
    public async Task S11_ItemWiseBranch_WhenHeaderDiscountIsAFixedAmount_ItDoesNotReduceTheTaxableBase()
    {
        var doc = TestCalculationDocument.With(
            TestCalculationSubItem.Line(gross: 600m, cgstRate: 9m, sgstRate: 9m),
            TestCalculationSubItem.Line(gross: 400m, cgstRate: 9m, sgstRate: 9m));
        doc.ItemWiseTax = true;
        doc.DiscAmtOrPer = true;
        doc.DiscountAmount = 100m;

        await Service().UpdateTotalsAsync(doc);

        Assert.Equal(1000m, doc.TotalGrossAmount);
        Assert.Equal(900m, doc.TotalBasicAmount);
        Assert.Equal(900m, doc.TotalTaxable);

        // Tax on the UNDISCOUNTED 1000, not on the taxable 900.
        Assert.Equal(90m, doc.TotalCGSTAmount);
        Assert.Equal(90m, doc.TotalSGSTAmount);
        Assert.NotEqual(81m, doc.TotalCGSTAmount);

        Assert.Equal(1080m, doc.GrandTotal);
    }

    /// <summary>
    /// Statement 11, the other side of the same condition. With DiscAmtOrPer FALSE and
    /// no line discounts, the header discount IS spread by proportion:
    ///   L1 600 - 60 = 540 -> 48.6 ; L2 400 - 40 = 360 -> 32.4 ; total 81.
    /// Compare with the test above: the same 100 of discount, taxed differently purely
    /// because of a boolean flag.
    /// </summary>
    [Fact]
    public async Task S11_ItemWiseBranch_WhenHeaderDiscountIsAPercentage_ItIsSpreadAcrossLinesByProportion()
    {
        var doc = TestCalculationDocument.With(
            TestCalculationSubItem.Line(gross: 600m, cgstRate: 9m, sgstRate: 9m),
            TestCalculationSubItem.Line(gross: 400m, cgstRate: 9m, sgstRate: 9m));
        doc.ItemWiseTax = true;
        doc.DiscAmtOrPer = false;
        doc.DiscountPercent = 10m;      // -> DiscountAmount 100

        await Service().UpdateTotalsAsync(doc);

        Assert.Equal(100m, doc.DiscountAmount);
        Assert.Equal(900m, doc.TotalTaxable);
        Assert.Equal(81m, doc.TotalCGSTAmount);
        Assert.Equal(81m, doc.TotalSGSTAmount);
        Assert.Equal(1062m, doc.GrandTotal);
    }

    /// <summary>
    /// Statement 12, the full per-line formula at :67-72.
    ///
    /// Lines 600/-60 and 400/-40; freight 100, packing 50 and insurance 30 all fixed.
    ///   line discounts exist -> DiscountAmount = 100, basic = 900,
    ///   taxable = 900 + 100 + 50 + 30 = 1080
    ///   proportionalDiscount = 0 (a line carries its own discount) - :63
    ///   L1 base = 600 - 60 + 0.6 x 180 = 540 + 108 = 648 -> CGST/SGST 9% = 58.32 each
    ///   L2 base = 400 - 40 + 0.4 x 180 = 360 +  72 = 432 -> IGST 18%      = 77.76
    ///   grand   = 1080 + 58.32 + 58.32 + 77.76 = 1274.40
    /// </summary>
    [Fact]
    public async Task S12_ItemWiseBranch_PerLineTaxableBase_SubtractsLineDiscountAndAddsProportionalCharges()
    {
        var doc = TestCalculationDocument.With(
            TestCalculationSubItem.Line(gross: 600m, lineDiscount: 60m, cgstRate: 9m, sgstRate: 9m),
            TestCalculationSubItem.Line(gross: 400m, lineDiscount: 40m, igstRate: 18m));
        doc.ItemWiseTax = true;
        doc.FreightCharges = 100m;
        doc.PackingAmtOrPer = true;
        doc.PackingCharges = 50m;
        doc.InsuranceAmtOrPer = true;
        doc.InsuranceCharges = 30m;

        await Service().UpdateTotalsAsync(doc);

        Assert.Equal(100m, doc.DiscountAmount);
        Assert.Equal(900m, doc.TotalBasicAmount);
        Assert.Equal(1080m, doc.TotalTaxable);
        Assert.Equal(58.32m, doc.TotalCGSTAmount);
        Assert.Equal(58.32m, doc.TotalSGSTAmount);
        Assert.Equal(77.76m, doc.TotalIGSTAmount);
        Assert.Equal(1274.40m, doc.GrandTotal);
    }

    /// <summary>
    /// Statement 13. The engine accumulates into the three HEADER totals only
    /// (:74-76). LineCGSTAmount / LineSGSTAmount / LineIGSTAmount are get-only on
    /// ICalculationDocumentSubItem (:23-25) and are never written - the sentinels
    /// below survive the call untouched. Whoever populates the per-line amounts for
    /// printing, it is not this service.
    /// </summary>
    [Fact]
    public async Task S13_ItemWiseBranch_PerLineTaxAmountsAreNeverWritten_OnlyHeaderTotalsAccumulate()
    {
        var line = new TestCalculationSubItem
        {
            LineGross = 1000m,
            LineCGSTRate = 9m,
            LineSGSTRate = 9m,
            LineCGSTAmount = 12345m,   // sentinels
            LineSGSTAmount = 23456m,
            LineIGSTAmount = 34567m,
            LineBasicAmount = 45678m,
        };
        var doc = TestCalculationDocument.With(line);
        doc.ItemWiseTax = true;

        await Service().UpdateTotalsAsync(doc);

        Assert.Equal(90m, doc.TotalCGSTAmount);
        Assert.Equal(90m, doc.TotalSGSTAmount);

        Assert.Equal(12345m, line.LineCGSTAmount);
        Assert.Equal(23456m, line.LineSGSTAmount);
        Assert.Equal(34567m, line.LineIGSTAmount);
        Assert.Equal(45678m, line.LineBasicAmount);
    }

    // -------------------------------------------------------------------------
    //  Statement 14 - the HEADER-WISE branch (:81-83)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Statement 14. HasItemWiseTax false: the header rates apply to the whole
    /// TotalTaxable and the per-line rates (99% below) are ignored completely.
    /// 1000 x 9% = 90 each; 1000 x 0% IGST = 0.
    /// </summary>
    [Fact]
    public async Task S14_HeaderWiseBranch_UsesHeaderRates_AndPerLineRatesAreIgnored()
    {
        var doc = TestCalculationDocument.With(
            TestCalculationSubItem.Line(gross: 600m, cgstRate: 99m, sgstRate: 99m, igstRate: 99m),
            TestCalculationSubItem.Line(gross: 400m, cgstRate: 99m, sgstRate: 99m, igstRate: 99m));
        doc.ItemWiseTax = false;
        doc.CGstRate = 9m;
        doc.SGstRate = 9m;
        doc.IGstRate = 0m;

        await Service().UpdateTotalsAsync(doc);

        Assert.Equal(1000m, doc.TotalTaxable);
        Assert.Equal(90m, doc.TotalCGSTAmount);
        Assert.Equal(90m, doc.TotalSGSTAmount);
        Assert.Equal(0m, doc.TotalIGSTAmount);
        Assert.Equal(1180m, doc.GrandTotal);
    }

    // -------------------------------------------------------------------------
    //  Statements 15 and 16 - TCS and the pre-round total (:87-99)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Statement 15. The TCS base is grandBeforeTCS = taxable + CGST + SGST + IGST
    /// (:87-90), i.e. TAX-INCLUSIVE, and OtherCharges is deliberately NOT in it (it is
    /// only added afterwards, at :99).
    ///   taxable 1000, CGST 90, SGST 90 -> base 1180
    ///   TCS 1% of 1180 = 11.80  (NOT 12.80, which is 1% of 1180 + 100 other charges)
    /// </summary>
    [Fact]
    public async Task S15_TcsPercent_IsAppliedToATaxInclusiveBase_AndOtherChargesAreExcluded()
    {
        var doc = TestCalculationDocument.With(TestCalculationSubItem.Line(gross: 1000m));
        doc.CGstRate = 9m;
        doc.SGstRate = 9m;
        doc.TCSAmtOrPer = false;
        doc.TCSPercent = 1m;
        doc.OtherCharges = 100m;

        await Service().UpdateTotalsAsync(doc);

        Assert.Equal(11.80m, doc.TCSAmount);
        Assert.NotEqual(12.80m, doc.TCSAmount);
        Assert.Equal(1291.80m, doc.GrandTotal);
    }

    /// <summary>Statement 15, the true branch at :94 - the caller's amount survives.</summary>
    [Fact]
    public async Task S15_WhenTcsAmtOrPerIsTrue_CallersFixedTcsAmountSurvives()
    {
        var doc = TestCalculationDocument.With(TestCalculationSubItem.Line(gross: 1000m));
        doc.TCSAmtOrPer = true;
        doc.TCSAmount = 25m;
        doc.TCSPercent = 50m;   // ignored on this branch

        await Service().UpdateTotalsAsync(doc);

        Assert.Equal(25m, doc.TCSAmount);
        Assert.Equal(1025m, doc.GrandTotal);
    }

    /// <summary>
    /// Statement 16 (:99). Each of the three addends is distinct so none can be dropped
    /// without failing: 1000 taxable + 180 tax = 1180, + 20 TCS + 5 other = 1205.
    /// </summary>
    [Fact]
    public async Task S16_PreRoundGrandTotal_IsGrandBeforeTcsPlusTcsPlusOtherCharges()
    {
        var doc = TestCalculationDocument.With(TestCalculationSubItem.Line(gross: 1000m));
        doc.CGstRate = 9m;
        doc.SGstRate = 9m;
        doc.TCSAmtOrPer = true;
        doc.TCSAmount = 20m;
        doc.OtherCharges = 5m;

        await Service().UpdateTotalsAsync(doc);

        Assert.Equal(1000m, doc.TotalTaxable);
        Assert.Equal(20m, doc.TCSAmount);
        Assert.Equal(1205m, doc.GrandTotal);
        Assert.Equal(0m, doc.RoundOff);
    }

    // -------------------------------------------------------------------------
    //  Statements 17 and 18 - round-off and precision (:101-111)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Statement 17. preRoundGrandTotal is exactly 100.5. MidpointRounding.AwayFromZero
    /// (:103) gives 101; banker's rounding - the .NET default for Math.Round - would
    /// give 100, because 100 is even. This test is the discriminator between the two.
    /// </summary>
    [Fact]
    public async Task S17_WhenRoundOffEnabled_MidpointRoundsAwayFromZero_NotBankers()
    {
        var doc = TestCalculationDocument.With(TestCalculationSubItem.Line(gross: 100.5m));
        doc.IsRoundOffEnabled = true;

        await Service().UpdateTotalsAsync(doc);

        Assert.Equal(100.5m, doc.TotalTaxable);
        Assert.Equal(101m, doc.GrandTotal);      // banker's would be 100
        Assert.Equal(0.5m, doc.RoundOff);
    }

    /// <summary>
    /// Statement 17, the second midpoint. 102.5 rounds to 103 away from zero; banker's
    /// would give 102. Both midpoint tests are needed because either one alone agrees
    /// with banker's rounding for half of the possible integers.
    /// </summary>
    [Fact]
    public async Task S17_WhenRoundOffEnabled_AndAnOddIntegerMidpoint_StillRoundsAwayFromZero()
    {
        var doc = TestCalculationDocument.With(TestCalculationSubItem.Line(gross: 102.5m));
        doc.IsRoundOffEnabled = true;

        await Service().UpdateTotalsAsync(doc);

        Assert.Equal(103m, doc.GrandTotal);      // banker's would be 102
        Assert.Equal(0.5m, doc.RoundOff);
    }

    /// <summary>
    /// Statement 17. RoundOff is SIGNED: rounded - preRound (:104). Rounding DOWN
    /// therefore yields a negative RoundOff, which a caller that treats it as a
    /// magnitude will get wrong.
    /// </summary>
    [Fact]
    public async Task S17_WhenRoundingDown_RoundOffIsNegative()
    {
        var doc = TestCalculationDocument.With(TestCalculationSubItem.Line(gross: 100.4m));
        doc.IsRoundOffEnabled = true;

        await Service().UpdateTotalsAsync(doc);

        Assert.Equal(100m, doc.GrandTotal);
        Assert.Equal(-0.4m, doc.RoundOff);
        Assert.True(doc.RoundOff < 0m);
    }

    /// <summary>Statement 17, the disabled branch (:109-110).</summary>
    [Fact]
    public async Task S17_WhenRoundOffDisabled_RoundOffIsZero_AndGrandTotalIsNotRounded()
    {
        var doc = TestCalculationDocument.With(TestCalculationSubItem.Line(gross: 100.4m));
        doc.IsRoundOffEnabled = false;

        await Service().UpdateTotalsAsync(doc);

        Assert.Equal(100.4m, doc.GrandTotal);
        Assert.Equal(0m, doc.RoundOff);
    }

    /// <summary>
    /// Statement 18. There is exactly ONE Math.Round in the whole method, at :103.
    /// Nothing is rounded to currency precision along the way, so five decimal places
    /// survive all the way to the grand total:
    ///   discount 100 x 33.333 / 100 = 33.333 ; basic = taxable = 66.667
    ///   IGST     66.667 x 18 / 100  = 12.00006
    ///   grand    66.667 + 12.00006  = 78.66706
    /// If any intermediate were rounded to 2 decimal places this would be 78.67 or
    /// 78.66 - which is exactly the regression this test exists to catch, and exactly
    /// the contract POST /api/documents/calculate (M2) must honour.
    /// </summary>
    [Fact]
    public async Task S18_NoIntermediateRounding_FullDecimalPrecisionSurvivesToTheGrandTotal()
    {
        var doc = TestCalculationDocument.With(TestCalculationSubItem.Line(gross: 100m));
        doc.DiscAmtOrPer = false;
        doc.DiscountPercent = 33.333m;
        doc.IGstRate = 18m;
        doc.IsRoundOffEnabled = false;

        await Service().UpdateTotalsAsync(doc);

        Assert.Equal(33.333m, doc.DiscountAmount);
        Assert.Equal(66.667m, doc.TotalBasicAmount);
        Assert.Equal(12.00006m, doc.TotalIGSTAmount);
        Assert.Equal(78.66706m, doc.GrandTotal);
    }

    // -------------------------------------------------------------------------
    //  Statement 19 - the async signature (:113)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Statement 19. The method is declared async and ends with `await
    /// Task.CompletedTask` (:113), but does no asynchronous work: the returned Task is
    /// ALREADY completed before it is awaited.
    ///
    /// This is load-bearing. Four call sites invoke UpdateTotalsAsync WITHOUT awaiting
    /// it, from void handlers -
    /// V.SMART/V.SMART.Shared/Pages/OutSourcing_Module_pages/DebitNote_pages/DebitNoteUpsert.razor:2629,
    /// :2635, :2641, :2647 - and they are correct only because of this. If the engine
    /// ever became genuinely asynchronous, this test failing is the warning that those
    /// four handlers now read stale totals.
    /// </summary>
    [Fact]
    public void S19_UpdateTotalsAsync_CompletesSynchronously_DespiteTheAsyncSignature()
    {
        var doc = TestCalculationDocument.With(TestCalculationSubItem.Line(gross: 1000m));

        var task = Service().UpdateTotalsAsync(doc);

        Assert.True(task.IsCompleted);
        Assert.Equal(1000m, doc.GrandTotal);
    }

    // -------------------------------------------------------------------------
    //  Statement 21 - no rate validation (R-15)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Statement 21 / risk R-15. 17% is NOT a member of CommonConstants.IGSTRates
    /// (CommonConstants.cs:11-16). CalculationService never calls GetIGST/GetGST and
    /// never validates a rate, so it multiplies by 17 quite happily:
    /// 1000 x 17 / 100 = 170.
    ///
    /// The R-15 hazard is the pairing: anything that routes the rate through
    /// CommonConstants.GetIGST first would silently turn 17 into 0 and produce 0 tax
    /// instead of 170 - see CommonConstantsGstRateTests.
    /// </summary>
    [Fact]
    public async Task S21_R15_AnUnlistedGstRate_IsAppliedWithoutValidationOrCoercionToZero()
    {
        Assert.DoesNotContain(17m, CommonConstants.IGSTRates);

        var doc = TestCalculationDocument.With(TestCalculationSubItem.Line(gross: 1000m));
        doc.IGstRate = 17m;

        await Service().UpdateTotalsAsync(doc);

        Assert.Equal(170m, doc.TotalIGSTAmount);
        Assert.NotEqual(0m, doc.TotalIGSTAmount);
        Assert.Equal(1170m, doc.GrandTotal);
    }
}
