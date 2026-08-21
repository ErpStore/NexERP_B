using System.ComponentModel.DataAnnotations;
using System.Globalization;
using V.SMART.Shared.Utility_Constants;

namespace V.SMART.Api.Contracts
{
    /// <summary>
    /// M2-B09 — validates that a GST rate is one of the permitted ladder values, rejecting
    /// anything else with a 400 rather than letting it through. This is the API-boundary half
    /// of the fix for <b>R-15</b>.
    ///
    /// <para><b>The defect this exists to stop.</b>
    /// <c>CommonConstants.GetIGST</c>/<c>GetGST</c> are
    /// <c>rates.FirstOrDefault(r =&gt; r == rate)</c>, and <c>FirstOrDefault</c> over
    /// <c>List&lt;decimal&gt;</c> returns <c>default(decimal)</c> — <b>zero</b> — when nothing
    /// matches. So <c>GetIGST(19m)</c> returns <c>0</c>, which is indistinguishable from the
    /// legitimate <c>GetIGST(0m)</c>. A typo becomes a zero-tax invoice, silently.</para>
    ///
    /// <para><b>Why this does not call <c>GetIGST</c>/<c>GetGST</c>.</b> It cannot: their
    /// return value is exactly the ambiguity being fixed. Membership is tested against the
    /// ladder lists directly, where "absent" and "zero" are distinct answers.</para>
    ///
    /// <para><b>What is deliberately NOT changed.</b>
    /// <c>CommonConstants.cs</c> is untouched. Its two methods have 105 call sites across the
    /// Blazor app; changing their return type or making them throw would alter behaviour for
    /// every one of them, which is a separate decision with a separate blast radius. R-15 is
    /// therefore <b>partially</b> resolved: correct at the API boundary, still coercing
    /// in-process. Closing it fully means changing those methods and auditing all 105 callers.</para>
    /// </summary>
    /// <remarks>
    /// Decimal comparison here is exact equality against the ladder, matching what
    /// <c>CommonConstants</c> itself does. That is correct for this data: the ladders are
    /// literal <c>decimal</c> constants and the wire value is parsed as <c>decimal</c>, so
    /// there is no binary-floating-point rounding in the path. Note <c>decimal</c> equality
    /// ignores trailing zeroes — <c>18m == 18.000m</c> is true — so a client may send either
    /// form. A <c>double</c>-based comparison would not have that property, which is one more
    /// reason nothing in this path uses <c>double</c>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class GstRateAttribute : ValidationAttribute
    {
        /// <summary>Which ladder the annotated value must belong to.</summary>
        public GstLadder Ladder { get; }

        public GstRateAttribute(GstLadder ladder = GstLadder.Igst) => Ladder = ladder;

        private IReadOnlyList<decimal> PermittedValues => Ladder == GstLadder.Igst
            ? CommonConstants.IGSTRates
            : CommonConstants.GSTRates;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // A null rate is "not supplied", which is [Required]'s job, not this attribute's.
            // Conflating the two would make an optional rate impossible to express.
            if (value is null)
            {
                return ValidationResult.Success;
            }

            if (value is not decimal rate)
            {
                return new ValidationResult(
                    $"{validationContext.MemberName ?? "value"} must be a decimal GST rate.",
                    MemberName(validationContext));
            }

            if (PermittedValues.Contains(rate))
            {
                return ValidationResult.Success;
            }

            var permitted = string.Join(", ", PermittedValues.Select(r => r.ToString("0.000", CultureInfo.InvariantCulture)));
            var ladderName = Ladder == GstLadder.Igst ? "IGST" : "CGST/SGST";

            return new ValidationResult(
                $"'{rate.ToString("0.000", CultureInfo.InvariantCulture)}' is not a permitted {ladderName} rate. Permitted values: {permitted}.",
                MemberName(validationContext));
        }

        private static IEnumerable<string> MemberName(ValidationContext context) =>
            context.MemberName is null ? Array.Empty<string>() : new[] { context.MemberName };
    }

    /// <summary>Which of the two ladders in <c>CommonConstants</c> applies.</summary>
    public enum GstLadder
    {
        /// <summary>The integrated ladder — <c>CommonConstants.IGSTRates</c>.</summary>
        Igst = 0,

        /// <summary>The half-rate ladder CGST and SGST each carry — <c>CommonConstants.GSTRates</c>.</summary>
        CgstSgst = 1
    }
}
