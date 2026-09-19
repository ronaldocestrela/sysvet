using Core.Domain;
using Core.Domain.Auditing;
using Inventory.Application.Common;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using MediatR;

namespace Inventory.Application.StockMovements.Commands;

/// <summary>Registers a lot-aware stock movement and updates on-hand.</summary>
public sealed class RegisterStockMovementCommandHandler : IRequestHandler<RegisterStockMovementCommand, Result<Guid>>
{
    private readonly StockLedgerWriter _ledgerWriter;

    public RegisterStockMovementCommandHandler(StockLedgerWriter ledgerWriter) => _ledgerWriter = ledgerWriter;

    public Task<Result<Guid>> Handle(RegisterStockMovementCommand request, CancellationToken cancellationToken) =>
        _ledgerWriter.RegisterAsync(
            new RegisterStockLedgerRequest(
                request.ProductId,
                request.Type,
                request.Quantity,
                request.Reason,
                "Register",
                request.ProductLotId,
                request.AdjustmentDirection,
                request.BatchNumber,
                request.ExpirationDate,
                request.CorrelationId,
                request.MovementId),
            cancellationToken);
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

        var validate = Inventory.Domain.Services.StockQuantityApplier.ValidateTransferLots(product, source, destination);
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
