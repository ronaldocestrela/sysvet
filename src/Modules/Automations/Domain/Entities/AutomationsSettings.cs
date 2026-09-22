using Automations.Domain.ValueObjects;
using Core.Domain;

namespace Automations.Domain.Entities;

/// <summary>
/// Singleton tenant settings for Automations (business hours and time zone).
/// </summary>
public sealed class AutomationsSettings : AggregateRoot
{
    public const string SingletonKey = "default";

    public string Key { get; private set; } = SingletonKey;
    public string TimeZoneId { get; private set; } = "America/Sao_Paulo";
    public TimeOnly BusinessStart { get; private set; }
    public TimeOnly BusinessEnd { get; private set; }
    public string BusinessDaysJson { get; private set; } = "[]";

    private AutomationsSettings() { }

    /// <summary>
    /// Creates default settings for a new tenant schema.
    /// </summary>
    public static Result<AutomationsSettings> CreateDefault(Guid? id = null)
    {
        var settings = new AutomationsSettings
        {
            Id = id ?? Guid.NewGuid(),
            BusinessStart = new TimeOnly(8, 0),
            BusinessEnd = new TimeOnly(18, 0),
            BusinessDaysJson = SerializeDays(
            [
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday,
                DayOfWeek.Saturday
            ])
        };

        return Result.Success(settings);
    }

    /// <summary>
    /// Applies business window and time zone from validated input.
    /// </summary>
    public Result Update(string timeZoneId, BusinessHours hours)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return Result.Failure(ErrorCodes.Settings.InvalidBusinessHours);
        }

        TimeZoneId = timeZoneId.Trim();
        BusinessStart = hours.Start;
        BusinessEnd = hours.End;
        BusinessDaysJson = SerializeDays(hours.Days);
        return Result.Success();
    }

    /// <summary>
    /// Parses stored JSON into a <see cref="BusinessHours"/> value object.
    /// </summary>
    public Result<BusinessHours> ToBusinessHours()
    {
        var days = ParseDays(BusinessDaysJson);
        return BusinessHours.Create(BusinessStart, BusinessEnd, days);
    }

    /// <summary>
    /// Resolves the configured time zone or falls back to UTC.
    /// </summary>
    public TimeZoneInfo ResolveTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.Utc;
        }
    }

    private static string SerializeDays(IEnumerable<DayOfWeek> days) =>
        "[" + string.Join(",", days.Select(d => (int)d)) + "]";

    private static IEnumerable<DayOfWeek> ParseDays(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "[]")
        {
            return Array.Empty<DayOfWeek>();
        }

        var trimmed = json.Trim('[', ']');
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return Array.Empty<DayOfWeek>();
        }

        return trimmed.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => (DayOfWeek)int.Parse(s));
    }
}
