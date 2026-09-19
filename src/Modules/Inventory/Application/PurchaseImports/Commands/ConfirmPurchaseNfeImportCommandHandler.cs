using Core.Application.IntegrationEvents;
using Core.Application.Storage;
using Core.Domain;
using Core.Domain.Auditing;
using Inventory.Application.Common;
using Inventory.Domain.Entities;
using Inventory.Domain;
using InvErrors = Inventory.Domain.ErrorCodes;
using Inventory.Domain.Enums;
using Inventory.Domain.Repositories;
using Inventory.Domain.Services;
using MediatR;

namespace Inventory.Application.PurchaseImports.Commands;

/// <summary>Applies user mappings and posts purchase stock in one transaction.</summary>
public sealed class ConfirmPurchaseNfeImportCommandHandler : IRequestHandler<ConfirmPurchaseNfeImportCommand, Result<Guid>>
{
    private readonly IPurchaseInvoiceImportRepository _importRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IProductRepository _productRepository;
    private readonly IProductLotRepository _lotRepository;
    private readonly IStockMovementRepository _movementRepository;
    private readonly ISupplierProductMappingRepository _mappingRepository;
    private readonly StockCatalogReconciler _reconciler;
    private readonly IBlobStorage _blobStorage;
    private readonly ITenantContext _tenantContext;
    private readonly IAuditLogger _auditLogger;
    private readonly IPublisher _publisher;

    public ConfirmPurchaseNfeImportCommandHandler(
        IPurchaseInvoiceImportRepository importRepository,
        ISupplierRepository supplierRepository,
        IProductRepository productRepository,
        IProductLotRepository lotRepository,
        IStockMovementRepository movementRepository,
        ISupplierProductMappingRepository mappingRepository,
        StockCatalogReconciler reconciler,
        IBlobStorage blobStorage,
        ITenantContext tenantContext,
        IAuditLogger auditLogger,
        IPublisher publisher)
    {
        _importRepository = importRepository;
        _supplierRepository = supplierRepository;
        _productRepository = productRepository;
        _lotRepository = lotRepository;
        _movementRepository = movementRepository;
        _mappingRepository = mappingRepository;
        _reconciler = reconciler;
        _blobStorage = blobStorage;
        _tenantContext = tenantContext;
        _auditLogger = auditLogger;
        _publisher = publisher;
    }

    public async Task<Result<Guid>> Handle(ConfirmPurchaseNfeImportCommand request, CancellationToken cancellationToken)
    {
        var import = await _importRepository.GetByIdWithLinesAsync(request.ImportId, cancellationToken);
        if (import is null)
        {
            return Result.Failure<Guid>(InvErrors.PurchaseImport.NotFound);
        }

        if (import.Status == PurchaseImportStatus.Confirmed)
        {
            return Result.Failure<Guid>(InvErrors.PurchaseImport.AlreadyConfirmed);
        }

        var supplierResult = await ResolveSupplierAsync(import, request.Supplier, cancellationToken);
        if (supplierResult.IsFailure)
        {
            return Result.Failure<Guid>(supplierResult.Error);
        }

        var supplierId = supplierResult.Value;
        import.AssignSupplier(supplierId);

        var correlationId = import.Id;
        foreach (var lineAction in request.Lines)
        {
            var line = import.Lines.FirstOrDefault(l => l.Id == lineAction.LineId);
            if (line is null)
            {
                return Result.Failure<Guid>(InvErrors.PurchaseImport.LineNotFound);
            }

            var productResult = await ResolveProductAsync(line, lineAction, supplierId, cancellationToken);
            if (productResult.IsFailure)
            {
                return Result.Failure<Guid>(productResult.Error);
            }

            var product = productResult.Value;
            line.AssignProduct(product.Id);

            var movementResult = await ApplyStockAsync(import, line, product, correlationId, cancellationToken);
            if (movementResult.IsFailure)
            {
                return Result.Failure<Guid>(movementResult.Error);
            }

            await UpsertMappingAsync(supplierId, line.SupplierProductCode, product.Id, cancellationToken);
        }

        var confirm = import.Confirm();
        if (confirm.IsFailure)
        {
            return Result.Failure<Guid>(confirm.Error);
        }

        _importRepository.Update(import);

        await _auditLogger.LogAsync(
            _tenantContext.TenantId,
            _tenantContext.UserId,
            import.Id,
            "PurchaseInvoiceImport",
            "ImportConfirm",
            $"NF-e {import.AccessKey}",
            cancellationToken);

        var duplicates = await ReadDuplicatesFromBlobAsync(import.BlobKey, cancellationToken);
        await _publisher.Publish(
            new PurchaseInvoiceImportedEvent(
                import.Id,
                import.AccessKey,
                supplierId,
                import.TotalAmount,
                duplicates),
            cancellationToken);

        return Result.Success(import.Id);
    }

