using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace V.SMART.Api.Contracts
{
    /// <summary>
    /// The typed query DTO for <c>GET api/currencies</c> — the reference implementation of the
    /// M2-B02 list contract.
    ///
    /// <para>The four filters are exactly the keys <c>CurrencyFilterBuilder</c> understands
    /// (<c>CurrencyService.cs:193-207</c>): <c>CurrName</c> and <c>CreatedBy</c> as
    /// case-insensitive <c>LIKE '%value%'</c>, <c>FromDate</c> as
    /// <c>CreatedDate &gt;= fromDate.Date</c> and <c>ToDate</c> as
    /// <c>CreatedDate &lt;= toDate.Date.AddDays(1).AddTicks(-1)</c> (inclusive end of day).</para>
    ///
    /// <para><b><c>FromDate</c>/<c>ToDate</c> are <see cref="DateTime"/>?, not <c>string?</c>.</b>
    /// They were <c>string?</c> and re-parsed inside the filter builder, where an unparseable
    /// value fell through <c>_ =&gt; query</c> and was silently discarded. Model binding now
    /// rejects it as a 400 with an <c>errors</c> dictionary keyed by the field — ADR-002 §4.</para>
    /// </summary>
    public sealed record CurrencyQuery : PagedQuery
    {
        /// <summary>The wire name of <see cref="CurrName"/> — the query parameter and the
        /// <c>errors</c> dictionary key. See <see cref="PagedQuery"/> for why the wire names are
        /// declared explicitly rather than inherited from the C# property names.</summary>
        public const string CurrNameParameter = "currName";

        /// <summary>The wire name of <see cref="CreatedBy"/>.</summary>
        public const string CreatedByParameter = "createdBy";

        /// <summary>The wire name of <see cref="FromDate"/>.</summary>
        public const string FromDateParameter = "fromDate";

        /// <summary>The wire name of <see cref="ToDate"/>.</summary>
        public const string ToDateParameter = "toDate";

        /// <summary>
        /// Sortable fields for this resource, derived from what the list screen actually shows:
        /// the five grid columns (<c>CurrencyList.razor:119-145</c> — CurrId, CurrName, CurrSub,
        /// Symbol, IsSystemDefined) plus the two fields its filter panel exposes (CreatedBy and
        /// CreatedDate). Every name maps to a real column on <c>Currency</c>
        /// (<c>Data/Master/Accounts_Module/Currency.cs:9-29</c>); the navigation collection is
        /// deliberately absent.
        /// </summary>
        public static readonly IReadOnlyList<string> Sortable = new[]
        {
            "currId", "currName", "currSub", "symbol", "isSystemDefined", "createdBy", "createdDate"
        };

        /// <inheritdoc />
        protected override IReadOnlyList<string> SortableFields => Sortable;

        /// <summary>Case-insensitive contains match on the currency name. Wire name <c>currName</c>.</summary>
        [FromQuery(Name = CurrNameParameter)]
        public string? CurrName { get; init; }

        /// <summary>Case-insensitive contains match on the creating user. Wire name <c>createdBy</c>.</summary>
        [FromQuery(Name = CreatedByParameter)]
        public string? CreatedBy { get; init; }

        /// <summary>Inclusive lower bound on <c>CreatedDate</c>, applied at the start of the day.
        /// Wire name <c>fromDate</c>.</summary>
        [FromQuery(Name = FromDateParameter)]
        public DateTime? FromDate { get; init; }

        /// <summary>Inclusive upper bound on <c>CreatedDate</c>, applied at the end of the day.
        /// Wire name <c>toDate</c>.</summary>
        [FromQuery(Name = ToDateParameter)]
        public DateTime? ToDate { get; init; }

        /// <inheritdoc />
        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            foreach (var result in base.Validate(validationContext))
                yield return result;

            if (FromDate.HasValue && ToDate.HasValue && FromDate.Value.Date > ToDate.Value.Date)
            {
                yield return new ValidationResult(
                    "fromDate must be on or before toDate.",
                    new[] { FromDateParameter, ToDateParameter });
            }
        }
    }
}
