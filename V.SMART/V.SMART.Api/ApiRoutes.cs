namespace V.SMART.Api
{
    /// <summary>
    /// The single place the API's route version prefix is written (ADR-002 §6, "All routes
    /// under <c>/api/v1</c>"). Every controller composes its route from
    /// <see cref="V1"/> — no controller author writes the version string by hand.
    /// <para>
    /// Usage: <c>[Route($"{ApiRoutes.V1}/currencies")]</c>. The interpolated string is a
    /// compile-time constant because <see cref="V1"/> is <c>const</c>, so it is legal in an
    /// attribute argument.
    /// </para>
    /// <para>
    /// <b>Why a constant and not <c>Asp.Versioning.Mvc</c> (M2-B01).</b> A versioning library
    /// earns its keep when several versions must coexist, with negotiation, deprecation
    /// headers and one OpenAPI document per version. There is exactly one version and ADR-002
    /// asks for no more, so the library would be infrastructure with no consumer and would
    /// complicate the Swagger configuration M2-B10 generates the TypeScript client from. When a
    /// second version is genuinely needed, this is the seam to replace.
    /// </para>
    /// <para>
    /// Resources under the prefix are plural kebab-case (ADR-002 §2), e.g.
    /// <c>api/v1/sales-orders</c>. <c>auth</c> is the one non-collection segment, and ADR-002 §5
    /// writes it that way itself.
    /// </para>
    /// </summary>
    public static class ApiRoutes
    {
        /// <summary>Version 1 route prefix. No leading or trailing slash — MVC adds neither.</summary>
        public const string V1 = "api/v1";
    }
}
