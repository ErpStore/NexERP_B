using System.Linq.Expressions;
using V.SMART.Shared.Data.SalesAndLabour.Leads;

namespace V.SMART.Api.Authorization
{
    /// <summary>
    /// The list of entities that carry a row scope, and the property each one is scoped by
    /// (M2-A08, KB-108 §5). It is the row-scope counterpart of <see cref="ScreenCatalogue"/>: one
    /// declaration, read by both the query extension and the startup validator, so "is this entity
    /// scoped?" has exactly one answer in the whole API.
    ///
    /// <para><b>Today it holds exactly one entry — <c>Leads</c> — and that is a finding, not an
    /// omission.</b> INV-028 grepped <c>StateCodesCsv</c>/<c>StateCodes</c> across
    /// <c>V.SMART/</c> and found <b>no</b> customer, vendor, item, document or report query that
    /// references it. Row scope has never applied to anything but Leads
    /// (<c>LeadService.cs:128-152</c>). Adding a second entry here starts filtering rows that a
    /// live ERP shows today, which is a product decision with a blast radius, not a tidy-up —
    /// KB-108 decision P1 records it as owner-owned and currently answered <b>No</b>.</para>
    /// </summary>
    /// <remarks>
    /// <b>Why an expression and not an interface.</b> Marking <c>Leads</c> with an
    /// <c>IStateScoped</c> interface would mean editing <c>V.SMART.Shared</c>, which M2-A08 is not
    /// permitted to do beyond one repository query — and which the Blazor host also compiles
    /// against. A selector expression declared here reaches the same property with no change to the
    /// domain library, and it is the form EF Core needs anyway.
    /// </remarks>
    public static class ScopedEntityCatalogue
    {
        /// <summary>
        /// Entity type → the <c>int</c> state property the scope is applied to. Ordinal type
        /// identity; no inheritance walk, because an unregistered subtype must fail loudly rather
        /// than inherit a scope it was never checked against.
        /// </summary>
        private static readonly IReadOnlyDictionary<Type, LambdaExpression> StateSelectors =
            new Dictionary<Type, LambdaExpression>
            {
                // Leads.StateId (V.SMART/V.SMART.Shared/Data/SalesAndLabour/Leads/Leads.cs:46),
                // the property LeadService.cs:141-144 compares against the caller's StateCodesCsv.
                [typeof(Leads)] = (Expression<Func<Leads, int>>)(lead => lead.StateId)
            };

        /// <summary>The scoped entity types, for the startup validator and for tests.</summary>
        public static IReadOnlyCollection<Type> ScopedEntityTypes => (IReadOnlyCollection<Type>)StateSelectors.Keys;

        /// <summary>Whether <paramref name="type"/> is row-scoped.</summary>
        public static bool IsScoped(Type type) => type is not null && StateSelectors.ContainsKey(type);

        /// <summary>
        /// The state selector for <typeparamref name="T"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <typeparamref name="T"/> is not registered. <b>Fail loud, never fail open:</b> returning
        /// "no scope applies" for an unknown type is how a scoped entity quietly becomes unscoped,
        /// which is precisely the Blazor failure mode (claim 7) this mechanism exists to prevent.
        /// </exception>
        public static Expression<Func<T, int>> StateSelector<T>()
        {
            if (!StateSelectors.TryGetValue(typeof(T), out var selector))
            {
                throw new InvalidOperationException(
                    $"{typeof(T).FullName} is not registered in ScopedEntityCatalogue, so a row scope cannot be " +
                    "applied to it. Register it with its state selector, or call the query without ApplyRowScope " +
                    "and declare the endpoint [NoRowScope(\"<justification>\")].");
            }

            return (Expression<Func<T, int>>)selector;
        }
    }
}
