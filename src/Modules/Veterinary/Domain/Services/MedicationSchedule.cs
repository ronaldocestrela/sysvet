namespace Veterinary.Domain.Services;

/// <summary>Pure helpers to expand daily medication times into UTC administration slots.</summary>
public static class MedicationSchedule
{
    /// <summary>Maximum inclusive calendar days for a single medication order.</summary>
    public const int MaxOrderDays = 14;

    /// <summary>
    /// Expands daily times between <paramref name="startsOn"/> and <paramref name="endsOn"/> (inclusive) using UTC midnight boundaries.
    /// </summary>
    public static IReadOnlyList<DateTimeOffset> ExpandOccurrences(
        DateOnly startsOn,
        DateOnly endsOn,
        IReadOnlyList<TimeOnly> timesOfDay)
    {
        if (timesOfDay.Count == 0 || endsOn < startsOn)
        {
            return Array.Empty<DateTimeOffset>();
        }

        var daySpan = endsOn.DayNumber - startsOn.DayNumber;
        if (daySpan + 1 > MaxOrderDays)
        {
            return Array.Empty<DateTimeOffset>();
        }

        var results = new List<DateTimeOffset>();
        for (var dayOffset = 0; dayOffset <= daySpan; dayOffset++)
        {
            var day = startsOn.AddDays(dayOffset);
            foreach (var time in timesOfDay.OrderBy(t => t))
            {
                results.Add(new DateTimeOffset(day.ToDateTime(time, DateTimeKind.Utc)));
            }
        }

        return results;
    }

    /// <summary>Returns whether a scheduled instant falls on the given UTC calendar day.</summary>
    public static bool IsOnUtcDay(DateTimeOffset scheduledAt, DateOnly utcDay) =>
        DateOnly.FromDateTime(scheduledAt.UtcDateTime) == utcDay;
}
