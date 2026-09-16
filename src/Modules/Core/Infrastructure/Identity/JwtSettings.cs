using System.ComponentModel.DataAnnotations;

namespace Core.Infrastructure.Identity;

/// <summary>
/// JWT signing and validation parameters bound from configuration (never hardcoded in production).
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Configuration section name for JWT settings.
    /// </summary>
    public const string SectionName = "JwtSettings";

    /// <summary>
    /// Symmetric signing key; must meet minimum length for HMAC-SHA256.
    /// </summary>
    [Required]
    [MinLength(16)]
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Token issuer claim (<c>iss</c>).
    /// </summary>
    [Required]
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Token audience claim (<c>aud</c>).
    /// </summary>
    [Required]
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Access token lifetime in minutes.
    /// </summary>
    [Range(1, 10080)]
    public int ExpiryMinutes { get; set; } = 60;

    /// <summary>
    /// Refresh token lifetime in days.
    /// </summary>
    [Range(1, 365)]
    public int RefreshExpiryDays { get; set; } = 7;
}
