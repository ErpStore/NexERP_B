using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace V.SMART.Api.Contracts
{
    /// <summary>
    /// The base of every list-endpoint query DTO (M2-B02, ADR-002 §2).
    ///
    /// <para>ADR-002 §2 requires <b>typed query DTOs, not <c>Dictionary&lt;string, object&gt;</c></b>
    /// at the controller boundary. The dictionary the business services take survives only inside
    /// <see cref="FilterDictionaryAdapter"/>.</para>
    ///
    /// <para><b>Validation is declarative on purpose.</b> The <c>[Range]</c> attributes and the
    /// <see cref="IValidatableObject"/> hook below run during model binding, so M2-A06's
    /// <c>InvalidModelStateResponseFactory</c> (<c>ErrorContractExtensions.cs:21-25</c>) turns a
    /// violation into the agreed 400 <c>application/problem+json</c> with an <c>errors</c>
    /// dictionary before the action ever runs. No controller writes that code.</para>
    ///
    /// <para>The <c>[DefaultValue]</c> attributes exist so Swashbuckle emits the defaults into
    /// <c>/swagger/v1/swagger.json</c>: a C# property initialiser is invisible to the schema
    /// generator, and M2-B10's generated TypeScript client reads the defaults from the
    /// document, not from this file.</para>
    ///
    /// <para><b>Every property carries an explicit <c>[FromQuery(Name = …)]</c> wire name.</b>
    /// Without one, the binder and Swashbuckle both take the <em>C# property name</em>, so the
    /// OpenAPI document would advertise <c>PageNumber</c>/<c>PageSize</c>/<c>Sort</c> while
    /// ADR-002 §2a, KB-040 and the JSON response body all use camel case — and M2-B10 generates
    /// its TypeScript client from that document. The names below are the contract; the C#
    /// property names are an implementation detail. Binding stays case-insensitive, so callers
    /// sending either casing continue to work.</para>
    /// </summary>
    public abstract record PagedQuery : IValidatableObject
    {
        /// <summary>The wire name of <see cref="PageNumber"/> — the query parameter and the
        /// <c>errors</c> dictionary key.</summary>
        public const string PageNumberParameter = "pageNumber";

        /// <summary>The wire name of <see cref="PageSize"/>.</summary>
        public const string PageSizeParameter = "pageSize";

        /// <summary>The wire name of <see cref="Sort"/>.</summary>
        public const string SortParameter = "sort";

        /// <summary>The documented default page size, in effect when the caller omits <c>pageSize</c>.</summary>
        public const int DefaultPageSize = 20;

        /// <summary>
        /// The documented maximum page size. An unbounded <c>pageSize</c> is a denial-of-service
        /// vector: the tenant <c>ApplicationDbContext</c> allows a 60-second command timeout
        /// (<c>TenantDbContextFactory.cs:22</c>) and the response is materialised into memory and
        /// AutoMapper-projected per row. 100 covers every page size the live Blazor list offers
        /// (10 / 20 / 50 — <c>CurrencyList.razor:85-87</c>) with headroom.
        /// </summary>
        public const int MaxPageSize = 100;

        /// <summary>1-based page index. Default 1. Wire name <c>pageNumber</c>.</summary>
        [FromQuery(Name = PageNumberParameter)]
        [Range(1, int.MaxValue, ErrorMessage = "pageNumber must be 1 or greater.")]
        [DefaultValue(1)]
        public int PageNumber { get; init; } = 1;

        /// <summary>Rows per page. Default 20, maximum 100. Wire name <c>pageSize</c>.</summary>
        [FromQuery(Name = PageSizeParameter)]
        [Range(1, MaxPageSize, ErrorMessage = "pageSize must be between 1 and 100.")]
        [DefaultValue(DefaultPageSize)]
        public int PageSize { get; init; } = DefaultPageSize;

        /// <summary>
        /// Comma-separated sort fields, <c>-</c> prefix for descending — e.g.
        /// <c>-createdDate,currName</c>. Omit it to keep the resource's existing default ordering.
        /// Validated against <see cref="SortableFields"/>; an unknown field is a 400 that lists
        /// the permitted values. Wire name <c>sort</c>.
        /// </summary>
        [FromQuery(Name = SortParameter)]
        public string? Sort { get; init; }

        /// <summary>
        /// The per-resource allow-list of sortable wire field names. Non-public, so neither the
        /// model binder nor the OpenAPI schema generator treats it as a query parameter.
        /// </summary>
        protected abstract IReadOnlyList<string> SortableFields { get; }

        /// <summary>
        /// Cross-property validation. Overriders must call <c>base.Validate</c> so the sort
        /// allow-list stays enforced.
        ///
        /// <para>Member names are the <b>wire</b> names, not <c>nameof</c> of the C# property:
        /// an <see cref="IValidatableObject"/> member name becomes the <c>errors</c> dictionary
        /// key verbatim, and a binding failure on the same field is keyed by its
        /// <c>[FromQuery(Name = …)]</c>. Using <c>nameof</c> here would key one field two ways
        /// depending on which check rejected it.</para>
        /// </summary>
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!SortSpecification.TryParse(Sort, SortableFields, out _, out var error))
                yield return new ValidationResult(error, new[] { SortParameter });
        }

        /// <summary>
        /// The validated, canonical <c>sort</c> string to hand to the business service, or
        /// <c>null</c> for "keep the service's default ordering".
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The value is invalid. Unreachable through the API — model validation rejects it with a
        /// 400 first — so reaching this is a wiring defect and fails loudly rather than sorting
        /// nothing.
        /// </exception>
        public string? ToServiceSort()
        {
            if (!SortSpecification.TryParse(Sort, SortableFields, out var terms, out var error))
                throw new InvalidOperationException(error);

            return SortSpecification.ToServiceSort(terms);
        }
    }
}
