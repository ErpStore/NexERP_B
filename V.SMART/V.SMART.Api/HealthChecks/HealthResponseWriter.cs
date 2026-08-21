using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace V.SMART.Api.HealthChecks
{
    /// <summary>
    /// M2-B11 — the response body for <c>/health/live</c> and <c>/health/ready</c>.
    /// <para>
    /// <b>This class exists because the framework default is unsafe here.</b> ASP.NET Core's
    /// stock writer emits the plain word <c>Healthy</c>/<c>Unhealthy</c> and nothing else,
    /// which is not actionable; every worked example on the internet replaces it with one that
    /// serialises <c>entry.Description</c> and <c>entry.Exception</c>. Both endpoints are
    /// <b>anonymous</b> — an orchestrator cannot present a JWT — so a serialised
    /// <c>SqlException</c> would publish the server name, the database name and often the
    /// login to anyone who can reach the port.
    /// </para>
    /// <para>
    /// What is emitted is therefore an allow-list, never a projection of
    /// <see cref="HealthReport"/>: the overall status, and for each check its name, its status
    /// and its duration in milliseconds — plus, for the tenant check only, its own
    /// <c>data</c> dictionary, which <see cref="TenantDatabaseHealthCheck"/> constructs from
    /// opaque tenant ids and status words and nothing else. <c>Description</c>,
    /// <c>Exception</c> and <c>Tags</c> are never written.
    /// </para>
    /// <para>
    /// Health responses use this format, not <c>ProblemDetails</c>: they are not API errors and
    /// they must not be confused with M2-A06's contract, which is unchanged by this task.
    /// </para>
    /// </summary>
    public static class HealthResponseWriter
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = false
        };

        /// <summary>
        /// Writes the allow-listed body. Assigned to
        /// <c>HealthCheckOptions.ResponseWriter</c> for both endpoints in <c>Program.cs</c>.
        /// </summary>
        public static Task WriteAsync(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json; charset=utf-8";

            var body = new Dictionary<string, object?>
            {
                ["status"] = report.Status.ToString(),
                ["totalDurationMs"] = (long)report.TotalDuration.TotalMilliseconds,
                ["checks"] = report.Entries.Select(entry => BuildEntry(entry.Key, entry.Value)).ToArray()
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(body, SerializerOptions));
        }

        private static Dictionary<string, object?> BuildEntry(string name, HealthReportEntry entry)
        {
            var written = new Dictionary<string, object?>
            {
                ["name"] = name,
                ["status"] = entry.Status.ToString(),
                ["durationMs"] = (long)entry.Duration.TotalMilliseconds
            };

            // The per-check detail that makes "Unhealthy" actionable. Only the tenant check
            // populates Data, and only with keys of the form "tenant-{id}" and values that are
            // status words or counts (see TenantDatabaseHealthCheck). Values are stringified
            // rather than serialised structurally so that a future check adding a rich object
            // to Data cannot silently start leaking it through this writer.
            if (entry.Data.Count > 0)
            {
                written["detail"] = entry.Data.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value?.ToString());
            }

            return written;
        }
    }
}
