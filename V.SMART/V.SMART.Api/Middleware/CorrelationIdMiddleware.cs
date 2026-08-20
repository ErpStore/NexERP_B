namespace V.SMART.Api.Middleware
{
    /// <summary>
    /// Assigns the correlation id for the request and returns it in the
    /// <c>X-Correlation-Id</c> response header — on success responses as well as failures, so a
    /// client can quote it for any call.
    /// <para>
    /// Registered first in the pipeline: the id must exist before anything that might log or
    /// fail. A caller-supplied header is ignored — see <see cref="CorrelationId"/>.
    /// </para>
    /// </summary>
    public sealed class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

        public Task InvokeAsync(HttpContext context)
        {
            // Compute it now, so the id exists before anything downstream can log or fail.
            _ = CorrelationId.For(context);

            context.Response.OnStarting(static state =>
            {
                var ctx = (HttpContext)state;
                ctx.Response.Headers[CorrelationId.HeaderName] = CorrelationId.For(ctx);
                return Task.CompletedTask;
            }, context);

            return _next(context);
        }
    }
}
