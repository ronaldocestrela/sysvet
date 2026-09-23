using Core.Application.IntegrationEvents;
using Core.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sales.Domain.Enums;
using Sales.Domain.Services;
using Sales.Infrastructure.Persistence;

namespace Sales.Infrastructure.Integration;

/// <summary>Computes net paid POS sales for a UTC business-day window.</summary>
public sealed class GetSalesTodayKpisRequestHandler
    : IRequestHandler<GetSalesTodayKpisRequest, Result<SalesTodayKpiSnapshot>>
{
    private static readonly OrderStatus[] CountedStatuses =
    [
        OrderStatus.Paid,
        OrderStatus.PartiallyRefunded,
        OrderStatus.PartiallyReturned
    ];

    private readonly SalesDbContext _dbContext;

    /// <summary>Creates the handler.</summary>
    public GetSalesTodayKpisRequestHandler(SalesDbContext dbContext) => _dbContext = dbContext;

    /// <inheritdoc />
    public async Task<Result<SalesTodayKpiSnapshot>> Handle(
        GetSalesTodayKpisRequest request,
        CancellationToken cancellationToken)
    {
        var orders = await _dbContext.Orders
            .AsNoTracking()
            .Where(o => CountedStatuses.Contains(o.Status)
                        && o.PaidAt >= request.PeriodStartUtc
                        && o.PaidAt < request.PeriodEndUtc)
            .Select(o => new { o.Id, o.DiscountPercent })
            .ToListAsync(cancellationToken);

        var orderIds = orders.Select(o => o.Id).ToList();
        var subtotals = orderIds.Count == 0
            ? new Dictionary<Guid, decimal>()
            : await _dbContext.OrderItems
                .AsNoTracking()
                .Where(i => orderIds.Contains(i.OrderId))
                .GroupBy(i => i.OrderId)
                .Select(g => new { OrderId = g.Key, Subtotal = g.Sum(i => i.Quantity * i.UnitPrice.Amount) })
                .ToDictionaryAsync(x => x.OrderId, x => x.Subtotal, cancellationToken);

        decimal gross = 0;
        foreach (var order in orders)
        {
            if (subtotals.TryGetValue(order.Id, out var subtotal))
            {
                gross += subtotal - OrderPricing.ComputeDiscountAmount(subtotal, order.DiscountPercent);
            }
        }

        var returnAmounts = await _dbContext.SaleReturns
            .AsNoTracking()
            .Where(r => r.CreatedAt >= request.PeriodStartUtc && r.CreatedAt < request.PeriodEndUtc)
            .Select(r => r.RefundAmount.Amount)
            .ToListAsync(cancellationToken);
        var returns = returnAmounts.Sum();

        var net = gross - returns;
        var count = orders.Count;
        var average = count == 0 ? 0 : decimal.Round(net / count, 2, MidpointRounding.AwayFromZero);

        return Result.Success(new SalesTodayKpiSnapshot
        {
            NetAmount = net,
            OrderCount = count,
            AverageTicket = average
        });
    }
}
