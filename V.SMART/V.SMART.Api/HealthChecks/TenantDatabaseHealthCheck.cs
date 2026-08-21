using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using V.SMART.Shared.Data;

namespace V.SMART.Api.HealthChecks
{
    /// <summary>
    /// M2-B11 — readiness check for tenant databases, in a database-per-tenant system where a
    /// healthy master proves nothing about a tenant.
    /// <para>
    /// <b>It deliberately does not use <c>ITenantProvider</c> or
    /// <c>ITenantDbContextFactory</c>, and that is not a shortcut.</b>
    /// <c>TenantProvider.GetCurrentTenant()</c> resolves the tenant from
    /// <c>_httpContextAccessor?.HttpContext?.User?.FindFirst("TenantId")</c>
    /// (<c>V.SMART/V.SMART.Shared/Services/MultiCompanyService/TenantProvider.cs:33-34</c>),
    /// falling back to the request host, and <c>TenantDbContextFactory</c> depends on it
    /// (<c>TenantDbContextFactory.cs:7-12</c>). <b>A health probe has no HttpContext, no JWT
    /// and no user</b>, so that path cannot resolve anything. The only correct shape is to
    /// read <c>MasterDbContext.Tenants</c> (<c>MasterDbContext.cs:8</c>) and open the
    /// connection directly.
    /// </para>
    /// <para>
    /// <b>Disclosure.</b> Each probed tenant is reported as <c>tenant-{Id}</c> — an opaque
    /// integer, enough to tell an operator which tenant to look at, and deliberately not the
    /// <c>Name</c> (a customer name) or the <c>Hostname</c>. Nothing derived from the
    /// connection string, and no exception text, ever leaves this class.
    /// </para>
    /// </summary>
    public sealed class TenantDatabaseHealthCheck : IHealthCheck
    {
        /// <summary>The name this check is registered under, and the key in the response body.</summary>
        public const string Name = "tenant-db";

        private readonly MasterDbContext _masterDb;
        private readonly HealthProbeOptions _options;

        public TenantDatabaseHealthCheck(MasterDbContext masterDb, IOptions<HealthProbeOptions> options)
        {
            _masterDb = masterDb;
            _options = options.Value;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            List<TenantInfo> tenants;

            try
            {
                tenants = await SelectTenantsAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                // The master database is unreachable. MasterDatabaseHealthCheck already
                // reports that as its own entry; this one reports only that it could not
                // enumerate tenants, without duplicating the diagnosis or the detail.
                return new HealthCheckResult(
                    context.Registration.FailureStatus,
                    data: Detail(new Dictionary<string, object> { ["tenant-source"] = "unavailable" }));
            }

            if (tenants.Count == 0)
            {
                var noTenants = Detail(new Dictionary<string, object> { ["tenants-probed"] = 0 });

                return _options.RequireAtLeastOneTenant
                    ? new HealthCheckResult(context.Registration.FailureStatus, data: noTenants)
                    : HealthCheckResult.Healthy(data: noTenants);
            }

            var results = new Dictionary<string, object>();
            var anyFailed = false;

            foreach (var tenant in tenants)
            {
                var ok = await CanConnectAsync(tenant.ConnectionString, cancellationToken);
                results[$"tenant-{tenant.Id}"] = ok ? "Healthy" : "Unhealthy";
                anyFailed |= !ok;
            }

            return anyFailed
                ? new HealthCheckResult(_options.TenantFailureStatus, data: Detail(results))
                : HealthCheckResult.Healthy(data: Detail(results));
        }

        private async Task<List<TenantInfo>> SelectTenantsAsync(CancellationToken cancellationToken)
        {
            var query = _masterDb.Tenants.AsNoTracking();

            if (_options.TenantHostnames.Length > 0)
            {
                var hostnames = _options.TenantHostnames;
                query = query.Where(t => hostnames.Contains(t.Hostname));
            }

            var take = Math.Max(1, _options.TenantProbeCount);

            return await query.OrderBy(t => t.Id).Take(take).ToListAsync(cancellationToken);
        }

        private async Task<bool> CanConnectAsync(string connectionString, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return false;
            }

            try
            {
                // The stored connection string is used as configured, except that the connect
                // timeout is clamped: a probe must fail fast, and an unbounded 15-second ADO.NET
                // default would let one unreachable tenant hold the orchestrator's poll open
                // past its own timeout and be recorded as a probe failure of the wrong kind.
                var builder = new SqlConnectionStringBuilder(connectionString)
                {
                    ConnectTimeout = Math.Max(1, _options.ProbeTimeoutSeconds)
                };

                await using var connection = new SqlConnection(builder.ConnectionString);
                await connection.OpenAsync(cancellationToken);

                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1";
                command.CommandTimeout = Math.Max(1, _options.ProbeTimeoutSeconds);
                await command.ExecuteScalarAsync(cancellationToken);

                return true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                // Same reasoning as MasterDatabaseHealthCheck: a SqlException message names the
                // server and the database, and this response is anonymous. The boolean is the
                // whole answer that may be published.
                return false;
            }
        }

        private static IReadOnlyDictionary<string, object> Detail(Dictionary<string, object> data) => data;
    }
}
