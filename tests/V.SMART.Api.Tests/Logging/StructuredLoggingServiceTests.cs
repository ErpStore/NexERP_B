using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using V.SMART.Shared.Services;
using Xunit;

namespace V.SMART.Api.Tests.Logging
{
    /// <summary>
    /// M2-B11 — proves the audit trail survived the move off flat files.
    /// <para>
    /// The point of R-23's action item is not "we now use Serilog". It is that
    /// <c>Screen = "Sales Order"</c> is a queryable property, where
    /// <c>"| Screen: Sales Order |"</c> (<c>FileLoggingService.cs:33-34</c>) was a substring
    /// search. These tests assert on <see cref="LogEvent.Properties"/>, which is the thing a
    /// sink indexes, not on the rendered message.
    /// </para>
    /// </summary>
    public class StructuredLoggingServiceTests
    {
        private static (StructuredLoggingService Service, CollectingSink Sink) Build(
            IHttpContextAccessor? httpContextAccessor = null)
        {
            var sink = new CollectingSink();

            var serilog = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Sink(sink)
                .CreateLogger();

            var factory = new SerilogLoggerFactory(serilog);

            return (new StructuredLoggingService(
                factory.CreateLogger<StructuredLoggingService>(),
                httpContextAccessor), sink);
        }

        private static string Scalar(LogEvent logEvent, string property) =>
            ((ScalarValue)logEvent.Properties[property]).Value?.ToString() ?? string.Empty;

        [Fact]
        public async Task LogUserAction_emits_the_six_audit_fields_as_separate_named_properties()
        {
            var (service, sink) = Build();

            await service.LogUserAction(
                UserName: "vivek",
                Machine: "WS-014",
                IP_Address: "10.0.0.14",
                screen: "Sales Order",
                action: "Approve",
                additionalInfo: "SO-2026-0041");

            var logEvent = Assert.Single(sink.Events);

            // The six fields of FileLoggingService's flat line, now individually named.
            Assert.Equal("vivek", Scalar(logEvent, "UserName"));
            Assert.Equal("WS-014", Scalar(logEvent, "Machine"));
            Assert.Equal("10.0.0.14", Scalar(logEvent, "IpAddress"));
            Assert.Equal("Sales Order", Scalar(logEvent, "Screen"));
            Assert.Equal("Approve", Scalar(logEvent, "Action"));
            Assert.Equal("SO-2026-0041", Scalar(logEvent, "AdditionalInfo"));

            // Plus the discriminator that makes the audit stream separable, and the timestamp
            // Serilog supplies.
            Assert.Equal(
                StructuredLoggingService.UserActionEventType,
                Scalar(logEvent, "EventType"));
            Assert.NotEqual(default, logEvent.Timestamp);
        }

        [Fact]
        public async Task LogUserAction_is_queryable_by_user_by_screen_and_by_date_range()
        {
            // The acceptance test that matters (M2-B11 Testing item 9). If these three
            // predicates cannot be expressed over the emitted events, R-23's action item is
            // unmet.
            var (service, sink) = Build();

            await service.LogUserAction("vivek", "WS-014", "10.0.0.14", "Sales Order", "Approve");
            await service.LogUserAction("vivek", "WS-014", "10.0.0.14", "Purchase Order", "Create");
            await service.LogUserAction("asha", "WS-021", "10.0.0.21", "Sales Order", "Print");

            var events = sink.Events;
            var from = DateTimeOffset.UtcNow.AddMinutes(-5);
            var to = DateTimeOffset.UtcNow.AddMinutes(5);

            Assert.Equal(2, events.Count(e => Scalar(e, "UserName") == "vivek"));
            Assert.Equal(2, events.Count(e => Scalar(e, "Screen") == "Sales Order"));
            Assert.Equal(3, events.Count(e => e.Timestamp >= from && e.Timestamp <= to));

            // And the three combined — "what did user X do in the Sales module on date D".
            Assert.Single(events.Where(e =>
                Scalar(e, "UserName") == "vivek" &&
                Scalar(e, "Screen") == "Sales Order" &&
                e.Timestamp >= from && e.Timestamp <= to));
        }

