using System.Reflection;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using V.SMART.Api.Authorization;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.MasterScreeenManagement;
using Xunit;

namespace V.SMART.Api.Tests.PermissionMatrix
{
    /// <summary>
    /// M2-A03 — the annotation-completeness gate. These tests reflect over the whole
    /// <c>V.SMART.Api</c> assembly and fail the build if any action is ungated, half-gated, or
    /// names a screen that does not exist.
    ///
    /// <para><b>They need no edit when a controller is added.</b> Nothing below enumerates a
    /// controller by name or asserts a fixed endpoint count: a new controller is swept
    /// automatically, and it fails these tests until it is annotated or explicitly allow-listed.
    /// The one thing a new endpoint <i>can</i> require is an allow-list entry — which is the
    /// point, because that entry is a reviewable line in a diff.</para>
    /// </summary>
    public class EndpointDiscoveryTests
    {
        // =====================================================================================
        // The sweep itself
        // =====================================================================================

        /// <summary>
        /// A sweep that silently found nothing would make every other test here vacuously green.
        /// This is the guard against that: the API's controllers are found, and the three
        /// categories partition the surface exactly.
        /// </summary>
        [Fact]
        public void The_sweep_finds_the_api_surface_and_classifies_every_action_exactly_once()
        {
            var all = ApiEndpointDiscovery.All;

            Assert.NotEmpty(ApiEndpointDiscovery.ControllerTypes);
            Assert.NotEmpty(all);

            var anonymous = all.Count(e => e.IsAnonymous);
            var exempt = all.Count(e => !e.IsAnonymous && e.IsScreenRightExempt);
            var guarded = ApiEndpointDiscovery.Guarded.Count;

            Assert.Equal(all.Count, anonymous + exempt + guarded);
            Assert.True(guarded > 0, "No gated endpoint was discovered; the matrix would be empty.");

            // Keys are unique: two actions colliding on one key would let one of them inherit the
            // other's allow-list entry.
            Assert.Equal(all.Count, all.Select(e => e.Key).Distinct(StringComparer.Ordinal).Count());
        }

        /// <summary>
        /// The core acceptance criterion, stated once: <b>every action is gated unless it is on
        /// the checked-in allow-list</b>, carries exactly one <c>[RequireRight]</c>, and names a
        /// seeded screen. The failure message names every offender with its controller, action,
        /// screen and right.
        /// </summary>
        [Fact]
        public void Every_action_is_gated_or_explicitly_allow_listed()
        {
            var problems = AnnotationAudit.Problems(ApiEndpointDiscovery.All);

            Assert.True(
                problems.Count == 0,
                $"{problems.Count} endpoint(s) in V.SMART.Api are not correctly gated (ADR-004, R-03):" +
                Environment.NewLine + AnnotationAudit.Report(problems));
        }

        /// <summary>
        /// The same conditions, asserted through the code that actually gates the running host —
        /// <see cref="ScreenRightStartupValidator"/>, invoked at <c>Program.cs</c> startup. Two
        /// checks that could otherwise drift apart are pinned together: if the harness passes and
        /// the host would refuse to start, or vice versa, this fails.
        /// </summary>
        [Fact]
        public void The_sweep_agrees_with_the_production_startup_validator()
        {
            var services = ServicesFor(ApiEndpointDiscovery.All);

            var error = Record.Exception(() => ScreenRightStartupValidator.Validate(services));

            Assert.True(
                error is null,
                "The production startup validator rejects the API's own annotations, so the host would not " +
                $"start:{Environment.NewLine}{error?.Message}");
        }

        // =====================================================================================
        // The allow-list — exempt by declaration, never by omission
        // =====================================================================================

        /// <summary>
        /// <c>POST /api/v1/auth/login</c> is the only anonymous endpoint in the API, and the
        /// allow-list says so. Both directions are asserted: a new <c>[AllowAnonymous]</c> action
        /// fails until it is listed, and a listed action that is no longer anonymous fails until
        /// it is removed.
        /// </summary>
        [Fact]
        public void The_anonymous_allow_list_matches_the_assembly_exactly_and_has_one_entry()
        {
            var declared = ApiEndpointDiscovery.All
                .Where(e => e.IsAnonymous)
                .Select(e => e.Key)
                .OrderBy(k => k, StringComparer.Ordinal)
                .ToArray();

            var listed = ExemptEndpointAllowList.AnonymousActions.Keys
                .OrderBy(k => k, StringComparer.Ordinal)
                .ToArray();

            Assert.Equal(listed, declared);

            var only = Assert.Single(ExemptEndpointAllowList.AnonymousActions);
            Assert.Equal("AuthController.Login", only.Key, StringComparer.Ordinal);
            Assert.Equal("POST /api/v1/auth/login", only.Value.Route, StringComparer.Ordinal);
            Assert.False(string.IsNullOrWhiteSpace(only.Value.Justification));
        }

