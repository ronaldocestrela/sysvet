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

public class RefundOrderPaymentCommandHandlerTests
{
    [Fact]
    public async Task Handle_CashRefund_PersistsRefundAndUpdatesOrder()
    {
        var options = new DbContextOptionsBuilder<SalesDbContext>()
            .UseSqlite($"Data Source=file:sales-refund-{Guid.NewGuid():N}?mode=memory&cache=shared")
            .Options;

        await using var context = new SalesDbContext(options, new TestTenantContext());
        await context.Database.OpenConnectionAsync();
        await context.Database.MigrateAsync();

        var register = CashRegister.Open(Guid.NewGuid(), 0m).Value;
        context.CashRegisters.Add(register);
        var order = Order.Create(register.Id, Guid.NewGuid()).Value;
        order.AddProductItem(Guid.NewGuid(), "P", 1m, 40m);
        order.Pay([Payment.Create(PaymentMethod.Cash, 40m).Value]);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var paymentId = order.Payments.Single().Id;
        var orderRepo = new Sales.Infrastructure.Persistence.Repositories.OrderRepository(context);
        var publisher = Substitute.For<IPublisher>();
        var handler = new RefundOrderPaymentCommandHandler(orderRepo, new SimulatedPaymentTerminal(), publisher);

        var result = await handler.Handle(new RefundOrderPaymentCommand
        {
            OrderId = order.Id,
            PaymentId = paymentId,
            Amount = 40m
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await context.SaveChangesAsync();

        var saved = await context.Orders
            .Include(o => o.Payments)
            .ThenInclude(p => p.Refunds)
            .FirstAsync(o => o.Id == order.Id);
        saved.Status.Should().Be(OrderStatus.Refunded);
        saved.Payments.Single().RemainingRefundable.Should().Be(0m);
    }

    private sealed class TestTenantContext : ITenantContext
    {
        public Guid TenantId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; } = Guid.NewGuid();
        public string SchemaName { get; set; } = "dbo";
    }
}
