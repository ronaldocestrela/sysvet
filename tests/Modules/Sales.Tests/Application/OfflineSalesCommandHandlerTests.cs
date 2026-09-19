using Core.Application.Common.Interfaces;
using Core.Application.IntegrationEvents;
using Core.Domain;
using Core.Domain.Entities;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Sales.Application.CashRegisters.Commands;
using Sales.Application.Orders.Commands;
using Sales.Domain.Entities;
using Sales.Domain.Enums;
using Sales.Domain.Payments;
using Sales.Domain.Repositories;
using Sales.Infrastructure.Persistence;

namespace Sales.Tests.Application;

public class OfflineSalesCommandHandlerTests
{
    [Fact]
    public async Task CreateOrder_WithExistingClientId_ReturnsExistingWithoutDuplicate()
    {
        await using var context = await CreateContextAsync();
        var register = CashRegister.Open(Guid.NewGuid(), Guid.NewGuid(), 0m).Value;
        context.CashRegisters.Add(register);
        var orderId = Guid.NewGuid();
        var existing = Order.Create(orderId, register.Id, Guid.NewGuid()).Value;
        existing.AddProductItem(Guid.NewGuid(), "P", 1m, 5m);
        context.Orders.Add(existing);
        await context.SaveChangesAsync();

        var handler = CreateCreateHandler(context);
        var result = await handler.Handle(new CreateOrderCommand
        {
            OrderId = orderId,
            CashRegisterId = register.Id,
            Items = [new CreateOrderItemDto { Kind = OrderItemKind.Product, ProductId = Guid.NewGuid(), ProductName = "X", Quantity = 1, UnitPrice = 99m }]
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(orderId);
        (await context.Orders.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task PayOrder_WhenAlreadyPaid_ReturnsSuccessWithoutRepublishing()
    {
        await using var context = await CreateContextAsync();
        var register = CashRegister.Open(Guid.NewGuid(), 0m).Value;
        context.CashRegisters.Add(register);
        var order = Order.Create(register.Id, Guid.NewGuid()).Value;
        order.AddProductItem(Guid.NewGuid(), "P", 1m, 10m);
        order.Pay([Payment.Create(PaymentMethod.Cash, 10m).Value]);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var orderRepo = new Sales.Infrastructure.Persistence.Repositories.OrderRepository(context);
        var mediator = Substitute.For<IMediator>();
        var publisher = Substitute.For<IPublisher>();
        var commissionRules = Substitute.For<ICommissionRuleRepository>();
        commissionRules.ListAllAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<CommissionRule>());
        var handler = new PayOrderCommandHandler(orderRepo, commissionRules, new SimulatedPaymentTerminal(), publisher, mediator);

        var result = await handler.Handle(new PayOrderCommand
        {
            OrderId = order.Id,
            Payments = [new PayOrderPaymentDto { Method = PaymentMethod.Cash, Amount = 10m }]
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await mediator.DidNotReceive().Send(Arg.Any<ConsumeStockForSaleRequest>(), Arg.Any<CancellationToken>());
        await publisher.DidNotReceive().Publish(Arg.Any<INotification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task OpenCashRegister_WithExistingClientId_ReturnsExisting()
    {
        await using var context = await CreateContextAsync();
        var userId = Guid.NewGuid();
        var registerId = Guid.NewGuid();
        var register = CashRegister.Open(registerId, userId, 100m).Value;
        context.CashRegisters.Add(register);
        await context.SaveChangesAsync();

        var cashRepo = new Sales.Infrastructure.Persistence.Repositories.CashRegisterRepository(context);
        var tenant = new TestTenantContext { UserId = userId };
        var handler = new OpenCashRegisterCommandHandler(cashRepo, tenant);

        var result = await handler.Handle(new OpenCashRegisterCommand
        {
            CashRegisterId = registerId,
            OpeningBalance = 200m
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(registerId);
        (await context.CashRegisters.CountAsync()).Should().Be(1);
    }

    private static CreateOrderCommandHandler CreateCreateHandler(SalesDbContext context)
    {
        var orderRepo = new Sales.Infrastructure.Persistence.Repositories.OrderRepository(context);
        var cashRepo = new Sales.Infrastructure.Persistence.Repositories.CashRegisterRepository(context);
        var tutorRepo = Substitute.For<ITutorRepository>();
        var petRepo = Substitute.For<IPetRepository>();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.IsAuthenticated.Returns(true);
        currentUser.UserId.Returns(Guid.NewGuid().ToString());
        currentUser.AccessProfileId.Returns(Guid.NewGuid());
        var accessProfiles = Substitute.For<IAccessProfileRepository>();
        accessProfiles.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(AccessProfile.CreateSystem("Admin", "Admin", Core.Domain.Authorization.Permissions.AdminDefaults(), 100m).Value);
        return new CreateOrderCommandHandler(orderRepo, cashRepo, tutorRepo, petRepo, accessProfiles, currentUser);
    }

    private static async Task<SalesDbContext> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<SalesDbContext>()
            .UseSqlite($"Data Source=file:sales-offline-{Guid.NewGuid():N}?mode=memory&cache=shared")
            .Options;
        var context = new SalesDbContext(options, new TestTenantContext());
        await context.Database.OpenConnectionAsync();
        await context.Database.MigrateAsync();
        return context;
    }

    private sealed class TestTenantContext : ITenantContext
    {
        public Guid TenantId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; } = Guid.NewGuid();
        public string SchemaName { get; set; } = "dbo";
    }
}
