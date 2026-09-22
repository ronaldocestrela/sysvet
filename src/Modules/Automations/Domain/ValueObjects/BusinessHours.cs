using Core.Domain;

namespace Automations.Domain.ValueObjects;

/// <summary>
/// Tenant business window used to defer outbound jobs outside allowed days and times.
/// </summary>
public sealed record BusinessHours : ValueObject
{
    public TimeOnly Start { get; }
    public TimeOnly End { get; }
    public IReadOnlyList<DayOfWeek> Days { get; }

    private BusinessHours(TimeOnly start, TimeOnly end, IReadOnlyList<DayOfWeek> days)
    {
        Start = start;
        End = end;
        Days = days;
    }

    /// <summary>
    /// Creates business hours when start is strictly before end and at least one weekday is allowed.
    /// </summary>
    public static Result<BusinessHours> Create(TimeOnly start, TimeOnly end, IEnumerable<DayOfWeek> days)
    {
        var dayList = days.Distinct().ToList();
        if (dayList.Count == 0 || start >= end)
        {
            return Result.Failure<BusinessHours>(ErrorCodes.Settings.InvalidBusinessHours);
        }

        return Result.Success(new BusinessHours(start, end, dayList));
    }

    /// <summary>
    /// Whether the instant falls inside an allowed day and time in the given time zone.
    /// </summary>
    public bool IsOpenAt(DateTimeOffset instant, TimeZoneInfo timeZone)
    {
        var local = TimeZoneInfo.ConvertTime(instant, timeZone);
        if (!Days.Contains(local.DayOfWeek))
        {
            return false;
        }

        var time = TimeOnly.FromDateTime(local.DateTime);
        return time >= Start && time < End;
    }

    /// <summary>
    /// Returns the next UTC instant when the window opens at or after <paramref name="instant"/>.
    /// </summary>
    public DateTimeOffset NextOpenInstant(DateTimeOffset instant, TimeZoneInfo timeZone)
    {
        for (var i = 0; i < 8; i++)
        {
            var candidate = instant.AddDays(i);
            var local = TimeZoneInfo.ConvertTime(candidate, timeZone);
            if (!Days.Contains(local.DayOfWeek))
            {
                continue;
            }

            var openLocal = local.Date + Start.ToTimeSpan();
            var openUtc = TimeZoneInfo.ConvertTimeToUtc(openLocal, timeZone);
            if (openUtc >= instant)
            {
                return new DateTimeOffset(openUtc, TimeSpan.Zero);
            }

            var time = TimeOnly.FromDateTime(local.DateTime);
            if (time >= Start && time < End)
            {
                return instant;
            }
        }

        return instant.AddDays(1);
    }
}
