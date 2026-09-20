using FluentAssertions;
using Sales.Domain.Entities;
using Sales.Domain.Enums;

namespace Sales.Tests.Domain;

public class PrepaidBalanceTests
{
    private static PrepaidBalance CreateBalance()
    {
        var tutorId = Guid.NewGuid();
        var petId = Guid.NewGuid();
        return PrepaidBalance.Create(tutorId, petId, ServiceCode.Banho).Value;
    }

    [Fact]
    public void Credit_ThenConsume_DecrementsRemainingUses()
    {
        var balance = CreateBalance();
        var orderId = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();

        balance.Credit(orderId, orderItemId, 4).IsSuccess.Should().BeTrue();
        balance.RemainingUses.Should().Be(4);

        balance.Consume(ServiceCode.Banho, balance.PetId).IsSuccess.Should().BeTrue();
        balance.RemainingUses.Should().Be(3);
    }

    [Fact]
    public void Consume_WrongServiceCode_ReturnsFailure()
    {
        var balance = CreateBalance();
        balance.Credit(Guid.NewGuid(), Guid.NewGuid(), 1);

        var result = balance.Consume(ServiceCode.Tosa, balance.PetId);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Package.ServiceMismatch");
    }

    [Fact]
    public void ReverseCredit_WhenUsesAlreadyConsumed_ReturnsFailure()
    {
        var balance = CreateBalance();
        var orderItemId = Guid.NewGuid();
        balance.Credit(Guid.NewGuid(), orderItemId, 1);
        balance.Consume(ServiceCode.Banho, balance.PetId);

        var reverse = balance.ReverseCredit(orderItemId, 1);

        reverse.IsFailure.Should().BeTrue();
        reverse.Error.Code.Should().Be("Package.ReturnAfterConsumption");
    }
}
