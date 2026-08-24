using System.Reflection;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using V.SMART.Api.Controllers;
using Xunit;

namespace V.SMART.Api.Tests
{
    /// <summary>
    /// M2-B10 — the OpenAPI obligations of the controller template (KB-114 §11), enforced over
    /// <b>every</b> controller in the assembly rather than over a list somebody must remember to
    /// extend.
    ///
    /// <para><b>Why a test and not a review checklist.</b> An operation id is the generated
    /// TypeScript method name and a tag is its grouping. An action that omits either still
    /// compiles, still serves traffic and still passes every other test — it just emits
    /// <c>apiV1CurrenciesGet</c> into the SPA, or a group nobody expected, and nobody notices
    /// until a call site is written against it. The failure is silent everywhere except here.</para>
    ///
    /// <para>These tests read attributes only. They start no host, open no connection and assert
    /// nothing about behaviour (R-43 still stands: this project has no
    /// <c>Microsoft.AspNetCore.Mvc.Testing</c> reference).</para>
    /// </summary>
    public class OpenApiConformanceTests
    {
        private static IEnumerable<Type> Controllers() =>
            typeof(CurrencyController).Assembly
                .GetTypes()
                .Where(t => t is { IsAbstract: false, IsPublic: true }
                            && typeof(ControllerBase).IsAssignableFrom(t))
                .OrderBy(t => t.FullName, StringComparer.Ordinal);

        private static IEnumerable<(Type Controller, MethodInfo Action, HttpMethodAttribute Route)> Actions() =>
            Controllers().SelectMany(c => c
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .SelectMany(m => m.GetCustomAttributes<HttpMethodAttribute>()
                    .Select(a => (Controller: c, Action: m, Route: a))));

        public static TheoryData<string> ActionNames()
        {
            var data = new TheoryData<string>();
            foreach (var (controller, action, _) in Actions())
            {
                data.Add($"{controller.Name}.{action.Name}");
            }

            return data;
        }

        /// <summary>
        /// Every action declares an explicit operation id, as the route <c>Name</c>. Program.cs's
        /// <c>CustomOperationIds</c> reads exactly this; an action without one emits a null
        /// operationId and the generator falls back to a name derived from the URL.
        /// </summary>
        [Theory]
        [MemberData(nameof(ActionNames))]
        public void Every_action_declares_an_explicit_operation_id(string actionName)
        {
            var (_, _, route) = Actions().Single(a => $"{a.Controller.Name}.{a.Action.Name}" == actionName);

            Assert.False(
                string.IsNullOrWhiteSpace(route.Name),
                $"{actionName} has no route Name. It is the OpenAPI operationId and therefore the "
                + "generated TypeScript method name (KB-114 §11): add Name = \"…\" to its "
                + "[HttpGet]/[HttpPost]/… attribute, then run tools/generate-api-client.sh.");
        }

        /// <summary>Operation ids are globally unique — two operations sharing one id collide in the
        /// generated client.</summary>
        [Fact]
        public void Operation_ids_are_unique_across_the_api()
        {
            var duplicates = Actions()
                .Select(a => a.Route.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .GroupBy(n => n, StringComparer.Ordinal)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToArray();

            Assert.True(duplicates.Length == 0, $"Duplicate operation ids: {string.Join(", ", duplicates!)}");
        }

        /// <summary>
        /// Every controller declares its OpenAPI tag explicitly, so renaming a C# class cannot
        /// silently regroup the generated client, and so two controllers serving one resource can
        /// share a group (CurrencyController and CurrencyExcelController do).
        /// </summary>
        [Fact]
        public void Every_controller_declares_an_explicit_tag()
        {
            var untagged = Controllers()
                .Where(c => !c.GetCustomAttributes().OfType<ITagsMetadata>().Any())
                .Select(c => c.Name)
                .ToArray();

            Assert.True(
                untagged.Length == 0,
                $"Controllers without [Tags(\"…\")]: {string.Join(", ", untagged)}. KB-114 §11 requires "
                + "a declared resource tag on every controller.");
        }

        /// <summary>
        /// Every action declares at least one <see cref="ProducesResponseTypeAttribute"/>. This is
        /// the weak form on purpose: which statuses an action can return is a judgement only its
        /// body can settle (KB-114 §11 forbids over-declaring as firmly as under-declaring), and a
        /// test that guessed would be wrong for half the API. Declaring none, however, is never
        /// right — it means the generated client has no type for any response.
        /// </summary>
        [Theory]
        [MemberData(nameof(ActionNames))]
        public void Every_action_declares_at_least_one_response_type(string actionName)
        {
            var (_, action, _) = Actions().Single(a => $"{a.Controller.Name}.{a.Action.Name}" == actionName);

            Assert.NotEmpty(action.GetCustomAttributes<ProducesResponseTypeAttribute>());
        }
    }
}
