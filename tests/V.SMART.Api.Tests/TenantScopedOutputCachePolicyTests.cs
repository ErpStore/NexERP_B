using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using V.SMART.Api.Caching;
using Xunit;

namespace V.SMART.Api.Tests
{
    /// <summary>
    /// M2-B09 — tests for the single highest-risk line in the task: the output-cache key.
    ///
    /// <para>Five of the six reference lists are read through <c>ApplicationDbContext</c>, which
    /// is resolved per tenant. <b>A cache keyed on the URL alone serves tenant A's states to
    /// tenant B.</b> These tests assert the key varies by tenant, and — just as important —
    /// that the policy <b>fails closed</b>, disabling caching entirely rather than falling back
    /// to an unkeyed entry whenever it cannot establish who the caller is.</para>
    ///
    /// <para><b>Scope, stated honestly.</b> These exercise the policy object directly against a
    /// constructed <see cref="OutputCacheContext"/>. They prove the key composition and the
    /// fail-closed behaviour, which is where a cross-tenant leak would originate. They are
    /// <i>not</i> an end-to-end two-tenant HTTP test — that needs a running host and two tenant
    /// databases, and this project has no <c>WebApplicationFactory</c> harness. See the task's
    /// execution record for what remains unproven.</para>
    /// </summary>
    public class TenantScopedOutputCachePolicyTests
    {
        private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(60);

        private static OutputCacheContext ContextFor(string? tenantId, string method = "GET", bool authenticated = true)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = method;

            var claims = new List<Claim>();
            if (tenantId is not null)
            {
                claims.Add(new Claim("TenantId", tenantId));
            }

