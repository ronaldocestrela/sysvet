using FluentAssertions;
using Inventory.Domain;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;

namespace Inventory.Tests.Domain;

public class InventoryCountTests
{
    [Fact]
    public void Start_WithValidCode_Succeeds()
    {
        var result = InventoryCount.Start("INV-20260918-A1B2");
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(InventoryCountStatus.InProgress);
        result.Value.Lines.Should().BeEmpty();
    }

    [Fact]
    public void AddOrIncrementLine_SameProductLot_IncrementsQuantity()
    {
        var session = InventoryCount.Start("INV-TEST-001").Value;
        var productId = Guid.NewGuid();
        var lotId = Guid.NewGuid();

        session.AddOrIncrementLine(productId, lotId, 2m).IsSuccess.Should().BeTrue();
        session.AddOrIncrementLine(productId, lotId, 3m).IsSuccess.Should().BeTrue();

        session.Lines.Should().HaveCount(1);
        session.Lines[0].CountedQuantity.Should().Be(5m);
    }

    [Fact]
    public void Submit_ComputesVariance()
    {
        var session = InventoryCount.Start("INV-TEST-002").Value;
        var productId = Guid.NewGuid();
        var lineId = session.AddOrIncrementLine(productId, null, 8m).Value;

        var submit = session.Submit(new Dictionary<Guid, decimal> { [lineId] = 10m });
        submit.IsSuccess.Should().BeTrue();
        session.Status.Should().Be(InventoryCountStatus.Submitted);
        session.Lines[0].ExpectedQuantity.Should().Be(10m);
        session.Lines[0].Variance.Should().Be(-2m);
    }

    [Fact]
    public void Submit_WithoutLines_Fails()
    {
        var session = InventoryCount.Start("INV-TEST-003").Value;
        session.Submit(new Dictionary<Guid, decimal>()).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Approve_FromSubmitted_Succeeds()
    {
        var session = InventoryCount.Start("INV-TEST-004").Value;
        var lineId = session.AddOrIncrementLine(Guid.NewGuid(), null, 5m).Value;
        session.Submit(new Dictionary<Guid, decimal> { [lineId] = 5m });

        session.Approve().IsSuccess.Should().BeTrue();
        session.Status.Should().Be(InventoryCountStatus.Approved);
    }

    [Fact]
    public void Approve_FromInProgress_Fails()
    {
        var session = InventoryCount.Start("INV-TEST-005").Value;
        session.AddOrIncrementLine(Guid.NewGuid(), null, 1m);
        session.Approve().IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Cancel_FromInProgress_Succeeds()
    {
        var session = InventoryCount.Start("INV-TEST-006").Value;
        session.Cancel().IsSuccess.Should().BeTrue();
        session.Status.Should().Be(InventoryCountStatus.Cancelled);
    }
}
