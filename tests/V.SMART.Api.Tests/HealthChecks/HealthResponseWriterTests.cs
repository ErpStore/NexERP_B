using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using V.SMART.Api.HealthChecks;
using Xunit;

namespace V.SMART.Api.Tests.HealthChecks
{
    /// <summary>
    /// M2-B11 — the information-disclosure tests for <c>/health/live</c> and
    /// <c>/health/ready</c>.
    /// <para>
    /// Both endpoints are anonymous, because an orchestrator cannot present a JWT. That makes
    /// the response body a disclosure surface reachable by anyone who can reach the port. The
    /// stock advice — serialise <c>entry.Description</c> and <c>entry.Exception</c> — would
    /// publish the SQL Server instance name, the database name and often the login, which is
    /// the same credential-leak risk as the <see cref="V.SMART.Shared.Data.TenantInfo"/> one
    /// through a different door.
    /// </para>
    /// <para>
    /// Each test therefore builds a <see cref="HealthReport"/> that is <b>deliberately full of
    /// things that must not be published</b> — a real-shaped connection string in the
    /// description, a <c>SqlException</c>-shaped exception message, tags — and asserts none of
    /// it reaches the body.
    /// </para>
    /// </summary>
    public class HealthResponseWriterTests
    {
        private const string LeakyDescription =
            "Login failed for user 'sa'. Server=DESKTOP-EXAMPLE\\SQLEXPRESS;Database=NexGenErpDb_Tenant7;Password=Sup3rS3cret!";

        private static async Task<string> WriteAsync(HealthReport report)
        {
            var context = new DefaultHttpContext();
            var body = new MemoryStream();
            context.Response.Body = body;

            await HealthResponseWriter.WriteAsync(context, report);

            body.Position = 0;
            return await new StreamReader(body, Encoding.UTF8).ReadToEndAsync();
        }

        private static HealthReport LeakyReport(HealthStatus status) => new(
            new Dictionary<string, HealthReportEntry>
            {
                [MasterDatabaseHealthCheck.Name] = new HealthReportEntry(
                    status,
                    description: LeakyDescription,
                    duration: TimeSpan.FromMilliseconds(12),
                    exception: new InvalidOperationException(LeakyDescription),
                    data: new Dictionary<string, object>(),
                    tags: new[] { "ready", "internal-only" }),
                [TenantDatabaseHealthCheck.Name] = new HealthReportEntry(
                    status,
                    description: LeakyDescription,
                    duration: TimeSpan.FromMilliseconds(31),
                    exception: new InvalidOperationException(LeakyDescription),
                    data: new Dictionary<string, object> { ["tenant-7"] = "Unhealthy" },
                    tags: new[] { "ready" })
            },
            TimeSpan.FromMilliseconds(45));

        [Theory]
        [InlineData(HealthStatus.Unhealthy)]
        [InlineData(HealthStatus.Degraded)]
        [InlineData(HealthStatus.Healthy)]
        public async Task No_failure_body_discloses_a_connection_string_server_name_database_name_or_exception(
            HealthStatus status)
        {
            var body = await WriteAsync(LeakyReport(status));

            Assert.DoesNotContain("Password", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Sup3rS3cret", body, StringComparison.Ordinal);
            Assert.DoesNotContain("SQLEXPRESS", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("DESKTOP-EXAMPLE", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("NexGenErpDb", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Login failed", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("InvalidOperationException", body, StringComparison.Ordinal);
            Assert.DoesNotContain("StackTrace", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("description", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("internal-only", body, StringComparison.Ordinal);
        }

        [Fact]
        public async Task The_body_still_names_each_check_and_its_status_individually()
        {
            // "Unhealthy" without naming which check failed is not actionable, which is the
            // other half of the contract.
            var body = await WriteAsync(LeakyReport(HealthStatus.Unhealthy));

            Assert.Contains(MasterDatabaseHealthCheck.Name, body, StringComparison.Ordinal);
            Assert.Contains(TenantDatabaseHealthCheck.Name, body, StringComparison.Ordinal);
            Assert.Contains("Unhealthy", body, StringComparison.Ordinal);
        }

        [Fact]
        public async Task The_body_names_which_tenant_failed_by_opaque_id_only()
        {
            var body = await WriteAsync(LeakyReport(HealthStatus.Unhealthy));

            // tenant-7 is enough for an operator to act. The tenant's Name (a customer name)
            // and Hostname are deliberately never in Data — see TenantDatabaseHealthCheck.
            Assert.Contains("tenant-7", body, StringComparison.Ordinal);
        }

        [Fact]
        public async Task A_liveness_report_with_no_checks_writes_an_empty_check_list()
        {
            // /health/live runs no checks at all (Predicate = _ => false), so this is its
            // actual response shape.
            var body = await WriteAsync(new HealthReport(
                new Dictionary<string, HealthReportEntry>(),
                TimeSpan.Zero));

            Assert.Contains("\"status\":\"Healthy\"", body, StringComparison.Ordinal);
            Assert.Contains("\"checks\":[]", body, StringComparison.Ordinal);
        }

        [Fact]
        public async Task The_response_is_json_and_not_the_M2_A06_problem_contract()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            await HealthResponseWriter.WriteAsync(
                context,
                new HealthReport(new Dictionary<string, HealthReportEntry>(), TimeSpan.Zero));

            // Health responses are not API errors. They must not be mistaken for
            // application/problem+json, whose contract M2-A06 owns and this task leaves alone.
            Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);
        }
    }
}