        /// <summary>
        /// The second exemption: authenticated but carrying no screen right. Same two-directional
        /// comparison, and every entry must carry a justification in both places — the production
        /// attribute and this list.
        /// </summary>
        [Fact]
        public void The_screen_right_exempt_allow_list_matches_the_assembly_exactly()
        {
            var declared = ApiEndpointDiscovery.All
                .Where(e => !e.IsAnonymous && e.IsScreenRightExempt)
                .ToList();

            Assert.Equal(
                ExemptEndpointAllowList.ScreenRightExemptActions.Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray(),
                declared.Select(e => e.Key).OrderBy(k => k, StringComparer.Ordinal).ToArray());

            foreach (var endpoint in declared)
            {
                Assert.False(
                    string.IsNullOrWhiteSpace(endpoint.ScreenRightExemption!.Justification),
                    $"{endpoint.Describe()} carries [NoScreenRight] with no justification.");

                Assert.False(
                    string.IsNullOrWhiteSpace(ExemptEndpointAllowList.ScreenRightExemptActions[endpoint.Key].Justification),
                    $"{endpoint.Describe()} has a blank justification on the allow-list.");
            }
        }

        /// <summary>An exempt endpoint is exempt once, not twice: the two lists never overlap.</summary>
        [Fact]
        public void The_two_exemption_lists_are_disjoint()
        {
            Assert.Empty(ExemptEndpointAllowList.AnonymousActions.Keys
                .Intersect(ExemptEndpointAllowList.ScreenRightExemptActions.Keys, StringComparer.Ordinal));

            Assert.Equal(
                ExemptEndpointAllowList.AnonymousActions.Count + ExemptEndpointAllowList.ScreenRightExemptActions.Count,
                ExemptEndpointAllowList.AllExemptKeys.Count);
        }

        // =====================================================================================
        // Screen names — the failure mode that denies silently
        // =====================================================================================

        /// <summary>
        /// Every declared screen name exists in the catalogue, and the near-miss the seed
        /// contains — <c>"Currency"</c> (Id 5) beside <c>"Currency Today"</c> (Id 21) — is
        /// present in the catalogue as proof that this check is a set membership test and not a
        /// "non-empty string" test, which both names would pass.
        /// </summary>
        [Fact]
        public void Every_declared_screen_name_exists_in_the_seeded_catalogue()
        {
            foreach (var endpoint in ApiEndpointDiscovery.Guarded.Where(e => e.Screen is { Seeded: true }))
            {
                Assert.True(
                    ScreenCatalogue.SeededScreenNames.Contains(endpoint.Screen!.ScreenName),
                    $"{endpoint.Describe()} names a screen that is not seeded. A wrong name denies every call " +
                    "in every tenant, silently.");
            }

            Assert.Contains("Currency", ScreenCatalogue.SeededScreenNames);
            Assert.Contains("Currency Today", ScreenCatalogue.SeededScreenNames);
        }

