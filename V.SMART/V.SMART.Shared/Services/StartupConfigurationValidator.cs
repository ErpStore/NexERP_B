using Microsoft.Extensions.Configuration;
using System;
using System.Security.Cryptography;
using System.Text;

namespace V.SMART.Shared.Services
{
    /// <summary>
    /// Fail-fast startup configuration validation (task M0-03-03). Call once, immediately
    /// after <c>WebApplication.CreateBuilder(args)</c> and before any registration that
    /// consumes a secret: in <c>V.SMART.Api</c> that means before the <c>Jwt:Secret</c> read
    /// and before <c>JwtTokenService</c> is constructed; in <c>V.SMART.Web</c> before
    /// <c>MasterDbContext</c> is registered.
    ///
    /// A host that starts with no secret configured, or with a published default secret, is
    /// worse than a host that will not start: the first silently issues forgeable tokens
    /// (KB-060 R-02) or runs against nobody's real database; the second tells an operator
    /// exactly what to fix. Every failure throws <see cref="InvalidOperationException"/>
    /// naming the failing key, why it failed, and the exact remediation.
    ///
    /// <b>No exception message ever includes the offending value</b> — exception messages
    /// reach logs, and this application's flat-file logger writes plaintext files per user
    /// per day (KB-060 R-23).
    ///
    /// The known-defaults deny-list below is a <b>tripwire, not a security control</b>. It
    /// catches only the exact pre-rotation values this repository is known to have carried
    /// (KB-060 R-01 / R-02), stored as SHA-256 hex digests so that the plaintext never
    /// re-enters source control. A rotated value that is weak for some other reason is not
    /// caught here; only exact match against a known-leaked value and a minimum length are
    /// checked. The digests were computed locally from git history by the session that wrote
    /// this file; the plaintext they were computed from is not, and must never be, committed
    /// anywhere in this repository — not in a comment, not in a test fixture.
    /// </summary>
    public static class StartupConfigurationValidator
    {
        // KB-060 R-01 — distinct pre-rotation connection-string values this repository is
        // known to have carried, across V.SMART.Web/appsettings.json,
        // MasterDbContextFactory.cs, ApplicationDbContextFactory.cs and MauiProgram.cs.
        // Digests only; see that risk-register entry for the file:line provenance of each.
        private static readonly string[] KnownDefaultConnectionStringHashes =
        {
            // local SQLEXPRESS master DB — the form committed in Web/appsettings.json,
            // MasterDbContextFactory.cs and MauiProgram.cs's active registration
            "077f8b4baafe9d8b0d6abb54acca3bad866b8337dc995408804dc860cb6dec60",
            // the commented production alternative present beside each of the above
            "50790109ecb8bdf7993c370a9ea21824dc05a09c7de05c135ab4a418094c8ebb",
            // MauiProgram.cs's third, separately-hosted commented alternative
            "2abdd5f44afcec2248289fef3b79128a183f7df71d269cc588489fcabd4a3952",
            // the local tenant-database default from ApplicationDbContextFactory.cs
            "90a99fd827e6e6f04f1888dbfb3be3c83ba0cd5e69ca05b8b603bf042deb846b",
            // its commented production counterpart in the same file
            "d5fa0e32b7335843a1d6e91f8a76a66d5512831a528acec1bb897eedccbf4141",
            // the later local master-DB variant committed on the M0-15 debugging branch
            "d14927840ad518be48703be8541135a5390958433b4d8554373258ba4f897cfe",
            // a further local variant recorded by the earlier M0-03-03 working session from
            // V.SMART.Api/appsettings.json while that file was still untracked. It is NOT
            // re-derivable from git history (the file was never committed with a value);
            // carried forward because R-01's stance is to treat every observed pre-rotation
            // value as harvested. A digest cannot leak the value it covers.
            "af0dd1d6b96a33ecd30ef530a7c245e9d91358a0ffa6ca255052f507edfdaba3",
        };

        // KB-060 R-02 — the hardcoded default Jwt:Secret this repository is known to have
        // carried (V.SMART.Api/appsettings.json, and, via the KB itself, committed history).
        // Digest only.
        private static readonly string[] KnownDefaultJwtSecretHashes =
        {
            "48426b2009f1f684e66a4eff9d194fa0ff5fe1b65827cd4cd125c54691926732",
        };

        // What SymmetricSecurityKey + HMAC-SHA256 (V.SMART.Api/Program.cs) needs before the
        // signature has meaningful strength. A floor, not a strength guarantee.
        private const int MinJwtSecretUtf8Bytes = 32;

