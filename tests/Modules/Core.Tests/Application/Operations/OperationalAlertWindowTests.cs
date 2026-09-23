using Core.Application.Operations;
using FluentAssertions;
using Xunit;

namespace Core.Tests.Application.Operations;

public class OperationalAlertWindowTests
{
    [Fact]
    public void Record_ShouldRaiseOnce_WhenThresholdCrossed()
    {
        var window = new OperationalAlertWindow(TimeSpan.FromMinutes(1), 3);
        var now = DateTimeOffset.Parse("2026-09-23T12:00:00Z");

        window.Record(now).Should().BeFalse();
        window.Record(now.AddSeconds(1)).Should().BeFalse();
        window.Record(now.AddSeconds(2)).Should().BeTrue();
        window.Record(now.AddSeconds(3)).Should().BeFalse();
    }

    [Fact]
    public void Record_ShouldResetAfterWindowExpires()
    {
        var window = new OperationalAlertWindow(TimeSpan.FromMinutes(1), 2);
        var start = DateTimeOffset.Parse("2026-09-23T12:00:00Z");

        window.Record(start).Should().BeFalse();
        window.Record(start.AddSeconds(1)).Should().BeTrue();
        window.IsAboveThreshold(start.AddMinutes(2)).Should().BeFalse();
        window.Record(start.AddMinutes(2)).Should().BeFalse();
        window.Record(start.AddMinutes(2).AddSeconds(1)).Should().BeTrue();
    }
}
