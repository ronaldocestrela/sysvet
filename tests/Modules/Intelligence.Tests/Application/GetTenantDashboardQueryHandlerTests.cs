using Core.Application.Authorization;
using Core.Application.Common.Interfaces;
using Core.Application.Entitlements;
using Core.Application.IntegrationEvents;
using Core.Domain;
using Core.Domain.Authorization;
using Core.Domain.Entitlements;
using Core.Domain.Entities;
using FluentAssertions;
using Intelligence.Application.Dashboard.Queries;
using Intelligence.Domain.Repositories;
using MediatR;
using NSubstitute;

namespace Intelligence.Tests.Application;

public class GetTenantDashboardQueryHandlerTests
{
    [Fact]
    public async Task Handle_CashierLayout_ExcludesGroomingWidget()
    {
        var profile = AccessProfile.CreateSystem("Cashier", ApplicationRoles.Cashier, Permissions.CashierDefaults()).Value;

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.AccessProfileId.Returns(profile.Id);
        currentUser.TenantId.Returns(Guid.NewGuid());

        var accessProfiles = Substitute.For<IAccessProfileRepository>();
        accessProfiles.GetByIdAsync(profile.Id, Arg.Any<CancellationToken>()).Returns(profile);

        var layouts = Substitute.For<IProfileDashboardLayoutRepository>();
        layouts.GetByAccessProfileIdAsync(profile.Id, Arg.Any<CancellationToken>())
            .Returns((Intelligence.Domain.Entities.ProfileDashboardLayout?)null);

        var entitlements = Substitute.For<ITenantEntitlementReader>();
        entitlements.GetEnabledModulesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new HashSet<CommercialModule>(Enum.GetValues<CommercialModule>()));

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetSalesTodayKpisRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new SalesTodayKpiSnapshot { NetAmount = 50, OrderCount = 1, AverageTicket = 50 }));
        mediator.Send(Arg.Any<GetSalesByHourKpisRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new SalesByHourKpiSnapshot()));

        var handler = new GetTenantDashboardQueryHandler(
            currentUser,
            accessProfiles,
            layouts,
            entitlements,
            mediator);

        var result = await handler.Handle(new GetTenantDashboardQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Widgets.Select(w => w.Key).Should().NotContain("GroomingToday");
        result.Value.Widgets.Should().Contain(w => w.Key == "SalesToday");
    }
}
