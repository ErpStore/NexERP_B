using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace V.SMART.Shared.Services
{
    /// <summary>
    /// M2-B11 — the structured replacement for <see cref="FileLoggingService"/> (R-23).
    /// <para>
    /// <b>The interface is unchanged.</b> The same three frozen signatures; only the
    /// destination changes. Where <see cref="FileLoggingService"/> interpolated six audit
    /// fields into one <c>|</c>-delimited line in a per-user text file, this emits them as six
    /// separately named structured properties, so <c>Screen = "Sales Order"</c> is a query
    /// rather than a substring search.
    /// </para>
    /// <para>
    /// <b>It depends on <see cref="ILogger{TCategoryName}"/>, not on Serilog.</b> That is
    /// deliberate: V.SMART.Shared is referenced by all three hosts, and adding a Serilog
    /// package reference here would impose the sink on every one of them. Serilog is wired in
    /// as the logging provider by whichever host wants it — today only
    /// <c>V.SMART/V.SMART.Api/Program.cs</c> — and it reads the named properties straight off
    /// the message template.
    /// </para>
    /// <para>
    /// <b>Which hosts resolve this (M2-B11 decision, KB-113 §6).</b> Only V.SMART.Api.
    /// <c>AddVSmartDomain()</c> still registers <see cref="FileLoggingService"/>
    /// (<c>ServiceCollectionExtensions.cs</c>), and V.SMART.Api overrides that registration
    /// after calling it. The Blazor and MAUI hosts keep flat-file logging untouched, because
    /// neither has a Serilog sink configured and swapping them to <see cref="ILogger"/> alone
    /// would send a live audit trail to a console provider — losing it, which is precisely
    /// what R-23 forbids. This also settles the <c>IHttpContextAccessor</c> question: the
    /// dependency is optional (see the constructor), so the class is safe to register in the
    /// MAUI host whenever that host gains a durable sink, but it is not registered there yet.
    /// </para>
    /// </summary>
    public sealed class StructuredLoggingService : ILoggingService
    {
        /// <summary>
        /// The discriminator that makes the audit stream separable from diagnostics. The API
        /// host routes on exactly this property value to a dedicated sink with its own,
        /// longer, retention (see <c>V.SMART/V.SMART.Api/Program.cs</c>).
        /// </summary>
        public const string UserActionEventType = "UserAction";

        /// <summary>The discriminator carried by developer/diagnostic events.</summary>
        public const string DiagnosticEventType = "Diagnostic";

        private readonly ILogger<StructuredLoggingService> _logger;
        private readonly IHttpContextAccessor? _httpContextAccessor;

        /// <param name="httpContextAccessor">
        /// Optional on purpose. The MAUI host has no <see cref="IHttpContextAccessor"/> at all,
        /// and a required dependency would break it at resolution time. The same optional
        /// pattern is already used by
        /// <c>V.SMART/V.SMART.Shared/Services/MultiCompanyService/TenantProvider.cs:18</c>.
        /// When it is absent the correlation id falls back to <see cref="Activity"/> alone.
        /// </param>
        public StructuredLoggingService(
            ILogger<StructuredLoggingService> logger,
            IHttpContextAccessor? httpContextAccessor = null)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// The audit record. Emits the six fields of the flat-file format as six named
        /// properties, plus <c>EventType</c> and the M2-A06 correlation id. Serilog supplies
        /// the timestamp.
        /// <para>
        /// The <c>= null!</c> defaults on this method and the two below are <b>not</b> a
        /// signature change — the default value is still <see langword="null"/>, exactly as
        /// <c>ILoggingService.cs:5-7</c> declares it. The <c>!</c> suppresses the three
        /// <c>CS8625</c> warnings a nullable-enabled project would otherwise raise, which
        /// matters because CI's warning gate is a **ratchet**: the total may fall, never rise
        /// (<c>ci/warning-baseline.json</c>). Without it this task would have added 3 warnings
        /// to each host build and failed the gate.
        /// </para>
        /// </summary>
        public Task LogUserAction(string UserName, string Machine, string IP_Address, string screen, string action, string additionalInfo = null!)
        {
            // additionalInfo is free text assembled at ~494 call sites, several of which
            // interpolate whole diffs into it (for example
            // V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/.../PaymentsService.cs
            // builds "Payments Id: {id}\n{changes}"). It is the one audit field that could
            // plausibly carry a pasted connection string, so it is scrubbed. The other five
            // are a user, a machine, an IP, a screen name and an action verb.
            _logger.LogInformation(
                "User action: {UserName} performed {Action} on {Screen} from {Machine} ({IpAddress}) [{EventType} {CorrelationId}] {AdditionalInfo}",
                UserName,
                action,
                screen,
                Machine,
                IP_Address,
                UserActionEventType,
                CurrentCorrelationId(),
                SensitiveDataRedactor.Redact(additionalInfo));

            return Task.CompletedTask;
        }

        /// <summary>Developer/diagnostic information. Not part of the audit stream.</summary>
        public Task LogDeveloperInfo(string message, string source = null!)
        {
            _logger.LogInformation(
                "{Source}: {Message} [{EventType} {CorrelationId}]",
                source,
                SensitiveDataRedactor.Redact(message),
                DiagnosticEventType,
                CurrentCorrelationId());

            return Task.CompletedTask;
        }

        /// <summary>
        /// Developer error. The <see cref="Exception"/> is passed as the exception argument,
        /// not preformatted into <c>"EXCEPTION: {message}\nSTACKTRACE: {stack}"</c> as
        /// <c>FileLoggingService.cs:60</c> did — every sink renders an exception, its type and
        /// its stack trace better than a string does, and a multi-line string in a
        /// line-oriented file was one of R-23's four impacts.
        /// </summary>
        public Task LogDeveloperError(Exception ex, string source = null!)
        {
            _logger.LogError(
                ex,
                "{Source}: unhandled error [{EventType} {CorrelationId}]",
                source,
                DiagnosticEventType,
                CurrentCorrelationId());

            return Task.CompletedTask;
        }

        /// <summary>
        /// The M2-A06 correlation id, computed with the same definition as
        /// <c>V.SMART/V.SMART.Api/Middleware/CorrelationId.cs:41</c>
        /// (<c>Activity.Current?.Id ?? HttpContext.TraceIdentifier</c>).
        /// <para>
        /// <b>Why it is restated rather than called.</b> <c>CorrelationId.For</c> lives in
        /// <c>V.SMART.Api</c>; V.SMART.Shared cannot reference it without inverting the
        /// project dependency. The duplication is recorded as a drift risk in KB-113 rather
        /// than hidden. In practice both sides return the same value: the first term is
        /// identical and the second reads the same <see cref="HttpContext"/>.
        /// </para>
        /// </summary>
        private string? CurrentCorrelationId() =>
            Activity.Current?.Id ?? _httpContextAccessor?.HttpContext?.TraceIdentifier;
    }
}