        [Fact]
        public async Task The_audit_stream_is_separable_from_diagnostics_by_EventType()
        {
            var (service, sink) = Build();

            await service.LogUserAction("vivek", "WS-014", "10.0.0.14", "Sales Order", "Approve");
            await service.LogDeveloperInfo("cache warmed", "UserRightsProvider");

            var events = sink.Events;

            Assert.Single(events.Where(e =>
                Scalar(e, "EventType") == StructuredLoggingService.UserActionEventType));
            Assert.Single(events.Where(e =>
                Scalar(e, "EventType") == StructuredLoggingService.DiagnosticEventType));
        }

        [Fact]
        public async Task LogDeveloperError_attaches_the_Exception_object_not_a_preformatted_string()
        {
            var (service, sink) = Build();

            Exception thrown;
            try
            {
                throw new InvalidOperationException("boom");
            }
            catch (Exception ex)
            {
                thrown = ex;
            }

            await service.LogDeveloperError(thrown, "CurrencyController");

            var logEvent = Assert.Single(sink.Events);

            // FileLoggingService.cs:60 built "EXCEPTION: {message}" plus "STACKTRACE: {stack}"
            // — a multi-line string in a line-oriented file. The exception now travels as an
            // exception, which every sink renders with its type and stack trace.
            Assert.NotNull(logEvent.Exception);
            Assert.Same(thrown, logEvent.Exception);
            Assert.DoesNotContain("STACKTRACE", logEvent.RenderMessage(), StringComparison.Ordinal);
            Assert.Equal(LogEventLevel.Error, logEvent.Level);
        }

        [Fact]
        public async Task A_user_action_carries_the_correlation_id_of_the_request_that_caused_it()
        {
            var httpContext = new DefaultHttpContext { TraceIdentifier = "trace-abc-123" };
            var accessor = new HttpContextAccessor { HttpContext = httpContext };

            var (service, sink) = Build(accessor);

            // The M2-A06 definition is Activity.Current?.Id ?? TraceIdentifier. With no
            // ambient Activity the second term must be used, which is the branch that would
            // silently produce null if the optional accessor were ignored.
            var previous = Activity.Current;
            Activity.Current = null;
            try
            {
                await service.LogUserAction("vivek", "WS-014", "10.0.0.14", "Sales Order", "Approve");
            }
            finally
            {
                Activity.Current = previous;
            }

            var logEvent = Assert.Single(sink.Events);
            Assert.Equal("trace-abc-123", Scalar(logEvent, "CorrelationId"));
        }

        [Fact]
        public async Task It_works_with_no_IHttpContextAccessor_at_all_which_is_the_MAUI_case()
        {
            // The MAUI host has no IHttpContextAccessor registered. A required dependency
            // would fail at resolution time; this asserts the optional one does not.
            var (service, sink) = Build(httpContextAccessor: null);

            await service.LogUserAction("vivek", "WS-014", "10.0.0.14", "Sales Order", "Approve");

            var logEvent = Assert.Single(sink.Events);
            Assert.Equal("vivek", Scalar(logEvent, "UserName"));
        }

        [Fact]
        public async Task The_frozen_ILoggingService_contract_is_what_is_implemented()
        {
            // Guards the one change that would turn this 3-day task into a repository-wide
            // refactor: widening or renaming any of the three signatures. Calling through the
            // interface, with additionalInfo omitted, exercises the default argument too.
            ILoggingService service = Build().Service;

            await service.LogUserAction("vivek", "WS-014", "10.0.0.14", "Sales Order", "Approve");
            await service.LogDeveloperInfo("message");
            await service.LogDeveloperError(new Exception("x"));
        }
    }
}
