using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        public static DefaultHttpContext Create(string path = "/api/currencies/7", string method = "GET")
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
    }
}
