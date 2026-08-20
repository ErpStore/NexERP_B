namespace V.SMART.Api.Authorization
{
    /// <summary>One user's rights on one screen — a detached copy of a <c>UserRight</c> row.</summary>
    public sealed record ScreenRightEntry(
        string ScreenName,
        bool CanView,
        bool CanCreate,
        bool CanEdit,
        bool CanDelete,
        bool IsHide);

    /// <summary>
    /// An immutable, detached projection of the caller's <c>UserRight</c> rows (KB-105 §2.5).
    /// <para>
    /// Never EF-tracked entities: caching a tracked graph whose <c>DbContext</c> is scoped per
    /// request is a lifetime bug — the graph would outlive its context and either throw on lazy
    /// access or hand a later request another request's tracker state. M2-A01-03 caches this type.
    /// </para>
    /// <para>
    /// <see cref="Entries"/> preserves the order the repository returned. That order is
    /// undefined — <c>UserRightsRepository.cs:24-27</c> applies no <c>OrderBy</c> (KB-105 B-7,
    /// F-3) — and it is inherited deliberately, because first-match-wins under duplicates is
    /// exactly what Blazor does today (B-4).
    /// </para>
    /// </summary>
    public sealed class ScreenRightSet
    {
        /// <summary>An empty set. Deny-by-default for every screen (BR-AUTH-002).</summary>
        public static readonly ScreenRightSet Empty = new(Array.Empty<ScreenRightEntry>());

        public ScreenRightSet(IReadOnlyList<ScreenRightEntry> entries)
        {
            Entries = entries ?? throw new ArgumentNullException(nameof(entries));
        }

        public IReadOnlyList<ScreenRightEntry> Entries { get; }

        /// <summary>
        /// Ordinal, first-match-wins. Mirrors
        /// <c>rights.FirstOrDefault(r =&gt; r.Screens.ScreenName == screenName)?.CanX ?? false</c>
        /// (<c>V.SMART/V.SMART.Shared/Shared/RightsHelper.cs:7-20</c>) case for case, including
        /// both the <c>FirstOrDefault</c> and the <c>?? false</c>. C#'s <c>string ==</c> is an
        /// ordinal comparison, so <see cref="StringComparison.Ordinal"/> here is the same
        /// operator spelt out rather than a change of semantics (KB-105 D-1, T-9).
        /// </summary>
        public bool Has(string screenName, Right right)
        {
            var entry = Entries.FirstOrDefault(e => string.Equals(e.ScreenName, screenName, StringComparison.Ordinal));

            return right switch
            {
                Right.View => entry?.CanView ?? false,
                Right.Create => entry?.CanCreate ?? false,
                Right.Edit => entry?.CanEdit ?? false,
                Right.Delete => entry?.CanDelete ?? false,

                // An undefined enum value can only arrive by a cast. Deny-by-default is the only
                // safe answer; it must never fall through to an allow.
                _ => false
            };
        }

        /// <summary>
        /// Number of entries matching <paramref name="screenName"/>. Used only for the KB-105 D-2
        /// ambiguity warning — it never changes the outcome.
        /// </summary>
        public int MatchCount(string screenName)
            => Entries.Count(e => string.Equals(e.ScreenName, screenName, StringComparison.Ordinal));
    }
}
