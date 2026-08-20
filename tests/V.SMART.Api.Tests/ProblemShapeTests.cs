using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using V.SMART.Api.Middleware;
using V.SMART.Api.Tests.Infrastructure;
using Xunit;

namespace V.SMART.Api.Tests
{
    /// <summary>
    /// M2-A06 — the shapes themselves, independent of who produced them. In particular the
    /// <c>403</c>, whose body was frozen by the M2-A01-01 spec
    /// (<c>docs/kb/architecture/server-side-authorization-spec.md</c> §7.1) and which M2-A06 must
    /// serialise, not redesign.
    /// </summary>
    public class ProblemShapeTests
    {
        [Fact]
        public void Screen_right_denied_matches_the_M2_A01_01_spec_shape()
        {
            var context = ErrorContractTestContext.Create("/api/v1/sales-orders/1042");
            var json = ErrorContractTestContext.Serialize(
                ApiProblems.ScreenRightDenied(context, "Sales Order", "Edit"));

            Assert.Equal(
                "https://api.v-smart.local/problems/screen-right-denied",
                json.GetProperty("type").GetString());
            Assert.Equal("Screen right denied.", json.GetProperty("title").GetString());
            Assert.Equal(403, json.GetProperty("status").GetInt32());
            Assert.Equal(
                "You do not have the 'Edit' right for the 'Sales Order' screen.",
                json.GetProperty("detail").GetString());
            Assert.Equal("/api/v1/sales-orders/1042", json.GetProperty("instance").GetString());
            Assert.Equal("Sales Order", json.GetProperty("screen").GetString());
            Assert.Equal("Edit", json.GetProperty("right").GetString());
            Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("traceId").GetString()));

            Assert.Equal(
                new[] { "type", "title", "status", "detail", "instance", "screen", "right", "traceId" }
                    .OrderBy(x => x),
                ErrorContractTestContext.MemberNames(json).OrderBy(x => x));
        }

        [Fact]
        public async Task The_403_written_by_a_filter_and_by_the_shared_path_are_byte_identical()
        {
            // There is exactly one producer, so "both places" is the same method reached two ways:
            // an MVC result (what the M2-A01-02 filter returns) and a direct middleware write.
            var controllerContext = ErrorContractTestContext.Create("/api/v1/sales-orders/1042");
            var filterResult = (ObjectResult)new StubController
            {
                ControllerContext = new ControllerContext { HttpContext = controllerContext }
            }.ScreenRightDeniedProblem("Sales Order", "Edit");

            var middlewareContext = ErrorContractTestContext.Create("/api/v1/sales-orders/1042");
            await ApiProblems.WriteAsync(
                middlewareContext,
                ApiProblems.ScreenRightDenied(middlewareContext, "Sales Order", "Edit"));

            var fromFilter = ErrorContractTestContext.Serialize((ProblemDetails)filterResult.Value!);
            var fromMiddleware = ErrorContractTestContext.ReadJson(middlewareContext);

            Assert.Equal(fromFilter.GetRawText(), fromMiddleware.GetRawText());
            Assert.Equal(ApiProblems.ContentType, middlewareContext.Response.ContentType);
            Assert.Equal(StatusCodes.Status403Forbidden, middlewareContext.Response.StatusCode);
        }

        [Fact]
        public void Every_status_in_the_ADR_002_table_names_a_type_uri_under_one_base()
        {
            foreach (var status in new[] { 400, 401, 403, 404, 409, 500 })
            {
                Assert.StartsWith(ProblemTypes.Base, ProblemTypes.ForStatus(status));
            }
        }

        [Fact]
        public async Task Every_problem_body_is_written_as_application_problem_json()
        {
            var context = ErrorContractTestContext.Create();

            foreach (var problem in new[]
            {
                ApiProblems.BusinessRuleRefusal(context, "Cannot delete."),
                ApiProblems.NotFound(context, "Currency not found."),
                ApiProblems.Unauthenticated(context, "Invalid username or password."),
                ApiProblems.ScreenRightDenied(context, "Currency", "Delete"),
                ApiProblems.TenantUnresolved(context, StatusCodes.Status400BadRequest, "Unable to resolve tenant."),
                ApiProblems.Unhandled(context)
            })
            {
                var target = ErrorContractTestContext.Create();
                await ApiProblems.WriteAsync(target, problem);

                Assert.Equal(ApiProblems.ContentType, target.Response.ContentType);
                Assert.Equal(problem.Status, target.Response.StatusCode);

                var json = ErrorContractTestContext.ReadJson(target);
                Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("type").GetString()));
                Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("title").GetString()));
                Assert.Equal(JsonValueKind.String, json.GetProperty("traceId").ValueKind);
            }
        }

        private sealed class StubController : ControllerBase
        {
        }
    }
}
