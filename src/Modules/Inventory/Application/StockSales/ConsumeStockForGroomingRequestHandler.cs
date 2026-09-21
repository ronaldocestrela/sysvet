using Core.Application.IntegrationEvents;
using Core.Domain;
using Inventory.Application.Common;
using Inventory.Domain;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.Repositories;
using Inventory.Domain.Services;
using MediatR;

namespace Inventory.Application.StockSales;

/// <summary>
/// Debits inventory for grooming supply lines on service completion; idempotent by attendance id.
/// </summary>
public sealed class ConsumeStockForGroomingRequestHandler : IRequestHandler<ConsumeStockForGroomingRequest, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductLotRepository _lotRepository;
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly StockCatalogReconciler _reconciler;

    public ConsumeStockForGroomingRequestHandler(
        IProductRepository productRepository,
        IProductLotRepository lotRepository,
        IStockMovementRepository stockMovementRepository,
        StockCatalogReconciler reconciler)
    {
        _productRepository = productRepository;
        _lotRepository = lotRepository;
        _stockMovementRepository = stockMovementRepository;
        _reconciler = reconciler;
    }

    public async Task<Result> Handle(ConsumeStockForGroomingRequest request, CancellationToken cancellationToken)
    {
        var existing = await _stockMovementRepository.ListByCorrelationIdAsync(request.AttendanceId, cancellationToken);
        if (existing.Any(m => m.Type == MovementType.Out && m.Reason.StartsWith(StockMovementReasons.Grooming, StringComparison.Ordinal)))
        {
            return Result.Success();
        }

        if (request.Lines.Count == 0)
        {
            return Result.Success();
        }

        foreach (var line in request.Lines)
        {
            var product = await _productRepository.GetByIdAsync(line.ProductId, cancellationToken);
            if (product is null || !product.IsActive)
            {
                return Result.Failure(Domain.ErrorCodes.Product.NotFound);
            }

            var lots = await _lotRepository.ListByProductIdAsync(product.Id, cancellationToken);
            if (lots.Count == 0)
            {
                var legacy = await TryLegacyBalanceOutAsync(product, line.Quantity, request.AttendanceId, cancellationToken);
                if (legacy.IsFailure)
                {
                    return legacy;
                }

                continue;
            }

            var allocation = LotAllocationService.Allocate(lots, line.Quantity);
            if (allocation.Count == 0 || allocation.Sum(a => a.Quantity) < line.Quantity)
            {
                return Result.Failure(Domain.ErrorCodes.ProductBalance.InsufficientFunds);
            }

            foreach (var (lot, qty) in allocation)
            {
                var adjust = lot.AdjustQuantity(-qty);
                if (adjust.IsFailure)
                {
                    return Result.Failure(adjust.Error);
                }

                _lotRepository.Update(lot);
                var reason = $"{StockMovementReasons.Grooming} - Attendance {request.AttendanceId}";
                var movement = StockMovement.Create(
                    product.Id,
                    MovementType.Out,
                    qty,
                    lot.LotNumber,
                    lot.ExpirationDate,
                    reason,
                    lot.Id,
                    correlationId: request.AttendanceId);

                if (movement.IsFailure)
                {
                    return Result.Failure(movement.Error);
                }

                _stockMovementRepository.Add(movement.Value);
            }

            await _reconciler.ReconcileAsync(product, cancellationToken);
        }

        return Result.Success();
    }

    private async Task<Result> TryLegacyBalanceOutAsync(
        Product product,
        decimal quantity,
        Guid attendanceId,
        CancellationToken cancellationToken)
    {
        var balance = await _productRepository.GetBalanceAsync(product.Id, cancellationToken);
        if (balance is null)
        {
            return Result.Failure(Domain.ErrorCodes.ProductBalance.InsufficientFunds);
        }

        var update = balance.UpdateBalance(quantity, MovementType.Out);
        if (update.IsFailure)
        {
            return Result.Failure(Domain.ErrorCodes.ProductBalance.InsufficientFunds);
        }

        await _productRepository.UpdateBalanceAsync(balance, cancellationToken);
        var reason = $"{StockMovementReasons.Grooming} - Attendance {attendanceId}";
        var movement = StockMovement.Create(product.Id, MovementType.Out, quantity, null, null, reason, correlationId: attendanceId);
        if (movement.IsFailure)
        {
            return Result.Failure(movement.Error);
        }

        _stockMovementRepository.Add(movement.Value);
        return Result.Success();
    }
}
