using Core.Domain;
using FluentAssertions;
using NSubstitute;
using Sales.Application.CashRegisters.Commands;
using Sales.Domain.Entities;
using Sales.Domain.Repositories;

namespace Sales.Tests.Application;

public class CloseCashRegisterCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenRegisterIsOpen_ClosesIt()
    {
        var register = CashRegister.Open(Guid.NewGuid(), 100m).Value;
        var repo = Substitute.For<ICashRegisterRepository>();
        var orderRepo = Substitute.For<IOrderRepository>();
        repo.GetByIdAsync(register.Id, Arg.Any<CancellationToken>()).Returns(register);
        orderRepo.GetPaymentTotalsForCashRegisterAsync(register.Id, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Sales.Domain.Queries.CashRegisterPaymentTotals>());

        var handler = new CloseCashRegisterCommandHandler(repo, orderRepo);
        var result = await handler.Handle(new CloseCashRegisterCommand
        {
            CashRegisterId = register.Id,
            ActualClosingBalance = 150m
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        repo.Received(1).Update(register);
    }

    [Fact]
    public async Task Handle_WhenAlreadyClosed_ReturnsSuccessWithoutUpdate()
    {
        var register = CashRegister.Open(Guid.NewGuid(), 10m).Value;
        register.Close(10m, 0m);
        var repo = Substitute.For<ICashRegisterRepository>();
        repo.GetByIdAsync(register.Id, Arg.Any<CancellationToken>()).Returns(register);

        var orderRepo = Substitute.For<IOrderRepository>();
        var handler = new CloseCashRegisterCommandHandler(repo, orderRepo);
        var result = await handler.Handle(new CloseCashRegisterCommand
        {
            CashRegisterId = register.Id,
            ActualClosingBalance = 10m
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        repo.DidNotReceive().Update(Arg.Any<CashRegister>());
    }

    [Fact]
    public async Task Handle_WhenMissing_ReturnsNotFound()
    {
        var repo = Substitute.For<ICashRegisterRepository>();
        repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((CashRegister?)null);

        var orderRepo = Substitute.For<IOrderRepository>();
        var handler = new CloseCashRegisterCommandHandler(repo, orderRepo);
        var result = await handler.Handle(new CloseCashRegisterCommand
        {
            CashRegisterId = Guid.NewGuid(),
            ActualClosingBalance = 0m
        }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CashRegister.NotFound");
    }
}
