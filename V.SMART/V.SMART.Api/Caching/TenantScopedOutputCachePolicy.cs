using System.Security.Claims;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace V.SMART.Api.Caching
{
    /// <summary>
    /// M2-B09 — the output-cache policy for <c>/api/v1/reference</c>. It caches
    /// <b>authenticated</b> responses, keyed on the caller's tenant, and refuses to cache
    /// anything whose tenant it cannot establish.
    ///
    /// <para><b>Why this is a hand-written policy and not
    /// <c>OutputCachePolicyBuilder</c>.</b> The framework's default policy deliberately
    /// declines to cache a request that carries an <c>Authorization</c> header or an
    /// authenticated user — a sensible default, because caching an authenticated response
    /// without saying who it belongs to is how one user is served another's data. Every
    /// endpoint in this group is <c>[Authorize]</c>, so composing on the default policy would
    /// have produced a cache that <b>silently never stores anything</b>: the endpoints would
    /// work, the tests would pass, the measurements would be meaningless, and nobody would
    /// find out until someone profiled it. Opting into authenticated caching is a decision that
    /// has to be made explicitly, and the price of making it is owning the key.</para>
    ///
    /// <para><b>The key.</b> Five of the six reference lists are read through
    /// <c>ApplicationDbContext</c>, which is resolved <i>per tenant</i>. A cache keyed on the
    /// URL alone would serve tenant A's states to tenant B — the single highest-risk line in
    /// this task. The key therefore includes the <c>TenantId</c> claim, the same value
    /// <c>TenantProvider</c> and <c>UserRightsProvider</c> resolve tenancy from
    /// (BR-TEN-001/002).</para>
    ///
    /// <para><b>Fail closed.</b> If the caller is unauthenticated, or the <c>TenantId</c> claim
    /// is missing, unparseable or non-positive, this policy disables caching for that request
    /// rather than falling back to a URL-only key. A cache miss costs a query; a cache hit on
    /// an unkeyed entry costs a cross-tenant disclosure. The asymmetry decides it.</para>
    ///
    /// <para><b>Not user-keyed, on purpose.</b> These six lists are tenant-wide reference data,
    /// identical for every user in the tenant. None of them is filtered by the caller — in
    /// particular <c>/reference/screens</c> is the permission <i>vocabulary</i>, not anyone's
    /// rights; a caller's own rights come from <c>GET /api/v1/me</c> (M2-A07). If any endpoint
    /// in this group ever becomes caller-dependent, it must either add the user to this key or
    /// leave the group.</para>
    /// </summary>
    public sealed class TenantScopedOutputCachePolicy : IOutputCachePolicy
    {
        private readonly TimeSpan _expiration;

        public TenantScopedOutputCachePolicy(TimeSpan expiration) => _expiration = expiration;

        /// <summary>The vary-by key name; also asserted by the tests.</summary>
        public const string TenantVaryByKey = "tenant";

        ValueTask IOutputCachePolicy.CacheRequestAsync(OutputCacheContext context, CancellationToken cancellationToken)
        {
            var request = context.HttpContext.Request;

            // Only safe, idempotent verbs are cacheable. HEAD is included because the framework
            // serves it from the same entry as GET.
            var cacheableMethod =
                HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method);

            var tenantId = ResolveTenantId(context.HttpContext.User);
            var cacheable = cacheableMethod && tenantId is not null;

            context.EnableOutputCaching = cacheable;
            context.AllowCacheLookup = cacheable;
            context.AllowCacheStorage = cacheable;

            // Locking collapses a stampede of concurrent misses into one upstream call, which is
            // the whole point for a list every screen requests on load.
            context.AllowLocking = true;

            context.ResponseExpirationTimeSpan = _expiration;

            if (cacheable)
            {
                // The tenant is part of the key, not merely part of the response.
                context.CacheVaryByRules.VaryByValues[TenantVaryByKey] = tenantId!;
            }

            return ValueTask.CompletedTask;
        }

        ValueTask IOutputCachePolicy.ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellationToken)
            => ValueTask.CompletedTask;

        ValueTask IOutputCachePolicy.ServeResponseAsync(OutputCacheContext context, CancellationToken cancellationToken)
        {
            var response = context.HttpContext.Response;

            // Never store a non-200 — an error must not be replayed to the rest of the tenant
            // for the whole TTL.
            if (response.StatusCode != StatusCodes.Status200OK)
            {
                context.AllowCacheStorage = false;
                return ValueTask.CompletedTask;
            }

            // A response that sets cookies is caller-specific by definition.
            if (!StringValues.IsNullOrEmpty(response.Headers.SetCookie))
            {
                context.AllowCacheStorage = false;
                return ValueTask.CompletedTask;
            }

            // The wire contract must agree with the server-side policy. `private` keeps any
            // shared proxy between here and the browser from holding a tenant-scoped body — the
            // same cross-tenant risk as an unkeyed server cache, one hop further out.
            response.Headers[HeaderNames.CacheControl] =
                $"private, max-age={(int)_expiration.TotalSeconds}";

            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// The tenant claim, or <c>null</c> when it is absent, unparseable or non-positive.
        /// Mirrors <c>ScreenRightAuthorizationFilter</c>'s treatment of the same claim, so the
        /// cache and the authorization filter cannot disagree about who the caller is.
        /// </summary>
        private static string? ResolveTenantId(ClaimsPrincipal? user)
        {
            if (user?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var raw = user.FindFirst("TenantId")?.Value;

            if (string.IsNullOrWhiteSpace(raw)
                || !int.TryParse(raw, out var tenantId)
                || tenantId <= 0)
            {
                return null;
            }

            return tenantId.ToString();
        }
    }
}
