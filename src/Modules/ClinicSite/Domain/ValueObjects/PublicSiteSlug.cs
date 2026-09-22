using System.Text.RegularExpressions;

namespace ClinicSite.Domain.ValueObjects;

/// <summary>
/// Normalized subdomain slug for public clinic sites ({slug}.vetnexus.app).
/// </summary>
public static partial class PublicSiteSlug
{
    private const int MinLength = 3;
    private const int MaxLength = 63;

    [GeneratedRegex("^[a-z0-9](?:[a-z0-9-]{1,61}[a-z0-9])?$", RegexOptions.CultureInvariant)]
    private static partial Regex SlugPattern();

    /// <summary>
    /// Normalizes user input to lowercase slug form.
    /// </summary>
    public static string Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        var trimmed = raw.Trim().ToLowerInvariant();
        return trimmed.Replace(" ", "-");
    }

    /// <summary>
    /// Validates normalized slug length and character rules.
    /// </summary>
    public static bool IsValid(string normalized)
    {
        if (string.IsNullOrEmpty(normalized) || normalized.Length is < MinLength or > MaxLength)
        {
            return false;
        }

        return SlugPattern().IsMatch(normalized);
    }
}