    private async Task<Result<Guid>> ResolveSupplierAsync(
        PurchaseInvoiceImport import,
        ConfirmSupplierAction action,
        CancellationToken cancellationToken)
    {
        if (action.Mode == SupplierConfirmMode.LinkExisting)
        {
            if (action.SupplierId is null || action.SupplierId == Guid.Empty)
            {
                return Result.Failure<Guid>(InvErrors.Supplier.NotFound);
            }

            var supplier = await _supplierRepository.GetByIdAsync(action.SupplierId.Value, cancellationToken);
            if (supplier is null || !supplier.IsActive)
            {
                return Result.Failure<Guid>(InvErrors.Supplier.NotFound);
            }

            return Result.Success(supplier.Id);
        }

        var existing = await _supplierRepository.GetByDocumentAsync(import.EmitterDocument, cancellationToken);
        if (existing is not null)
        {
            return Result.Success(existing.Id);
        }

        var created = Supplier.Create(
            import.EmitterLegalName,
            string.IsNullOrWhiteSpace(import.EmitterTradeName) ? import.EmitterLegalName : import.EmitterTradeName,
            import.EmitterDocument,
            null,
            null);

        if (created.IsFailure)
        {
            return Result.Failure<Guid>(created.Error);
        }

        _supplierRepository.Add(created.Value);
        return Result.Success(created.Value.Id);
    }

    private async Task<Result<Product>> ResolveProductAsync(
        PurchaseInvoiceImportLine line,
        ConfirmLineAction action,
        Guid supplierId,
        CancellationToken cancellationToken)
    {
        if (action.Mode == LineConfirmMode.LinkExisting)
        {
            if (action.ProductId is null || action.ProductId == Guid.Empty)
            {
                return Result.Failure<Product>(InvErrors.Product.NotFound);
            }

            var product = await _productRepository.GetByIdAsync(action.ProductId.Value, cancellationToken);
            if (product is null || !product.IsActive)
            {
                return Result.Failure<Product>(InvErrors.Product.NotFound);
            }

            return Result.Success(product);
        }

        var payload = action.CreateProduct;
        if (payload is null)
        {
            return Result.Failure<Product>(InvErrors.Product.NotFound);
        }

        if (await _productRepository.GetBySkuAsync(payload.Sku, cancellationToken) is not null)
        {
            return Result.Failure<Product>(InvErrors.Product.SkuConflict);
        }

        if (await _productRepository.GetByBarcodeAsync(payload.Barcode, cancellationToken) is not null)
        {
            return Result.Failure<Product>(InvErrors.Product.BarcodeConflict);
        }

        var productResult = Product.Create(
            line.Description,
            payload.Description ?? line.Description,
            payload.Sku,
            payload.Barcode,
            line.UnitOfMeasure,
            payload.ReorderLevel,
            payload.Category,
            line.Ncm,
            null,
            0,
            supplierId,
            payload.RequiresLot);

        if (productResult.IsFailure)
        {
            return Result.Failure<Product>(productResult.Error);
        }

        var newProduct = productResult.Value;
        _productRepository.Add(newProduct);
        await _productRepository.AddBalanceAsync(new ProductBalance(newProduct.Id, 0m), cancellationToken);
        return Result.Success(newProduct);
    }

