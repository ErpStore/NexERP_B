namespace V.SMART.Api.Contracts
{
    /// <summary>
    /// The single paged-list response body for every list endpoint in this API (M2-B02).
    ///
    /// <para>
    /// The property names are ADR-002 §2's <c>{ items, totalCount, pageNumber, pageSize }</c>
    /// verbatim; with ASP.NET Core's default camel-case JSON policy they serialise to exactly
    /// those four names. This is <b>one generic type</b>, deliberately replacing the
    /// controller-local paged-response record pattern (the one `CurrencyController` used to declare): 60–80 structurally identical
    /// response records would become 60–80 distinct interfaces in the generated TypeScript
    /// client (M2-B10).
    /// </para>
    ///
    /// <para><b>Why there is no <c>totalPages</c>.</b> ADR-002 §2 names four properties and the
    /// generated client is frozen at M2-B03, so adding a fifth here is a contract decision, not a
    /// convenience. It is derivable on the client from <c>totalCount</c> and <c>pageSize</c>.
    /// Note that an unrelated, unused <c>V.SMART.Shared.ViewModels.PagedResult&lt;T&gt;</c>
    /// (<c>RejectionMasterVM.cs:33-40</c>) does carry <c>TotalPages</c> — it has zero references
    /// and is not this type. Do not confuse the two; if a file needs both namespaces it must
    /// alias one.
    /// </para>
    ///
    /// <para>
    /// <c>TotalCount</c> is the count of the <b>filtered, unpaged</b> query — see
    /// <c>CurrencyService.cs:76</c>, where <c>CountAsync()</c> runs after the filters and before
    /// <c>Skip</c>/<c>Take</c>. Every service in <c>BusinessLayer/</c> follows that convention
    /// (INV-002); a list endpoint that reports anything else is a defect.
    /// </para>
    /// </summary>
    public sealed record PagedResult<T>(
        IReadOnlyList<T> Items,
        int TotalCount,
        int PageNumber,
        int PageSize);
}
