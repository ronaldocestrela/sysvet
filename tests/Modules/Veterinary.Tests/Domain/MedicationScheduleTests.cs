using FluentAssertions;
using Veterinary.Domain.Services;

namespace Veterinary.Tests.Domain;

public class MedicationScheduleTests
{
    [Fact]
    public void ExpandOccurrences_WithTwoDaysAndTwoTimes_ReturnsFourSlots()
    {
        var starts = new DateOnly(2026, 9, 18);
        var ends = new DateOnly(2026, 9, 19);
        var times = new[] { new TimeOnly(8, 0), new TimeOnly(20, 0) };

        var slots = MedicationSchedule.ExpandOccurrences(starts, ends, times);

        slots.Should().HaveCount(4);
        slots[0].UtcDateTime.Should().Be(new DateTime(2026, 9, 18, 8, 0, 0, DateTimeKind.Utc));
        slots[3].UtcDateTime.Should().Be(new DateTime(2026, 9, 19, 20, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void ExpandOccurrences_WhenExceedsMaxDays_ReturnsEmpty()
    {
        var starts = new DateOnly(2026, 9, 1);
        var ends = new DateOnly(2026, 9, 20);
        var times = new[] { new TimeOnly(8, 0) };

        MedicationSchedule.ExpandOccurrences(starts, ends, times).Should().BeEmpty();
    }

    [Fact]
    public void IsOnUtcDay_MatchesScheduledInstant()
    {
        var day = new DateOnly(2026, 9, 18);
        var at = new DateTimeOffset(new DateTime(2026, 9, 18, 14, 30, 0, DateTimeKind.Utc));
        MedicationSchedule.IsOnUtcDay(at, day).Should().BeTrue();
    }
}
