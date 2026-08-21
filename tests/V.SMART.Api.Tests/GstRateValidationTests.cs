using System.ComponentModel.DataAnnotations;
using V.SMART.Api.Contracts;
using V.SMART.Shared.Utility_Constants;
using Xunit;

namespace V.SMART.Api.Tests
{
    /// <summary>
    /// M2-B09 — the API-boundary half of the <b>R-15</b> fix.
    ///
    /// <para>The defect: <c>CommonConstants.GetIGST</c>/<c>GetGST</c> are
    /// <c>FirstOrDefault(r =&gt; r == rate)</c> over a <c>List&lt;decimal&gt;</c>, so an
    /// unknown rate returns <c>default(decimal)</c> — zero — which is indistinguishable from the
    /// legitimate zero rate. A typo becomes a zero-tax invoice.</para>
    ///
    /// <para>These tests pin the two properties that matter: an off-ladder rate is
    /// <b>rejected</b> rather than coerced, and <c>0.000</c> is still <b>accepted</b>. The second
    /// is as important as the first — an over-eager fix that rejected zero would break every
    /// genuinely zero-rated line.</para>
    /// </summary>
    public class GstRateValidationTests
    {
        private sealed class IgstHolder
        {
            [GstRate(GstLadder.Igst)]
            public decimal? Rate { get; init; }
        }

        private sealed class CgstSgstHolder
        {
            [GstRate(GstLadder.CgstSgst)]
            public decimal? Rate { get; init; }
        }

        private static (bool IsValid, IList<ValidationResult> Results) Validate(object instance)
        {
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(
                instance, new ValidationContext(instance), results, validateAllProperties: true);

            return (isValid, results);
        }

        [Fact]
        public void Every_seeded_IGST_rate_is_accepted()
        {
            // The ladder is the authority; this asserts the attribute agrees with it in full
            // rather than spot-checking a couple of values.
            foreach (var rate in CommonConstants.IGSTRates)
            {
                var (isValid, _) = Validate(new IgstHolder { Rate = rate });

                Assert.True(isValid, $"IGST rate {rate} is in CommonConstants.IGSTRates but was rejected.");
            }
        }

        [Fact]
        public void Every_seeded_CgstSgst_rate_is_accepted()
        {
            foreach (var rate in CommonConstants.GSTRates)
            {
                var (isValid, _) = Validate(new CgstSgstHolder { Rate = rate });

                Assert.True(isValid, $"CGST/SGST rate {rate} is in CommonConstants.GSTRates but was rejected.");
            }
        }

        [Fact]
        public void Zero_is_accepted_because_it_is_a_legitimate_rate()
        {
            // The whole point of R-15 is that "zero" and "not found" were the same answer.
            // Distinguishing them must not be done by rejecting zero.
            var (isValid, _) = Validate(new IgstHolder { Rate = 0.000m });

            Assert.True(isValid);
        }

        [Theory]
        [InlineData(19)]     // plausible typo for 18
        [InlineData(28.5)]
        [InlineData(-5)]
        [InlineData(100)]
        [InlineData(0.001)]  // just off 0.000
        public void An_off_ladder_rate_is_rejected(decimal rate)
        {
            var (isValid, results) = Validate(new IgstHolder { Rate = rate });

            Assert.False(isValid, $"Off-ladder rate {rate} was accepted.");
            Assert.Single(results);
        }

        [Fact]
        public void The_rejection_message_lists_the_permitted_values()
        {
            // ADR-002 §4: the error must tell the caller what would have been valid, not merely
            // that they were wrong.
            var (_, results) = Validate(new IgstHolder { Rate = 19m });

            var message = Assert.Single(results).ErrorMessage;

            Assert.NotNull(message);
            Assert.Contains("not a permitted", message);

            foreach (var permitted in CommonConstants.IGSTRates)
            {
                Assert.Contains(permitted.ToString("0.000"), message);
            }
        }

        [Fact]
        public void The_rejection_names_the_offending_member()
        {
            // The 400 body's `errors` entry is keyed by member name; without this the client
            // cannot highlight the field.
            var (_, results) = Validate(new IgstHolder { Rate = 19m });

            Assert.Contains(nameof(IgstHolder.Rate), Assert.Single(results).MemberNames);
        }

        [Fact]
        public void Null_is_left_to_Required_rather_than_treated_as_invalid()
        {
            // Conflating "absent" with "invalid" would make an optional rate inexpressible.
            var (isValid, _) = Validate(new IgstHolder { Rate = null });

            Assert.True(isValid);
        }

        [Fact]
        public void The_two_ladders_are_enforced_separately()
        {
            // 28.000 is a valid IGST rate and NOT a valid CGST/SGST rate. If the attribute
            // validated against the wrong list this passes silently in production.
            Assert.Contains(28.000m, CommonConstants.IGSTRates);
            Assert.DoesNotContain(28.000m, CommonConstants.GSTRates);

            Assert.True(Validate(new IgstHolder { Rate = 28.000m }).IsValid);
            Assert.False(Validate(new CgstSgstHolder { Rate = 28.000m }).IsValid);
        }

        [Fact]
        public void The_attribute_does_not_depend_on_the_coercing_helpers()
        {
            // A regression guard for the specific mistake the task warns about: using
            // GetIGST/GetGST to test validity re-imports the ambiguity being fixed, because
            // their 0 means both "zero rate" and "not found".
            Assert.Equal(0m, CommonConstants.GetIGST(19m));   // the defect, pinned
            Assert.Equal(0m, CommonConstants.GetIGST(0m));    // indistinguishable from it

            // The attribute tells them apart, which is only possible without those helpers.
            Assert.False(Validate(new IgstHolder { Rate = 19m }).IsValid);
            Assert.True(Validate(new IgstHolder { Rate = 0m }).IsValid);
        }
    }
}
