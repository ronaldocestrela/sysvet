namespace Automations.Infrastructure.Reminders;

/// <summary>
/// Converts between UTC instants and tenant-local calendar dates.
/// </summary>
internal static class ReminderTimeHelper
{
    public static DateOnly ToLocalDate(DateTimeOffset instant, TimeZoneInfo timeZone) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(instant, timeZone).DateTime);

    public static DateTimeOffset DayStartUtc(DateOnly localDay, TimeZoneInfo timeZone)
    {
        var localMidnight = localDay.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        var utc = TimeZoneInfo.ConvertTimeToUtc(localMidnight, timeZone);
        return new DateTimeOffset(utc, TimeSpan.Zero);
    }

    public static bool IsBirthdayToday(DateOnly? birthDate, DateOnly localToday)
    {
        if (birthDate is null)
        {
            return false;
        }

        if (birthDate.Value.Month == localToday.Month && birthDate.Value.Day == localToday.Day)
        {
            return true;
        }

        return birthDate.Value is { Month: 2, Day: 29 }
               && localToday is { Month: 2, Day: 28 }
               && !DateTime.IsLeapYear(localToday.Year);
    }
}
