using Core.Application.IntegrationEvents;
using Core.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sales.Domain.Enums;
using Sales.Domain.Services;
using Sales.Infrastructure.Persistence;

namespace Sales.Infrastructure.Integration;

/// <summary>Computes hourly paid POS totals for chart widgets.</summary>
public sealed class GetSalesByHourKpisRequestHandler
    : IRequestHandler<GetSalesByHourKpisRequest, Result<SalesByHourKpiSnapshot>>
{
    private static readonly OrderStatus[] CountedStatuses =
    [
        OrderStatus.Paid,
        OrderStatus.PartiallyRefunded,
        OrderStatus.PartiallyReturned
    ];

    private readonly SalesDbContext _dbContext;

    /// <summary>Creates the handler.</summary>
    public GetSalesByHourKpisRequestHandler(SalesDbContext dbContext) => _dbContext = dbContext;

    /// <inheritdoc />
    public async Task<Result<SalesByHourKpiSnapshot>> Handle(
        GetSalesByHourKpisRequest request,
        CancellationToken cancellationToken)
    {
        TimeZoneInfo timeZone;
        try
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById(request.TimeZoneId);
        }
        catch
        {
            timeZone = TimeZoneInfo.Utc;
        }

        var orders = await _dbContext.Orders
            .AsNoTracking()
            .Where(o => CountedStatuses.Contains(o.Status)
                        && o.PaidAt >= request.PeriodStartUtc
                        && o.PaidAt < request.PeriodEndUtc)
            .Select(o => new { o.Id, o.DiscountPercent, o.PaidAt })
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

        var buckets = Enumerable.Range(0, 24).ToDictionary(h => h, h => new SalesHourBucketSnapshot { Hour = h, Amount = 0, OrderCount = 0 });

        foreach (var order in orders)
        {
            if (!order.PaidAt.HasValue || !subtotals.TryGetValue(order.Id, out var subtotal))
            {
                continue;
            }

            var local = TimeZoneInfo.ConvertTime(order.PaidAt.Value, timeZone);
            var hour = local.Hour;
            var net = subtotal - OrderPricing.ComputeDiscountAmount(subtotal, order.DiscountPercent);
            var bucket = buckets[hour];
            buckets[hour] = new SalesHourBucketSnapshot
            {
                Hour = hour,
                Amount = bucket.Amount + net,
                OrderCount = bucket.OrderCount + 1
            };
        }

        return Result.Success(new SalesByHourKpiSnapshot
        {
            Buckets = buckets.Values.OrderBy(b => b.Hour).ToList()
        });
    }
}
