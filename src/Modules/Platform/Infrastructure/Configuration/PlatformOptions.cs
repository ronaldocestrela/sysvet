namespace Platform.Infrastructure.Configuration;

/// <summary>Platform module configuration (optional connection override).</summary>
public sealed class PlatformOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Platform";

    /// <summary>Optional module-specific connection string override.</summary>
    public string? ConnectionString { get; set; }
}
