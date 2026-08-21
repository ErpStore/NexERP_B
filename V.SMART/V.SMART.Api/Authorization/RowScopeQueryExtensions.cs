using System.Linq.Expressions;

namespace V.SMART.Api.Authorization
{
    /// <summary>
    /// Applies a <see cref="RowScope"/> <b>to the query</b> (M2-A08, KB-108 §5).
    ///
    /// <para><b>This is the whole point of the mechanism.</b> The Blazor design put the choice at
    /// the call site — <c>ILeadService</c> declares both <c>GetAllLoadLeadsAsync()</c> (scoped,
    /// <c>ILeadService.cs:28</c>) and <c>GetAllLeadsAsync()</c> (unscoped,
    /// <c>ILeadService.cs:24</c>), and the unscoped sibling has <b>four</b> call sites against the
    /// scoped one's single call site (<c>LeadsList.razor:398</c>, <c>:476</c>;
    /// <c>LeadsUpsert.razor:1275</c>, <c>:1287</c>). Nothing in the type system stops a caller
    /// picking the wrong one. Here the predicate is composed into the <see cref="IQueryable{T}"/>
    /// before it is executed, so the database never returns an out-of-scope row and no amount of
    /// paging, sorting or filtering downstream can reach one.</para>
    ///
    /// <para><b>SQL-side, never in memory.</b> <c>LeadService.cs:141-144</c> materialises every lead
    /// and discards in <c>.Where(...)</c>. The task forbids carrying that into the API, so the
    /// predicate built here is <c>codes.Contains(entity.StateId)</c> — the form EF Core renders as
    /// <c>WHERE StateId IN (...)</c>. Of the two CSV predicates in the codebase, this is the SQL-side
    /// family (<c>LeadService.GetUsersBySelectedStateId</c>, <c>LeadService.cs:35-47</c>, is the
    /// other direction: which users cover a state). Exactly one is adopted, per claim 11.</para>
    /// </summary>
    public static class RowScopeQueryExtensions
    {
        /// <summary>
        /// Returns <paramref name="source"/> restricted to <paramref name="scope"/>.
        ///
        /// <list type="bullet">
        /// <item><see cref="RowScope.IsUnrestricted"/> — returned unchanged (the
        /// <c>UserId == 1</c> carve-out of <c>LeadsList.razor:470-484</c>).</item>
        /// <item>An <b>empty</b> scope yields a predicate no row satisfies, so the caller gets zero
        /// rows and a <c>200</c> — never everything, never an error. That is
        /// <c>LeadService.cs:136-144</c>'s fail-closed behaviour, preserved (KB-108 P2).</item>
        /// </list>
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <typeparamref name="T"/> is not in <see cref="ScopedEntityCatalogue"/>.
        /// </exception>
        public static IQueryable<T> ApplyRowScope<T>(this IQueryable<T> source, RowScope scope)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(scope);

            // Resolved BEFORE the unrestricted short-circuit on purpose: an unregistered entity type
            // is a programming error, and it must be reported whoever the caller happens to be. If
            // it were resolved after, the bug would hide from user 1 and surface only in production
            // for everybody else.
            var selector = ScopedEntityCatalogue.StateSelector<T>();

            if (scope.IsUnrestricted)
            {
                return source;
            }

            // A List<int> rather than the IReadOnlyList, because EF Core's IN translation is written
            // against the concrete collection Contains overloads.
            var codes = scope.StateCodes.ToList();

            var parameter = selector.Parameters[0];

            var predicate = Expression.Lambda<Func<T, bool>>(
                Expression.Call(
                    typeof(Enumerable),
                    nameof(Enumerable.Contains),
                    new[] { typeof(int) },
                    Expression.Constant(codes),
                    selector.Body),
                parameter);

            return source.Where(predicate);
        }
    }
}
