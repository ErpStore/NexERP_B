using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using V.SMART.Api.Auth;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Enum;
using V.SMART.Shared.Data.Master.Admin;
using Xunit;

namespace V.SMART.Api.Tests
{
    /// <summary>
    /// M2-A04 — <c>RefreshTokenService</c> against a real EF Core query pipeline (InMemory
    /// provider), the same rationale <c>RowScopeQueryTests</c> already established for this
    /// project: this proves the hash lookup, expiry, revocation and <c>IsActive</c> re-check are
    /// genuinely composed into the store, not merely true against a mock.
    /// </summary>
    public class RefreshTokenServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _db;
        private readonly RefreshTokenService _sut;

        public RefreshTokenServiceTests()
        {
            _db = new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                    .Options);
            _db.Database.EnsureCreated();

            _db.Users.Add(new User
            {
                UserId = 1,
                UserName = "alice",
                UserPassword = "hash-not-relevant-here",
                IsActive = true,
                Role = UserRole.Administrator
            });
            _db.Users.Add(new User
            {
                UserId = 2,
                UserName = "bob",
                UserPassword = "hash-not-relevant-here",
                IsActive = false,
                Role = UserRole.User
            });
            _db.SaveChanges();

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:RefreshTokenExpiresDays"] = "14"
                })
                .Build();

            _sut = new RefreshTokenService(_db, configuration);
        }

        public void Dispose() => _db.Dispose();

        private static string HashOf(string raw)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));

        [Fact]
        public async Task Issue_stores_only_the_hash_never_the_raw_value()
        {
            var issued = await _sut.IssueAsync(1);

            var row = Assert.Single(_db.RefreshTokens);
            Assert.NotEqual(issued.RawValue, row.TokenHash);
            Assert.Equal(HashOf(issued.RawValue), row.TokenHash);
            Assert.Equal(64, row.TokenHash.Length); // SHA-256, hex-encoded
            Assert.Null(row.RevokedAtUtc);
            Assert.Equal(issued.ExpiresAtUtc, row.ExpiresAtUtc);
        }

        [Fact]
        public async Task Rotate_with_an_unknown_token_is_NotFound()
        {
            var result = await _sut.RotateAsync("token-never-issued");

            Assert.Equal(RefreshOutcome.NotFound, result.Outcome);
            Assert.Null(result.Issued);
            Assert.Null(result.UserId);
        }

        [Fact]
        public async Task Rotate_succeeds_mints_a_new_pair_and_revokes_the_presented_token()
        {
            var issued = await _sut.IssueAsync(1);

            var result = await _sut.RotateAsync(issued.RawValue);

            Assert.Equal(RefreshOutcome.Success, result.Outcome);
            Assert.Equal(1, result.UserId);
            Assert.NotNull(result.Issued);
            Assert.NotEqual(issued.RawValue, result.Issued!.RawValue);

            var originalRow = _db.RefreshTokens.Single(t => t.TokenHash == HashOf(issued.RawValue));
            Assert.NotNull(originalRow.RevokedAtUtc);

            var newRow = _db.RefreshTokens.Single(t => t.TokenHash == HashOf(result.Issued.RawValue));
            Assert.Null(newRow.RevokedAtUtc);
        }

        [Fact]
        public async Task Rotation_is_one_time_use_a_replayed_token_is_refused()
        {
            var issued = await _sut.IssueAsync(1);
            await _sut.RotateAsync(issued.RawValue); // consumes it, mints a replacement

            var replay = await _sut.RotateAsync(issued.RawValue);

            Assert.Equal(RefreshOutcome.Revoked, replay.Outcome);
            Assert.Null(replay.Issued);
        }

        [Fact]
        public async Task Rotate_refuses_an_expired_token()
        {
            var issued = await _sut.IssueAsync(1);
            var row = _db.RefreshTokens.Single();
            row.ExpiresAtUtc = DateTime.UtcNow.AddSeconds(-1);
            await _db.SaveChangesAsync();

            var result = await _sut.RotateAsync(issued.RawValue);

            Assert.Equal(RefreshOutcome.Expired, result.Outcome);
        }

        [Fact]
        public async Task Rotate_refuses_a_deactivated_users_token_the_central_security_assertion()
        {
            var issued = await _sut.IssueAsync(2); // bob, IsActive = false

            var result = await _sut.RotateAsync(issued.RawValue);

            Assert.Equal(RefreshOutcome.UserInactive, result.Outcome);
        }

        [Fact]
        public async Task Revoke_makes_a_subsequent_rotation_fail_as_Revoked()
        {
            var issued = await _sut.IssueAsync(1);

            await _sut.RevokeAsync(issued.RawValue);
            var result = await _sut.RotateAsync(issued.RawValue);

            Assert.Equal(RefreshOutcome.Revoked, result.Outcome);
        }

        [Fact]
        public async Task Revoke_is_a_silent_no_op_for_an_unknown_token()
        {
            await _sut.RevokeAsync("token-never-issued"); // must not throw, must not create a row

            Assert.Empty(_db.RefreshTokens);
        }

        [Fact]
        public async Task Revoke_is_idempotent_for_an_already_revoked_token()
        {
            var issued = await _sut.IssueAsync(1);
            await _sut.RevokeAsync(issued.RawValue);

            await _sut.RevokeAsync(issued.RawValue); // second call must not throw

            var row = _db.RefreshTokens.Single();
            Assert.NotNull(row.RevokedAtUtc);
        }

        [Fact]
        public async Task Two_issued_tokens_for_the_same_user_hash_to_different_values()
        {
            var first = await _sut.IssueAsync(1);
            var second = await _sut.IssueAsync(1);

            Assert.NotEqual(first.RawValue, second.RawValue);
            Assert.Equal(2, _db.RefreshTokens.Count());
        }

        [Fact]
        public async Task A_token_issued_in_one_tenants_database_is_unknown_in_anothers()
        {
            // Target Result 5 / Testing item 7 — "a refresh token issued for one tenant cannot
            // mint a token for another". Database-per-tenant means this is structural: two
            // completely separate ApplicationDbContext instances (as two tenants genuinely are,
            // never one context switching rows), not a filter this service applies.
            var issued = await _sut.IssueAsync(1);

            using var otherTenantDb = new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                    .Options);
            otherTenantDb.Database.EnsureCreated();
            otherTenantDb.Users.Add(new User
            {
                UserId = 1, // same UserId can legitimately recur across tenants
                UserName = "alice",
                UserPassword = "hash-not-relevant-here",
                IsActive = true,
                Role = UserRole.Administrator
            });
            otherTenantDb.SaveChanges();

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:RefreshTokenExpiresDays"] = "14" })
                .Build();
            var otherTenantService = new RefreshTokenService(otherTenantDb, configuration);

            var result = await otherTenantService.RotateAsync(issued.RawValue);

            Assert.Equal(RefreshOutcome.NotFound, result.Outcome);
            Assert.Empty(otherTenantDb.RefreshTokens);
        }
    }
}
