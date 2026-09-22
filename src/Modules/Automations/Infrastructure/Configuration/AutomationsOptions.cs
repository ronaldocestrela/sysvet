namespace Automations.Infrastructure.Configuration;

/// <summary>
/// Automations worker and queue tuning options.
/// </summary>
public class AutomationsOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Automations";

    /// <summary>
    /// Optional connection string override for <see cref="Persistence.AutomationsDbContext"/>.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Worker polling interval in seconds.
    /// </summary>
    public int PollIntervalSeconds { get; set; } = 2;

    /// <summary>
    /// Maximum jobs processed per worker cycle.
    /// </summary>
    public int BatchSize { get; set; } = 20;

    /// <summary>
    /// Default max delivery attempts when not specified on the job.
    /// </summary>
    public int DefaultMaxAttempts { get; set; } = 5;
}
