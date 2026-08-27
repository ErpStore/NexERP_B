namespace V.SMART.Api.Auth
{
    /// <summary>
    /// M2-A04 — issues, rotates and revokes refresh tokens. Behind an interface so
    /// <c>AuthController</c> tests can mock it exactly the way they already mock
    /// <c>IUserRepository</c>/<c>ITenantProvider</c> (no DbContext in a controller unit test).
    /// </summary>
    public interface IRefreshTokenService
    {
        /// <summary>Generates a new refresh token for <paramref name="userId"/>, stores it hashed,
        /// and returns the raw (one-time-visible) value plus its expiry.</summary>
        Task<IssuedRefreshToken> IssueAsync(int userId);

        /// <summary>
        /// Validates <paramref name="presentedToken"/> against the store, and if it is live
        /// (known, unexpired, unrevoked, and its user still <c>IsActive</c>), revokes it and
        /// issues a replacement in the same call — rotation, one-time use.
        ///
        /// <para>Deliberately does <b>not</b> catch a store failure: a database fault must
        /// surface as an unhandled exception (the M2-A06 middleware turns it into a 500), never
        /// as a refused outcome (<see cref="RefreshOutcome"/>) — repeating <c>LoginAsync</c>'s R-19 pattern
        /// here would misreport an outage as "your session is invalid".</para>
        /// </summary>
        Task<RefreshRotationResult> RotateAsync(string presentedToken);

        /// <summary>
        /// Revokes <paramref name="presentedToken"/> if it exists and is not already revoked.
        /// Idempotent and silent either way — logout must never reveal whether a token was ever
        /// valid (Target Result 4 / the logout error-model row).
        /// </summary>
        Task RevokeAsync(string presentedToken);
    }

    /// <summary>A freshly issued refresh token. <see cref="RawValue"/> is visible exactly once —
    /// the store only ever keeps its hash.</summary>
    public sealed record IssuedRefreshToken(string RawValue, DateTime ExpiresAtUtc);

    public enum RefreshOutcome
    {
        /// <summary>Rotated successfully; <see cref="RefreshRotationResult.UserId"/> and
        /// <see cref="RefreshRotationResult.Issued"/> are populated.</summary>
        Success,

        /// <summary>Unknown token hash — never issued, or the store was reset.</summary>
        NotFound,

        /// <summary>Found, but already revoked (either a prior rotation consumed it, or logout
        /// revoked it) — a reused or stolen-then-logged-out token.</summary>
        Revoked,

        /// <summary>Found and never revoked, but past <c>ExpiresAtUtc</c>.</summary>
        Expired,

        /// <summary>Found, live, but the owning user is no longer <c>IsActive</c> — the task's
        /// central security assertion (Target Result 6).</summary>
        UserInactive
    }

    /// <summary>
    /// The outcome of a rotation attempt. <see cref="Outcome"/> is for server-side logging only —
    /// the controller must map every non-<see cref="RefreshOutcome.Success"/> value to the same
    /// opaque 401 (Target Result 5 / Testing item 10, "failure opacity").
    /// </summary>
    public sealed record RefreshRotationResult(RefreshOutcome Outcome, int? UserId, IssuedRefreshToken? Issued)
    {
        public static RefreshRotationResult Failure(RefreshOutcome outcome) => new(outcome, null, null);
    }
}
