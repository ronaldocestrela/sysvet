using Core.Application.Entitlements;
using Core.Application.IntegrationEvents;
using Core.Domain;
using Core.Domain.Entitlements;
using FluentAssertions;
using Intelligence.Application.Reports;
using MediatR;
using NSubstitute;

namespace Intelligence.Tests.Application;

public class IntelligenceReportComposerTests
{
    [Fact]
    public async Task BuildAbcCustomersAsync_ClassifiesAndResolvesNames()
    {
        var tutorId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetSalesReportAggregatesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new SalesReportAggregatesSnapshot
            {
                Customers = [new CustomerRevenueAggregateRow(tutorId, 100m)]
            }));
        mediator.Send(Arg.Any<GetTutorDisplayNamesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyDictionary<Guid, string>>(
                new Dictionary<Guid, string> { [tutorId] = "Tutor A" }));

        var entitlements = Substitute.For<ITenantEntitlementReader>();
        var composer = new IntelligenceReportComposer(mediator, entitlements);

        var from = new DateOnly(2026, 3, 1);
        var to = new DateOnly(2026, 3, 31);
        var result = await composer.BuildAbcCustomersAsync(from, to, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Rows.Should().ContainSingle();
        result.Value.Rows[0].DisplayName.Should().Be("Tutor A");
        result.Value.Rows[0].Class.Should().Be(Intelligence.Domain.Reports.AbcClass.A);
    }
}
