using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace V.SMART.Api.HealthChecks
{
    /// <summary>
    /// M2-B11 — configuration for <c>GET /health/ready</c>, bound from the <c>Health</c>
    /// section of <c>appsettings.json</c>. Named <c>HealthProbeOptions</c> rather than
    /// <c>HealthCheckOptions</c> so it cannot be confused with the framework's
    /// <see cref="Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions"/>, which
    /// <c>Program.cs</c> also uses.
    /// </summary>
    public sealed class HealthProbeOptions
    {
        public const string SectionName = "Health";

        /// <summary>
        /// How many tenant databases <c>/health/ready</c> probes, lowest <c>TenantInfo.Id</c>
        /// first. Default 1.
        /// <para>
        /// <b>Why not all of them (KB-113 §2).</b> This is a database-per-tenant system; a
        /// readiness probe runs on every orchestrator poll, and probing N tenants turns one
        /// poll into N+1 connections. It does not scale, and it makes one sick tenant able to
        /// take the whole service out of the load balancer. One tenant is enough to prove the
        /// tenant path works at all, which is the question a readiness probe is asking. Raise
        /// it deliberately, knowing the cost.
        /// </para>
        /// </summary>
        public int TenantProbeCount { get; set; } = 1;

        /// <summary>
        /// Optionally pin the probe to specific tenants by <c>TenantInfo.Hostname</c>. When
        /// empty (the default) the lowest <see cref="TenantProbeCount"/> ids are probed, which
        /// is stable across restarts and needs no per-environment configuration.
        /// </summary>
        public string[] TenantHostnames { get; set; } = Array.Empty<string>();

        /// <summary>Connection timeout, in seconds, applied to every probe. Default 5.</summary>
        public int ProbeTimeoutSeconds { get; set; } = 5;

        /// <summary>
        /// Whether an unreachable tenant makes the whole service unready (<c>503</c>) or merely
        /// degraded (<c>200</c>). Default <see langword="true"/> — <c>503</c>.
        /// <para>
        /// <b>The decision, made explicitly (KB-113 §2).</b> With the default probe set of one
        /// tenant, an unreachable tenant is not "one customer is down": it is the only evidence
        /// this instance has that its tenant-resolution and tenant-connection path works at
        /// all, and a request routed here would fail. Ready means "send me traffic", and this
        /// instance cannot honestly say that. An operator running a large probe set, where one
        /// tenant genuinely is one customer, should set this to <see langword="false"/>; the
        /// per-tenant detail in the response body still names the failure either way.
        /// </para>
        /// </summary>
        public bool TenantFailureIsFatal { get; set; } = true;

        /// <summary>
        /// Whether a master database containing no tenant rows is a failure. Default
        /// <see langword="true"/>: a deployment with no tenants can serve nobody, and silently
        /// reporting healthy would hide a failed or half-run provisioning step.
        /// </summary>
        public bool RequireAtLeastOneTenant { get; set; } = true;

        internal HealthStatus TenantFailureStatus =>
            TenantFailureIsFatal ? HealthStatus.Unhealthy : HealthStatus.Degraded;
    }
}
