using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using V.SMART.Shared.Data;
using V.SMART.Shared.Services.MultiCompanyService;
using Xunit;

namespace V.SMART.Shared.Tests.Services
{
    /// <summary>
    /// M2-A05 — <c>TenantProvider.SetTenant</c>/<c>GetCurrentTenant()</c>'s new step 0: explicit
    /// binding, set by <c>AuthController</c>'s Login/Refresh/Logout actions before this class's
    /// scoped instance is first resolved via the tenant-aware <c>ApplicationDbContext</c>
    /// factory. Before this task <c>SetTenant</c> assigned <c>_manualTenant</c> but
    /// <c>GetCurrentTenant()</c> never read it — a dead setter, confirmed here as fixed rather
    /// than assumed.
    ///
    /// <para>No existing test file covered <see cref="TenantProvider"/> at all before this
    /// task; every scenario here is new.</para>
    /// </summary>
    public class TenantProviderTests
    {
        private static MasterDbContext InMemoryMasterDb(params TenantInfo[] tenants)
        {
            var db = new MasterDbContext(
                new DbContextOptionsBuilder<MasterDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                    .Options);
            // ConnectionString is required by the model; these tests never read it (that is
            // TenantDbContextFactory's job, not TenantProvider's), so a placeholder is enough.
            foreach (var tenant in tenants)
                tenant.ConnectionString ??= "irrelevant-to-this-test";
            db.Tenants.AddRange(tenants);
            db.SaveChanges();
            return db;
        }

        [Fact]
        public void SetTenant_resolves_by_Name()
        {
            using var db = InMemoryMasterDb(new TenantInfo { Id = 1, Name = "Acme", Hostname = "acme.example" });
            var provider = new TenantProvider(db);

            provider.SetTenant("Acme");

            var tenant = provider.GetCurrentTenant();
            Assert.NotNull(tenant);
            Assert.Equal(1, tenant.Id);
        }

        [Fact]
        public void SetTenant_resolves_by_Hostname()
        {
            using var db = InMemoryMasterDb(new TenantInfo { Id = 2, Name = "Acme", Hostname = "acme.example" });
            var provider = new TenantProvider(db);

            provider.SetTenant("acme.example");

            var tenant = provider.GetCurrentTenant();
            Assert.NotNull(tenant);
            Assert.Equal(2, tenant.Id);
        }

        [Fact]
        public void SetTenant_with_an_unknown_identifier_and_no_other_resolution_path_returns_null()
        {
            using var db = InMemoryMasterDb(new TenantInfo { Id = 1, Name = "Acme", Hostname = "acme.example" });
            var provider = new TenantProvider(db);

            provider.SetTenant("no-such-tenant");

            // No IHttpContextAccessor was supplied, so steps 1 (JWT claim) and 2 (host) have
            // nothing to read either; step 3 (tenant.json) finds no file in the test's
            // AppContext.BaseDirectory. Every step fails, and GetCurrentTenant() returns null
            // rather than throwing — matching its documented failure shape.
            Assert.Null(provider.GetCurrentTenant());
        }

        [Fact]
        public void GetCurrentTenant_before_any_SetTenant_call_falls_through_to_the_existing_steps()
        {
            // The dead-setter finding, inverted: confirms step 0 is additive, not a replacement
            // — a caller that never calls SetTenant sees exactly the pre-M2-A05 behaviour
            // (JWT claim, then host, then tenant.json; all absent here, so null).
            using var db = InMemoryMasterDb(new TenantInfo { Id = 1, Name = "Acme", Hostname = "acme.example" });
            var provider = new TenantProvider(db);

            Assert.Null(provider.GetCurrentTenant());
        }

        [Fact]
        public void An_explicit_binding_takes_priority_over_the_JWT_claim()
        {
            // Never actually reachable from AuthController's own routes today (SetTenant is
            // only ever called from [AllowAnonymous] actions, which carry no authenticated JWT
            // claim in practice) — this test documents the ordering decision itself, so a
            // future caller of SetTenant on an authenticated request does not discover this
            // priority the hard way.
            using var db = InMemoryMasterDb(
                new TenantInfo { Id = 1, Name = "ClaimTenant", Hostname = "claim.example" },
                new TenantInfo { Id = 2, Name = "ExplicitTenant", Hostname = "explicit.example" });

            // A mocked HttpContext, not DefaultHttpContext: this test project deliberately has
            // no ASP.NET Core framework reference (it is V.SMART.Shared's domain-only suite,
            // per its own project-file header), and DefaultHttpContext's concrete assembly is
            // not on its compile path — only the abstract HttpContext/IHttpContextAccessor
            // types V.SMART.Shared itself already depends on are.
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim("TenantId", "1") }));
            var httpContext = new Mock<HttpContext>();
            httpContext.SetupGet(c => c.User).Returns(claimsPrincipal);
            var accessor = new HttpContextAccessorStub(httpContext.Object);

            var provider = new TenantProvider(db, accessor);
            provider.SetTenant("ExplicitTenant");

            var tenant = provider.GetCurrentTenant();
            Assert.NotNull(tenant);
            Assert.Equal(2, tenant.Id);
        }

        [Fact]
        public void GetCurrentTenant_caches_the_explicitly_bound_result()
        {
            using var db = InMemoryMasterDb(new TenantInfo { Id = 1, Name = "Acme", Hostname = "acme.example" });
            var provider = new TenantProvider(db);
            provider.SetTenant("Acme");

            var first = provider.GetCurrentTenant();
            var second = provider.GetCurrentTenant();

            Assert.Same(first, second);
        }

        /// <summary>Minimal stand-in — the real <c>IHttpContextAccessor</c> needs no mocking
        /// framework for a single fixed context.</summary>
        private sealed class HttpContextAccessorStub : IHttpContextAccessor
        {
            public HttpContextAccessorStub(HttpContext context) => HttpContext = context;
            public HttpContext? HttpContext { get; set; }
        }
    }
}
