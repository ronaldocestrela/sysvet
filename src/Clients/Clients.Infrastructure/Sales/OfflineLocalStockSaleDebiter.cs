using Clients.Infrastructure;
using Core.Domain;
using Inventory.Domain.Entities;
using Inventory.Domain.Services;
using Microsoft.EntityFrameworkCore;
using InvErrorCodes = Inventory.Domain.ErrorCodes;

namespace Clients.Infrastructure.Sales;

/// <summary>
/// Optimistic local stock debit for offline PDV (no StockMovement row — server creates Sale on sync).
/// </summary>
internal static class OfflineLocalStockSaleDebiter
{
    public static async Task<Result> DebitAsync(
        OfflineDbContext dbContext,
        IReadOnlyList<(Guid ProductId, decimal Quantity)> lines,
        CancellationToken cancellationToken)
    {
        foreach (var (productId, quantity) in lines)
        {
            var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
            if (product is null || !product.IsActive)
            {
                return Result.Failure(InvErrorCodes.Product.NotFound);
            }

            var lots = await dbContext.ProductLots.Where(l => l.ProductId == productId && l.IsActive).ToListAsync(cancellationToken);
            if (lots.Count == 0)
            {
                var balance = await dbContext.ProductBalances.FirstOrDefaultAsync(b => b.ProductId == productId, cancellationToken);
                if (balance is null)
                {
                    return Result.Failure(InvErrorCodes.ProductBalance.InsufficientFunds);
                }

                var update = balance.ApplySignedDelta(-quantity);
                if (update.IsFailure)
                {
                    return Result.Failure(InvErrorCodes.ProductBalance.InsufficientFunds);
                }

                dbContext.ProductBalances.Update(balance);
                continue;
            }

            var allocation = LotAllocationService.Allocate(lots, quantity);
            if (allocation.Count == 0 || allocation.Sum(a => a.Quantity) < quantity)
            {
                return Result.Failure(InvErrorCodes.ProductBalance.InsufficientFunds);
            }

            foreach (var (lot, qty) in allocation)
            {
                var adjust = lot.AdjustQuantity(-qty);
                if (adjust.IsFailure)
                {
                    return Result.Failure(adjust.Error);
                }

                dbContext.ProductLots.Update(lot);
            }

            await ReconcileLocalBalanceAsync(dbContext, product, lots, cancellationToken);
        }

        return Result.Success();
    }

    public static async Task<Result> CreditAsync(
        OfflineDbContext dbContext,
        IReadOnlyList<(Guid ProductId, decimal Quantity)> lines,
        CancellationToken cancellationToken)
    {
        foreach (var (productId, quantity) in lines)
        {
            var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
            if (product is null || !product.IsActive)
            {
                return Result.Failure(InvErrorCodes.Product.NotFound);
            }

            var lots = await dbContext.ProductLots.Where(l => l.ProductId == productId && l.IsActive).ToListAsync(cancellationToken);
            if (lots.Count == 0)
            {
                var balance = await dbContext.ProductBalances.FirstOrDefaultAsync(b => b.ProductId == productId, cancellationToken);
                if (balance is null)
                {
                    return Result.Failure(InvErrorCodes.ProductBalance.InsufficientFunds);
                }

                var update = balance.ApplySignedDelta(quantity);
                if (update.IsFailure)
                {
                    return Result.Failure(update.Error);
                }

                dbContext.ProductBalances.Update(balance);
                continue;
            }

            var lot = lots.OrderByDescending(l => l.IsFractional).First();
            var adjust = lot.AdjustQuantity(quantity);
            if (adjust.IsFailure)
            {
                return Result.Failure(adjust.Error);
            }

            dbContext.ProductLots.Update(lot);
            await ReconcileLocalBalanceAsync(dbContext, product, lots, cancellationToken);
        }

        return Result.Success();
    }

    private static async Task ReconcileLocalBalanceAsync(
        OfflineDbContext dbContext,
        Product product,
        List<ProductLot> lots,
        CancellationToken cancellationToken)
    {
        var total = lots.Where(l => l.IsActive).Sum(l => l.Quantity);
        var balance = await dbContext.ProductBalances.FirstOrDefaultAsync(b => b.ProductId == product.Id, cancellationToken);
        if (balance is null)
        {
            await dbContext.ProductBalances.AddAsync(new ProductBalance(product.Id, total), cancellationToken);
        }
        else
        {
            balance.SyncFromLots(total);
            dbContext.ProductBalances.Update(balance);
        }
    }
}
