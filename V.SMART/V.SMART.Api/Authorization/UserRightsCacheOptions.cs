namespace V.SMART.Api.Authorization
{
    /// <summary>
    /// The screen-rights cache settings, resolved once at startup (KB-105 §8.2).
    /// <para>
    /// Read eagerly through <see cref="FromConfiguration"/> in <c>Program.cs</c> rather than through
    /// the options pattern on purpose: <c>ValidateOnBuild</c> is off in this host
    /// (<c>Program.cs</c>, M2-B07), so an <c>IValidateOptions</c> failure would surface on the first
    /// authorized request instead of at startup. KB-105 §8.4 consequence 2 requires a misconfigured
    /// TTL to fail <b>startup</b>, so the check has to run before <c>builder.Build()</c>.
    /// </para>
    /// </summary>
    public sealed class UserRightsCacheOptions
    {
        /// <summary>Configuration key holding the TTL in seconds (KB-105 §8.2).</summary>
        public const string TtlSecondsConfigurationKey = "Authorization:RightsCacheSeconds";

        /// <summary>ADR-004 §2's "≈60 s".</summary>
        public const int DefaultTtlSeconds = 60;

        /// <summary>
        /// KB-105 §8.4 consequence 2. Every <c>UserRight</c> write site today runs in the Blazor
        /// host, a different process, so this TTL is the <b>only</b> bound on how long a revoked
        /// right keeps working. Raising it past five minutes is a security regression, not a
        /// tuning decision, so it fails startup instead of being silently accepted.
        /// </summary>
        public const int MaxTtlSeconds = 300;

        public UserRightsCacheOptions(int ttlSeconds)
        {
            if (ttlSeconds < 0)
            {
                throw new InvalidOperationException(
                    $"Configuration '{TtlSecondsConfigurationKey}' is {ttlSeconds}. It must be between 0 and " +
                    $"{MaxTtlSeconds} seconds. Use 0 to disable the screen-rights cache entirely.");
            }

            if (ttlSeconds > MaxTtlSeconds)
            {
                throw new InvalidOperationException(
                    $"Configuration '{TtlSecondsConfigurationKey}' is {ttlSeconds} seconds, above the maximum of " +
                    $"{MaxTtlSeconds}. The cache is in-process and every UserRight write happens in the Blazor " +
                    $"host, so this TTL is the only bound on how long a revoked screen right keeps working " +
                    $"(KB-105 §8.4). Lower it, or close the gap with a distributed cache first (Q-29).");
            }

            TtlSeconds = ttlSeconds;
        }

        /// <summary>Configured TTL in seconds. <c>0</c> disables caching.</summary>
        public int TtlSeconds { get; }

        /// <summary>
        /// <c>false</c> when the TTL is 0 — every call then goes to the database. This is the
        /// documented bypass for M2-A03's permission-matrix harness, which flips rights between
        /// cases and would otherwise read a warm entry and pass falsely.
        /// </summary>
        public bool IsEnabled => TtlSeconds > 0;

        /// <summary>Absolute lifetime of one cache entry. Never sliding — KB-105 §8.2.</summary>
        public TimeSpan Ttl => TimeSpan.FromSeconds(TtlSeconds);

        /// <summary>
        /// Binds and validates the TTL. A missing or blank value takes
        /// <see cref="DefaultTtlSeconds"/>; a non-numeric, negative or over-large value throws
        /// <see cref="InvalidOperationException"/> naming the key.
        /// </summary>
        public static UserRightsCacheOptions FromConfiguration(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            var raw = configuration[TtlSecondsConfigurationKey];

            if (string.IsNullOrWhiteSpace(raw))
            {
                return new UserRightsCacheOptions(DefaultTtlSeconds);
            }

            if (!int.TryParse(raw.Trim(), System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture, out var seconds))
            {
                throw new InvalidOperationException(
                    $"Configuration '{TtlSecondsConfigurationKey}' must be a whole number of seconds between 0 " +
                    $"and {MaxTtlSeconds}. It could not be parsed as an integer.");
            }

            return new UserRightsCacheOptions(seconds);
        }
    }
}
