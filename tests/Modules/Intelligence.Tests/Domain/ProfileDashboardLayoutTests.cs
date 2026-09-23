using FluentAssertions;
using Intelligence.Domain.Dashboard;
using Intelligence.Domain.Entities;
using Intelligence.Domain;

namespace Intelligence.Tests.Domain;

public class ProfileDashboardLayoutTests
{
    [Fact]
    public void Create_RejectsUnknownWidget()
    {
        var slots = new[]
        {
            new DashboardWidgetSlot { WidgetKey = "NotAWidget", IsVisible = true, SortOrder = 0 }
        };

        var result = ProfileDashboardLayout.Create(Guid.NewGuid(), slots);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.DashboardLayout.UnknownWidget);
    }

    [Fact]
    public void Create_RejectsDuplicateWidget()
    {
        var slots = new[]
        {
            new DashboardWidgetSlot { WidgetKey = WidgetCatalog.SalesToday, IsVisible = true, SortOrder = 0 },
            new DashboardWidgetSlot { WidgetKey = WidgetCatalog.SalesToday, IsVisible = false, SortOrder = 1 }
        };

        var result = ProfileDashboardLayout.Create(Guid.NewGuid(), slots);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.DashboardLayout.DuplicateWidget);
    }
}
