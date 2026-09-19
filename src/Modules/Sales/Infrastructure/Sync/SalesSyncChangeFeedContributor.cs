using Core.Application.Sync;
using Microsoft.EntityFrameworkCore;
using Sales.Domain.Entities;
using Sales.Infrastructure.Persistence;

namespace Sales.Infrastructure.Sync;

/// <summary>
/// Exposes sales orders and cash registers to the Core sync pull feed (ADR-026).
/// </summary>
public sealed class SalesSyncChangeFeedContributor : ISyncChangeFeedContributor
{
    private readonly SalesDbContext _dbContext;

    /// <summary>Creates the contributor.</summary>
    public SalesSyncChangeFeedContributor(SalesDbContext dbContext) => _dbContext = dbContext;

    /// <inheritdoc />
    public async Task<SyncContributorChanges> ReadChangesAsync(DateTimeOffset since, int take, CancellationToken cancellationToken)
    {
        var maxUpdated = since;
        var hasMore = false;

        var registers = await ReadPageAsync(
            _dbContext.CashRegisters.AsNoTracking(),
            since,
            take,
            c => c.UpdatedAt,
            cancellationToken);
        hasMore |= registers.HasMore;
        maxUpdated = Max(maxUpdated, registers.Items.Select(c => c.UpdatedAt));

        var orders = await ReadPageAsync(
            _dbContext.Orders.AsNoTracking()
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .ThenInclude(p => p.Refunds),
            since,
            take,
            o => o.UpdatedAt,
            cancellationToken);
        hasMore |= orders.HasMore;
        maxUpdated = Max(maxUpdated, orders.Items.Select(o => o.UpdatedAt));

        return new SyncContributorChanges
        {
            SalesCashRegisters = registers.Items.Select(MapRegister).ToList(),
            SalesOrders = orders.Items.Select(MapOrder).ToList(),
            MaxUpdatedAt = maxUpdated,
            HasMore = hasMore
        };
    }

    private static DateTimeOffset Max(DateTimeOffset current, IEnumerable<DateTimeOffset> values)
    {
        foreach (var value in values)
        {
            if (value > current)
            {
                current = value;
            }
        }

        return current;
    }

    private static async Task<(List<T> Items, bool HasMore)> ReadPageAsync<T>(
        IQueryable<T> query,
        DateTimeOffset since,
        int take,
        Func<T, DateTimeOffset> updatedAt,
        CancellationToken cancellationToken)
    {
        var all = await query.ToListAsync(cancellationToken);
        var candidates = all.Where(x => updatedAt(x) > since).OrderBy(updatedAt).Take(take + 1).ToList();
        var hasMore = candidates.Count > take;
        if (hasMore)
        {
            candidates = candidates.Take(take).ToList();
        }

        return (candidates, hasMore);
    }

    private static SyncSalesCashRegisterDto MapRegister(CashRegister c) =>
        new()
        {
            Id = c.Id,
            OpenedByUserId = c.OpenedByUserId,
            OpenedAt = c.OpenedAt,
            ClosedAt = c.ClosedAt,
            OpeningBalance = c.OpeningBalance.Amount,
            ClosingBalance = c.ClosingBalance.Amount,
            Status = c.Status.ToString(),
            UpdatedAt = c.UpdatedAt,
            RowVersion = Convert.ToBase64String(c.RowVersion ?? Array.Empty<byte>())
        };

    private static SyncSalesOrderDto MapOrder(Order o) =>
        new()
        {
            Id = o.Id,
            CashRegisterId = o.CashRegisterId,
            Status = o.Status.ToString(),
            TutorId = o.TutorId,
            PetId = o.PetId,
            SourceQuoteId = o.SourceQuoteId,
            FinanceIntegrationStatus = o.FinanceIntegrationStatus.ToString(),
            CreatedAt = o.CreatedAt,
            PaidAt = o.PaidAt,
            UpdatedAt = o.UpdatedAt,
            RowVersion = Convert.ToBase64String(o.RowVersion ?? Array.Empty<byte>()),
            Items = o.Items.Select(i => new SyncSalesOrderItemDto
            {
                Id = i.Id,
                Kind = i.Kind.ToString(),
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice.Amount
            }).ToList(),
            Payments = o.Payments.Select(p => new SyncSalesOrderPaymentDto
            {
                Id = p.Id,
                Method = p.Method.ToString(),
                Amount = p.Amount.Amount,
                Nsu = p.Nsu,
                AuthorizationCode = p.AuthorizationCode,
                Provider = p.Provider,
                TerminalId = p.TerminalId,
                Brand = p.Brand,
                Installments = p.Installments,
                Refunds = p.Refunds.Select(r => new SyncSalesOrderPaymentRefundDto
                {
                    Id = r.Id,
                    Amount = r.Amount.Amount,
                    RefundNsu = r.RefundNsu,
                    CreatedAt = r.CreatedAt
                }).ToList()
            }).ToList()
        };
}
