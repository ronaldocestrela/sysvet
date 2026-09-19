using Core.Domain;
using FluentAssertions;
using NSubstitute;
using Sales.Application.CashRegisters.Queries;
using Sales.Domain.Entities;
using Sales.Domain.Enums;
using Sales.Domain.Repositories;

namespace Sales.Tests.Application;

public class GetOpenCashRegisterQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenNoOpenRegister_ReturnsNullDto()
    {
        var cashRepo = Substitute.For<ICashRegisterRepository>();
        var orderRepo = Substitute.For<IOrderRepository>();
        var tenant = Substitute.For<ITenantContext>();
        tenant.UserId.Returns(Guid.NewGuid());
        cashRepo.GetOpenCashRegisterByUserAsync(tenant.UserId, Arg.Any<CancellationToken>()).Returns((CashRegister?)null);

        var handler = new GetOpenCashRegisterQueryHandler(cashRepo, orderRepo, tenant);
        var result = await handler.Handle(new GetOpenCashRegisterQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenOpen_IncludesCashSalesInCurrentBalance()
    {
        var userId = Guid.NewGuid();
        var register = CashRegister.Open(userId, 40m).Value;
        var cashRepo = Substitute.For<ICashRegisterRepository>();
        var orderRepo = Substitute.For<IOrderRepository>();
        var tenant = Substitute.For<ITenantContext>();
        tenant.UserId.Returns(userId);
        cashRepo.GetOpenCashRegisterByUserAsync(userId, Arg.Any<CancellationToken>()).Returns(register);
        orderRepo.GetPaymentTotalsForCashRegisterAsync(register.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Sales.Domain.Queries.CashRegisterPaymentTotals>
            {
                new() { Method = PaymentMethod.Cash, Gross = 25m, Refunded = 0m }
            });

        var handler = new GetOpenCashRegisterQueryHandler(cashRepo, orderRepo, tenant);
        var result = await handler.Handle(new GetOpenCashRegisterQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(register.Id);
        result.Value.OpeningBalance.Should().Be(40m);
        result.Value.CurrentBalance.Should().Be(65m);
        result.Value.Status.Should().Be("Open");
    }
}
