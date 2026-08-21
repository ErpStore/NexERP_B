using System.Globalization;

namespace V.SMART.Api.Authorization
{
    /// <summary>
    /// One caller's <b>row scope</b> — the second authorization axis, beside the screen right
    /// (M2-A08, KB-108). A screen right answers "may this user open the Leads screen"; a row scope
    /// answers "which Leads rows may this user see". ADR-004 covers only the first.
    ///
    /// <para><b>What this type is a port of.</b>
    /// <c>V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/LeadService/LeadService.cs:128-152</c>
    /// (<c>GetAllLoadLeadsAsync</c>) is the <i>only</i> row filter in the codebase: it splits the
    /// current user's <c>User.StateCodesCsv</c> and keeps leads whose <c>StateId</c> is in that set.
    /// This type carries the same rule, and in particular the same <b>fail-closed</b> outcome — a
    /// user with a null or blank <c>StateCodesCsv</c> gets an <see cref="Empty"/> scope and
    /// therefore sees <b>zero</b> rows, never all of them (<c>LeadService.cs:136-144</c>, KB-108
    /// decision P2).</para>
    ///
    /// <para><b>Immutable and detached.</b> Like <see cref="ScreenRightSet"/>, an instance may be
    /// cached beyond the request that produced it, so it holds no EF-tracked entity and no
    /// <c>DbContext</c>.</para>
    /// </summary>
    public sealed class RowScope
    {
        /// <summary>
        /// No state codes at all — the fail-closed default. Every scoped query returns zero rows.
        /// Ported from <c>LeadService.cs:136-144</c>: a blank <c>StateCodesCsv</c> produces an
        /// empty token list, and <c>userStatecode.Contains(...)</c> is then false for every lead.
        /// </summary>
        public static readonly RowScope Empty = new(Array.Empty<int>(), isUnrestricted: false);

        /// <summary>
        /// Every row, unfiltered.
        /// <para>
        /// <b>This exists for exactly one reason and it is a ported carve-out, not a convenience.</b>
        /// <c>V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/Leads_Pages/LeadsList.razor:470-484</c>
        /// branches on <c>UserId == 1</c> and calls the <i>unscoped</i> <c>GetAllLeadsAsync()</c> for
        /// that user. User 1 sees every lead today. Dropping the carve-out on the port would take
        /// data away from that account on the day the API ships (KB-108 decision P6).
        /// </para>
        /// <para>
        /// It is resolved <b>once</b>, inside <see cref="RowScopeProvider"/>, from the caller's
        /// <c>UserId</c> claim — never chosen per call site. That is the difference from the Blazor
        /// design, where the choice of scoped or unscoped sibling method lived at each call site
        /// (KB-108 §5, claim 7).
        /// </para>
        /// </summary>
        public static readonly RowScope Unrestricted = new(Array.Empty<int>(), isUnrestricted: true);

        /// <summary>
        /// The user id that is exempt from row scope, from
        /// <c>LeadsList.razor:470</c> (<c>if (Userid==1)</c>). Not a role and not a claim — a literal
        /// primary key, exactly as the Blazor page has it. The same literal gates the trial and
        /// device checks (<c>Login.razor:271</c>, <c>:277</c> — <c>user.UserId &gt; 1</c>).
        /// </summary>
        public const int UnscopedUserId = 1;

        /// <summary>
        /// The title returned when a caller addresses a single row outside their scope
        /// (KB-108 decision P8: <b>404, not 403</b> — a 403 would confirm the row exists).
        /// A genuinely missing row must return this same string, or the two become distinguishable
        /// and the 404 leaks exactly what it was chosen to hide.
        /// </summary>
        public const string OutOfScopeNotFoundTitle = "The requested resource was not found.";

        private readonly HashSet<int> _stateCodes;

        private RowScope(IEnumerable<int> stateCodes, bool isUnrestricted)
        {
            _stateCodes = new HashSet<int>(stateCodes);
            IsUnrestricted = isUnrestricted;
        }

        /// <summary>True when no scope predicate applies — see <see cref="Unrestricted"/>.</summary>
        public bool IsUnrestricted { get; }

        /// <summary>
        /// The state codes this caller may see, ascending. Empty both for <see cref="Empty"/> (sees
        /// nothing) and for <see cref="Unrestricted"/> (sees everything) — read
        /// <see cref="IsUnrestricted"/> to tell them apart, never <c>Count == 0</c>.
        /// </summary>
        public IReadOnlyList<int> StateCodes => _stateCodes.Order().ToList();

        /// <summary>
        /// Parses a <c>User.StateCodesCsv</c> value into a scope.
        ///
        /// <para><b>Divergence, deliberate and recorded (KB-108 §6).</b> The Blazor filter compares
        /// the CSV <i>tokens as strings</i> to <c>l.StateId.ToString()</c>
        /// (<c>LeadService.cs:136-144</c>); this parses them to <see cref="int"/>, because only an
        /// integer set translates to a SQL <c>IN</c> and the task forbids carrying the in-memory
        /// filter into the API. The two agree for every well-formed value. They differ only for a
        /// zero-padded token — <c>"07"</c> matches <c>StateId 7</c> here and does not in Blazor.</para>
        ///
        /// <para>A token that is not an integer is <b>dropped</b>, not thrown on. That matches what
        /// the Blazor filter does with it (a non-numeric token simply never equals any
        /// <c>StateId.ToString()</c>) and diverges from <c>User.StateCodes</c>
        /// (<c>User.cs:53-64</c>), whose <c>int.Parse</c> would throw. An unparseable column must
        /// not turn a login into a 500.</para>
        /// </summary>
        public static RowScope FromStateCodesCsv(string? stateCodesCsv)
        {
            if (string.IsNullOrWhiteSpace(stateCodesCsv))
            {
                // LeadService.cs:136-139 — "?? new List<string>()". Fail closed.
                return Empty;
            }

            var codes = new List<int>();

            // Split and Trim mirror LeadService.cs:136-139 exactly, including
            // StringSplitOptions.RemoveEmptyEntries, so "1,,2" and "1, 2" behave as they do today.
            foreach (var token in stateCodesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(token.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var code))
                {
                    codes.Add(code);
                }
            }

            return codes.Count == 0 ? Empty : new RowScope(codes, isUnrestricted: false);
        }

        /// <summary>
        /// Builds a scope from explicit codes. For tests and for a future non-CSV scope source;
        /// production resolution goes through <see cref="RowScopeProvider"/>.
        /// </summary>
        public static RowScope ForStateCodes(params int[] stateCodes)
            => stateCodes is null || stateCodes.Length == 0
                ? Empty
                : new RowScope(stateCodes, isUnrestricted: false);

        /// <summary>
        /// Whether one already-materialised row is in scope. Used for the single-row (by id) path,
        /// where there is no <see cref="IQueryable{T}"/> left to filter. The collection path must use
        /// <see cref="RowScopeQueryExtensions.ApplyRowScope{T}"/> instead, so the database does the
        /// filtering.
        /// </summary>
        public bool Allows(int stateId) => IsUnrestricted || _stateCodes.Contains(stateId);
    }
}
