using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using V.SMART.Api.HealthChecks;
using V.SMART.Shared.Data;
using Xunit;

namespace V.SMART.Api.Tests.HealthChecks
{
    /// <summary>
    /// M2-B11 — the tenant readiness check.
    /// <para>
    /// The master database is an EF in-memory store here, which is enough because the only
    /// thing this check asks of it is <c>Tenants</c>. The tenant probe itself is real ADO.NET
    /// against a connection string pointed at a port nothing listens on — that is the
    /// "deliberately broken tenant connection string" case the task requires be exercised, and
    /// it is the path that would leak if the check ever surfaced its exception.
    /// </para>
    /// <para>
    /// Note what is <b>not</b> mocked and could not be: <c>ITenantProvider</c> is not
    /// involved at all. It resolves the tenant from <c>HttpContext.User</c>
    /// (<c>TenantProvider.cs:33-34</c>) and a probe has no user, which is precisely why this
    /// check reads <c>MasterDbContext.Tenants</c> and connects directly.
    /// </para>
    /// </summary>
    public class TenantDatabaseHealthCheckTests
    {
        // Port 1 on the loopback interface. Nothing listens there, so the connection is
        // refused immediately rather than waiting out a timeout.
        private const string UnreachableConnectionString =
            "Server=127.0.0.1,1;Database=NexGenErpDb_Tenant7;User Id=sa;Password=Sup3rS3cret!;TrustServerCertificate=True";

        private static MasterDbContext MasterWith(params TenantInfo[] tenants)
        {
            var options = new DbContextOptionsBuilder<MasterDbContext>()
                .UseInMemoryDatabase($"master-{Guid.NewGuid()}")
                .Options;

            var context = new MasterDbContext(options);
            context.Tenants.AddRange(tenants);
            context.SaveChanges();

            return context;
        }

        private static HealthCheckContext ContextFor(IHealthCheck check) => new()
        {
            Registration = new HealthCheckRegistration(
                TenantDatabaseHealthCheck.Name,
                _ => check,
                HealthStatus.Unhealthy,
                new[] { "ready" })
        };

        private static TenantDatabaseHealthCheck Build(MasterDbContext master, HealthProbeOptions options) =>
            new(master, Options.Create(options));

        [Fact]
        public async Task An_unreachable_tenant_database_is_reported_unhealthy_and_named_by_id()
        {
            using var master = MasterWith(new TenantInfo
            {
                Id = 7,
                Name = "Contoso Manufacturing",
                Hostname = "contoso.example.com",
                ConnectionString = UnreachableConnectionString
            });

            var check = Build(master, new HealthProbeOptions { ProbeTimeoutSeconds = 1 });
            var result = await check.CheckHealthAsync(ContextFor(check));

            Assert.Equal(HealthStatus.Unhealthy, result.Status);
            Assert.Equal("Unhealthy", result.Data["tenant-7"]);
        }

        [Fact]
        public async Task A_failed_tenant_probe_discloses_nothing_about_the_connection()
        {
            using var master = MasterWith(new TenantInfo
            {
                Id = 7,
                Name = "Contoso Manufacturing",
                Hostname = "contoso.example.com",
                ConnectionString = UnreachableConnectionString
            });

            var check = Build(master, new HealthProbeOptions { ProbeTimeoutSeconds = 1 });
            var result = await check.CheckHealthAsync(ContextFor(check));

            // Everything the check could possibly publish, flattened.
            var published = string.Join(
                "|",
                result.Description,
                result.Exception?.ToString(),
                string.Join("|", result.Data.Select(pair => pair.Key + "=" + pair.Value)));

            Assert.DoesNotContain("Password", published, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Sup3rS3cret", published, StringComparison.Ordinal);
            Assert.DoesNotContain("NexGenErpDb_Tenant7", published, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("127.0.0.1", published, StringComparison.Ordinal);
            // Not the customer name and not the hostname either — only the opaque id.
            Assert.DoesNotContain("Contoso", published, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("contoso.example.com", published, StringComparison.OrdinalIgnoreCase);
            Assert.Null(result.Exception);
        }

        [Fact]
        public async Task A_failed_tenant_can_be_configured_to_degrade_rather_than_fail_the_service()
        {
            using var master = MasterWith(new TenantInfo
            {
                Id = 7,
                Hostname = "contoso.example.com",
                Name = "Contoso",
                ConnectionString = UnreachableConnectionString
            });

            var check = Build(master, new HealthProbeOptions
            {
                ProbeTimeoutSeconds = 1,
                TenantFailureIsFatal = false
            });

            var result = await check.CheckHealthAsync(ContextFor(check));

            // The default is Unhealthy (503). This asserts the decision is genuinely
            // configurable, not that Degraded is the recommended setting — see
            // HealthProbeOptions.TenantFailureIsFatal for the reasoning behind the default.
            Assert.Equal(HealthStatus.Degraded, result.Status);
        }

        [Fact]
        public async Task Only_the_configured_number_of_tenants_is_probed()
        {
            using var master = MasterWith(
                new TenantInfo { Id = 1, Hostname = "a", Name = "A", ConnectionString = UnreachableConnectionString },
                new TenantInfo { Id = 2, Hostname = "b", Name = "B", ConnectionString = UnreachableConnectionString },
                new TenantInfo { Id = 3, Hostname = "c", Name = "C", ConnectionString = UnreachableConnectionString });

            var check = Build(master, new HealthProbeOptions
            {
                ProbeTimeoutSeconds = 1,
                TenantProbeCount = 2
            });

            var result = await check.CheckHealthAsync(ContextFor(check));

            // Probing every tenant on every readiness poll does not scale; the subset is the
            // point. Lowest id first, so the choice is stable across restarts.
            Assert.Equal(2, result.Data.Count);
            Assert.True(result.Data.ContainsKey("tenant-1"));
            Assert.True(result.Data.ContainsKey("tenant-2"));
            Assert.False(result.Data.ContainsKey("tenant-3"));
        }

        [Fact]
        public async Task A_master_database_with_no_tenants_fails_readiness_by_default()
        {
            using var master = MasterWith();

            var check = Build(master, new HealthProbeOptions { ProbeTimeoutSeconds = 1 });
            var result = await check.CheckHealthAsync(ContextFor(check));

            // A deployment with no tenants can serve nobody. Reporting healthy would hide a
            // failed or half-run provisioning step.
            Assert.Equal(HealthStatus.Unhealthy, result.Status);
        }

        [Fact]
        public async Task A_master_database_with_no_tenants_can_be_configured_not_to_fail()
        {
            using var master = MasterWith();

            var check = Build(master, new HealthProbeOptions
            {
                ProbeTimeoutSeconds = 1,
                RequireAtLeastOneTenant = false
            });

            var result = await check.CheckHealthAsync(ContextFor(check));

            Assert.Equal(HealthStatus.Healthy, result.Status);
        }

        [Fact]
        public async Task A_tenant_row_with_an_empty_connection_string_is_unhealthy_not_an_exception()
        {
            using var master = MasterWith(new TenantInfo
            {
                Id = 4,
                Hostname = "d",
                Name = "D",
                ConnectionString = ""
            });

            var check = Build(master, new HealthProbeOptions { ProbeTimeoutSeconds = 1 });
            var result = await check.CheckHealthAsync(ContextFor(check));

            Assert.Equal(HealthStatus.Unhealthy, result.Status);
            Assert.Equal("Unhealthy", result.Data["tenant-4"]);
        }
    }
}
