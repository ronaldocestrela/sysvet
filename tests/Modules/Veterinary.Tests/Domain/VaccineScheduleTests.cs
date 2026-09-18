using FluentAssertions;
using Veterinary.Domain.Services;

namespace Veterinary.Tests.Domain;

public class VaccineScheduleTests
{
    [Fact]
    public void ComputeNextDueDate_WhenIntervalProvided_ReturnsAppliedAtPlusDays()
    {
        var appliedAt = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

        var next = VaccineSchedule.ComputeNextDueDate(appliedAt, nextDoseIntervalInDays: 365);

        next.Should().Be(appliedAt.AddDays(365));
    }

    [Fact]
    public void ComputeNextDueDate_WhenNoInterval_ReturnsNull()
    {
        var appliedAt = DateTimeOffset.UtcNow;

        VaccineSchedule.ComputeNextDueDate(appliedAt, null).Should().BeNull();
    }

    [Fact]
    public void ClassifyAlert_WhenPastDue_ReturnsOverdue()
    {
        var utcNow = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var nextDue = utcNow.AddDays(-1);

        VaccineSchedule.ClassifyAlert(nextDue, utcNow, horizonDays: 7).Should().Be(VaccineAlertKind.Overdue);
    }

    [Fact]
    public void ClassifyAlert_WhenWithinHorizon_ReturnsUpcoming()
    {
        var utcNow = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var nextDue = utcNow.AddDays(3);

        VaccineSchedule.ClassifyAlert(nextDue, utcNow, horizonDays: 7).Should().Be(VaccineAlertKind.Upcoming);
    }

    [Fact]
    public void ClassifyAlert_WhenBeyondHorizon_ReturnsNone()
    {
        var utcNow = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var nextDue = utcNow.AddDays(30);

        VaccineSchedule.ClassifyAlert(nextDue, utcNow, horizonDays: 7).Should().Be(VaccineAlertKind.None);
    }

    [Fact]
    public void GetAgeInDays_WhenBirthDateMissing_ReturnsNull()
    {
        VaccineSchedule.GetAgeInDays(null, DateOnly.FromDateTime(DateTime.UtcNow)).Should().BeNull();
    }

    [Fact]
    public void IsAgeEligible_WhenWithinRange_ReturnsTrue()
    {
        VaccineSchedule.IsAgeEligible(ageInDays: 60, minAgeInDays: 42, maxAgeInDays: 90).Should().BeTrue();
    }

    [Fact]
    public void IsAgeEligible_WhenBelowMin_ReturnsFalse()
    {
        VaccineSchedule.IsAgeEligible(ageInDays: 10, minAgeInDays: 42, maxAgeInDays: null).Should().BeFalse();
    }
}