            httpContext.User = authenticated
                ? new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "TestAuth"))
                : new ClaimsPrincipal(new ClaimsIdentity(claims));

            return new OutputCacheContext { HttpContext = httpContext };
        }

        private static async Task<OutputCacheContext> RunCacheRequestAsync(OutputCacheContext context)
        {
            IOutputCachePolicy policy = new TenantScopedOutputCachePolicy(Ttl);
            await policy.CacheRequestAsync(context, CancellationToken.None);

            return context;
        }

        [Fact]
        public async Task Two_different_tenants_produce_two_different_cache_keys()
        {
            // The property the whole task rests on.
            var tenantA = await RunCacheRequestAsync(ContextFor("1"));
            var tenantB = await RunCacheRequestAsync(ContextFor("2"));

            var keyA = tenantA.CacheVaryByRules.VaryByValues[TenantScopedOutputCachePolicy.TenantVaryByKey];
            var keyB = tenantB.CacheVaryByRules.VaryByValues[TenantScopedOutputCachePolicy.TenantVaryByKey];

            Assert.Equal("1", keyA);
            Assert.Equal("2", keyB);
            Assert.NotEqual(keyA, keyB);
        }

        [Fact]
        public async Task The_same_tenant_produces_the_same_cache_key()
        {
            // Otherwise the cache never hits and the whole feature is inert.
            var first = await RunCacheRequestAsync(ContextFor("7"));
            var second = await RunCacheRequestAsync(ContextFor("7"));

            Assert.Equal(
                first.CacheVaryByRules.VaryByValues[TenantScopedOutputCachePolicy.TenantVaryByKey],
                second.CacheVaryByRules.VaryByValues[TenantScopedOutputCachePolicy.TenantVaryByKey]);
        }

        [Fact]
        public async Task An_authenticated_tenant_request_is_actually_cacheable()
        {
            // Guards the trap this policy exists to avoid: the framework's DEFAULT policy
            // declines to cache authenticated responses, and every endpoint in this group is
            // [Authorize]. A cache that silently stores nothing would look identical from the
            // outside — same responses, same green tests — so this asserts the opt-in explicitly.
            var context = await RunCacheRequestAsync(ContextFor("1"));

            Assert.True(context.EnableOutputCaching);
            Assert.True(context.AllowCacheStorage);
            Assert.True(context.AllowCacheLookup);
        }

        [Theory]
        [InlineData(null)]   // claim absent
        [InlineData("")]     // claim present but empty
        [InlineData("   ")]  // whitespace
        [InlineData("abc")]  // unparseable
        [InlineData("0")]    // non-positive
        [InlineData("-3")]
        public async Task It_fails_closed_when_the_tenant_cannot_be_established(string? tenantId)
        {
            // A cache miss costs a query. A cache hit on an unkeyed entry costs a cross-tenant
            // disclosure. The asymmetry is why this must disable caching rather than degrade to
            // a URL-only key.
            var context = await RunCacheRequestAsync(ContextFor(tenantId));

            Assert.False(context.EnableOutputCaching);
            Assert.False(context.AllowCacheStorage);
            Assert.False(context.AllowCacheLookup);
            Assert.DoesNotContain(
                TenantScopedOutputCachePolicy.TenantVaryByKey,
                context.CacheVaryByRules.VaryByValues.Keys);
        }

        [Fact]
        public async Task An_unauthenticated_caller_is_never_cached_even_with_a_tenant_claim()
        {
            // A claim on an unauthenticated principal is caller-supplied, not verified.
            var context = await RunCacheRequestAsync(ContextFor("1", authenticated: false));

            Assert.False(context.EnableOutputCaching);
            Assert.False(context.AllowCacheStorage);
        }

        [Theory]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        [InlineData("PATCH")]
        public async Task Only_safe_verbs_are_cacheable(string method)
        {
            var context = await RunCacheRequestAsync(ContextFor("1", method));

            Assert.False(context.EnableOutputCaching);
            Assert.False(context.AllowCacheStorage);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("HEAD")]
        public async Task Get_and_Head_are_cacheable(string method)
        {
            var context = await RunCacheRequestAsync(ContextFor("1", method));

            Assert.True(context.EnableOutputCaching);
        }

        [Fact]
        public async Task A_non_200_response_is_never_stored()
        {
            // Otherwise one failure is replayed to the whole tenant for the TTL.
            IOutputCachePolicy policy = new TenantScopedOutputCachePolicy(Ttl);
            var context = ContextFor("1");
            context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.AllowCacheStorage = true;

            await policy.ServeResponseAsync(context, CancellationToken.None);

            Assert.False(context.AllowCacheStorage);
        }

        [Fact]
        public async Task A_response_that_sets_cookies_is_never_stored()
        {
            IOutputCachePolicy policy = new TenantScopedOutputCachePolicy(Ttl);
            var context = ContextFor("1");
            context.HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            context.HttpContext.Response.Headers.SetCookie = "session=abc";
            context.AllowCacheStorage = true;

            await policy.ServeResponseAsync(context, CancellationToken.None);

            Assert.False(context.AllowCacheStorage);
        }

        [Fact]
        public async Task A_cached_response_is_marked_private_so_no_shared_proxy_holds_it()
        {
            // The wire contract must agree with the server-side policy. `public` here would let
            // a proxy between the API and the browser hold a tenant-scoped body — the same
            // cross-tenant risk as an unkeyed server cache, one hop further out.
            IOutputCachePolicy policy = new TenantScopedOutputCachePolicy(Ttl);
            var context = ContextFor("1");
            context.HttpContext.Response.StatusCode = StatusCodes.Status200OK;

            await policy.ServeResponseAsync(context, CancellationToken.None);

            var cacheControl = context.HttpContext.Response.Headers.CacheControl.ToString();

            Assert.Contains("private", cacheControl);
            Assert.DoesNotContain("public", cacheControl);
            Assert.Contains("max-age=60", cacheControl);
        }

        [Fact]
        public async Task The_expiration_comes_from_the_configured_ttl()
        {
            IOutputCachePolicy policy = new TenantScopedOutputCachePolicy(TimeSpan.FromSeconds(123));
            var context = ContextFor("1");

            await policy.CacheRequestAsync(context, CancellationToken.None);

            Assert.Equal(TimeSpan.FromSeconds(123), context.ResponseExpirationTimeSpan);
        }
    }
}
