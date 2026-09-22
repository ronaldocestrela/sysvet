using Automations.Application.Abstractions;
using Automations.Application.Reminders;
using Automations.Domain.Repositories;
using Automations.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Automations.Infrastructure.Reminders;

/// <summary>
/// Runs all reminder sources and persists enqueued jobs for the current tenant.
/// </summary>
public sealed class ReminderScanService
{
    private readonly IEnumerable<IReminderCandidateSource> _sources;
    private readonly ReminderPlanner _planner;
    private readonly IAutomationsSettingsRepository _settingsRepository;
    private readonly IAutomationsUnitOfWork _unitOfWork;
    private readonly AutomationsOptions _options;

    public ReminderScanService(
        IEnumerable<IReminderCandidateSource> sources,
        ReminderPlanner planner,
        IAutomationsSettingsRepository settingsRepository,
        IAutomationsUnitOfWork unitOfWork,
        IOptions<AutomationsOptions> options)
    {
        _sources = sources;
        _planner = planner;
        _settingsRepository = settingsRepository;
        _unitOfWork = unitOfWork;
        _options = options.Value;
    }

    /// <summary>
    /// Collects candidates for the tenant-local today and enqueues outbound jobs.
    /// </summary>
    public async Task<int> ScanAsync(CancellationToken cancellationToken)
    {
        var tz = await ResolveTimeZoneAsync(cancellationToken);
        var localToday = ReminderTimeHelper.ToLocalDate(DateTimeOffset.UtcNow, tz);
        var all = new List<ReminderCandidate>();
        foreach (var source in _sources)
        {
            all.AddRange(await source.CollectAsync(localToday, cancellationToken));
        }

        var enqueued = await _planner.EnqueueAsync(all, cancellationToken);
        if (enqueued > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return enqueued;
    }

    private async Task<TimeZoneInfo> ResolveTimeZoneAsync(CancellationToken cancellationToken)
    {
        var settings = await _settingsRepository.GetSingletonAsync(cancellationToken);
        if (settings is not null)
        {
            return settings.ResolveTimeZone();
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(_options.TimeZoneId);
        }
        catch
        {
            return TimeZoneInfo.Utc;
        }
    }
}
