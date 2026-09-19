using Core.Application.IntegrationEvents;
using Core.Domain;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Sales.Application.Orders.Commands;
using Sales.Domain.Entities;
using Sales.Domain.Enums;
using Sales.Domain.Payments;
using Sales.Infrastructure.Persistence;

namespace Sales.Tests.Application;

public class PayOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithSuccessfulStockDebit_MarksOrderPaid()
    {
        var options = new DbContextOptionsBuilder<SalesDbContext>()
            .UseSqlite($"Data Source=file:sales-pay-{Guid.NewGuid():N}?mode=memory&cache=shared")
            .Options;

        await using var context = new SalesDbContext(options, new TestTenantContext());
        await context.Database.OpenConnectionAsync();
        await context.Database.MigrateAsync();

        var register = CashRegister.Open(Guid.NewGuid(), 0m).Value;
        context.CashRegisters.Add(register);
        var order = Order.Create(register.Id).Value;
        order.AddProductItem(Guid.NewGuid(), "P", 1m, 10m);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var orderRepo = new Sales.Infrastructure.Persistence.Repositories.OrderRepository(context);
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ConsumeStockForSaleRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var publisher = Substitute.For<IPublisher>();
        var terminal = new SimulatedPaymentTerminal();
        var handler = new PayOrderCommandHandler(orderRepo, terminal, publisher, mediator);

        var result = await handler.Handle(new PayOrderCommand
        {
            OrderId = order.Id,
            Payments = [new PayOrderPaymentDto { Method = PaymentMethod.Cash, Amount = 10m }]
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await context.SaveChangesAsync();

        var saved = await context.Orders.FirstAsync(o => o.Id == order.Id);
        saved.Status.Should().Be(OrderStatus.Paid);
        saved.FinanceIntegrationStatus.Should().Be(FinanceIntegrationStatus.Pending);
    }

    private sealed class TestTenantContext : ITenantContext
    {
        public Guid TenantId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; } = Guid.NewGuid();
        public string SchemaName { get; set; } = "dbo";
    }
}
