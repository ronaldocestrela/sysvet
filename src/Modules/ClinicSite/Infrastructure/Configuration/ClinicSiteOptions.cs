namespace ClinicSite.Infrastructure.Configuration;

/// <summary>
/// Configuration for public clinic site URLs.
/// </summary>
public sealed class ClinicSiteOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "ClinicSite";

    /// <summary>Base domain for subdomain sites (e.g. vetnexus.app).</summary>
    public string BaseDomain { get; set; } = "vetnexus.app";
}
