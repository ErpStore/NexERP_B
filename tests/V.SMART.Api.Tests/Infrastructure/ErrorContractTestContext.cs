using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using V.SMART.Api.Auth;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IAdminService;
using V.SMART.Shared.Repository.IRepository;

namespace V.SMART.Api.Tests.Infrastructure
{
    /// <summary>
    /// Helpers shared by the M2-A06 error-contract tests. No host and no database: the contract
    /// is exercised over a <see cref="DefaultHttpContext"/> with a real response body stream.
    /// </summary>
    internal static class ErrorContractTestContext
    {
        internal static readonly JsonSerializerOptions ReadOptions = new(JsonSerializerDefaults.Web);

        /// <summary>A request context with a readable response body.</summary>
        public static DefaultHttpContext Create(string path = "/api/v1/currencies/7", string method = "GET")
        {
            var context = new DefaultHttpContext();
            context.Request.Method = method;
            context.Request.Path = path;
            context.Response.Body = new MemoryStream();
            context.TraceIdentifier = "test-trace-identifier";
            return context;
        }

        /// <summary>Reads the response body back as raw JSON text.</summary>
        public static string ReadBody(HttpContext context)
        {
            context.Response.Body.Position = 0;
            using var reader = new StreamReader(context.Response.Body, leaveOpen: true);
            return reader.ReadToEnd();
        }

        /// <summary>Reads the response body back as a parsed JSON document root.</summary>
        public static JsonElement ReadJson(HttpContext context)
            => JsonDocument.Parse(ReadBody(context)).RootElement.Clone();

        /// <summary>
        /// Serialises a controller-produced problem body the same way the response pipeline
        /// does, so a test can assert on the wire shape rather than on the CLR object.
        /// </summary>
        public static JsonElement Serialize(ProblemDetails problem)
            => JsonDocument
                .Parse(JsonSerializer.Serialize(problem, problem.GetType(), ReadOptions))
                .RootElement.Clone();

        /// <summary>The member names present at the top level of a problem body.</summary>
        public static IReadOnlyList<string> MemberNames(JsonElement element)
            => element.EnumerateObject().Select(p => p.Name).ToList();

        /// <summary>
        /// M2-A05 — <c>AuthController</c> no longer constructor-injects <c>IUnitOfWork</c>,
        /// <c>IRefreshTokenService</c> or <c>IUserRightService</c> directly (all three reach the
        /// tenant-scoped <c>ApplicationDbContext</c>, which must not be resolved before the
        /// action body has bound the request's <c>tenant</c> field). It resolves them from an
        /// injected <see cref="IServiceProvider"/> instead, after tenant binding. This builds a
        /// minimal mock of that provider, wired only for whichever of the three a test actually
        /// needs — an omitted one throws if the controller unexpectedly reaches for it, which is
        /// the same "MockBehavior.Strict-shaped" failure a missing setup gave before this task.
        /// </summary>
        public static IServiceProvider ServiceProvider(
            IUnitOfWork? unitOfWork = null,
            IRefreshTokenService? refreshTokenService = null,
            IUserRightService? userRightService = null)
        {
            var provider = new Mock<IServiceProvider>();
            if (unitOfWork != null)
                provider.Setup(p => p.GetService(typeof(IUnitOfWork))).Returns(unitOfWork);
            if (refreshTokenService != null)
                provider.Setup(p => p.GetService(typeof(IRefreshTokenService))).Returns(refreshTokenService);
            if (userRightService != null)
                provider.Setup(p => p.GetService(typeof(IUserRightService))).Returns(userRightService);
            return provider.Object;
        }
    }
}
