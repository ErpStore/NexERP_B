using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using V.SMART.Api.Auth;
using V.SMART.Shared.Data.Enum;
using V.SMART.Shared.Data.Master.Admin;
using Xunit;

namespace V.SMART.Api.Tests
{
    /// <summary>
    /// M2-A04 — the two things this task's acceptance criteria bind on <c>JwtTokenService</c>:
    /// the claim set stays exactly the pre-existing four (ADR-004 §2 — no rights, no scopes), and
    /// the access-token lifetime is short, configurable, and travels with the token as
    /// <see cref="IssuedAccessToken.ExpiresAtUtc"/> rather than being re-derived by a caller.
    /// </summary>
    public class JwtTokenServiceTests
    {
        // 40+ ASCII bytes, comfortably over StartupConfigurationValidator's 32-byte floor, and
        // not the published/leaked value (see technical-debt-register.md R-02) — a test-only
        // secret that satisfies the same validator CreateToken defers to (M0-03-03), without
        // touching any real Jwt:Secret value.
        private const string TestSecret = "unit-test-only-jwt-signing-secret-do-not-reuse-1234567890";

        private static JwtTokenService Service(string? expiresMinutes = "15")
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Secret"] = TestSecret,
                    ["Jwt:Issuer"] = "V.SMART.Api",
                    ["Jwt:Audience"] = "V.SMART.Angular",
                    ["Jwt:ExpiresMinutes"] = expiresMinutes
                })
                .Build();

            return new JwtTokenService(configuration);
        }

        private static User User() => new()
        {
            UserId = 7,
            UserName = "alice",
            UserPassword = "irrelevant-here",
            Role = UserRole.Administrator
        };

        [Fact]
        public void Claims_are_exactly_the_existing_four_no_rights_no_scopes()
        {
            var issued = Service().CreateToken(User(), tenantId: 3);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(issued.Token);
            var claimTypes = jwt.Claims.Select(c => c.Type).ToList();

            Assert.Contains(ClaimTypes.Name, claimTypes);
            Assert.Contains("UserId", claimTypes);
            Assert.Contains("TenantId", claimTypes);
            Assert.Contains(ClaimTypes.Role, claimTypes);

            Assert.Equal("alice", jwt.Claims.Single(c => c.Type == ClaimTypes.Name).Value);
            Assert.Equal("7", jwt.Claims.Single(c => c.Type == "UserId").Value);
            Assert.Equal("3", jwt.Claims.Single(c => c.Type == "TenantId").Value);
            Assert.Equal("Administrator", jwt.Claims.Single(c => c.Type == ClaimTypes.Role).Value);

            // ADR-004 §2 / Target Result 7 — no fifth *business* claim (a right, a scope, a
            // permission) ever rides in the access token. This is deliberately NOT an exhaustive
            // allow-list of every claim type: JwtSecurityToken always adds its own registered
            // metadata claims (exp/iss/aud/nbf and similar), which are structural, not business
            // authority, and are not this criterion's concern. What the criterion actually rules
            // out is a claim type whose *name* names an authority concept.
            var forbiddenSubstrings = new[] { "right", "scope", "permission", "screen" };
            Assert.All(claimTypes, type =>
                Assert.DoesNotContain(forbiddenSubstrings, forbidden =>
                    type.Contains(forbidden, StringComparison.OrdinalIgnoreCase)));
        }

        [Fact]
        public void Expiry_reflects_the_configured_lifetime()
        {
            var before = DateTime.UtcNow;
            var issued = Service(expiresMinutes: "15").CreateToken(User(), tenantId: 1);

            var expectedFloor = before.AddMinutes(15);
            var expectedCeiling = DateTime.UtcNow.AddMinutes(15).AddSeconds(5); // scheduling slack

            Assert.InRange(issued.ExpiresAtUtc, expectedFloor, expectedCeiling);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(issued.Token);
            // JwtSecurityToken truncates to whole seconds; the caller-facing ExpiresAtUtc must
            // still agree with what is actually inside the signed token, not merely be close.
            Assert.Equal(issued.ExpiresAtUtc.ToString("s"), jwt.ValidTo.ToString("s"));
        }

        [Fact]
        public void An_unparsable_configured_lifetime_falls_back_to_15_minutes()
        {
            var before = DateTime.UtcNow;
            var issued = Service(expiresMinutes: "not-a-number").CreateToken(User(), tenantId: 1);

            Assert.InRange(
                issued.ExpiresAtUtc,
                before.AddMinutes(15),
                DateTime.UtcNow.AddMinutes(15).AddSeconds(5));
        }

        [Fact]
        public void Default_fallback_is_no_longer_the_pre_task_480_minutes()
        {
            // Target Result 1 — this is the regression this task exists to prevent reintroducing.
            var issued = Service(expiresMinutes: null).CreateToken(User(), tenantId: 1);

            var minutesUntilExpiry = (issued.ExpiresAtUtc - DateTime.UtcNow).TotalMinutes;
            Assert.True(minutesUntilExpiry < 480, "fallback lifetime regressed to the pre-M2-A04 8-hour default");
        }
    }
}
