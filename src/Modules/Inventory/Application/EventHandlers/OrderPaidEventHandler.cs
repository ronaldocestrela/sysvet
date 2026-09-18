using Core.Application.IntegrationEvents;
using Core.Domain;
using Core.Domain.Auditing;
using Inventory.Application.Common;
using Inventory.Domain;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using Inventory.Domain.Services;
using MediatR;

namespace Inventory.Application.EventHandlers;

/// <summary>
/// Applies FEFO lot consumption when a paid order includes inventory items.
/// </summary>
public class OrderPaidEventHandler : INotificationHandler<OrderPaidEvent>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductLotRepository _lotRepository;
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly StockCatalogReconciler _reconciler;
    private readonly IAuditLogger _auditLogger;
    private readonly ITenantContext _tenantContext;

    public OrderPaidEventHandler(
        IProductRepository productRepository,
        IProductLotRepository lotRepository,
        IStockMovementRepository stockMovementRepository,
        StockCatalogReconciler reconciler,
        IAuditLogger auditLogger,
        ITenantContext tenantContext)
    {
        _productRepository = productRepository;
        _lotRepository = lotRepository;
        _stockMovementRepository = stockMovementRepository;
        _reconciler = reconciler;
        _auditLogger = auditLogger;
        _tenantContext = tenantContext;
    }

    public async Task Handle(OrderPaidEvent notification, CancellationToken cancellationToken)
    {
        foreach (var item in notification.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (product is null || !product.IsActive)
            {
                continue;
            }

            var lots = await _lotRepository.ListByProductIdAsync(product.Id, cancellationToken);
            if (lots.Count == 0)
            {
                await TryLegacyBalanceOutAsync(product, item.Quantity, notification.OrderId, cancellationToken);
                continue;
            }

            var allocation = LotAllocationService.Allocate(lots, item.Quantity);
            if (allocation.Count == 0)
            {
                await _auditLogger.LogAsync(
                    _tenantContext.TenantId,
                    _tenantContext.UserId,
                    notification.OrderId,
                    "StockMovement",
                    "SaleSkippedInsufficientStock",
                    $"Product {product.Sku} qty {item.Quantity}",
                    cancellationToken);
                continue;
            }

            foreach (var (lot, qty) in allocation)
            {
                var adjust = lot.AdjustQuantity(-qty);
                if (adjust.IsFailure)
                {
                    continue;
                }

                _lotRepository.Update(lot);
                var reason = $"{StockMovementReasons.Sale} - Order {notification.OrderId}";
                var movement = StockMovement.Create(
                    product.Id,
                    MovementType.Out,
                    qty,
                    lot.LotNumber,
                    lot.ExpirationDate,
                    reason,
                    lot.Id);

                if (movement.IsSuccess)
                {
                    _stockMovementRepository.Add(movement.Value);
                }
            }

            await _reconciler.ReconcileAsync(product, cancellationToken);
        }
    }

    private async Task TryLegacyBalanceOutAsync(
        Product product,
        decimal quantity,
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var balance = await _productRepository.GetBalanceAsync(product.Id, cancellationToken);
        if (balance is null)
        {
            return;
        }

        var update = balance.UpdateBalance(quantity, MovementType.Out);
        if (update.IsFailure)
        {
            await _auditLogger.LogAsync(
                _tenantContext.TenantId,
                _tenantContext.UserId,
                orderId,
                "StockMovement",
                "SaleSkippedInsufficientStock",
                $"Product {product.Sku} legacy balance",
                cancellationToken);
            return;
        }

        await _productRepository.UpdateBalanceAsync(balance, cancellationToken);
        var reason = $"{StockMovementReasons.Sale} - Order {orderId}";
        var movement = StockMovement.Create(product.Id, MovementType.Out, quantity, null, null, reason);
        if (movement.IsSuccess)
        {
            _stockMovementRepository.Add(movement.Value);
        }
    }
}
