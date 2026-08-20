using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using V.SMART.Api.Authorization;
using V.SMART.Api.Contracts;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.SalesAndLabour.Leads;
using Xunit;

namespace V.SMART.Api.Tests
{
    /// <summary>
    /// M2-A08 — <b>the test that outlives the task</b> (the spec's test 15). Claim 7 showed the
    /// Blazor row scope failed because scoping was a choice made per call site with nothing to
    /// notice when a call site chose wrong. A check that fails the boot when a new endpoint forgets
    /// is the only thing that stops the same failure recurring in the API.
    /// </summary>
    public class RowScopeStartupValidatorTests
    {
        [Fact]
        public void An_endpoint_over_a_scoped_entity_that_declares_nothing_refuses_to_start()
        {
            var services = Services(Action(nameof(StubController.ListLeads)));

            var error = Assert.Throws<InvalidOperationException>(() => RowScopeStartupValidator.Validate(services));

            Assert.Contains("LeadsController.ListLeads", error.Message, StringComparison.Ordinal);
            Assert.Contains("[RowScoped(typeof(Leads))]", error.Message, StringComparison.Ordinal);
            Assert.Contains("[NoRowScope(", error.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void The_scoped_entity_is_found_however_deeply_it_is_wrapped()
        {
            // Task<ActionResult<PagedResult<Leads>>> — the exact shape M2-B02's paging contract and
            // M2-B03's controller template produce.
            var services = Services(Action(nameof(StubController.PagedLeads)));

            var error = Assert.Throws<InvalidOperationException>(() => RowScopeStartupValidator.Validate(services));

            Assert.Contains("LeadsController.PagedLeads", error.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void Declaring_RowScoped_satisfies_it()
        {
            var services = Services(Action(nameof(StubController.ListLeads), new RowScopedAttribute(typeof(Leads))));

            RowScopeStartupValidator.Validate(services);
        }

        [Fact]
        public void Opting_out_explicitly_satisfies_it()
        {
            // Unscoping stays possible — it just cannot happen by omission, and the justification
            // makes every instance greppable.
            var services = Services(Action(
                nameof(StubController.ListLeads),
                new NoRowScopeAttribute("Administrative export, KB-108 §5")));

            RowScopeStartupValidator.Validate(services);
        }

        [Fact]
        public void Declaring_both_is_a_contradiction_and_refuses_to_start()
        {
            var services = Services(Action(
                nameof(StubController.ListLeads),
                new RowScopedAttribute(typeof(Leads)),
                new NoRowScopeAttribute("cannot also be true")));

            var error = Assert.Throws<InvalidOperationException>(() => RowScopeStartupValidator.Validate(services));

            Assert.Contains("both", error.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void RowScoped_over_an_unregistered_entity_refuses_to_start()
        {
            // Claiming to scope something no predicate exists for is worse than not claiming it:
            // the declaration would read as a guarantee at review.
            var services = Services(Action(
                nameof(StubController.ListCurrencies),
                new RowScopedAttribute(typeof(Currency))));

            var error = Assert.Throws<InvalidOperationException>(() => RowScopeStartupValidator.Validate(services));

            Assert.Contains("ScopedEntityCatalogue", error.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void An_endpoint_over_an_unscoped_entity_needs_no_declaration()
        {
            // The sweep must stay silent for the entities that are not scoped, or every controller
            // in the API acquires ceremony it does not need.
            var services = Services(Action(nameof(StubController.ListCurrencies)));

            RowScopeStartupValidator.Validate(services);
        }

        [Fact]
        public void The_APIs_own_actions_all_pass_today()
        {
            // No endpoint returns a scoped entity yet — Leads has no controller. The value of this
            // assertion is that the first one cannot be added without deciding.
            var services = Services(
                Action(nameof(StubController.ListCurrencies)),
                Action(nameof(StubController.Nothing)));

            RowScopeStartupValidator.Validate(services);
        }

        [Fact]
        public void Every_offender_is_reported_in_one_message()
        {
            var services = Services(
                Action(nameof(StubController.ListLeads)),
                Action(nameof(StubController.PagedLeads)));

            var error = Assert.Throws<InvalidOperationException>(() => RowScopeStartupValidator.Validate(services));

            Assert.Contains("LeadsController.ListLeads", error.Message, StringComparison.Ordinal);
            Assert.Contains("LeadsController.PagedLeads", error.Message, StringComparison.Ordinal);
        }

        // ---- fixtures ----------------------------------------------------------------------

        /// <summary>Return-type shapes only; none of these is ever invoked.</summary>
        private sealed class StubController
        {
            public Task<IEnumerable<Leads>> ListLeads() => throw new NotSupportedException();

            public Task<ActionResult<PagedResult<Leads>>> PagedLeads() => throw new NotSupportedException();

            public Task<ActionResult<PagedResult<Currency>>> ListCurrencies() => throw new NotSupportedException();

            public IActionResult Nothing() => throw new NotSupportedException();
        }

        private static ControllerActionDescriptor Action(string method, params object[] metadata)
            => new()
            {
                ControllerName = method.Contains("Lead", StringComparison.Ordinal) ? "Leads" : "Currency",
                ActionName = method,
                MethodInfo = typeof(StubController).GetMethod(method, BindingFlags.Public | BindingFlags.Instance)!,
                EndpointMetadata = metadata
            };

        private static IServiceProvider Services(params ActionDescriptor[] actions)
            => new ServiceCollection()
                .AddSingleton<IActionDescriptorCollectionProvider>(new StubActionDescriptorCollectionProvider(actions))
                .BuildServiceProvider();

        private sealed class StubActionDescriptorCollectionProvider : IActionDescriptorCollectionProvider
        {
            public StubActionDescriptorCollectionProvider(IReadOnlyList<ActionDescriptor> actions)
            {
                ActionDescriptors = new ActionDescriptorCollection(actions.ToList(), version: 1);
            }

            public ActionDescriptorCollection ActionDescriptors { get; }
        }
    }
}
