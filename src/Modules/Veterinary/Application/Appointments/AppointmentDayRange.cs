namespace Veterinary.Application.Appointments;

/// <summary>
/// Normalizes a <see cref="DateTimeOffset"/> to UTC day boundaries for agenda queries.
/// </summary>
internal static class AppointmentDayRange
{
    internal static (DateTimeOffset Start, DateTimeOffset End) ForDate(DateTimeOffset date)
    {
        var utcDay = DateTime.SpecifyKind(date.UtcDateTime.Date, DateTimeKind.Utc);
        var start = new DateTimeOffset(utcDay, TimeSpan.Zero);
        return (start, start.AddDays(1));
    }
}
