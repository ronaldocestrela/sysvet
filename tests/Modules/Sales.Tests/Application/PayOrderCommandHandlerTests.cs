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
using Sales.Domain.Repositories;
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
        var order = Order.Create(register.Id, Guid.NewGuid()).Value;
        order.AddProductItem(Guid.NewGuid(), "P", 1m, 10m);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var orderRepo = new Sales.Infrastructure.Persistence.Repositories.OrderRepository(context);
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ConsumeStockForSaleRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var publisher = Substitute.For<IPublisher>();
        var terminal = new SimulatedPaymentTerminal();
        var commissionRules = Substitute.For<ICommissionRuleRepository>();
        commissionRules.ListAllAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<CommissionRule>());
        var kitRepo = Substitute.For<IProductKitRepository>();
        var packageRepo = Substitute.For<IServicePackageRepository>();
        var prepaidRepo = Substitute.For<IPrepaidBalanceRepository>();
        var handler = new PayOrderCommandHandler(
            orderRepo,
            commissionRules,
            kitRepo,
            packageRepo,
            prepaidRepo,
            terminal,
            publisher,
            mediator);

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

    [Fact]
    public async Task Handle_PackageLine_CreditsPrepaidBalance()
    {
        var options = new DbContextOptionsBuilder<SalesDbContext>()
            .UseSqlite($"Data Source=file:sales-pkg-{Guid.NewGuid():N}?mode=memory&cache=shared")
            .Options;

        await using var context = new SalesDbContext(options, new TestTenantContext());
        await context.Database.OpenConnectionAsync();
        await context.Database.MigrateAsync();

        var package = ServicePackage.Create("Banho 4x", ServiceCode.Banho, 4).Value;
        context.ServicePackages.Add(package);

        var register = CashRegister.Open(Guid.NewGuid(), 0m).Value;
        context.CashRegisters.Add(register);
        var tutorId = Guid.NewGuid();
        var petId = Guid.NewGuid();
        var order = Order.Create(
            cashRegisterId: register.Id,
            sellerUserId: Guid.NewGuid(),
            tutorId: tutorId,
            petId: petId).Value;
        var addLine = order.AddPackageItem(package.Id, package.Name, 1m, 120m);
        addLine.IsSuccess.Should().BeTrue(addLine.IsFailure ? addLine.Error.Code : "ok");
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var orderRepo = new Sales.Infrastructure.Persistence.Repositories.OrderRepository(context);
        var kitRepo = new Sales.Infrastructure.Persistence.Repositories.ProductKitRepository(context);
        var packageRepo = new Sales.Infrastructure.Persistence.Repositories.ServicePackageRepository(context);
        var prepaidRepo = new Sales.Infrastructure.Persistence.Repositories.PrepaidBalanceRepository(context);
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ConsumeStockForSaleRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var commissionRules = Substitute.For<ICommissionRuleRepository>();
        commissionRules.ListAllAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<CommissionRule>());
        var handler = new PayOrderCommandHandler(
            orderRepo,
            commissionRules,
            kitRepo,
            packageRepo,
            prepaidRepo,
            new SimulatedPaymentTerminal(),
            Substitute.For<IPublisher>(),
            mediator);

        var result = await handler.Handle(new PayOrderCommand
        {
            OrderId = order.Id,
            Payments = [new PayOrderPaymentDto { Method = PaymentMethod.Cash, Amount = 120m }]
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.IsFailure ? result.Error.Code : "ok");
        await context.SaveChangesAsync();
        var balance = await context.PrepaidBalances.FirstAsync(b => b.PetId == petId);
        balance.RemainingUses.Should().Be(4);
    }

    private sealed class TestTenantContext : ITenantContext
    {
        public Guid TenantId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; } = Guid.NewGuid();
        public string SchemaName { get; set; } = "dbo";
    }
}
