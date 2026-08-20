namespace V.SMART.Api.Middleware
{
    /// <summary>
    /// The global exception handler. Registered immediately after
    /// <see cref="CorrelationIdMiddleware"/> and before CORS, authentication, authorization and
    /// MVC, so it wraps all of them.
    /// <para>
    /// Every escaping exception becomes a <c>500</c> <c>problem+json</c> carrying <c>traceId</c>
    /// only. Infrastructure faults must never be dressed up as a misleading <c>4xx</c> (R-19);
    /// the single exception to "everything is a 500" is the tenant-resolution failure below,
    /// which is a diagnosable condition with a known signature and would otherwise reach the
    /// caller as an unhandled <see cref="NullReferenceException"/> with no diagnostic at all
    /// (KB-014 problem 4).
    /// </para>
    /// </summary>
    public sealed class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException && context.RequestAborted.IsCancellationRequested)
                {
                    // The client went away. There is nobody to answer, and this is not a fault.
                    _logger.LogInformation(
                        "Request {Method} {Path} aborted by the client. CorrelationId {CorrelationId}",
                        context.Request.Method, context.Request.Path, CorrelationId.For(context));
                    return;
                }

                if (context.Response.HasStarted)
                {
                    // The body is already partly on the wire; replacing it would produce a
                    // truncated hybrid. Let the server tear the connection down instead.
                    _logger.LogError(
                        ex,
                        "Unhandled exception after the response had started. {Method} {Path}. CorrelationId {CorrelationId}",
                        context.Request.Method, context.Request.Path, CorrelationId.For(context));
                    throw;
                }

                var isTenantFailure = IsTenantResolutionFailure(ex);

                _logger.LogError(
                    ex,
                    "Unhandled exception for {Method} {Path}. CorrelationId {CorrelationId}",
                    context.Request.Method, context.Request.Path, CorrelationId.For(context));

                context.Response.Clear();

                var problem = isTenantFailure
                    ? ApiProblems.TenantUnresolved(
                        context,
                        StatusCodes.Status503ServiceUnavailable,
                        "Tenant context is unavailable.",
                        "The tenant for this request could not be resolved. Sign in again, or contact your administrator.")
                    : ApiProblems.Unhandled(context);

                await ApiProblems.WriteAsync(context, problem);
            }
        }

        /// <summary>
        /// Recognises the documented silent tenant failure: <c>TenantProvider.GetCurrentTenant()</c>
        /// returns <c>null</c> when no resolution step succeeds (<c>TenantProvider.cs:75-82</c>)
        /// and <c>TenantDbContextFactory.CreateDbContext()</c> then dereferences
        /// <c>.ConnectionString</c> on it (<c>TenantDbContextFactory.cs:19</c>).
        /// <para>
        /// Matched on the throwing frame rather than on the exception type alone, so an unrelated
        /// <see cref="NullReferenceException"/> stays a 500. This is detection only — no
        /// behaviour in <c>V.SMART.Shared</c> is changed, because that library is live under
        /// Blazor Server.
        /// </para>
        /// </summary>
        public static bool IsTenantResolutionFailure(Exception? exception)
        {
            for (var depth = 0; exception is not null && depth < 8; depth++, exception = exception.InnerException)
            {
                if (exception is NullReferenceException &&
                    exception.StackTrace is { } stack &&
                    stack.Contains("TenantDbContextFactory", StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
