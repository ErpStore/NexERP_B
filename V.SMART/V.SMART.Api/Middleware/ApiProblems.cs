using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace V.SMART.Api.Middleware
{
    /// <summary>
    /// The one place that builds every error body this API returns (ADR-002 §4).
    /// Nothing else may construct a <see cref="ProblemDetails"/>: the whole point of M2-A06 is
    /// that there is exactly one shape per status, and in particular exactly one producer of the
    /// <c>403</c> body (KB-105 §7.1).
    /// </summary>
    public static class ApiProblems
    {
        /// <summary>RFC 7807 media type. Every error response carries it.</summary>
        public const string ContentType = "application/problem+json";

        /// <summary>
        /// Web defaults, deliberately unmodified. The middleware layers serialise here while MVC
        /// serialises a controller's <see cref="ObjectResult"/> itself; leaving both on the same
        /// options is what makes a 404 (or a 403) byte-identical whichever path produced it.
        /// Adding an ignore condition here would silently desynchronise the two.
        /// </summary>
        internal static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

        /// <summary>Builds a problem body with the canonical members set, including <c>traceId</c>.</summary>
        public static ProblemDetails Create(
            HttpContext context,
            int status,
            string type,
            string title,
            string? detail = null)
        {
            var problem = new ProblemDetails
            {
                Status = status,
                Type = type,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path.HasValue ? context.Request.Path.Value : null
            };

            problem.Extensions["traceId"] = CorrelationId.For(context);
            return problem;
        }

        /// <summary>
        /// <b>409 — business-rule refusal.</b> The service's own message is carried into
        /// <c>title</c> <b>verbatim</b>: not reworded, not prefixed, not truncated. Those strings
        /// are product UX written by the domain team (BR-SO-001, ADR-002 §4).
        /// </summary>
        public static ProblemDetails BusinessRuleRefusal(HttpContext context, string message)
            => Create(context, StatusCodes.Status409Conflict, ProblemTypes.BusinessRule, message);

        /// <summary>404 — minimal, per ADR-002 §4.</summary>
        public static ProblemDetails NotFound(HttpContext context, string message)
            => Create(context, StatusCodes.Status404NotFound, ProblemTypes.NotFound, message);

        /// <summary>
        /// 401 — deliberately uninformative. Callers learn no more than they do today
        /// (<c>AuthController</c> returned "Invalid username or password." for every failure).
        /// </summary>
        public static ProblemDetails Unauthenticated(HttpContext context, string title)
            => Create(context, StatusCodes.Status401Unauthorized, ProblemTypes.Unauthenticated, title);

        /// <summary>
        /// <b>403 — screen right denied.</b> The shape is frozen by KB-105 §7.1 and this is the
        /// only method allowed to produce it; the M2-A01-02 authorization filter calls here
        /// rather than writing its own body.
        /// <c>detail</c> is composed from the <i>required</i> screen and right only — never from
        /// what was found — so T-1/T-2/T-5/T-13 stay indistinguishable.
        /// </summary>
        public static ProblemDetails ScreenRightDenied(HttpContext context, string screen, string right)
        {
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Type = ProblemTypes.ScreenRightDenied,
                Title = "Screen right denied.",
                Detail = $"You do not have the '{right}' right for the '{screen}' screen.",
                Instance = context.Request.Path.HasValue ? context.Request.Path.Value : null
            };

            problem.Extensions["screen"] = screen;
            problem.Extensions["right"] = right;
            problem.Extensions["traceId"] = CorrelationId.For(context);
            return problem;
        }

        /// <summary>
        /// <b>413 — payload too large.</b> Produced by the file endpoints when an upload exceeds
        /// the configured maximum (M2-B06). The title states the limit, because a client that does
        /// not know the ceiling cannot retry sensibly; it reveals nothing about any other tenant.
        /// </summary>
        public static ProblemDetails PayloadTooLarge(HttpContext context, string title)
            => Create(context, StatusCodes.Status413PayloadTooLarge, ProblemTypes.PayloadTooLarge, title);

        /// <summary>
        /// The tenant could not be resolved. <b>No connection string, host name or configuration
        /// value ever appears in this body</b> (R-01).
        /// </summary>
        /// <param name="status">
        /// 400 on the login path, where the caller supplied the tenant context and the endpoint
        /// already answered 400 before M2-A06. 503 when the failure surfaces mid-request from
        /// <c>TenantDbContextFactory</c>, because <c>TenantProvider.cs:77-80</c> swallows every
        /// exception before returning null, so an unknown tenant and a MasterDb outage are
        /// indistinguishable there and a 4xx would be a misleading blame of the caller (R-19).
        /// </param>
        public static ProblemDetails TenantUnresolved(HttpContext context, int status, string title, string? detail = null)
            => Create(context, status, ProblemTypes.TenantUnresolved, title, detail);

        /// <summary>
        /// <b>500 — unhandled.</b> Carries <c>traceId</c> and a constant title only. No exception
        /// message, type, stack trace or inner exception, in any environment. R-20
        /// (<c>DetailedErrors = true</c> unconditionally in Blazor Server) is the precedent for
        /// why an environment-gated version is not worth the risk here.
        /// </summary>
        public static ProblemDetails Unhandled(HttpContext context)
            => Create(
                context,
                StatusCodes.Status500InternalServerError,
                ProblemTypes.Unhandled,
                "An unexpected error occurred.");

        /// <summary>
        /// <b>400 — validation.</b> The <c>errors</c> dictionary is keyed by field and carries the
        /// <c>DataAnnotations</c> messages verbatim (e.g. <c>CurrencyVM.cs:14-25</c>).
        /// </summary>
        public static ValidationProblemDetails Validation(HttpContext context, ModelStateDictionary modelState)
        {
            var problem = new ValidationProblemDetails(modelState)
            {
                Status = StatusCodes.Status400BadRequest,
                Type = ProblemTypes.ValidationFailed,
                Title = "One or more validation errors occurred.",
                Instance = context.Request.Path.HasValue ? context.Request.Path.Value : null
            };

            problem.Extensions["traceId"] = CorrelationId.For(context);
            return problem;
        }

        /// <summary>Wraps a problem body in an <see cref="ObjectResult"/> for a controller to return.</summary>
        public static ObjectResult ToResult(ProblemDetails problem)
        {
            var result = new ObjectResult(problem)
            {
                StatusCode = problem.Status,
                DeclaredType = problem.GetType()
            };
            result.ContentTypes.Add(ContentType);
            return result;
        }

        /// <summary>
        /// Writes a problem body directly to the response. Used by the middleware layers, which
        /// run outside MVC and therefore cannot return an <see cref="ObjectResult"/>.
        /// </summary>
        public static async Task WriteAsync(HttpContext context, ProblemDetails problem)
        {
            context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
            context.Response.ContentType = ContentType;
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(problem, problem.GetType(), SerializerOptions),
                context.RequestAborted);
        }
    }
}
