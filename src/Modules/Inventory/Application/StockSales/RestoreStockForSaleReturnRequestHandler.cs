using Core.Application.IntegrationEvents;
using Core.Domain;
using Inventory.Application.Common;
using Inventory.Domain;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.Repositories;
using MediatR;

namespace Inventory.Application.StockSales;

/// <summary>
/// Credits inventory for returned product lines, mirroring original sale lot debits (idempotent by return id).
/// </summary>
public sealed class RestoreStockForSaleReturnRequestHandler : IRequestHandler<RestoreStockForSaleReturnRequest, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductLotRepository _lotRepository;
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly StockCatalogReconciler _reconciler;

    public RestoreStockForSaleReturnRequestHandler(
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

    public async Task<Result> Handle(RestoreStockForSaleReturnRequest request, CancellationToken cancellationToken)
    {
        var existingReturn = await _stockMovementRepository.ListByCorrelationIdAsync(request.ReturnId, cancellationToken);
        if (existingReturn.Any(m => m.Reason == StockMovementReasons.SaleReturn))
        {
            return Result.Success();
        }

        var saleOuts = (await _stockMovementRepository.ListByCorrelationIdAsync(request.OrderId, cancellationToken))
            .Where(m => m.Type == MovementType.Out && m.Reason.StartsWith(StockMovementReasons.Sale, StringComparison.Ordinal))
            .OrderByDescending(m => m.Date)
            .ToList();

        foreach (var line in request.Lines)
        {
            var product = await _productRepository.GetByIdAsync(line.ProductId, cancellationToken);
            if (product is null || !product.IsActive)
            {
                return Result.Failure(Domain.ErrorCodes.Product.NotFound);
            }

            var remaining = line.Quantity;
            var productOuts = saleOuts.Where(m => m.ProductId == line.ProductId).ToList();

            foreach (var saleOut in productOuts)
            {
                if (remaining <= 0)
                {
                    break;
                }

                var restoreQty = Math.Min(remaining, saleOut.Quantity);
                if (restoreQty <= 0)
                {
                    continue;
                }

                if (saleOut.ProductLotId is Guid lotId)
                {
                    var lot = await _lotRepository.GetByIdAsync(lotId, cancellationToken);
                    if (lot is null)
                    {
                        return Result.Failure(Domain.ErrorCodes.ProductLot.NotFound);
                    }

                    var adjust = lot.AdjustQuantity(restoreQty);
                    if (adjust.IsFailure)
                    {
                        return Result.Failure(adjust.Error);
                    }

                    _lotRepository.Update(lot);
                }
                else
                {
                    var balance = await _productRepository.GetBalanceAsync(product.Id, cancellationToken);
                    if (balance is null)
                    {
                        return Result.Failure(Domain.ErrorCodes.ProductBalance.InsufficientFunds);
                    }

                    var update = balance.UpdateBalance(restoreQty, MovementType.In);
                    if (update.IsFailure)
                    {
                        return Result.Failure(update.Error);
                    }

                    await _productRepository.UpdateBalanceAsync(balance, cancellationToken);
                }

                var reason = $"{StockMovementReasons.SaleReturn} - Order {request.OrderId}";
                var movement = StockMovement.Create(
                    product.Id,
                    MovementType.In,
                    restoreQty,
                    saleOut.BatchNumber,
                    saleOut.ExpirationDate,
                    reason,
                    saleOut.ProductLotId,
                    correlationId: request.ReturnId);

                if (movement.IsFailure)
                {
                    return Result.Failure(movement.Error);
                }

                _stockMovementRepository.Add(movement.Value);
                remaining -= restoreQty;
            }

            if (remaining > 0)
            {
                return Result.Failure(Domain.ErrorCodes.ProductBalance.InsufficientFunds);
            }

            await _reconciler.ReconcileAsync(product, cancellationToken);
        }

        return Result.Success();
    }
}
