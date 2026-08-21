using Serilog;
using V.SMART.Api.Logging;
using V.SMART.Shared.Data;
using V.SMART.Shared.Services;
using Xunit;

namespace V.SMART.Api.Tests.Logging
{
    /// <summary>
    /// M2-B11 — the credential-leak tests. These are the reason the destructuring policy and
    /// the redactor exist.
    /// <para>
    /// <b>The failure they guard against is worse than the problem this task fixes.</b>
    /// <see cref="TenantInfo"/> carries a live, credential-bearing connection string
    /// (<c>V.SMART/V.SMART.Shared/Data/TenantInfo.cs:8</c>). Structured logging serialises
    /// objects. Publishing that string into a searchable, retained, possibly third-party sink
    /// would be strictly worse than the flat files R-23 replaces — the flat files at least
    /// never held it.
    /// </para>
    /// </summary>
    public class TenantCredentialRedactionTests
    {
        private const string TenantConnectionString =
            @"Server=DESKTOP-EXAMPLE\SQLEXPRESS;Database=NexGenErpDb_Tenant7;User Id=sa;Password=Sup3rS3cret!;TrustServerCertificate=True";

        private static (ILogger Logger, CollectingSink Sink) BuildLogger()
        {
            var sink = new CollectingSink();

            var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                // Exactly the policy Program.cs installs. If this line and Program.cs ever
                // disagree, this test is testing nothing — that coupling is deliberate and is
                // called out in KB-113 §5.
                .Destructure.With(new TenantInfoDestructuringPolicy())
                .WriteTo.Sink(sink)
                .CreateLogger();

            return (logger, sink);
        }

        [Fact]
        public void Destructuring_a_TenantInfo_emits_no_connection_string_and_no_password()
        {
            var (logger, sink) = BuildLogger();

            var tenant = new TenantInfo
            {
                Id = 7,
                Name = "Contoso Manufacturing",
                Hostname = "contoso.example.com",
                ConnectionString = TenantConnectionString
            };

            // The exact call shape that would leak: destructuring the whole object.
            logger.Information("Resolved {@Tenant}", tenant);

            var output = sink.RenderAll();

            Assert.DoesNotContain("Password", output, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Sup3rS3cret", output, StringComparison.Ordinal);
            Assert.DoesNotContain("SQLEXPRESS", output, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("NexGenErpDb_Tenant7", output, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(TenantConnectionString, output, StringComparison.Ordinal);
        }

        [Fact]
        public void Destructuring_a_TenantInfo_still_emits_the_id_and_hostname_that_make_it_diagnosable()
        {
            var (logger, sink) = BuildLogger();

            logger.Information("Resolved {@Tenant}", new TenantInfo
            {
                Id = 7,
                Name = "Contoso Manufacturing",
                Hostname = "contoso.example.com",
                ConnectionString = TenantConnectionString
            });

            var output = sink.RenderAll();

            Assert.Contains("TenantId", output, StringComparison.Ordinal);
            Assert.Contains("contoso.example.com", output, StringComparison.Ordinal);
        }

        [Fact]
        public void Stringifying_a_TenantInfo_also_leaks_nothing()
        {
            var (logger, sink) = BuildLogger();

            var tenant = new TenantInfo
            {
                Id = 7,
                Name = "Contoso",
                Hostname = "contoso.example.com",
                ConnectionString = TenantConnectionString
            };

            // A scalar capture rather than a destructured one. TenantInfo does not override
            // ToString(), so this renders the type name — asserted here so that a future
            // ToString() override that included the connection string breaks a test rather
            // than a production deployment.
            logger.Information("Resolved {Tenant}", tenant);

            Assert.DoesNotContain("Sup3rS3cret", sink.RenderAll(), StringComparison.Ordinal);
        }

        [Fact]
        public void A_connection_string_pasted_into_a_user_action_additionalInfo_is_redacted()
        {
            // The path a destructuring policy cannot see: a free-text audit field. 494
            // LogUserAction call sites build additionalInfo by interpolation, some of them
            // from whole diffs, so a configuration value reaching it is entirely plausible.
            var redacted = SensitiveDataRedactor.Redact(
                $"Tenant 7 reconnected using {TenantConnectionString}");

            Assert.NotNull(redacted);
            Assert.DoesNotContain("Sup3rS3cret", redacted!, StringComparison.Ordinal);
            Assert.DoesNotContain("NexGenErpDb_Tenant7", redacted!, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("DESKTOP-EXAMPLE", redacted!, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(SensitiveDataRedactor.Placeholder, redacted!, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData("Password=hunter2")]
        [InlineData("pwd=hunter2")]
        [InlineData("User Id=sa")]
        [InlineData("Initial Catalog=Payroll")]
        [InlineData("Data Source=10.0.0.4")]
        public void Every_credential_shaped_keyword_is_redacted(string fragment)
        {
            var redacted = SensitiveDataRedactor.Redact($"context before; {fragment}; context after");

            Assert.NotNull(redacted);
            Assert.Contains(SensitiveDataRedactor.Placeholder, redacted!, StringComparison.Ordinal);
            Assert.Contains("context before", redacted!, StringComparison.Ordinal);
            Assert.Contains("context after", redacted!, StringComparison.Ordinal);
        }

        [Fact]
        public void Redaction_leaves_ordinary_audit_text_alone()
        {
            const string ordinary = "Payments Id: 4471\nGrandTotal 1200.00 -> 1350.00";

            Assert.Equal(ordinary, SensitiveDataRedactor.Redact(ordinary));
        }
    }
}
