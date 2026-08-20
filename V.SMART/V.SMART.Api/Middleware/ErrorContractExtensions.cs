using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace V.SMART.Api.Middleware
{
    /// <summary>
    /// Registration for the M2-A06 error contract, kept in one place so a future host cannot wire
    /// half of it.
    /// </summary>
    public static class ErrorContractExtensions
    {
        /// <summary>
        /// Makes MVC's automatic <c>400</c> (the <c>[ApiController]</c> model-state short-circuit)
        /// use the same canonical body as everything else, and registers <c>ProblemDetails</c>
        /// services for framework-generated responses. Call after <c>AddControllers()</c>.
        /// </summary>
        public static IServiceCollection AddErrorContract(this IServiceCollection services)
        {
            services.AddProblemDetails();

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                    ApiProblems.ToResult(ApiProblems.Validation(context.HttpContext, context.ModelState));
            });

            return services;
        }

        /// <summary>
        /// Inserts the error contract at the head of the pipeline: correlation id, then the global
        /// exception handler, then a status-code handler that gives a body to framework responses
        /// that would otherwise have none (an unauthenticated call returns a bodiless 401; an
        /// unmatched route a bodiless 404).
        /// <para>
        /// <c>403</c> is deliberately excluded from the status-code handler:
        /// <see cref="ApiProblems.ScreenRightDenied"/> is the only permitted producer of a 403
        /// body and it needs the screen and the right, which are not knowable here. Emitting a
        /// second, screen-less 403 shape is exactly what this task forbids.
        /// </para>
        /// Must be called before <c>UseCors</c>.
        /// </summary>
        public static IApplicationBuilder UseErrorContract(this IApplicationBuilder app)
        {
            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseStatusCodePages(async statusCodeContext =>
            {
                var context = statusCodeContext.HttpContext;
                var status = context.Response.StatusCode;

                if (status < 400 || status == StatusCodes.Status403Forbidden)
                {
                    return;
                }

                var problem = status switch
                {
                    StatusCodes.Status401Unauthorized =>
                        ApiProblems.Unauthenticated(context, "Authentication is required."),
                    StatusCodes.Status404NotFound =>
                        ApiProblems.NotFound(context, "The requested resource was not found."),
                    StatusCodes.Status500InternalServerError =>
                        ApiProblems.Unhandled(context),
                    _ => ApiProblems.Create(context, status, ProblemTypes.ForStatus(status), TitleFor(status))
                };

                await ApiProblems.WriteAsync(context, problem);
            });

            return app;
        }

        /// <summary>
        /// A constant, non-revealing title for a status this API does not name explicitly. Never
        /// derived from the request or from an exception.
        /// </summary>
        private static string TitleFor(int statusCode)
        {
            var phrase = ReasonPhrases.GetReasonPhrase(statusCode);
            return string.IsNullOrEmpty(phrase) ? "Request failed." : phrase;
        }
    }
}
