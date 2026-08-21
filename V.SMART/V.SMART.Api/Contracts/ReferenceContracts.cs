namespace V.SMART.Api.Contracts
{
    /// <summary>
    /// M2-B09 — the flat wire shapes for <c>/api/v1/reference</c>.
    ///
    /// <para><b>Why DTOs rather than the entities (ADR-002 §2 deviation, documented).</b>
    /// ADR-002 §2 asks controllers to return the domain's own view models. These six lists are
    /// the exception, for two reasons that do not apply elsewhere:</para>
    /// <list type="number">
    ///   <item><description><b>Navigation properties would serialise a graph.</b>
    ///   <c>Currency.CurrencyRates</c> is an <c>ICollection&lt;CurrencyToday&gt;</c> and
    ///   <c>Screens.UserRights</c> is an <c>ICollection&lt;UserRight&gt;</c> — the latter is
    ///   every screen right of every user in the tenant. Returning the entity from a cached,
    ///   authenticated endpoint would leak the tenant's whole permission matrix through a
    ///   dropdown feed. That is not a theoretical objection; it is one <c>Include</c> away at
    ///   any time.</description></item>
    ///   <item><description><b>Audit columns are not reference data.</b> <c>CreatedBy</c>,
    ///   <c>ModifiedDate</c> and friends are storage bookkeeping. A dropdown needs a code and a
    ///   label.</description></item>
    /// </list>
    /// <para>Every record here is flat by construction: only primitives, no collections, no
    /// entity types. That is the property the tests assert, not the specific field list.</para>
    /// </summary>
    /// <remarks>
    /// These are deliberately <b>not</b> shaped for any one screen. They are the smallest
    /// faithful projection of the underlying row, so that a future consumer never has to go
    /// back to the entity for a field this layer dropped for convenience.
    /// </remarks>
    public static class ReferenceContractsDoc
    {
        // Documentation anchor only — the records below carry the behaviour.
    }

    /// <summary>
    /// The two GST ladders, paired by index: <c>CgstSgst[i]</c> is exactly half of
    /// <c>Igst[i]</c>, because CGST and SGST each carry half the integrated rate.
    /// </summary>
    /// <remarks>
    /// Read straight from <c>CommonConstants.IGSTRates</c>/<c>GSTRates</c> and never retyped —
    /// a literal here would drift from the domain the first time a rate changes, and the
    /// drift would be silent. The paired shape is exposed rather than left for the client to
    /// infer, so no consumer recomputes <c>igst / 2</c> in TypeScript.
    /// </remarks>
    public sealed record GstRatesResponse(
        IReadOnlyList<decimal> Igst,
        IReadOnlyList<decimal> CgstSgst);

    /// <summary>A state. <c>StateCode</c> is the key the documents store.</summary>
    public sealed record StateDto(int StateCode, string StateName, bool IsSystemDefined);

    /// <summary>
    /// A currency. Excludes <c>CurrencyRates</c> — the daily rate feed is a different concern
    /// with a different lifetime, and it is what makes the entity unsafe to cache.
    /// </summary>
    public sealed record CurrencyDto(int CurrId, string? CurrName, string? CurrSub, string? Symbol, bool IsSystemDefined);

    /// <summary>A unit of measure. <c>UnitCode</c> is the key — this table has no integer id.</summary>
    public sealed record UomDto(string UnitCode, string? UnitDescription, bool IsSystemDefined);

    /// <summary>
    /// An active terms-and-conditions entry. Only active rows are returned; the endpoint is
    /// backed by <c>ICommonService.GetAllActiveTermsAsync()</c>, so the filter is the domain's,
    /// not this layer's.
    /// </summary>
    public sealed record TermsDto(int Id, string? Title, string? Details);

    /// <summary>
    /// A screen in the permission catalogue. Excludes <c>UserRights</c> — see the type-level
    /// remarks; that navigation is the tenant's entire permission matrix.
    /// </summary>
    /// <remarks>
    /// This is the permission <i>vocabulary</i>, identical for every user in the tenant. It is
    /// <b>not</b> the caller's own rights — those come from <c>GET /api/v1/me</c> (M2-A07).
    /// Keeping the two apart is why this endpoint can be cached per tenant rather than per user.
    /// </remarks>
    public sealed record ScreenDto(int Id, int ScreenCode, string ScreenName, bool IsPrintRequired);
}
