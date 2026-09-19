using Core.Domain;
using Core.Domain.Auditing;
using Inventory.Application.Common;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using Inventory.Domain.Services;
using MediatR;

namespace Inventory.Application.StockMovements.Commands;

/// <summary>Registers a lot-aware stock movement and updates on-hand.</summary>
public sealed class RegisterStockMovementCommandHandler : IRequestHandler<RegisterStockMovementCommand, Result<Guid>>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductLotRepository _lotRepository;
    private readonly IStockMovementRepository _movementRepository;
    private readonly StockCatalogReconciler _reconciler;
    private readonly ITenantContext _tenantContext;
    private readonly IAuditLogger _auditLogger;

    public RegisterStockMovementCommandHandler(
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

    public async Task<Result<Guid>> Handle(RegisterStockMovementCommand request, CancellationToken cancellationToken)
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
            correlationId: request.CorrelationId);

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
            "Register",
            $"{request.Type} {request.Quantity} product {product.Sku}",
            cancellationToken);

        return Result.Success(movementResult.Value.Id);
    }
}

/// <summary>Transfers quantity between two lots of the same product.</summary>
public sealed class TransferStockCommandHandler : IRequestHandler<TransferStockCommand, Result<Guid>>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductLotRepository _lotRepository;
    private readonly IStockMovementRepository _movementRepository;
    private readonly StockCatalogReconciler _reconciler;
    private readonly ITenantContext _tenantContext;
    private readonly IAuditLogger _auditLogger;

    public TransferStockCommandHandler(
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

    public async Task<Result<Guid>> Handle(TransferStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null || !product.IsActive)
        {
            return Result.Failure<Guid>(Inventory.Domain.ErrorCodes.Product.NotFound);
        }

        var source = await _lotRepository.GetByIdAsync(request.SourceLotId, cancellationToken);
        var destination = await _lotRepository.GetByIdAsync(request.DestinationLotId, cancellationToken);
        if (source is null || destination is null)
        {
            return Result.Failure<Guid>(Inventory.Domain.ErrorCodes.ProductLot.NotFound);
        }

        var validate = StockQuantityApplier.ValidateTransferLots(product, source, destination);
        if (validate.IsFailure)
        {
            return Result.Failure<Guid>(validate.Error);
        }

        if (request.Quantity <= 0)
        {
            return Result.Failure<Guid>(Inventory.Domain.ErrorCodes.StockMovement.InvalidQuantity);
        }

        var outAdjust = source.AdjustQuantity(-request.Quantity);
        if (outAdjust.IsFailure)
        {
            return Result.Failure<Guid>(outAdjust.Error);
        }

        var inAdjust = destination.AdjustQuantity(request.Quantity);
        if (inAdjust.IsFailure)
        {
            return Result.Failure<Guid>(inAdjust.Error);
        }

        _lotRepository.Update(source);
        _lotRepository.Update(destination);

        var correlationId = request.CorrelationId == Guid.Empty ? Guid.NewGuid() : request.CorrelationId;
        var reason = string.IsNullOrWhiteSpace(request.Reason) ? Inventory.Domain.StockMovementReasons.Transfer : request.Reason;

        var outMovement = StockMovement.Create(
            product.Id,
            MovementType.Out,
            request.Quantity,
            source.LotNumber,
            source.ExpirationDate,
            reason,
            source.Id,
            correlationId: correlationId);

        var inMovement = StockMovement.Create(
            product.Id,
            MovementType.In,
            request.Quantity,
            destination.LotNumber,
            destination.ExpirationDate,
            reason,
            destination.Id,
            correlationId: correlationId);

        if (outMovement.IsFailure)
        {
            return Result.Failure<Guid>(outMovement.Error);
        }

        if (inMovement.IsFailure)
        {
            return Result.Failure<Guid>(inMovement.Error);
        }

        _movementRepository.Add(outMovement.Value);
        _movementRepository.Add(inMovement.Value);
        await _reconciler.ReconcileAsync(product, cancellationToken);

        await _auditLogger.LogAsync(
            _tenantContext.TenantId,
            _tenantContext.UserId,
            correlationId,
            "StockMovement",
            "Transfer",
            $"{request.Quantity} from {source.LotNumber} to {destination.LotNumber}",
            cancellationToken);

        return Result.Success(correlationId);
    }
}
