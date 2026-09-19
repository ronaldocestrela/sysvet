using FluentAssertions;
using Sales.Domain.Entities;
using Sales.Domain.Enums;

namespace Sales.Tests.Domain;

public class CashRegisterTests
{
    [Fact]
    public void Open_WithClientId_UsesProvidedId()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var result = CashRegister.Open(id, userId, 50m);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(id);
        result.Value.OpenedByUserId.Should().Be(userId);
        result.Value.Status.Should().Be(CashRegisterStatus.Open);
        result.Value.OpeningBalance.Amount.Should().Be(50m);
    }

    [Fact]
    public void Open_WithEmptyClientId_ReturnsFailure()
    {
        var result = CashRegister.Open(Guid.Empty, Guid.NewGuid(), 0m);

        result.IsFailure.Should().BeTrue();
    }
}
