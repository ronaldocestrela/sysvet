using Core.Domain;
using Core.Domain.Auditing;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using Inventory.Domain.Services;

namespace Inventory.Application.Common;

/// <summary>
/// Applies on-hand changes and persists immutable stock ledger lines.
/// </summary>
public sealed class StockLedgerWriter
{
    private readonly IProductRepository _productRepository;
    private readonly IProductLotRepository _lotRepository;
    private readonly IStockMovementRepository _movementRepository;
    private readonly StockCatalogReconciler _reconciler;
    private readonly ITenantContext _tenantContext;
    private readonly IAuditLogger _auditLogger;

    public StockLedgerWriter(
        IProductRepository productRepository,
        IProductLotRepository lotRepository,
        IStockMovementRepository movementRepository,
        StockCatalogReconciler reconciler,
        ITenantContext tenantContext,
        IAuditLogger auditLogger)
    {
        _productRepository = productRepository;
        _lotRepository = lotRepository;
        _movementRepository = movementRepository;
        _reconciler = reconciler;
        _tenantContext = tenantContext;
        _auditLogger = auditLogger;
    }

    /// <summary>Registers a movement and updates lot or legacy balance.</summary>
    public async Task<Result<Guid>> RegisterAsync(
        RegisterStockLedgerRequest request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null || !product.IsActive)
        {
            return Result.Failure<Guid>(Inventory.Domain.ErrorCodes.Product.NotFound);
        }

        var lots = await _lotRepository.ListByProductIdAsync(product.Id, cancellationToken);
        var hasAnyLots = lots.Count > 0;
        ProductLot? lot = null;
        if (request.ProductLotId is not null)
        {
            lot = await _lotRepository.GetByIdAsync(request.ProductLotId.Value, cancellationToken);
        }

        var contextResult = StockQuantityApplier.ValidateLotContext(product, lot, request.ProductLotId, hasAnyLots);
        if (contextResult.IsFailure)
        {
            return Result.Failure<Guid>(contextResult.Error);
        }

        var deltaResult = StockQuantityApplier.ResolveDelta(request.Type, request.Quantity, request.AdjustmentDirection);
        if (deltaResult.IsFailure)
        {
            return Result.Failure<Guid>(deltaResult.Error);
        }

        var useLotPath = product.RequiresLot || hasAnyLots;
        if (useLotPath)
        {
            var adjust = lot!.AdjustQuantity(deltaResult.Value);
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

            var update = balance.ApplySignedDelta(deltaResult.Value);
            if (update.IsFailure)
            {
                return Result.Failure<Guid>(update.Error);
            }

            await _productRepository.UpdateBalanceAsync(balance, cancellationToken);
        }

        var movementId = request.MovementId == Guid.Empty ? Guid.NewGuid() : request.MovementId;
        var batch = lot?.LotNumber ?? request.BatchNumber;
        var expiry = lot?.ExpirationDate ?? request.ExpirationDate;
        var movementResult = StockMovement.Create(
            product.Id,
            request.Type,
            request.Quantity,
            batch,
            expiry,
            request.Reason,
            lot?.Id,
            request.AdjustmentDirection,
            movementId,
            occurredAt: null,
            correlationId: request.CorrelationId,
            supplierId: request.SupplierId,
            notes: request.Notes);

        if (movementResult.IsFailure)
        {
            return Result.Failure<Guid>(movementResult.Error);
        }

        _movementRepository.Add(movementResult.Value);

        if (useLotPath)
        {
            await _reconciler.ReconcileAsync(product, cancellationToken);
        }

        await _auditLogger.LogAsync(
            _tenantContext.TenantId,
            _tenantContext.UserId,
            movementResult.Value.Id,
            "StockMovement",
            request.AuditAction,
            $"{request.Type} {request.Quantity} product {product.Sku}",
            cancellationToken);

        return Result.Success(movementResult.Value.Id);
    }
}

/// <summary>Input for <see cref="StockLedgerWriter.RegisterAsync"/>.</summary>
public sealed record RegisterStockLedgerRequest(
    Guid ProductId,
    MovementType Type,
    decimal Quantity,
    string Reason,
    string AuditAction,
    Guid? ProductLotId = null,
    AdjustmentDirection? AdjustmentDirection = null,
    string? BatchNumber = null,
    DateTimeOffset? ExpirationDate = null,
    Guid? CorrelationId = null,
    Guid MovementId = default,
    Guid? SupplierId = null,
    string? Notes = null);
