using System.Diagnostics;

namespace V.SMART.Api.Middleware
{
    /// <summary>
    /// The single definition of this API's correlation id.
    /// <para>
    /// The value is <c>Activity.Current?.Id ?? HttpContext.TraceIdentifier</c> — the definition
    /// already fixed for the <c>403</c> body by KB-105 §7.1
    /// (<c>docs/kb/architecture/server-side-authorization-spec.md:697</c>), so the same id
    /// appears in every status from 400 to 500 and in the response header.
    /// </para>
    /// <para>
    /// <b>Design decision (M2-A06): a caller-supplied correlation header is IGNORED.</b> The id
    /// is always generated server-side. A client-controlled value would let a caller forge or
    /// collide log correlation, and nothing in the KB or the code asked for inbound propagation
    /// (grepped 2026-08-20: zero implementation of <c>X-Correlation-Id</c> anywhere before this
    /// task). If distributed tracing later needs inbound propagation, it arrives through W3C
    /// <c>traceparent</c>, which <see cref="Activity"/> already honours, not through this header.
    /// </para>
    /// </summary>
    public static class CorrelationId
    {
        /// <summary>The response header carrying the correlation id.</summary>
        public const string HeaderName = "X-Correlation-Id";

        private const string ItemKey = "__VSmart_CorrelationId";

        /// <summary>
        /// Returns the correlation id for this request, computing and caching it on first use so
        /// the header and every <c>problem+json</c> body agree for the whole request.
        /// </summary>
        public static string For(HttpContext context)
        {
            if (context.Items.TryGetValue(ItemKey, out var existing) &&
                existing is string cached && cached.Length > 0)
            {
                return cached;
            }

            var id = Activity.Current?.Id ?? context.TraceIdentifier;
            context.Items[ItemKey] = id;
            return id;
        }
    }
}
