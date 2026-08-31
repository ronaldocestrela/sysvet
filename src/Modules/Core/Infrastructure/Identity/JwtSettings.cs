using System.ComponentModel.DataAnnotations;

namespace Core.Infrastructure.Identity;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    [Required]
    [MinLength(16)]
    public string Secret { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Range(1, 10080)]
    public int ExpiryMinutes { get; set; } = 60;
}
