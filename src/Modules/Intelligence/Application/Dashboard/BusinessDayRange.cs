namespace Intelligence.Application.Dashboard;

/// <summary>
/// Resolves the operational business day in a fixed IANA timezone (roadmap 10.1).
/// </summary>
public static class BusinessDayRange
{
    /// <summary>Clinic business timezone for dashboard KPIs.</summary>
    public const string TimeZoneId = "America/Sao_Paulo";

    /// <summary>
    /// Returns UTC bounds and local calendar date for the business day containing <paramref name="instantUtc"/>.
    /// </summary>
    public static (DateTimeOffset StartUtc, DateTimeOffset EndUtc, DateOnly BusinessDate) ForInstant(DateTimeOffset instantUtc)
    {
        var timeZone = ResolveTimeZone();
        var local = TimeZoneInfo.ConvertTime(instantUtc, timeZone);
        var businessDate = DateOnly.FromDateTime(local.DateTime);
        var localMidnight = new DateTime(businessDate.Year, businessDate.Month, businessDate.Day, 0, 0, 0, DateTimeKind.Unspecified);
        var startUtc = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localMidnight, timeZone));
        return (startUtc, startUtc.AddDays(1), businessDate);
    }

    /// <summary>Resolves the configured business timezone, falling back to UTC when unavailable.</summary>
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
