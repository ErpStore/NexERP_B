namespace V.SMART.Api.Middleware
{
    /// <summary>
    /// The RFC 7807 <c>type</c> URIs used by this API. They are stable identifiers, not
    /// dereferenceable URLs.
    /// <para>
    /// The base authority is <c>https://api.v-smart.local/problems/</c>, taken from
    /// <c>docs/kb/architecture/server-side-authorization-spec.md:683</c> (KB-105 §7.1), which
    /// froze the <c>403</c> body first and states at :702 that M2-A06 owns the shared
    /// serialisation and must not change that shape. KB-041's illustrative
    /// <c>https://api.vsmart/errors/…</c> (api-readiness-assessment.md:206) is superseded by
    /// this constant — see the M2-A06 note in that document.
    /// </para>
    /// </summary>
    public static class ProblemTypes
    {
        public const string Base = "https://api.v-smart.local/problems/";

        /// <summary>400 — model binding or DataAnnotations failure.</summary>
        public const string ValidationFailed = Base + "validation-failed";

        /// <summary>401 — the caller is not authenticated, or the credentials were rejected.</summary>
        public const string Unauthenticated = Base + "unauthenticated";

        /// <summary>401 — the token is present but unusable (KB-105 §7.2, M2-A01-02).</summary>
        public const string InvalidToken = Base + "invalid-token";

        /// <summary>403 — screen right denied (KB-105 §7.1, BR-AUTH-002).</summary>
        public const string ScreenRightDenied = Base + "screen-right-denied";

        /// <summary>404 — the addressed resource does not exist.</summary>
        public const string NotFound = Base + "not-found";

        /// <summary>413 — the upload exceeds the configured maximum request size (M2-B06).</summary>
        public const string PayloadTooLarge = Base + "payload-too-large";

        /// <summary>409 — a business-rule refusal. <c>title</c> carries the service message verbatim.</summary>
        public const string BusinessRule = Base + "business-rule";

        /// <summary>400/503 — the tenant for this request could not be resolved.</summary>
        public const string TenantUnresolved = Base + "tenant-unresolved";

        /// <summary>500 — unhandled. Body carries <c>traceId</c> only.</summary>
        public const string Unhandled = Base + "unhandled";

        /// <summary>Fallback for a status this API does not name explicitly.</summary>
        public static string ForStatus(int statusCode) => statusCode switch
        {
            400 => ValidationFailed,
            401 => Unauthenticated,
            403 => ScreenRightDenied,
            404 => NotFound,
            409 => BusinessRule,
            413 => PayloadTooLarge,
            500 => Unhandled,
            _ => Base + "http-" + statusCode.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };
    }
}
