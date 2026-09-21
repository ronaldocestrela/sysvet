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
    public void Close_WhenOpen_SetsClosedStatusAndExpected()
    {
        var register = CashRegister.Open(Guid.NewGuid(), 100m).Value;

        var result = register.Close(120m, cashNet: 20m);

        result.IsSuccess.Should().BeTrue();
        register.Status.Should().Be(CashRegisterStatus.Closed);
        register.ExpectedClosingBalance.Amount.Should().Be(120m);
        register.ClosingBalance.Amount.Should().Be(120m);
        register.ClosingVariance.Should().Be(0m);
        register.ClosedAt.Should().NotBeNull();
    }

    [Fact]
    public void Close_WhenAlreadyClosed_ReturnsFailure()
    {
        var register = CashRegister.Open(Guid.NewGuid(), 10m).Value;
        register.Close(10m, 0m);

        register.Close(10m, 0m).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void RecordDrop_SubtractsFromExpectedCash()
    {
        var register = CashRegister.Open(Guid.NewGuid(), 100m).Value;

        register.RecordDrop(30m, "Cofre", cashNet: 50m).IsSuccess.Should().BeTrue();

        register.ComputeExpectedCash(50m).Should().Be(120m);
    }

    [Fact]
    public void RecordDrop_WhenExceedsExpected_ReturnsFailure()
    {
        var register = CashRegister.Open(Guid.NewGuid(), 100m).Value;

        var result = register.RecordDrop(200m, "Cofre", cashNet: 0m);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CashRegister.InsufficientCash");
    }

    [Fact]
    public void RecordSupply_AddsToExpectedCash()
    {
        var register = CashRegister.Open(Guid.NewGuid(), 100m).Value;

        register.RecordSupply(10m, "Troco").IsSuccess.Should().BeTrue();

        register.ComputeExpectedCash(50m).Should().Be(160m);
    }

    [Fact]
    public void Close_WithDropAndSupply_MatchesPlanFormula()
    {
        var register = CashRegister.Open(Guid.NewGuid(), 100m).Value;
        register.RecordDrop(30m, "Sangria", cashNet: 50m);
        register.RecordSupply(10m, "Suprimento");

        register.ComputeExpectedCash(50m).Should().Be(130m);
        register.Close(125m, 50m).IsSuccess.Should().BeTrue();
        register.ClosingVariance.Should().Be(-5m);
    }

    [Fact]
    public void RestoreFromSync_RehydratesClosedRegisterWithMovements()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var openedAt = DateTimeOffset.UtcNow.AddHours(-2);
        var closedAt = DateTimeOffset.UtcNow;
        var movement = CashMovement.Restore(
            Guid.NewGuid(),
            id,
            CashMovementKind.Drop,
            20m,
            "Sync",
            openedAt.AddMinutes(30),
            openedAt.AddMinutes(30));

        var restored = CashRegister.RestoreFromSync(
            id,
            userId,
            openedAt,
            closedAt,
            50m,
            80m,
            78m,
            CashRegisterStatus.Closed,
            closedAt,
            [movement]);

        restored.Id.Should().Be(id);
        restored.Status.Should().Be(CashRegisterStatus.Closed);
        restored.ClosingBalance.Amount.Should().Be(78m);
        restored.ExpectedClosingBalance.Amount.Should().Be(80m);
        restored.Movements.Should().HaveCount(1);
    }

    [Fact]
    public void Open_WithEmptyClientId_ReturnsFailure()
    {
        var result = CashRegister.Open(Guid.Empty, Guid.NewGuid(), 0m);

        result.IsFailure.Should().BeTrue();
    }
}
