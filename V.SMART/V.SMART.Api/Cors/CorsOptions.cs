namespace V.SMART.Api.Cors
{
    /// <summary>
    /// M2-A05 — configuration for the SPA's CORS policy, bound from the <c>Cors</c> section of
    /// <c>appsettings.json</c>. Replaces the hardcoded <c>"AngularDev"</c> policy that only
    /// ever allowed <c>http://localhost:4200</c> (ADR-002 §5).
    ///
    /// <para>
    /// <b>Why the origin list is empty in <c>appsettings.json</c> and populated only in
    /// <c>appsettings.Development.json</c>.</b> Q-16 (the reverse-proxy / deployment topology
    /// and TLS termination) is explicitly deferred as of 2026-08-27 — the repository owner,
    /// asked directly, does not yet know the deployment topology. Per this task's own
    /// Prerequisites, an unanswered Q-16 means the task ships the configuration
    /// <i>mechanism</i>, with real origin values left to deployment, rather than inventing
    /// them. An empty production default fails closed (no origin is allowed until one is
    /// configured) instead of failing open with a guessed value.
    /// </para>
    /// </summary>
    public sealed class CorsOptions
    {
        public const string SectionName = "Cors";
        public const string PolicyName = "SpaOrigins";

        /// <summary>
        /// The exact origins (scheme + host + port, no path, no trailing slash) the SPA is
        /// allowed to call this API from. Empty by default — see the class doc comment. An
        /// environment that has not configured this section allows nothing, not everything.
        /// </summary>
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Whether the policy allows credentials (cookies, <c>Authorization</c> headers read
        /// via <c>fetch</c>'s <c>credentials: 'include'</c>) across origins.
        /// <para>
        /// <b>The decision, made explicitly, per this task's own Target Result 6.</b>
        /// <see langword="false"/> — the SPA sends its access and refresh tokens in the request
        /// body/header it controls directly (Q-16's own INV-063 note: the refresh token
        /// travels in the JSON body, deliberately not an <c>HttpOnly</c> cookie, because a
        /// cookie's <c>Secure</c>/<c>SameSite</c>/domain attributes cannot be set correctly
        /// without knowing the deployment topology this same Q-16 defers). Nothing this API
        /// issues today is a cookie, so there is nothing that needs <c>AllowCredentials</c>,
        /// and leaving it off keeps the policy compatible with a future wildcard-style origin
        /// match if one is ever needed (CORS forbids combining credentials with
        /// <c>AllowAnyOrigin</c>, though this policy never uses that either — see below).
        /// </para>
        /// </summary>
        public bool AllowCredentials { get; set; }
    }
}
