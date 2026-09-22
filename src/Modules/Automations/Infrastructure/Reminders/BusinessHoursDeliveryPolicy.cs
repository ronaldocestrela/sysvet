using Automations.Application.Abstractions;
using Automations.Domain.Repositories;
using Automations.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Automations.Infrastructure.Reminders;

/// <summary>
/// Applies tenant DB settings with configuration fallback for business-hour gating.
/// </summary>
public sealed class BusinessHoursDeliveryPolicy : IAutomationsDeliveryPolicy
{
    private readonly IAutomationsSettingsRepository _settingsRepository;
    private readonly AutomationsOptions _options;

    public BusinessHoursDeliveryPolicy(
        IAutomationsSettingsRepository settingsRepository,
        IOptions<AutomationsOptions> options)
    {
        _settingsRepository = settingsRepository;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<DeliveryPolicyResult> EvaluateAsync(DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        var settings = await _settingsRepository.GetSingletonAsync(cancellationToken);
        if (settings is not null)
        {
            var hoursResult = settings.ToBusinessHours();
            if (hoursResult.IsSuccess)
            {
                var tz = settings.ResolveTimeZone();
                if (hoursResult.Value.IsOpenAt(now, tz))
                {
                    return new DeliveryPolicyResult(true, now);
                }

                return new DeliveryPolicyResult(false, hoursResult.Value.NextOpenInstant(now, tz));
            }
        }

        return EvaluateFromOptions(now);
    }

    private DeliveryPolicyResult EvaluateFromOptions(DateTimeOffset now)
    {
        var start = TimeOnly.Parse(_options.BusinessHours.Start);
        var end = TimeOnly.Parse(_options.BusinessHours.End);
        var days = _options.BusinessHours.Days.Select(Enum.Parse<DayOfWeek>).ToList();
        var hoursResult = Domain.ValueObjects.BusinessHours.Create(start, end, days);
        if (hoursResult.IsFailure)
        {
            return new DeliveryPolicyResult(true, now);
        }

        TimeZoneInfo tz;
        try
        {
            tz = TimeZoneInfo.FindSystemTimeZoneById(_options.TimeZoneId);
        }
        catch
        {
            tz = TimeZoneInfo.Utc;
        }

        if (hoursResult.Value.IsOpenAt(now, tz))
        {
            return new DeliveryPolicyResult(true, now);
        }

        return new DeliveryPolicyResult(false, hoursResult.Value.NextOpenInstant(now, tz));
    }
}
