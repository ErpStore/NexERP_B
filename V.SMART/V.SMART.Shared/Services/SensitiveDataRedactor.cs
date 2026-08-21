using System.Text.RegularExpressions;

namespace V.SMART.Shared.Services
{
    /// <summary>
    /// M2-B11 — last-resort scrubbing of credential-shaped text out of anything about to be
    /// handed to a log sink.
    /// <para>
    /// The primary defence against leaking <see cref="Data.TenantInfo"/> is the Serilog
    /// destructuring policy in the API host
    /// (<c>V.SMART/V.SMART.Api/Logging/TenantInfoDestructuringPolicy.cs</c>), which stops the
    /// object ever being serialised. This class is the second line: <c>additionalInfo</c>,
    /// exception messages and free-text diagnostic messages are strings that no destructuring
    /// policy inspects, and a connection string reaches them easily — a SqlException message,
    /// a "could not connect to {conn}" diagnostic, a copy-pasted config value.
    /// </para>
    /// <para>
    /// This is deliberately conservative and pattern-based, not a parser. It cannot prove a
    /// string is credential-free; it removes the shapes that actually occur. Treat it as
    /// defence in depth, never as the reason it is safe to pass a connection string to a
    /// logger.
    /// </para>
    /// </summary>
    public static class SensitiveDataRedactor
    {
        /// <summary>The text substituted for a redacted value.</summary>
        public const string Placeholder = "***REDACTED***";

        private static readonly RegexOptions Options =
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled;

        // ADO.NET connection-string keywords whose VALUE is a credential or a credential-
        // adjacent locator. Matched up to the next ';' or end of string, which is exactly how
        // a connection string delimits a value.
        private static readonly Regex CredentialKeyword = new(
            @"\b(password|pwd|user\s*id|uid|accountkey|shared\s*access\s*key|apikey|api\s*key|token|secret)\s*=\s*[^;]*",
            Options);

        // The locator half. A server or database name is not a credential, but the health-check
        // contract (KB-113) and R-23 both forbid disclosing it, and its presence is the reliable
        // signal that a whole connection string has been pasted into a message.
        private static readonly Regex LocatorKeyword = new(
            @"\b(server|data\s*source|initial\s*catalog|database|address|addr|network\s*address)\s*=\s*[^;]*",
            Options);

        /// <summary>
        /// Returns <paramref name="value"/> with credential- and locator-shaped
        /// <c>keyword=value</c> pairs replaced by <see cref="Placeholder"/>. Null and
        /// whitespace-only input is returned unchanged.
        /// </summary>
        public static string? Redact(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var scrubbed = CredentialKeyword.Replace(value, m => Keyword(m) + "=" + Placeholder);
            scrubbed = LocatorKeyword.Replace(scrubbed, m => Keyword(m) + "=" + Placeholder);
            return scrubbed;
        }

        private static string Keyword(Match match) => match.Groups[1].Value;
    }
}
