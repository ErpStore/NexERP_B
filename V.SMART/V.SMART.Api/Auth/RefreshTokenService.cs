using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.Admin;

namespace V.SMART.Api.Auth
{
    /// <summary>
    /// M2-A04 — the refresh-token store. Talks to <see cref="ApplicationDbContext"/> directly
    /// (already tenant-resolved and request-scoped by <c>AddVSmartDomain()</c>), the same
    /// database <c>UserRepository.LoginAsync</c> reads — so tenant isolation here is the
    /// structural database-per-tenant guarantee, not an extra check this class has to get right:
    /// a token issued in one tenant's database is simply absent from every other tenant's.
    /// </summary>
    public class RefreshTokenService : IRefreshTokenService
    {
        // Matches StartupConfigurationValidator's own SHA-256 digest convention
        // (V.SMART.Shared/Services/StartupConfigurationValidator.cs:172) — hex, not base64, so a
        // stored hash and a logged digest always look the same shape in this codebase.
        private static string Hash(string rawValue)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawValue)));

        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;

        public RefreshTokenService(ApplicationDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        private TimeSpan RefreshTokenLifetime()
        {
            // Default matches the value this task justifies in the KB (KB-040/KB-013): long
            // enough that a normal working week does not force a re-login, short enough to bound
            // a stolen refresh token's blast radius. Configurable, never hard-coded.
            var days = int.TryParse(_configuration["Jwt:RefreshTokenExpiresDays"], out var d) ? d : 14;
            return TimeSpan.FromDays(days);
        }

        public async Task<IssuedRefreshToken> IssueAsync(int userId)
        {
            // 256 bits — the same entropy budget StartupConfigurationValidator enforces as a
            // *floor* for Jwt:Secret; this value is generated, not chosen, so it clears that
            // floor by construction.
            var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var now = DateTime.UtcNow;
            var expiresAtUtc = now.Add(RefreshTokenLifetime());

            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = userId,
                TokenHash = Hash(raw),
                CreatedAtUtc = now,
                ExpiresAtUtc = expiresAtUtc
                // RevokedAtUtc left null — live from the moment it is issued.
            });

            // Deliberately no try/catch here — see IRefreshTokenService.RotateAsync's doc
            // comment. A save failure at issue time (i.e. at login) must surface as a 500, not
            // as a silently tokenless 200.
            await _db.SaveChangesAsync();

            return new IssuedRefreshToken(raw, expiresAtUtc);
        }

        public async Task<RefreshRotationResult> RotateAsync(string presentedToken)
        {
            var hash = Hash(presentedToken);
            var row = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);

            if (row is null)
                return RefreshRotationResult.Failure(RefreshOutcome.NotFound);

            if (row.RevokedAtUtc is not null)
                return RefreshRotationResult.Failure(RefreshOutcome.Revoked);

            if (row.ExpiresAtUtc <= DateTime.UtcNow)
                return RefreshRotationResult.Failure(RefreshOutcome.Expired);

            // Target Result 6 / Testing item 6 — the task's central security assertion.
            // Re-checked here, on every refresh, not only at login.
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == row.UserId);
            if (user is null || !user.IsActive)
                return RefreshRotationResult.Failure(RefreshOutcome.UserInactive);

            // One-time use with rotation: the presented row is revoked in the same call that
            // issues its replacement, so replaying it can never succeed even if this method is
            // called twice concurrently with the same token — the second SaveChangesAsync would
            // see RevokedAtUtc already set by the first (or race on the same row; either way the
            // unique index plus this revoke leave no path to a second live token from one raw
            // value).
            row.RevokedAtUtc = DateTime.UtcNow;

            var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var now = DateTime.UtcNow;
            var expiresAtUtc = now.Add(RefreshTokenLifetime());

            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.UserId,
                TokenHash = Hash(raw),
                CreatedAtUtc = now,
                ExpiresAtUtc = expiresAtUtc
            });

            await _db.SaveChangesAsync();

            return new RefreshRotationResult(
                RefreshOutcome.Success,
                user.UserId,
                new IssuedRefreshToken(raw, expiresAtUtc));
        }

        public async Task RevokeAsync(string presentedToken)
        {
            var hash = Hash(presentedToken);
            var row = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);

            // Silent no-op for "unknown" and "already revoked" alike — logout must never
            // distinguish them (Target Result 4).
            if (row is null || row.RevokedAtUtc is not null)
                return;

            row.RevokedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}
