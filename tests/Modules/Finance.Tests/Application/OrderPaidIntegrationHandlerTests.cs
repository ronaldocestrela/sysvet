using Core.Application.IntegrationEvents;
using Finance.Application.Integration;
using Finance.Domain.Entities;
using Finance.Domain.Enums;
using Finance.Domain.Repositories;
using FluentAssertions;
using MediatR;
using NSubstitute;

namespace Finance.Tests.Application;

public class OrderPaidIntegrationHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateReceivableAndMarkLinked()
    {
        var orderId = Guid.NewGuid();
        var titleRepo = Substitute.For<IFinancialTitleRepository>();
        titleRepo.GetBySourceAsync(TitleSourceType.Sale, orderId, "-", Arg.Any<CancellationToken>())
            .Returns((FinancialTitle?)null);

        var categoryRepo = Substitute.For<IFinancialCategoryRepository>();
        categoryRepo.GetByCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((FinancialCategory?)null);

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<MarkOrderFinanceLinkedRequest>(), Arg.Any<CancellationToken>())
            .Returns(Core.Domain.Result.Success());

        var handler = new OrderPaidIntegrationHandler(titleRepo, categoryRepo, mediator);
        var evt = new OrderPaidEvent(
            orderId,
            null,
            100m,
            "Pending",
            Array.Empty<OrderPaidItem>(),
            [new OrderPaidPayment("Cash", 100m)]);

        await handler.Handle(evt, CancellationToken.None);

        titleRepo.Received(1).Add(Arg.Any<FinancialTitle>());
        await mediator.Received(1).Send(Arg.Is<MarkOrderFinanceLinkedRequest>(r => r.OrderId == orderId), Arg.Any<CancellationToken>());
    }
}
