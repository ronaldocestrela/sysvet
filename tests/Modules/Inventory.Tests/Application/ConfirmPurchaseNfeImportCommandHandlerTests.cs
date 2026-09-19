using Core.Domain;
using Core.Domain.Auditing;
using FluentAssertions;
using Inventory.Application.Common;
using Inventory.Application.PurchaseImports.Commands;
using Inventory.Application.PurchaseImports;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.Repositories;
using Inventory.Domain.ValueObjects;
using Inventory.Tests.Fixtures;
using MediatR;
using NSubstitute;
using Core.Application.Storage;

namespace Inventory.Tests.Application;

public class ConfirmPurchaseNfeImportCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithDraftImport_PostsStockMovement()
    {
        var importId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var accessKey = AccessKey.Create("35250900000000000000550010000000011000000001").Value;
        var line = PurchaseInvoiceImportLine.Create(importId, 1, "P1", "7891234567890", "Item", "23091000", "UN", 10m, 5m, 50m, null, null, lineId);
        var import = PurchaseInvoiceImport.CreateDraft(
            accessKey,
            "Legal",
            "Trade",
            "11222333000181",
            "1",
            "1",
            DateTimeOffset.UtcNow,
            50m,
            "blob/key",
            [line],
            null,
            importId).Value;

        var importRepository = Substitute.For<IPurchaseInvoiceImportRepository>();
        importRepository.GetByIdWithLinesAsync(importId, Arg.Any<CancellationToken>()).Returns(import);

        var supplierRepository = Substitute.For<ISupplierRepository>();
        supplierRepository.GetByDocumentAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Supplier.Create("L", "T", "11222333000181", null, null).Value);

        var productRepository = Substitute.For<IProductRepository>();
        productRepository.GetBySkuAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Product?)null);
        productRepository.GetByBarcodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Product?)null);

        var lotRepository = Substitute.For<IProductLotRepository>();
        lotRepository.ListByProductIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Array.Empty<ProductLot>());

        var movementRepository = Substitute.For<IStockMovementRepository>();
        var mappingRepository = Substitute.For<ISupplierProductMappingRepository>();
        mappingRepository.GetBySupplierAndCodeAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((SupplierProductMapping?)null);

        var tenantContext = Substitute.For<ITenantContext>();
        var auditLogger = Substitute.For<IAuditLogger>();
        var blobStorage = Substitute.For<IBlobStorage>();
        await using var xml = File.OpenRead(NfeFixturePaths.Resolve("sample-nfeProc.xml"));
        blobStorage.OpenReadAsync("blob/key", Arg.Any<CancellationToken>()).Returns(Result.Success<Stream>(xml));

        var publisher = Substitute.For<IPublisher>();
        var reconciler = new StockCatalogReconciler(productRepository, lotRepository);

        var handler = new ConfirmPurchaseNfeImportCommandHandler(
            importRepository,
            supplierRepository,
            productRepository,
            lotRepository,
            movementRepository,
            mappingRepository,
            reconciler,
            blobStorage,
            tenantContext,
            auditLogger,
            publisher);

        var command = new ConfirmPurchaseNfeImportCommand(
            importId,
            new ConfirmSupplierAction(SupplierConfirmMode.CreateFromEmitter, null),
            [
                new ConfirmLineAction(
                    lineId,
                    LineConfirmMode.CreateNew,
                    null,
                    new CreateProductPayload("SKU1", "7891111222333", ProductCategory.Food, false, 1m, "Item"))
            ]);

        var result = await handler.Handle(command, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        movementRepository.Received(1).Add(Arg.Any<StockMovement>());
    }
}
