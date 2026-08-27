using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.Master.Admin
{
    /// <summary>
    /// M2-A04 — one issued refresh token. Lives in the tenant database, exactly like <see cref="User"/>
    /// (database-per-tenant, so tenant isolation is structural: a token issued in one tenant's
    /// database can never be looked up from another tenant's <c>ApplicationDbContext</c>).
    ///
    /// <para>The raw token is never stored. <see cref="TokenHash"/> is a SHA-256 digest (hex, 64
    /// chars) of the cryptographically random value actually handed to the caller — the same
    /// "never persist the secret itself" shape as password hashing, but a fast digest rather than
    /// PBKDF2, because the raw value is already 256 bits of random entropy and not a
    /// human-chosen secret that needs slowing down against guessing.</para>
    ///
    /// <para>Rotation and revocation are both expressed through <see cref="RevokedAtUtc"/>: a
    /// refresh sets it on the presented row (one-time use) while issuing a brand-new row; logout
    /// sets it directly. There is no separate "used" flag — a non-null <see cref="RevokedAtUtc"/>
    /// means the token can never mint another access token again, regardless of why.</para>
    /// </summary>
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        /// <summary>SHA-256 digest of the raw token, hex-encoded (64 chars). Indexed, unique.</summary>
        [Required]
        [StringLength(64)]
        public string TokenHash { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; }

        public DateTime ExpiresAtUtc { get; set; }

        /// <summary>Null while live. Set on rotation (this row was presented and replaced) or on
        /// logout (this row was explicitly revoked). Either way, never usable again.</summary>
        public DateTime? RevokedAtUtc { get; set; }
    }
}
