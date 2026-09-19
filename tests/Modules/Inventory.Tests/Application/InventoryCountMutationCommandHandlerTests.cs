using Core.Domain;
using FluentAssertions;
using Inventory.Application.InventoryCounts.Commands;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using NSubstitute;

namespace Inventory.Tests.Application;

public class InventoryCountMutationCommandHandlerTests
{
    [Fact]
    public async Task UpdateLine_SetsCountedQuantity()
    {
        var session = InventoryCount.Start("INV-UPD-001").Value;
        var lineId = session.AddOrIncrementLine(Guid.NewGuid(), null, 2m).Value;
        var repo = Substitute.For<IInventoryCountRepository>();
        repo.GetByIdWithLinesAsync(session.Id, Arg.Any<CancellationToken>()).Returns(session);

        var handler = new UpdateInventoryCountLineCommandHandler(repo);
        var result = await handler.Handle(new UpdateInventoryCountLineCommand(session.Id, lineId, 9m), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        session.Lines.Single().CountedQuantity.Should().Be(9m);
    }

    [Fact]
    public async Task RemoveLine_RemovesCountLine()
    {
        var session = InventoryCount.Start("INV-RM-001").Value;
        var lineId = session.AddOrIncrementLine(Guid.NewGuid(), null, 2m).Value;
        var repo = Substitute.For<IInventoryCountRepository>();
        repo.GetByIdWithLinesAsync(session.Id, Arg.Any<CancellationToken>()).Returns(session);

        var handler = new RemoveInventoryCountLineCommandHandler(repo);
        var result = await handler.Handle(new RemoveInventoryCountLineCommand(session.Id, lineId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        session.Lines.Should().BeEmpty();
    }

    [Fact]
    public async Task Cancel_CancelsInProgressSession()
    {
        var session = InventoryCount.Start("INV-CAN-001").Value;
        var repo = Substitute.For<IInventoryCountRepository>();
        repo.GetByIdWithLinesAsync(session.Id, Arg.Any<CancellationToken>()).Returns(session);

        var handler = new CancelInventoryCountCommandHandler(repo);
        var result = await handler.Handle(new CancelInventoryCountCommand(session.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        session.Status.Should().Be(Inventory.Domain.Enums.InventoryCountStatus.Cancelled);
    }

    [Fact]
    public async Task Cancel_WhenMissing_ReturnsNotFound()
    {
        var repo = Substitute.For<IInventoryCountRepository>();
        repo.GetByIdWithLinesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((InventoryCount?)null);

        var handler = new CancelInventoryCountCommandHandler(repo);
        var result = await handler.Handle(new CancelInventoryCountCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("InventoryCount.NotFound");
    }
}
