using Core.Domain;
using Core.Domain.Auditing;
using FluentAssertions;
using Inventory.Application.Common;
using Inventory.Application.StockMovements.Commands;
using Inventory.Domain;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.Repositories;
using NSubstitute;

namespace Inventory.Tests.Application;

public class RegisterStockMovementCommandHandlerTests
{
    [Fact]
    public async Task Handle_OutOnLot_ReducesLotQuantityAndBalance()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create("P", "D", "SKU-P", "7891234567890", "U", 5m, ProductCategory.Medication, "23091000", null, 0, null, id: productId).Value;
        var lot = ProductLot.Create(productId, "LOT-1", null, 10m, 20m).Value;

        var productRepository = Substitute.For<IProductRepository>();
        var lotRepository = Substitute.For<IProductLotRepository>();
        var movementRepository = Substitute.For<IStockMovementRepository>();
        var tenantContext = Substitute.For<ITenantContext>();
        var auditLogger = Substitute.For<IAuditLogger>();

        productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(product);
        lotRepository.ListByProductIdAsync(productId, Arg.Any<CancellationToken>()).Returns(new[] { lot });
        lotRepository.GetByIdAsync(lot.Id, Arg.Any<CancellationToken>()).Returns(lot);

        var balance = new ProductBalance(productId, 20m);
        productRepository.GetBalanceAsync(productId, Arg.Any<CancellationToken>()).Returns(balance);

        var ledgerWriter = new StockLedgerWriter(
            productRepository,
            lotRepository,
            movementRepository,
            new StockCatalogReconciler(productRepository, lotRepository),
            tenantContext,
            auditLogger);
        var handler = new RegisterStockMovementCommandHandler(ledgerWriter);

        var command = new RegisterStockMovementCommand(
            productId,
            MovementType.Out,
            5m,
            StockMovementReasons.Sale,
            lot.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        lot.Quantity.Should().Be(15m);
        movementRepository.Received(1).Add(Arg.Any<StockMovement>());
    }

    [Fact]
    public async Task Handle_LegacyInWithoutLots_UpdatesBalance()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create("P", "D", "SKU-L", "7891234567891", "U", 0, ProductCategory.Other, "23091000", null, 0, null, requiresLot: false, id: productId).Value;

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
        var handler = new RegisterStockMovementCommandHandler(ledgerWriter);

        var command = new RegisterStockMovementCommand(productId, MovementType.In, 5m, StockMovementReasons.Purchase);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        balance.TotalQuantity.Should().Be(15m);
    }
}
