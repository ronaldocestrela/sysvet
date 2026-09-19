using Core.Domain;
using Core.Domain.Auditing;
using FluentAssertions;
using Inventory.Application.Common;
using Inventory.Application.StockMovements.Commands;
using Inventory.Domain;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.Repositories;
using MediatR;
using NSubstitute;

namespace Inventory.Tests.Application;

public class StockLossAndReturnCommandHandlerTests
{
    [Fact]
    public async Task RegisterStockLoss_ReducesLotAndUsesMovementReason()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create("P", "D", "SKU-P", "7891234567890", "U", 5m, ProductCategory.Medication, "30049099", null, 0, null, id: productId).Value;
        var lot = ProductLot.Create(productId, "LOT-1", null, 10m, 20m).Value;

        var productRepository = Substitute.For<IProductRepository>();
        var lotRepository = Substitute.For<IProductLotRepository>();
        var movementRepository = Substitute.For<IStockMovementRepository>();
        var tenantContext = Substitute.For<ITenantContext>();
        var auditLogger = Substitute.For<IAuditLogger>();

        productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(product);
        lotRepository.ListByProductIdAsync(productId, Arg.Any<CancellationToken>()).Returns(new[] { lot });
        lotRepository.GetByIdAsync(lot.Id, Arg.Any<CancellationToken>()).Returns(lot);
        productRepository.GetBalanceAsync(productId, Arg.Any<CancellationToken>()).Returns(new ProductBalance(productId, 20m));

        var handler = new RegisterStockLossCommandHandler(new StockLedgerWriter(
            productRepository,
            lotRepository,
            movementRepository,
            new StockCatalogReconciler(productRepository, lotRepository),
            tenantContext,
            auditLogger));

        var result = await handler.Handle(
            new RegisterStockLossCommand(productId, lot.Id, 4m, StockLossReasons.Expired, "Lote vencido"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        lot.Quantity.Should().Be(16m);
        movementRepository.Received(1).Add(Arg.Is<StockMovement>(m =>
            m.Reason == StockMovementReasons.LossExpired && m.Type == MovementType.Out));
    }

    [Fact]
    public async Task RegisterStockLoss_InvalidReason_Fails()
    {
        var handler = new RegisterStockLossCommandHandler(null!);
        var result = await handler.Handle(
            new RegisterStockLossCommand(Guid.NewGuid(), null, 1m, "NotAllowed", null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("StockMovement.InvalidLossReason");
    }

    [Fact]
    public async Task FractionatePackage_CreatesFractionalLotAndKeepsTotal()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create("P", "", "SKU", "7891234567890", "UN", 0, ProductCategory.Medication, "30049099", null, 0, null, unitsPerPackage: 10m, id: productId).Value;
        var sealedLot = ProductLot.Create(productId, "LOT-A", null, 5m, 30m).Value;

        var productRepository = Substitute.For<IProductRepository>();
        var lotRepository = Substitute.For<IProductLotRepository>();
        ProductLot? fractionalLot = null;
        var movementRepository = Substitute.For<IStockMovementRepository>();
        var transferHandler = new TransferStockCommandHandler(
            productRepository,
            lotRepository,
            movementRepository,
            new StockCatalogReconciler(productRepository, lotRepository),
            Substitute.For<ITenantContext>(),
            Substitute.For<IAuditLogger>());

        productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(product);
        lotRepository.GetByIdAsync(sealedLot.Id, Arg.Any<CancellationToken>()).Returns(sealedLot);
        lotRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var id = call.Arg<Guid>();
                if (id == sealedLot.Id)
                {
                    return sealedLot;
                }

                return fractionalLot is not null && id == fractionalLot.Id ? fractionalLot : null;
            });
        lotRepository.GetByProductAndLotNumberAsync(productId, "LOT-A-F", Arg.Any<CancellationToken>()).Returns((ProductLot?)null);
        lotRepository.When(r => r.Add(Arg.Any<ProductLot>())).Do(call => fractionalLot = call.Arg<ProductLot>());

        var handler = new FractionatePackageCommandHandler(productRepository, lotRepository, transferHandler);

        var result = await handler.Handle(
            new FractionatePackageCommand(productId, sealedLot.Id, 2m),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        lotRepository.Received(1).Add(Arg.Is<ProductLot>(l => l.IsFractional && l.LotNumber == "LOT-A-F"));
        sealedLot.Quantity.Should().Be(10m);
    }

    [Fact]
    public async Task RegisterSupplierReturn_PublishesIntegrationEvent()
    {
        var productId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var product = Product.Create("P", "", "SKU", "7891234567890", "UN", 0, ProductCategory.Medication, "30049099", null, 0, supplierId, id: productId).Value;
        var lot = ProductLot.Create(productId, "LOT-1", null, 8m, 10m).Value;
        var supplier = Supplier.Create("Fornecedor SA", "Forn", "12.345.678/0001-95", null, null).Value;

        var productRepository = Substitute.For<IProductRepository>();
        var lotRepository = Substitute.For<IProductLotRepository>();
        var movementRepository = Substitute.For<IStockMovementRepository>();
        var supplierRepository = Substitute.For<ISupplierRepository>();
        var tenantContext = Substitute.For<ITenantContext>();
        var auditLogger = Substitute.For<IAuditLogger>();
        var publisher = Substitute.For<IPublisher>();

        productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(product);
        lotRepository.ListByProductIdAsync(productId, Arg.Any<CancellationToken>()).Returns(new[] { lot });
        lotRepository.GetByIdAsync(lot.Id, Arg.Any<CancellationToken>()).Returns(lot);
        supplierRepository.GetByIdAsync(supplierId, Arg.Any<CancellationToken>()).Returns(supplier);
        productRepository.GetBalanceAsync(productId, Arg.Any<CancellationToken>()).Returns(new ProductBalance(productId, 10m));

        var handler = new RegisterSupplierReturnCommandHandler(
            productRepository,
            lotRepository,
            supplierRepository,
            new StockLedgerWriter(
                productRepository,
                lotRepository,
                movementRepository,
                new StockCatalogReconciler(productRepository, lotRepository),
                tenantContext,
                auditLogger),
            publisher);

        var result = await handler.Handle(
            new RegisterSupplierReturnCommand(productId, lot.Id, 3m, supplierId, "Devolução"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await publisher.Received(1).Publish(Arg.Any<Core.Application.IntegrationEvents.SupplierReturnRegisteredEvent>(), Arg.Any<CancellationToken>());
    }
}
