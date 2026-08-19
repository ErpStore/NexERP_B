// =============================================================================
//  CHARACTERISATION SUITE - CommonConstants GST rate lists (task M0-12-02)
//  BR-CALC-002, statement 20. Companion to CalculationServiceCharacterisationTests.
// -----------------------------------------------------------------------------
//  THIS SUITE PINS CURRENT BEHAVIOUR, INCLUDING THE KNOWN DEFECT R-15.
//
//  A FAILING TEST HERE MEANS BEHAVIOUR CHANGED, not that the test is wrong. If
//  the change was intended, update the test IN THE SAME COMMIT as the production
//  change and record it in docs/kb/business-rules/business-rule-inventory.md
//  (KB-030, BR-CALC-002) and docs/kb/risks/technical-debt-register.md (KB-060,
//  R-15).
//
//  Subject under test (read-only, NEVER modified):
//      V.SMART/V.SMART.Shared/Utility_Constants/CommonConstants.cs
//      IGSTRates :11-16, GSTRates :18-23, GetIGST/GetGST :25-26
//      (re-verified against source on 2026-08-19)
//
//  R-15: GetIGST and GetGST are FirstOrDefault(r => r == rate). FirstOrDefault
//  returns default(decimal) - i.e. 0 - when nothing matches. An unlisted rate is
//  therefore silently coerced to ZERO TAX rather than rejected. Neither method
//  can distinguish "not found" from the legitimately listed 0.000m rate, so the
//  tests below can pin the returned value but NOT that distinction; making it
//  distinguishable would require a nullable return, which is a production change
//  and out of scope here.
//
//  Not asserted: the decimal SCALE of the list members. The list literals are
//  written as 0.000m / 18.000m, but decimal equality is numeric, so
//  Assert.Equal(18m, ...) passes for 18.000m. See the same note in
//  CalculationServiceCharacterisationTests.
// =============================================================================

using V.SMART.Shared.Utility_Constants;
using Xunit;

namespace V.SMART.Shared.Tests.Services;

public class CommonConstantsGstRateTests
{
    /// <summary>
    /// Statement 20. The IGST list is fixed and closed (CommonConstants.cs:11-16).
    /// Both membership and ORDER are pinned: any UI that binds a dropdown to this list
    /// renders it in exactly this sequence.
    /// </summary>
    [Fact]
    public void IGSTRates_IsTheFixedTwelveValueList_InThisOrder()
    {
        Assert.Equal(
            new[] { 0m, 0.1m, 0.25m, 1m, 1.5m, 3m, 5m, 6m, 7.5m, 12m, 18m, 28m },
            CommonConstants.IGSTRates);
    }

    /// <summary>
    /// Statement 20. The CGST/SGST list is the half-rate list
    /// (CommonConstants.cs:18-23) - each entry is half of the corresponding IGST rate.
    /// </summary>
    [Fact]
    public void GSTRates_IsTheFixedHalfRateList_InThisOrder()
    {
        Assert.Equal(
            new[] { 0m, 0.05m, 0.125m, 0.5m, 0.75m, 1.5m, 2.5m, 3m, 3.75m, 6m, 9m, 14m },
            CommonConstants.GSTRates);
    }

    /// <summary>Statement 20. A listed rate round-trips unchanged (:25).</summary>
    [Fact]
    public void GetIGST_WithAListedRate_ReturnsThatRate()
    {
        Assert.Equal(18m, CommonConstants.GetIGST(18m));
        Assert.Equal(0.25m, CommonConstants.GetIGST(0.25m));
    }

    /// <summary>Statement 20. A listed rate round-trips unchanged (:26).</summary>
    [Fact]
    public void GetGST_WithAListedRate_ReturnsThatRate()
    {
        Assert.Equal(9m, CommonConstants.GetGST(9m));
        Assert.Equal(0.125m, CommonConstants.GetGST(0.125m));
    }

    /// <summary>
    /// Statement 20 / risk R-15 - THE DEFECT, PINNED DELIBERATELY.
    ///
    /// 17 is not in IGSTRates, so FirstOrDefault returns default(decimal) = 0. A caller
    /// that trusts this result charges NO TAX on a document whose rate was merely
    /// mistyped. Nothing throws and nothing is logged.
    ///
    /// Do not "fix" this method to make the test read better. R-15 stays open in
    /// docs/kb/risks/technical-debt-register.md until a product decision is taken.
    /// </summary>
    [Fact]
    public void GetIGST_WithUnlistedRate_SilentlyReturnsZero_R15()
    {
        Assert.DoesNotContain(17m, CommonConstants.IGSTRates);

        Assert.Equal(0m, CommonConstants.GetIGST(17m));
        Assert.Equal(0m, CommonConstants.GetIGST(-5m));
        Assert.Equal(0m, CommonConstants.GetIGST(28.0001m));
    }

    /// <summary>Statement 20 / risk R-15, the CGST/SGST half of the same defect (:26).</summary>
    [Fact]
    public void GetGST_WithUnlistedRate_SilentlyReturnsZero_R15()
    {
        Assert.DoesNotContain(8.5m, CommonConstants.GSTRates);

        Assert.Equal(0m, CommonConstants.GetGST(8.5m));
        Assert.Equal(0m, CommonConstants.GetGST(18m));   // an IGST rate, not a GST rate
    }

    /// <summary>
    /// Statement 20 / risk R-15. The reason R-15 cannot be detected at the call site:
    /// the "not found" answer is byte-identical to the answer for the legitimately
    /// listed 0.000m rate (CommonConstants.cs:13, :20). A caller has no way to tell a
    /// zero-rated document from a mistyped one.
    /// </summary>
    [Fact]
    public void GetIGST_CannotDistinguishNotFoundFromTheListedZeroRate_R15()
    {
        Assert.Contains(0m, CommonConstants.IGSTRates);

        Assert.Equal(CommonConstants.GetIGST(0m), CommonConstants.GetIGST(17m));
    }
}
