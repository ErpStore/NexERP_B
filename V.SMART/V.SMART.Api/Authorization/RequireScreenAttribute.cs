namespace V.SMART.Api.Authorization
{
    /// <summary>
    /// Declares the screen a controller belongs to (KB-105 §2.2). The value is matched against
    /// <c>Screens.ScreenName</c> <b>ordinally and case-sensitively</b>, in memory, exactly as
    /// <c>RightsHelper.cs:8</c> does after <c>UserRightsRepository.cs:27</c> has materialised the
    /// rows (KB-105 D-1).
    /// </summary>
    /// <remarks>
    /// <c>AllowMultiple = false</c>: a controller declares exactly one screen. A controller that
    /// needs two screens is two controllers.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class RequireScreenAttribute : Attribute
    {
        public RequireScreenAttribute(string screenName)
        {
            ScreenName = screenName ?? throw new ArgumentNullException(nameof(screenName));
        }

        /// <summary>The seeded <c>Screens.ScreenName</c>, byte for byte — including the seed's own
        /// typos (<c>"Sub-Contrect GRN"</c>, KB-105 F-8).</summary>
        public string ScreenName { get; }

        /// <summary>
        /// Set to <c>false</c> only for a screen that is deliberately absent from the seeded
        /// catalogue. Requires written justification at review — see KB-105 D-6. When
        /// <c>false</c>, <see cref="ScreenRightStartupValidator"/> skips the catalogue check for
        /// this controller; it never affects the runtime decision, which always comes from the
        /// tenant database.
        /// </summary>
        public bool Seeded { get; init; } = true;
    }
}