        private const string MasterDbKey = "ConnectionStrings:MasterDb";
        private const string MasterDbEnvVar = "ConnectionStrings__MasterDb";
        private const string JwtSecretKey = "Jwt:Secret";
        private const string JwtSecretEnvVar = "Jwt__Secret";

        /// <summary>
        /// Validates <c>ConnectionStrings:MasterDb</c> unconditionally and — when
        /// <paramref name="requireJwt"/> is <c>true</c> — also <c>Jwt:Secret</c>,
        /// <c>Jwt:Issuer</c> and <c>Jwt:Audience</c>. Pass <c>requireJwt: true</c> for
        /// <c>V.SMART.Api</c>; <c>V.SMART.Web</c> has no <c>Jwt</c> section today, so it
        /// passes <c>false</c>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown for the first key that fails, with an actionable, value-free message.
        /// </exception>
        public static void Validate(IConfiguration configuration, bool requireJwt)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            ValidateMasterConnectionString(configuration);

            if (requireJwt)
            {
                ValidateJwtSecret(configuration);
                RequireNonEmpty(configuration, "Jwt:Issuer", "Jwt__Issuer");
                RequireNonEmpty(configuration, "Jwt:Audience", "Jwt__Audience");
            }
        }

        private static void ValidateMasterConnectionString(IConfiguration configuration)
        {
            var value = configuration[MasterDbKey];

            if (string.IsNullOrWhiteSpace(value))
            {
                throw Fail(MasterDbKey, "is missing, empty, or whitespace", MasterDbEnvVar);
            }

            if (MatchesKnownDefault(value, KnownDefaultConnectionStringHashes))
            {
                throw Fail(
                    MasterDbKey,
                    "matches a known pre-rotation default value (see docs/kb/risks/technical-debt-register.md R-01). "
                        + "That value is compromised and must be replaced with a rotated connection string, not reused",
                    MasterDbEnvVar);
            }
        }

        /// <summary>
        /// The single place that decides whether <c>Jwt:Secret</c> is acceptable. Called at
        /// startup by <c>V.SMART.Api</c> through <see cref="Validate"/>, and again by
        /// <c>JwtTokenService</c> before it signs a token, so that the two cannot diverge.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the secret is missing, empty, whitespace, shorter than 32 UTF-8
        /// bytes, or equal to a known default. The message never contains the value.
        /// </exception>
        public static void ValidateJwtSecret(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            var value = configuration[JwtSecretKey];

            if (string.IsNullOrWhiteSpace(value))
            {
                throw Fail(JwtSecretKey, "is missing, empty, or whitespace", JwtSecretEnvVar);
            }

            if (Encoding.UTF8.GetByteCount(value) < MinJwtSecretUtf8Bytes)
            {
                throw Fail(
                    JwtSecretKey,
                    $"is shorter than the required {MinJwtSecretUtf8Bytes} bytes in UTF-8, which is what "
                        + "SymmetricSecurityKey with HMAC-SHA256 needs",
                    JwtSecretEnvVar);
            }

            if (MatchesKnownDefault(value, KnownDefaultJwtSecretHashes))
            {
                throw Fail(
                    JwtSecretKey,
                    "matches a known published default value (see docs/kb/risks/technical-debt-register.md R-02). "
                        + "That value is compromised and must be replaced with a freshly generated secret, not reused",
                    JwtSecretEnvVar);
            }
        }

        private static void RequireNonEmpty(IConfiguration configuration, string key, string envVar)
        {
            if (string.IsNullOrWhiteSpace(configuration[key]))
            {
                throw Fail(key, "is missing, empty, or whitespace", envVar);
            }
        }

        private static bool MatchesKnownDefault(string value, string[] knownHashesHex)
        {
            var actual = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

            foreach (var known in knownHashesHex)
            {
                if (string.Equals(actual, known, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static InvalidOperationException Fail(string key, string reason, string envVar)
        {
            return new InvalidOperationException(
                $"Startup configuration is invalid: {key} {reason}. "
                    + "This host deliberately refuses to start (task M0-03-03); see docs/CONFIGURATION.md."
                    + Environment.NewLine
                    + $"  Environment variable (servers, CI): {envVar}=<value>"
                    + Environment.NewLine
                    + $"  User-secrets (developer machines): dotnet user-secrets set \"{key}\" \"<value>\" --project <this host's .csproj>"
                    + Environment.NewLine
                    + "  This message never repeats the offending value, because exception messages reach log files.");
        }
    }
}
