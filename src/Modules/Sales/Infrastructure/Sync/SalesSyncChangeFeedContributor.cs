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
                .ThenInclude(p => p.Refunds)
                .Include(o => o.Commissions)
                .Include(o => o.Returns)
                .ThenInclude(r => r.Lines),
            since,
            take,
            o => o.UpdatedAt,
            cancellationToken);
        hasMore |= orders.HasMore;
        maxUpdated = Max(maxUpdated, orders.Items.Select(o => o.UpdatedAt));

        var rules = await ReadPageAsync(
            _dbContext.CommissionRules.AsNoTracking(),
            since,
            take,
            r => r.UpdatedAt,
            cancellationToken);
        hasMore |= rules.HasMore;
        maxUpdated = Max(maxUpdated, rules.Items.Select(r => r.UpdatedAt));

        var kits = await ReadPageAsync(
            _dbContext.ProductKits.AsNoTracking().Include(k => k.Components),
            since,
            take,
            k => k.UpdatedAt,
            cancellationToken);
        hasMore |= kits.HasMore;
        maxUpdated = Max(maxUpdated, kits.Items.Select(k => k.UpdatedAt));

        var packages = await ReadPageAsync(
            _dbContext.ServicePackages.AsNoTracking(),
            since,
            take,
            p => p.UpdatedAt,
            cancellationToken);
        hasMore |= packages.HasMore;
        maxUpdated = Max(maxUpdated, packages.Items.Select(p => p.UpdatedAt));

        var balances = await ReadPageAsync(
            _dbContext.PrepaidBalances.AsNoTracking(),
            since,
            take,
            b => b.UpdatedAt,
            cancellationToken);
        hasMore |= balances.HasMore;
        maxUpdated = Max(maxUpdated, balances.Items.Select(b => b.UpdatedAt));

        return new SyncContributorChanges
        {
            SalesCashRegisters = registers.Items.Select(MapRegister).ToList(),
            SalesOrders = orders.Items.Select(MapOrder).ToList(),
            SalesCommissionRules = rules.Items.Select(MapRule).ToList(),
            SalesProductKits = kits.Items.Select(MapKit).ToList(),
            SalesServicePackages = packages.Items.Select(MapPackage).ToList(),
            SalesPrepaidBalances = balances.Items.Select(MapBalance).ToList(),
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
            SellerUserId = o.SellerUserId,
            DiscountPercent = o.DiscountPercent,
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
                CatalogOfferId = i.CatalogOfferId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice.Amount,
                PerformerUserId = i.PerformerUserId,
                PerformerRole = i.PerformerRole?.ToString(),
                ReturnedQuantity = i.ReturnedQuantity
            }).ToList(),
            Commissions = o.Commissions.Select(c => new SyncSalesCommissionAccrualDto
            {
                Id = c.Id,
                OrderItemId = c.OrderItemId,
                PayeeUserId = c.PayeeUserId,
                Role = c.Role.ToString(),
                RatePercent = c.RatePercent,
                BaseAmount = c.BaseAmount.Amount,
                CommissionAmount = c.CommissionAmount.Amount,
                Status = c.Status.ToString()
            }).ToList(),
            Returns = o.Returns.Select(r => new SyncSalesReturnDto
            {
                Id = r.Id,
                RefundAmount = r.RefundAmount.Amount,
                CreatedAt = r.CreatedAt,
                Lines = r.Lines.Select(l => new SyncSalesReturnLineDto
                {
                    Id = l.Id,
                    OrderItemId = l.OrderItemId,
                    Quantity = l.Quantity
                }).ToList()
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

    private static SyncCommissionRuleDto MapRule(CommissionRule r) =>
        new()
        {
            Id = r.Id,
            Role = r.Role.ToString(),
            AppliesTo = r.AppliesTo.ToString(),
            RatePercent = r.RatePercent,
            UpdatedAt = r.UpdatedAt,
            RowVersion = Convert.ToBase64String(r.RowVersion ?? Array.Empty<byte>())
        };

    private static SyncProductKitDto MapKit(ProductKit k) =>
        new()
        {
            Id = k.Id,
            Name = k.Name,
            IsActive = k.IsActive,
            UpdatedAt = k.UpdatedAt,
            RowVersion = Convert.ToBase64String(k.RowVersion ?? Array.Empty<byte>()),
            Components = k.Components.Select(c => new SyncProductKitComponentDto
            {
                ProductId = c.ProductId,
                QuantityPerKit = c.QuantityPerKit
            }).ToList()
        };

    private static SyncServicePackageDto MapPackage(ServicePackage p) =>
        new()
        {
            Id = p.Id,
            Name = p.Name,
            ServiceCode = p.ServiceCode.ToString(),
            UsesPerUnit = p.UsesPerUnit,
            IsActive = p.IsActive,
            UpdatedAt = p.UpdatedAt,
            RowVersion = Convert.ToBase64String(p.RowVersion ?? Array.Empty<byte>())
        };

    private static SyncPrepaidBalanceDto MapBalance(PrepaidBalance b) =>
        new()
        {
            Id = b.Id,
            TutorId = b.TutorId,
            PetId = b.PetId,
            ServiceCode = b.ServiceCode.ToString(),
            RemainingUses = b.RemainingUses,
            PurchasedUses = b.PurchasedUses,
            UpdatedAt = b.UpdatedAt,
            RowVersion = Convert.ToBase64String(b.RowVersion ?? Array.Empty<byte>())
        };
}
