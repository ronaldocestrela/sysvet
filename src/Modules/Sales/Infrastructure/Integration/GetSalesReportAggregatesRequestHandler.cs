using Core.Application.IntegrationEvents;
using Core.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sales.Domain.Enums;
using Sales.Infrastructure.Persistence;

namespace Sales.Infrastructure.Integration;

/// <summary>Aggregates paid POS lines for ABC and sales productivity (10.3).</summary>
public sealed class GetSalesReportAggregatesRequestHandler
    : IRequestHandler<GetSalesReportAggregatesRequest, Result<SalesReportAggregatesSnapshot>>
{
    private static readonly OrderStatus[] CountedStatuses =
    [
        OrderStatus.Paid,
        OrderStatus.PartiallyRefunded,
        OrderStatus.PartiallyReturned
    ];

    private readonly SalesDbContext _dbContext;

    /// <summary>Creates the handler.</summary>
    public GetSalesReportAggregatesRequestHandler(SalesDbContext dbContext) => _dbContext = dbContext;

    /// <inheritdoc />
    public async Task<Result<SalesReportAggregatesSnapshot>> Handle(
        GetSalesReportAggregatesRequest request,
        CancellationToken cancellationToken)
    {
        var ordersInWindow = (await _dbContext.Orders.AsNoTracking().ToListAsync(cancellationToken))
            .Where(o => CountedStatuses.Contains(o.Status)
                        && o.PaidAt >= request.PeriodStartUtc
                        && o.PaidAt < request.PeriodEndUtc)
            .Select(o => new { o.Id, o.TutorId })
            .ToList();

        var orderMap = ordersInWindow.ToDictionary(o => o.Id, o => o.TutorId);
        if (orderMap.Count == 0)
        {
            return Result.Success(new SalesReportAggregatesSnapshot());
        }

        var orderIdSet = orderMap.Keys.ToHashSet();
        var itemRows = (await _dbContext.OrderItems.AsNoTracking().ToListAsync(cancellationToken))
            .Where(i => orderIdSet.Contains(i.OrderId))
            .ToList();

        var lines = itemRows
            .Select(i => new LineRow(
                orderMap.GetValueOrDefault(i.OrderId),
                i.Kind,
                i.ProductId,
                i.ProductName,
                i.Quantity,
                i.ReturnedQuantity,
                i.UnitPrice.Amount,
                i.PerformerUserId))
            .ToList();

        var customers = lines
            .Where(l => l.TutorId is Guid tutorId && tutorId != Guid.Empty)
            .GroupBy(l => l.TutorId!.Value)
            .Select(g => new CustomerRevenueAggregateRow(g.Key, g.Sum(NetLine)))
            .ToList();

        var products = lines
            .Where(l => l.Kind == OrderItemKind.Product && l.ProductId is Guid productId && productId != Guid.Empty)
            .GroupBy(l => l.ProductId!.Value)
            .Select(g =>
            {
                var name = g.OrderByDescending(x => NetLine(x)).Select(x => x.ProductName).First();
                return new ProductRevenueAggregateRow(
                    g.Key,
                    name,
                    g.Sum(NetLine),
                    g.Sum(x => x.Quantity - x.ReturnedQuantity));
            })
            .ToList();

        var salesProductivity = lines
            .Where(l => l.PerformerUserId is Guid userId && userId != Guid.Empty)
            .GroupBy(l => l.PerformerUserId!.Value)
            .Select(g => new SalesProductivityAggregateRow(g.Key, g.Sum(NetLine), g.Sum(x => x.Quantity - x.ReturnedQuantity)))
            .ToList();

        return Result.Success(new SalesReportAggregatesSnapshot
        {
            Customers = customers,
            Products = products,
            SalesProductivity = salesProductivity
        });
    }

    private static decimal NetLine(LineRow line) =>
        (line.Quantity - line.ReturnedQuantity) * line.UnitPriceAmount;

    private sealed record LineRow(
        Guid? TutorId,
        OrderItemKind Kind,
        Guid? ProductId,
        string ProductName,
        decimal Quantity,
        decimal ReturnedQuantity,
        decimal UnitPriceAmount,
        Guid? PerformerUserId);
}