    private async Task<Result<Guid>> ApplyStockAsync(
        PurchaseInvoiceImport import,
        PurchaseInvoiceImportLine line,
        Product product,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        var lots = await _lotRepository.ListByProductIdAsync(product.Id, cancellationToken);
        var useLotPath = product.RequiresLot || lots.Count > 0 || !string.IsNullOrWhiteSpace(line.LotNumber);

        ProductLot? lot = null;
        if (useLotPath)
        {
            var lotNumber = string.IsNullOrWhiteSpace(line.LotNumber)
                ? $"NF-{import.InvoiceNumber}-{line.ItemNumber}"
                : line.LotNumber!;

            lot = await _lotRepository.GetByProductAndLotNumberAsync(product.Id, lotNumber, cancellationToken);
            if (lot is null)
            {
                var lotResult = ProductLot.Create(product.Id, lotNumber, line.ExpirationDate, line.UnitCost, 0m);
                if (lotResult.IsFailure)
                {
                    return Result.Failure<Guid>(lotResult.Error);
                }

                lot = lotResult.Value;
                _lotRepository.Add(lot);
            }
            else
            {
                var weightedCost = WeightedLotCost(lot.Quantity, lot.UnitCost, line.Quantity, line.UnitCost);
                lot.UpdateMetadata(line.ExpirationDate ?? lot.ExpirationDate, weightedCost);
                _lotRepository.Update(lot);
            }

            var adjust = lot.AdjustQuantity(line.Quantity);
            if (adjust.IsFailure)
            {
                return Result.Failure<Guid>(adjust.Error);
            }

            _lotRepository.Update(lot);
        }
        else
        {
            var balance = await _productRepository.GetBalanceAsync(product.Id, cancellationToken);
            if (balance is null)
            {
                balance = new ProductBalance(product.Id, 0m);
                await _productRepository.AddBalanceAsync(balance, cancellationToken);
            }

            var update = balance.ApplySignedDelta(line.Quantity);
            if (update.IsFailure)
            {
                return Result.Failure<Guid>(update.Error);
            }

            await _productRepository.UpdateBalanceAsync(balance, cancellationToken);
        }

        var movementId = Guid.NewGuid();
        var movementResult = StockMovement.Create(
            product.Id,
            MovementType.In,
            line.Quantity,
            lot?.LotNumber ?? line.LotNumber,
            lot?.ExpirationDate ?? line.ExpirationDate,
            StockMovementReasons.Purchase,
            lot?.Id,
            null,
            movementId,
            import.IssuedAt,
            correlationId);

        if (movementResult.IsFailure)
        {
            return Result.Failure<Guid>(movementResult.Error);
        }

        _movementRepository.Add(movementResult.Value);
        line.MarkStockApplied(lot?.Id, movementId);

        if (useLotPath)
        {
            await _reconciler.ReconcileAsync(product, cancellationToken);
        }

        return Result.Success(movementId);
    }

    private static decimal WeightedLotCost(decimal oldQty, decimal oldCost, decimal addQty, decimal addCost)
    {
        var totalQty = oldQty + addQty;
        if (totalQty <= 0)
        {
            return addCost;
        }

        return decimal.Round((oldQty * oldCost + addQty * addCost) / totalQty, 4, MidpointRounding.AwayFromZero);
    }

    private async Task UpsertMappingAsync(Guid supplierId, string code, Guid productId, CancellationToken cancellationToken)
    {
        var existing = await _mappingRepository.GetBySupplierAndCodeAsync(supplierId, code, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        var mapping = SupplierProductMapping.Create(supplierId, code, productId);
        if (mapping.IsSuccess)
        {
            _mappingRepository.Add(mapping.Value);
        }
    }

    private async Task<IReadOnlyList<PurchaseInvoiceImportedDuplicate>> ReadDuplicatesFromBlobAsync(
        string blobKey,
        CancellationToken cancellationToken)
    {
        var open = await _blobStorage.OpenReadAsync(blobKey, cancellationToken);
        if (open.IsFailure)
        {
            return Array.Empty<PurchaseInvoiceImportedDuplicate>();
        }

        await using var stream = open.Value;
        var parsed = NfePurchaseXmlParser.Parse(stream);
        if (parsed.IsFailure)
        {
            return Array.Empty<PurchaseInvoiceImportedDuplicate>();
        }

        return parsed.Value.Duplicates
            .Select(d => new PurchaseInvoiceImportedDuplicate(d.Number, d.DueDate, d.Amount))
            .ToList();
    }
}
