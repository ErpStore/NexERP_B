using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using V.SMART.Api.Middleware;
using V.SMART.Api.Tests.Infrastructure;
using V.SMART.Shared.Data;
using V.SMART.Shared.Services.MultiCompanyService;
using Xunit;

namespace V.SMART.Api.Tests
{
    /// <summary>
    /// M2-A06 — the middleware half of the error contract: correlation id, the 500 body, and the
    /// tenant-resolution failure that used to reach the caller as an unhandled
    /// <see cref="NullReferenceException"/>.
    /// </summary>
    public class ErrorContractMiddlewareTests
    {
        private static ExceptionHandlingMiddleware Handler(RequestDelegate next)
            => new(next, NullLogger<ExceptionHandlingMiddleware>.Instance);

        [Fact]
        public async Task Unhandled_exception_becomes_problem_json_500()
        {
            var context = ErrorContractTestContext.Create();
            await Handler(_ => throw new InvalidOperationException("boom")).InvokeAsync(context);

            Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
            Assert.Equal(ApiProblems.ContentType, context.Response.ContentType);

            var body = ErrorContractTestContext.ReadJson(context);
            Assert.Equal(ProblemTypes.Unhandled, body.GetProperty("type").GetString());
            Assert.Equal("An unexpected error occurred.", body.GetProperty("title").GetString());
            Assert.Equal(500, body.GetProperty("status").GetInt32());
            Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("traceId").GetString()));
        }

        [Fact]
        public async Task Five_hundred_body_leaks_no_exception_message_type_or_stack_trace()
        {
            var context = ErrorContractTestContext.Create();
            await Handler(_ => throw new InvalidOperationException(
                "Failed to retrieve stock details.",
                new ArgumentException("inner secret"))).InvokeAsync(context);

            var raw = ErrorContractTestContext.ReadBody(context);

            Assert.DoesNotContain("Failed to retrieve stock details.", raw);
            Assert.DoesNotContain("inner secret", raw);
            Assert.DoesNotContain("InvalidOperationException", raw);
            Assert.DoesNotContain("ArgumentException", raw);
            Assert.DoesNotContain("   at ", raw);
            Assert.DoesNotContain("stackTrace", raw);

            // Nothing beyond the canonical members. traceId is the only correlation the caller
            // gets; `detail` is absent because ProblemDetails omits its null members.
            Assert.Equal(
                new[] { "type", "title", "status", "instance", "traceId" }.OrderBy(x => x),
                ErrorContractTestContext.MemberNames(ErrorContractTestContext.ReadJson(context)).OrderBy(x => x));
        }

        [Fact]
        public async Task Correlation_id_header_matches_the_body_trace_id()
        {
            var context = ErrorContractTestContext.Create();

            var pipeline = new CorrelationIdMiddleware(
                ctx => Handler(_ => throw new InvalidOperationException("boom")).InvokeAsync(ctx));

            await pipeline.InvokeAsync(context);
            await context.Response.Body.FlushAsync();

            // DefaultHttpContext does not run OnStarting callbacks by itself.
            var header = CorrelationId.For(context);
            var body = ErrorContractTestContext.ReadJson(context);

            Assert.Equal(header, body.GetProperty("traceId").GetString());
            Assert.False(string.IsNullOrWhiteSpace(header));
        }

        [Fact]
        public void Correlation_id_is_stable_for_the_whole_request()
        {
            var context = ErrorContractTestContext.Create();
            Assert.Equal(CorrelationId.For(context), CorrelationId.For(context));
        }

        [Fact]
        public void A_caller_supplied_correlation_header_is_ignored()
        {
            // M2-A06 design decision: the id is always generated server-side, never adopted from
            // the request. A client-controlled value would let a caller forge log correlation.
            var context = ErrorContractTestContext.Create();
            context.Request.Headers[CorrelationId.HeaderName] = "attacker-supplied-value";

            Assert.NotEqual("attacker-supplied-value", CorrelationId.For(context));
        }

        [Fact]
        public async Task Unresolved_tenant_becomes_problem_json_and_never_shows_a_connection_string()
        {
            // The real failure path: TenantProvider returns null (TenantProvider.cs:82) and
            // TenantDbContextFactory dereferences .ConnectionString on it
            // (TenantDbContextFactory.cs:19). The exception is produced by the real type, so the
            // stack frame the middleware matches on is the real one, not a fabricated string.
            var factory = new TenantDbContextFactory(new NullTenantProvider());
            var context = ErrorContractTestContext.Create("/api/v1/currencies", "GET");

            await Handler(_ =>
            {
                factory.CreateDbContext();
                return Task.CompletedTask;
            }).InvokeAsync(context);

            Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
            Assert.Equal(ApiProblems.ContentType, context.Response.ContentType);

            var raw = ErrorContractTestContext.ReadBody(context);
            Assert.DoesNotContain("Server=", raw, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Data Source", raw, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Password", raw, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("NullReferenceException", raw);

            var body = ErrorContractTestContext.ReadJson(context);
            Assert.Equal(ProblemTypes.TenantUnresolved, body.GetProperty("type").GetString());
            Assert.Equal("Tenant context is unavailable.", body.GetProperty("title").GetString());
            Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("traceId").GetString()));
        }

        [Fact]
        public void An_unrelated_null_reference_is_not_mistaken_for_a_tenant_failure()
        {
            Exception? captured = null;
            try
            {
                string? nothing = null;
                _ = nothing!.Length;
            }
            catch (NullReferenceException ex)
            {
                captured = ex;
            }

            Assert.NotNull(captured);
            Assert.False(ExceptionHandlingMiddleware.IsTenantResolutionFailure(captured));
        }

        /// <summary>Reproduces <c>TenantProvider</c>'s documented "cannot determine tenant" outcome.</summary>
        private sealed class NullTenantProvider : ITenantProvider
        {
            public TenantInfo GetCurrentTenant() => null!;
            public void SetTenant(string tenantNameOrHost) { }
            public string GetTenantLogoPath() => string.Empty;
            public string GetTenantCorrespondenceUploadPath() => string.Empty;
        }
    }
}
