using Core.Application.IntegrationEvents;
using FluentAssertions;
using Inventory.Application.Common;
using Inventory.Application.StockSales;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace Inventory.Tests.Application;

public class ConsumeStockForGroomingRequestHandlerTests
{
    [Fact]
    public async Task Handle_WhenAlreadyDebited_IsIdempotent()
    {
        var attendanceId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productRepo = Substitute.For<IProductRepository>();
        var lotRepo = Substitute.For<IProductLotRepository>();
        var movementRepo = Substitute.For<IStockMovementRepository>();
        movementRepo.ListByCorrelationIdAsync(attendanceId, Arg.Any<CancellationToken>())
            .Returns([StockMovement.Create(productId, MovementType.Out, 1m, null, null, "Grooming - Attendance x", correlationId: attendanceId).Value]);

        var handler = new ConsumeStockForGroomingRequestHandler(
            productRepo,
            lotRepo,
            movementRepo,
            new StockCatalogReconciler(productRepo, lotRepo));

        var result = await handler.Handle(
            new ConsumeStockForGroomingRequest(attendanceId, [new ConsumeStockForGroomingLine(productId, 1m)]),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        movementRepo.DidNotReceive().Add(Arg.Any<StockMovement>());
    }
}
