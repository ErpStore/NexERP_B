using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using V.SMART.Api.Authorization;
using Xunit;

namespace V.SMART.Api.Tests
{
    /// <summary>
    /// M2-A01-02 — the startup half of KB-105 D-4 and D-6. A misannotated action must stop the
    /// host, not wait to deny somebody at request time.
    /// </summary>
    public class ScreenRightStartupValidatorTests
    {
        [Fact]
        public void A_correctly_annotated_action_passes()
        {
            var services = Services(Action(
                new AuthorizeAttribute(),
                new RequireScreenAttribute("Currency"),
                new RequireRightAttribute(Right.Edit)));

            ScreenRightStartupValidator.Validate(services);
        }

        [Fact]
        public void RequireRight_without_RequireScreen_throws_naming_the_action()
        {
            var services = Services(Action(new AuthorizeAttribute(), new RequireRightAttribute(Right.Edit)));

            var error = Assert.Throws<InvalidOperationException>(() => ScreenRightStartupValidator.Validate(services));

            Assert.Contains("CurrencyController.Update", error.Message, StringComparison.Ordinal);
            Assert.Contains("[RequireScreen]", error.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void RequireScreen_without_RequireRight_throws_naming_the_action()
        {
            var services = Services(Action(new AuthorizeAttribute(), new RequireScreenAttribute("Currency")));

            var error = Assert.Throws<InvalidOperationException>(() => ScreenRightStartupValidator.Validate(services));

            Assert.Contains("CurrencyController.Update", error.Message, StringComparison.Ordinal);
            Assert.Contains("[NoScreenRight]", error.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void An_unseeded_screen_name_throws()
        {
            var services = Services(Action(
                new AuthorizeAttribute(),
                new RequireScreenAttribute("Sub-Contract GRN"),
                new RequireRightAttribute(Right.View)));

            var error = Assert.Throws<InvalidOperationException>(() => ScreenRightStartupValidator.Validate(services));

            // The seed spells it "Sub-Contrect GRN" (KB-105 F-8). The typo is canonical; the
            // correctly spelt string is the one that must fail.
            Assert.Contains("Sub-Contract GRN", error.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void Seeded_false_exempts_a_screen_from_the_catalogue_check()
        {
            var services = Services(Action(
                new AuthorizeAttribute(),
                new RequireScreenAttribute("Deliberately Unseeded") { Seeded = false },
                new RequireRightAttribute(Right.View)));

            ScreenRightStartupValidator.Validate(services);
        }

        [Fact]
        public void Anonymous_and_opted_out_actions_are_skipped()
        {
            var services = Services(
                Action(new AllowAnonymousAttribute()),
                Action(new AuthorizeAttribute(), new NoScreenRightAttribute("GET /api/v1/me, KB-105 §2.4")));

            ScreenRightStartupValidator.Validate(services);
        }

        /// <summary>
        /// The direction M2-A01-02 deliberately leaves off: an authenticated action on a
        /// controller with no <c>[RequireScreen]</c> at all. Enabling it now would refuse to start
        /// the host over the API's six existing, unannotated endpoints. M2-A02 turns it on.
        /// </summary>
        [Fact]
        public void An_entirely_unannotated_action_does_not_throw_yet()
        {
            var services = Services(Action(new AuthorizeAttribute()));

            ScreenRightStartupValidator.Validate(services);
        }

        [Fact]
        public void Every_offender_is_reported_in_one_message()
        {
            var services = Services(
                Action(new AuthorizeAttribute(), new RequireRightAttribute(Right.Edit)),
                Action("Vendor", "Delete", new AuthorizeAttribute(), new RequireScreenAttribute("Vendor")));

            var error = Assert.Throws<InvalidOperationException>(() => ScreenRightStartupValidator.Validate(services));

            Assert.Contains("CurrencyController.Update", error.Message, StringComparison.Ordinal);
            Assert.Contains("VendorController.Delete", error.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void The_catalogue_holds_the_150_seeded_names_including_the_seed_typos()
        {
            Assert.Equal(150, ScreenCatalogue.SeededScreenNames.Count);
            Assert.Contains("Currency", ScreenCatalogue.SeededScreenNames);
            Assert.Contains("Sub-Contrect GRN", ScreenCatalogue.SeededScreenNames);
            Assert.Contains("Advaceadjustment", ScreenCatalogue.SeededScreenNames);
            Assert.Contains("Stock Position(Internal & External)", ScreenCatalogue.SeededScreenNames);
            Assert.DoesNotContain("currency", ScreenCatalogue.SeededScreenNames);
        }

        /// <summary>
        /// M2-A09 (R-65, KB-109 option A). <c>Bill Pending List</c> and <c>Bill Paid List</c> were
        /// seeded (<c>ApplicationDbContext.cs:1151</c>) then deleted by a later migration
        /// (<c>ScreenCode</c> 114/115) - every real database holds 150 rows, not 152. Before the
        /// fix these two names passed this validator (they were still in the catalogue) yet had no
        /// corresponding <c>Screens</c> row, so any <c>[RequireScreen]</c> naming one would boot
        /// clean and then 403 every user, forever, with no warning. The validator must refuse them
        /// like any other unseeded name.
        /// </summary>
        [Fact]
        public void A_deleted_phantom_screen_name_is_rejected()
        {
            var services = Services(Action(
                new AuthorizeAttribute(),
                new RequireScreenAttribute("Bill Paid List"),
                new RequireRightAttribute(Right.View)));

            var error = Assert.Throws<InvalidOperationException>(() => ScreenRightStartupValidator.Validate(services));

            Assert.Contains("Bill Paid List", error.Message, StringComparison.Ordinal);
        }

        private static ControllerActionDescriptor Action(params object[] metadata)
            => Action("Currency", "Update", metadata);

        private static ControllerActionDescriptor Action(string controller, string action, params object[] metadata)
            => new()
            {
                ControllerName = controller,
                ActionName = action,
                EndpointMetadata = metadata
            };

        private static IServiceProvider Services(params ActionDescriptor[] actions)
            => new ServiceCollection()
                .AddSingleton<IActionDescriptorCollectionProvider>(new StubActionDescriptorCollectionProvider(actions))
                .BuildServiceProvider();

        private sealed class StubActionDescriptorCollectionProvider : IActionDescriptorCollectionProvider
        {
            public StubActionDescriptorCollectionProvider(IReadOnlyList<ActionDescriptor> actions)
                => ActionDescriptors = new ActionDescriptorCollection(actions, 1);

            public ActionDescriptorCollection ActionDescriptors { get; }
        }
    }
}
