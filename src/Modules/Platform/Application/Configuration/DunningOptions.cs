using Platform.Domain.Services;

namespace Platform.Application.Configuration;

/// <summary>SaaS dunning and operational lock settings (9.5).</summary>
public sealed class DunningOptions
{
    /// <summary>Configuration section: Platform:Dunning.</summary>
    public const string SectionName = "Platform:Dunning";

    /// <summary>Fake (CI/dev) or Live notifier.</summary>
    public string Provider { get; set; } = "Fake";

    /// <summary>Days after past-due before operational lock.</summary>
    public int LockAfterDays { get; set; } = 7;

    /// <summary>Maximum automatic credit-card retries.</summary>
    public int CardMaxRetries { get; set; } = 3;

    /// <summary>Comma-separated notice day offsets (default 0,3,7).</summary>
    public string NoticeDays { get; set; } = "0,3,7";

    /// <summary>SMTP host for Live e-mail notices.</summary>
    public string? SmtpHost { get; set; }

    /// <summary>SMTP port.</summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>SMTP username.</summary>
    public string? SmtpUser { get; set; }

    /// <summary>SMTP password.</summary>
    public string? SmtpPassword { get; set; }

    /// <summary>From address for dunning e-mail.</summary>
    public string? SmtpFrom { get; set; }

    /// <summary>Optional SMS gateway base URL.</summary>
    public string? SmsBaseUrl { get; set; }

    /// <summary>Parses notice day offsets.</summary>
    public int[] ParseNoticeDays()
    {
        if (string.IsNullOrWhiteSpace(NoticeDays))
        {
            return DunningScheduleCalculator.DefaultNoticeDays;
        }

        return NoticeDays
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => int.TryParse(s, out var day) ? day : -1)
            .Where(d => d >= 0)
            .Distinct()
            .OrderBy(d => d)
            .ToArray();
    }
}
