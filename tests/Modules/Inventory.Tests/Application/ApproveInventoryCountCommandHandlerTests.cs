using Core.Domain;
using Core.Domain.Auditing;
using FluentAssertions;
using Inventory.Application.Common;
using Inventory.Application.InventoryCounts;
using Inventory.Application.InventoryCounts.Commands;
using Inventory.Domain;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.Repositories;
using NSubstitute;

namespace Inventory.Tests.Application;

public class ApproveInventoryCountCommandHandlerTests
{
    [Fact]
    public async Task Handle_SubmittedSession_AppliesAdjustmentToMatchCountedQuantity()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create("Count", "D", "SKU-CNT", "7891234567891", "U", 0m, ProductCategory.Medication, "23091000", null, 0, null, requiresLot: false, id: productId).Value;
        var session = InventoryCount.Start("INV-TEST-APPROVE").Value;
        var lineId = session.AddOrIncrementLine(productId, null, 8m).Value;
        session.Submit(new Dictionary<Guid, decimal> { [lineId] = 10m });

        var countRepository = Substitute.For<IInventoryCountRepository>();
        countRepository.GetByIdWithLinesAsync(session.Id, Arg.Any<CancellationToken>()).Returns(session);

        var productRepository = Substitute.For<IProductRepository>();
        var lotRepository = Substitute.For<IProductLotRepository>();
        var movementRepository = Substitute.For<IStockMovementRepository>();
        var tenantContext = Substitute.For<ITenantContext>();
        var auditLogger = Substitute.For<IAuditLogger>();

        productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(product);
        lotRepository.ListByProductIdAsync(productId, Arg.Any<CancellationToken>()).Returns(Array.Empty<ProductLot>());
        var balance = new ProductBalance(productId, 10m);
        productRepository.GetBalanceAsync(productId, Arg.Any<CancellationToken>()).Returns(balance);

        var ledgerWriter = new StockLedgerWriter(
            productRepository,
            lotRepository,
            movementRepository,
            new StockCatalogReconciler(productRepository, lotRepository),
            tenantContext,
            auditLogger);

        var handler = new ApproveInventoryCountCommandHandler(
            countRepository,
            new InventoryCountOnHandResolver(productRepository, lotRepository),
            ledgerWriter,
            tenantContext,
            auditLogger);

        var result = await handler.Handle(new ApproveInventoryCountCommand(session.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        session.Status.Should().Be(InventoryCountStatus.Approved);
        balance.TotalQuantity.Should().Be(8m);
        movementRepository.Received(1).Add(Arg.Is<StockMovement>(m =>
            m.Reason == StockMovementReasons.InventoryCount && m.Type == MovementType.Adjustment));
    }
}
