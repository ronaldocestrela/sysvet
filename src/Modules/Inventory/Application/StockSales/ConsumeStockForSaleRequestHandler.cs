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
/// Debits inventory for product lines on sale pay; fails when active product lacks stock.
/// </summary>
public sealed class ConsumeStockForSaleRequestHandler : IRequestHandler<ConsumeStockForSaleRequest, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductLotRepository _lotRepository;
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly StockCatalogReconciler _reconciler;

    public ConsumeStockForSaleRequestHandler(
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

    public async Task<Result> Handle(ConsumeStockForSaleRequest request, CancellationToken cancellationToken)
    {
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
                var legacy = await TryLegacyBalanceOutAsync(product, line.Quantity, request.OrderId, cancellationToken);
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
                var reason = $"{StockMovementReasons.Sale} - Order {request.OrderId}";
                var movement = StockMovement.Create(
                    product.Id,
                    MovementType.Out,
                    qty,
                    lot.LotNumber,
                    lot.ExpirationDate,
                    reason,
                    lot.Id,
                    correlationId: request.OrderId);

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
        Guid orderId,
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
        var reason = $"{StockMovementReasons.Sale} - Order {orderId}";
        var movement = StockMovement.Create(product.Id, MovementType.Out, quantity, null, null, reason, correlationId: orderId);
        if (movement.IsFailure)
        {
            return Result.Failure(movement.Error);
        }

        _stockMovementRepository.Add(movement.Value);
        return Result.Success();
    }
}
