using Core.Domain;
using Intelligence.Application.Dashboard;
using DomainErrorCodes = Intelligence.Domain.ErrorCodes;

namespace Intelligence.Application.Reports;

/// <summary>Validates civil report ranges and converts them to UTC bounds (10.3).</summary>
public static class ReportDateRange
{
    private const int MaxInclusiveDays = 366;

    /// <summary>Validates inclusive civil dates in the business timezone.</summary>
    public static Result Validate(DateOnly from, DateOnly to)
    {
        if (to < from)
        {
            return Result.Failure(DomainErrorCodes.Reports.InvalidDateRange);
        }

        var days = to.DayNumber - from.DayNumber + 1;
        if (days > MaxInclusiveDays)
        {
            return Result.Failure(DomainErrorCodes.Reports.RangeTooLarge);
        }

        return Result.Success();
    }

    /// <summary>Returns UTC half-open bounds for inclusive civil dates in America/Sao_Paulo.</summary>
    public static (DateTimeOffset StartUtc, DateTimeOffset EndUtc) ToUtcBounds(DateOnly from, DateOnly to)
    {
        var timeZone = BusinessDayRange.ResolveTimeZone();
        var startLocal = new DateTime(from.Year, from.Month, from.Day, 0, 0, 0, DateTimeKind.Unspecified);
        var endLocal = new DateTime(to.Year, to.Month, to.Day, 0, 0, 0, DateTimeKind.Unspecified).AddDays(1);
        var startUtc = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(startLocal, timeZone));
        var endUtc = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(endLocal, timeZone));
        return (startUtc, endUtc);
    }
}
