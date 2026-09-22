namespace Automations.Infrastructure.Configuration;

/// <summary>
/// Automations worker, reminder scan, and provider tuning options.
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
    /// Outbound provider mode: Fake (CI/dev) or Live (SMTP + Evolution).
    /// </summary>
    public string Provider { get; set; } = "Fake";

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

    /// <summary>
    /// Interval between reminder candidate scans in minutes.
    /// </summary>
    public int ReminderScanIntervalMinutes { get; set; } = 15;

    /// <summary>
    /// IANA time zone for reminder date windows.
    /// </summary>
    public string TimeZoneId { get; set; } = "America/Sao_Paulo";

    /// <summary>
    /// Fallback business hours when DB settings are missing.
    /// </summary>
    public BusinessHoursOptions BusinessHours { get; set; } = new();

    /// <summary>
    /// SMTP settings for Live e-mail delivery.
    /// </summary>
    public SmtpOptions Smtp { get; set; } = new();

    /// <summary>
    /// Evolution API settings for Live WhatsApp delivery.
    /// </summary>
    public EvolutionOptions Evolution { get; set; } = new();
}

/// <summary>
/// Business hours fallback configuration.
/// </summary>
public sealed class BusinessHoursOptions
{
    public string Start { get; set; } = "08:00";
    public string End { get; set; } = "18:00";
    public List<string> Days { get; set; } =
    [
        "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"
    ];
}

/// <summary>
/// SMTP connection settings.
/// </summary>
public sealed class SmtpOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
}

/// <summary>
/// Evolution API connection settings.
/// </summary>
public sealed class EvolutionOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Instance { get; set; } = string.Empty;
}
