namespace Core.Infrastructure.Identity;

/// <summary>
/// Persisted refresh token metadata; only a hash of the token is stored.
/// </summary>
public class UserRefreshToken
{
    /// <summary>
    /// Primary key.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ASP.NET Identity user identifier.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// SHA-256 hash of the refresh token (Base64).
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// UTC expiry instant.
    /// </summary>
    public DateTime ExpiresAtUtc { get; set; }

    /// <summary>
    /// When the token was revoked (rotation or logout), if applicable.
    /// </summary>
    public DateTime? RevokedAtUtc { get; set; }

    /// <summary>
    /// Identifier of the token that replaced this one during rotation.
    /// </summary>
    public Guid? ReplacedByTokenId { get; set; }

    /// <summary>
    /// Navigation to the user (optional for queries).
    /// </summary>
    public AppUser? User { get; set; }
}
