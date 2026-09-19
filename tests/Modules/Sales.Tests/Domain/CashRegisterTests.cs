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
    public void Open_WithNegativeBalance_ReturnsFailure()
    {
        var result = CashRegister.Open(Guid.NewGuid(), -1m);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Money.InvalidAmount");
    }

    [Fact]
    public void Close_WhenOpen_SetsClosedStatus()
    {
        var register = CashRegister.Open(Guid.NewGuid(), 100m).Value;

        var result = register.Close(120m);

        result.IsSuccess.Should().BeTrue();
        register.Status.Should().Be(CashRegisterStatus.Closed);
        register.ClosingBalance.Amount.Should().Be(120m);
        register.ClosedAt.Should().NotBeNull();
    }

    [Fact]
    public void Close_WhenAlreadyClosed_ReturnsFailure()
    {
        var register = CashRegister.Open(Guid.NewGuid(), 10m).Value;
        register.Close(10m);

        register.Close(10m).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void RestoreFromSync_RehydratesClosedRegister()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var openedAt = DateTimeOffset.UtcNow.AddHours(-2);
        var closedAt = DateTimeOffset.UtcNow;

        var restored = CashRegister.RestoreFromSync(
            id,
            userId,
            openedAt,
            closedAt,
            50m,
            80m,
            CashRegisterStatus.Closed,
            closedAt);

        restored.Id.Should().Be(id);
        restored.Status.Should().Be(CashRegisterStatus.Closed);
        restored.ClosingBalance.Amount.Should().Be(80m);
    }

    [Fact]
    public void Open_WithEmptyClientId_ReturnsFailure()
    {
        var result = CashRegister.Open(Guid.Empty, Guid.NewGuid(), 0m);

        result.IsFailure.Should().BeTrue();
    }
}