        /// <summary>
        /// <b>The drift check M2-A03 asks for.</b> The harness validates screen names against
        /// <see cref="ScreenCatalogue"/>, because that is what the production startup validator
        /// uses and because <c>Screens</c> lives in the per-tenant database with no tenant context
        /// at startup (<c>ScreenCatalogue.cs:3-30</c>). That leaves one hazard: the catalogue is a
        /// hand-copied mirror of the <c>ApplicationDbContext</c> seed and could drift from it. So
        /// this test reads the seed <b>from the EF model itself</b> — the same
        /// <c>builder.Entity&lt;Screens&gt;().HasData(...)</c> the application uses — and pins the
        /// relationship between the two.
        ///
        /// <para>The catalogue is a strict subset: the two rows a later migration deletes
        /// (<c>ScreenCode</c> 114 and 115 — M2-A09, R-65, KB-109 option A) are seeded but must not
        /// be usable in a <c>[RequireScreen]</c>. That exclusion is derived from the seed here, not
        /// hard-coded, so it stays true if the codes change.</para>
        /// </summary>
        [Fact]
        public void The_screen_catalogue_does_not_drift_from_the_ApplicationDbContext_seed()
        {
            var seeded = SeededScreens();

            Assert.NotEmpty(seeded);

            var seededNames = seeded.Select(s => s.Name).ToHashSet(StringComparer.Ordinal);

            // Nothing may be in the catalogue that the seed does not create — that would be a name
            // the harness accepts and the database has never heard of.
            var invented = ScreenCatalogue.SeededScreenNames.Except(seededNames, StringComparer.Ordinal).ToArray();
            Assert.True(
                invented.Length == 0,
                "ScreenCatalogue contains names the ApplicationDbContext seed does not create: " +
                string.Join(", ", invented));

            // And everything the seed creates is in the catalogue, except exactly the rows deleted
            // by the later migration (R-65).
            var deletedByMigration = seeded
                .Where(s => s.Code is 114 or 115)
                .Select(s => s.Name)
                .ToHashSet(StringComparer.Ordinal);

            var missing = seededNames
                .Except(ScreenCatalogue.SeededScreenNames, StringComparer.Ordinal)
                .ToHashSet(StringComparer.Ordinal);

            Assert.Equal(
                deletedByMigration.OrderBy(n => n, StringComparer.Ordinal).ToArray(),
                missing.OrderBy(n => n, StringComparer.Ordinal).ToArray());
        }

        // =====================================================================================
        // Helpers
        // =====================================================================================

        /// <summary>
        /// The seeded <c>Screens</c> rows, read by materialising
        /// <c>ApplicationDbContext</c>'s own <c>HasData</c> seed rather than from a transcribed
        /// list: <c>EnsureCreated()</c> applies the seed to the InMemory store, and the rows are
        /// then read back through the ordinary <c>DbSet</c>.
        ///
        /// <para>Two roads not taken, recorded so nobody re-walks them:
        /// <c>db.Model.FindEntityType(...).GetSeedData()</c> throws — the runtime model is
        /// read-optimised and does not carry seed data — and <c>IDesignTimeModel</c>, which would
        /// carry it, is not present in the EF Core assemblies this project references (probed in
        /// the build output, 2026-08-24). InMemory rather than Sqlite per INV-031: Sqlite cannot
        /// create this model at all.</para>
        /// </summary>
        private static IReadOnlyList<(int Code, string Name)> SeededScreens()
        {
            using var db = new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                    .Options);

            db.Database.EnsureCreated();

            return db.Screens
                .AsNoTracking()
                .Select(s => new { s.ScreenCode, s.ScreenName })
                .ToList()
                .Select(s => (Code: s.ScreenCode, Name: s.ScreenName))
                .ToList();
        }

        /// <summary>
        /// A service provider carrying exactly the discovered actions as MVC action descriptors,
        /// which is the only dependency <see cref="ScreenRightStartupValidator"/> has.
        /// </summary>
        internal static IServiceProvider ServicesFor(IEnumerable<ApiEndpoint> endpoints)
        {
            var descriptors = endpoints
                .Select(e => (ActionDescriptor)new ControllerActionDescriptor
                {
                    ControllerName = e.ControllerName,
                    ActionName = e.ActionName,
                    ControllerTypeInfo = e.ControllerType.GetTypeInfo(),
                    MethodInfo = e.Method,
                    EndpointMetadata = e.EndpointMetadata.ToList()
                })
                .ToList();

            return new ServiceCollection()
                .AddSingleton<IActionDescriptorCollectionProvider>(
                    new StaticActionDescriptorCollectionProvider(descriptors))
                .BuildServiceProvider();
        }

        private sealed class StaticActionDescriptorCollectionProvider : IActionDescriptorCollectionProvider
        {
            public StaticActionDescriptorCollectionProvider(IReadOnlyList<ActionDescriptor> descriptors)
                => ActionDescriptors = new ActionDescriptorCollection(descriptors, version: 1);

            public ActionDescriptorCollection ActionDescriptors { get; }
        }
    }
}
