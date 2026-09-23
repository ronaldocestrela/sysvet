using Bunit;
using Clients.Infrastructure.Crm;
using Clients.Infrastructure.Intelligence;
using Core.Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SharedUI.Pages;
using SharedUI.Services;
using Xunit;

namespace Clients.Tests.SharedUI;

public class DashboardPageTests : BunitContext
{
    public DashboardPageTests()
    {
        var intelligence = Substitute.For<IIntelligenceApiService>();
        intelligence.GetDashboardAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(new TenantDashboardClientDto
            {
                BusinessDate = new DateOnly(2026, 9, 23),
                Widgets =
                [
                    new DashboardWidgetClientDto
                    {
                        Key = "SalesToday",
                        IsSuccess = true,
                        Data = new SalesTodayClientDto { NetAmount = 4321m, OrderCount = 2, AverageTicket = 2160.5m }
                    }
                ]
            }));

        var vaccines = Substitute.For<IVaccineStore>();
        vaccines.CountOverdueAlertsAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(0));

        Services.AddSingleton(intelligence);
        Services.AddSingleton<IIntelligenceApiService>(intelligence);
        Services.AddSingleton(vaccines);

        var connectivity = Substitute.For<IConnectivityService>();
        connectivity.Status.Returns(ConnectivityStatus.Online);
        Services.AddSingleton(connectivity);
    }

    [Fact]
    public void Dashboard_RendersSalesValue_FromApi()
    {
        var cut = Render<Dashboard>();

        cut.Markup.Should().MatchRegex("4[.,]?321");
        cut.Markup.Should().NotContain("124");
    }
}
