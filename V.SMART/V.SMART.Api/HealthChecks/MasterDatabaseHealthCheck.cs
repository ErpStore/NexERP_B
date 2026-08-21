using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using V.SMART.Shared.Data;

namespace V.SMART.Api.HealthChecks
{
    /// <summary>
    /// M2-B11 — readiness check for the master database.
    /// <para>
    /// Uses <see cref="MasterDbContext"/>, which is a plain <c>AddDbContext</c> bound to
    /// <c>ConnectionStrings:MasterDb</c> and therefore needs no tenant, no
    /// <c>HttpContext</c> and no user — unlike the tenant path (see
    /// <see cref="TenantDatabaseHealthCheck"/>).
    /// </para>
    /// <para>
    /// <b>It never returns an exception message or a description carrying one.</b> These
    /// endpoints are anonymous; a <c>SqlException</c> message routinely names the server and
    /// the database. See <see cref="HealthResponseWriter"/>.
    /// </para>
    /// </summary>
    public sealed class MasterDatabaseHealthCheck : IHealthCheck
    {
        /// <summary>The name this check is registered under, and the key in the response body.</summary>
        public const string Name = "master-db";

        private readonly MasterDbContext _masterDb;

        public MasterDatabaseHealthCheck(MasterDbContext masterDb) => _masterDb = masterDb;

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var reachable = await _masterDb.Database.CanConnectAsync(cancellationToken);

                return reachable
                    ? HealthCheckResult.Healthy()
                    : new HealthCheckResult(context.Registration.FailureStatus);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                // Swallowed deliberately, and this is the one place in the repository where
                // that is correct: the caller is an anonymous orchestrator and the exception
                // detail is exactly what must not cross the wire. The exception is not logged
                // here either — a probe that runs every few seconds against a down database
                // would fill the diagnostic sink with identical stack traces. The
                // "Unhealthy" status IS the signal.
                return new HealthCheckResult(context.Registration.FailureStatus);
            }
        }
    }
}
