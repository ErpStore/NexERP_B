using Microsoft.AspNetCore.OutputCaching;

namespace V.SMART.Api.Caching
{
    /// <summary>
    /// M2-B09 — the single place the reference-data cache policy is named and configured, so a
    /// controller author never writes the policy name or the TTL by hand. Same reasoning as
    /// <c>ApiRoutes.V1</c> (M2-B01) for the version prefix.
    /// </summary>
    public static class ReferenceCachePolicy
    {
        /// <summary>The policy name referenced by <c>[OutputCache(PolicyName = …)]</c>.</summary>
        public const string PolicyName = "ReferenceData";

        /// <summary>Configuration key for the TTL, in seconds.</summary>
        public const string TtlConfigurationKey = "Caching:ReferenceDataSeconds";

        /// <summary>
        /// The default TTL when configuration is absent: <b>60 seconds</b>.
        ///
        /// <para><b>Why minutes rather than hours.</b> These lists are edited through Blazor
        /// screens that know nothing about this API's cache, so there is no invalidation path —
        /// and this task deliberately does not build one. A phantom invalidation mechanism that
        /// nobody calls is worse than a short TTL, because it reads as though staleness is
        /// handled. 60 seconds bounds the staleness a user can observe after editing a UOM in
        /// the Blazor app to something they will not notice, while still collapsing the burst of
        /// identical requests a page load produces, which is where the entire benefit is.</para>
        ///
        /// <para><b>Known limitation, stated rather than hidden:</b> for up to the TTL, a
        /// reference list edited in Blazor is stale over the API. That is accepted for this data
        /// class and would not be accepted for transactional data — which is why this policy is
        /// scoped to one route group and not applied globally.</para>
        /// </summary>
        public static readonly TimeSpan DefaultTtl = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Reads the TTL from configuration, falling back to <see cref="DefaultTtl"/>. A
        /// non-positive or unparseable value falls back too rather than disabling caching by
        /// accident — except for an explicit <c>0</c>, which is honoured as "do not cache" so
        /// the behaviour can be switched off in an environment without a code change.
        /// </summary>
        public static TimeSpan ResolveTtl(IConfiguration configuration)
        {
            var configured = configuration[TtlConfigurationKey];

            if (string.IsNullOrWhiteSpace(configured) || !int.TryParse(configured, out var seconds))
            {
                return DefaultTtl;
            }

            return seconds < 0 ? DefaultTtl : TimeSpan.FromSeconds(seconds);
        }

        /// <summary>Registers the named policy on the output-cache options.</summary>
        public static void Register(OutputCacheOptions options, IConfiguration configuration)
            => options.AddPolicy(PolicyName, new TenantScopedOutputCachePolicy(ResolveTtl(configuration)));
    }
}
