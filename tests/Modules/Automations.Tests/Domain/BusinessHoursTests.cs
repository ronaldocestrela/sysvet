using Automations.Domain.ValueObjects;
using FluentAssertions;

namespace Automations.Tests.Domain;

public class BusinessHoursTests
{
    private static readonly TimeZoneInfo SaoPaulo =
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "E. South America Standard Time" : "America/Sao_Paulo");

    [Fact]
    public void Create_WithInvalidRange_Fails()
    {
        var result = BusinessHours.Create(new TimeOnly(18, 0), new TimeOnly(8, 0), [DayOfWeek.Monday]);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void IsOpenAt_InsideWindow_ReturnsTrue()
    {
        var hours = BusinessHours.Create(new TimeOnly(8, 0), new TimeOnly(18, 0), [DayOfWeek.Monday]).Value;
        var mondayTenAm = new DateTimeOffset(2026, 9, 21, 13, 0, 0, TimeSpan.Zero);
        hours.IsOpenAt(mondayTenAm, SaoPaulo).Should().BeTrue();
    }

    [Fact]
    public void NextOpenInstant_BeforeOpen_ReturnsSameDayOpen()
    {
        var hours = BusinessHours.Create(new TimeOnly(8, 0), new TimeOnly(18, 0), [DayOfWeek.Monday]).Value;
        var early = new DateTimeOffset(2026, 9, 21, 10, 0, 0, TimeSpan.Zero);
        var next = hours.NextOpenInstant(early, SaoPaulo);
        next.Should().BeAfter(early.AddHours(-1));
    }
}
