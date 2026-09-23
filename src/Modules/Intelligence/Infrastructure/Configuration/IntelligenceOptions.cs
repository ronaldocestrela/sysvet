namespace Intelligence.Infrastructure.Configuration;

/// <summary>Intelligence module configuration.</summary>
public class IntelligenceOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Intelligence";

    /// <summary>Optional connection string override for <see cref="Persistence.IntelligenceDbContext"/>.</summary>
    public string? ConnectionString { get; set; }
}
