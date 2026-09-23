using System.ComponentModel.DataAnnotations;

namespace Core.Infrastructure.Configuration;

/// <summary>
/// Distributed cache provider for query results and entitlements (ADR-057).
/// </summary>
public sealed class CacheOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Cache";

    /// <summary><c>Memory</c> (dev/CI) or <c>Redis</c> (staging/production).</summary>
    [Required]
    public string Provider { get; set; } = "Memory";

    /// <summary>StackExchange.Redis connection string when <see cref="Provider"/> is Redis.</summary>
    public string? ConnectionString { get; set; }

    /// <summary>Returns true when Redis should be used.</summary>
    public bool UseRedis => string.Equals(Provider, "Redis", StringComparison.OrdinalIgnoreCase);
}
