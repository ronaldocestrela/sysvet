using System.Text.RegularExpressions;
using Core.Domain;

namespace Platform.Domain.ValueObjects;

/// <summary>
/// Normalized slug for tenant resolution via host or <c>X-Tenant-Slug</c> header.
/// </summary>
public static partial class TenantSlug
{
    private const int MinLength = 2;
    private const int MaxLength = 63;

    [GeneratedRegex("^[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?$", RegexOptions.CultureInvariant)]
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

        return raw.Trim().ToLowerInvariant().Replace(" ", "-");
    }

    /// <summary>
    /// Validates normalized slug length and character rules.
    /// </summary>
    public static bool IsValid(string normalized) =>
        !string.IsNullOrEmpty(normalized)
        && normalized.Length is >= MinLength and <= MaxLength
        && SlugPattern().IsMatch(normalized);

    /// <summary>
    /// Creates a validated slug or returns a domain error.
    /// </summary>
    public static Result<string> Create(string? raw)
    {
        var normalized = Normalize(raw);
        if (!IsValid(normalized))
        {
            return Result.Failure<string>(ErrorCodes.Tenant.InvalidSlug);
        }

        return Result.Success(normalized);
    }
}
