namespace Platform.Domain.Services;

/// <summary>
/// Resolves civil calendar month bounds in the VetNexus business timezone (roadmap 10.2).
/// </summary>
public static class SaasCivilMonthRange
{
    /// <summary>Timezone used for SaaS metrics month boundaries.</summary>
    public const string TimeZoneId = "America/Sao_Paulo";

    /// <summary>Returns UTC bounds for a civil month (inclusive start, exclusive end).</summary>
    public static (DateTimeOffset StartUtc, DateTimeOffset EndUtc) ForMonth(int year, int month)
    {
        var timeZone = ResolveTimeZone();
        var localStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var startUtc = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localStart, timeZone));
        var nextMonth = month == 12
            ? new DateTime(year + 1, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)
            : new DateTime(year, month + 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var endUtc = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(nextMonth, timeZone));
        return (startUtc, endUtc);
    }

    /// <summary>True when <paramref name="instantUtc"/> falls inside [<paramref name="startUtc"/>, <paramref name="endUtc"/>).</summary>
    public static bool Contains(DateTimeOffset instantUtc, DateTimeOffset startUtc, DateTimeOffset endUtc) =>
        instantUtc >= startUtc && instantUtc < endUtc;

    /// <summary>Resolves the configured timezone, falling back to UTC when unavailable.</summary>
    public static TimeZoneInfo ResolveTimeZone()
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
}
