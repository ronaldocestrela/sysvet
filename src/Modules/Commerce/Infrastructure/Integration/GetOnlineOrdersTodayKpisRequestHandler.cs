using Commerce.Domain.Enums;
using Commerce.Infrastructure.Persistence;
using Core.Application.IntegrationEvents;
using Core.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Infrastructure.Integration;

/// <summary>Aggregates confirmed online orders for the business day.</summary>
public sealed class GetOnlineOrdersTodayKpisRequestHandler
    : IRequestHandler<GetOnlineOrdersTodayKpisRequest, Result<OnlineOrdersTodayKpiSnapshot>>
{
    private readonly CommerceDbContext _dbContext;

    /// <summary>Creates the handler.</summary>
    public GetOnlineOrdersTodayKpisRequestHandler(CommerceDbContext dbContext) => _dbContext = dbContext;

    /// <inheritdoc />
    public async Task<Result<OnlineOrdersTodayKpiSnapshot>> Handle(
        GetOnlineOrdersTodayKpisRequest request,
        CancellationToken cancellationToken)
    {
        var orders = await _dbContext.OnlineOrders
            .AsNoTracking()
            .Where(o => o.ConfirmedAt >= request.PeriodStartUtc
                        && o.ConfirmedAt < request.PeriodEndUtc
                        && o.Status != OnlineOrderStatus.Cancelled
                        && o.Status != OnlineOrderStatus.Placed)
            .Select(o => o.Id)
            .ToListAsync(cancellationToken);

        if (orders.Count == 0)
        {
            return Result.Success(new OnlineOrdersTodayKpiSnapshot());
        }

        var total = await _dbContext.OnlineOrderLines
            .AsNoTracking()
            .Where(l => orders.Contains(l.OnlineOrderId))
            .SumAsync(l => l.Quantity * l.UnitPrice.Amount, cancellationToken);

        return Result.Success(new OnlineOrdersTodayKpiSnapshot
        {
            OrderCount = orders.Count,
            TotalAmount = total
        });
    }
}
