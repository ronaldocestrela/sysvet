using Core.Application.IntegrationEvents;
using Core.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Petshop.Infrastructure.Persistence;

namespace Petshop.Infrastructure.Integration;

/// <summary>Counts grooming appointments scheduled on the business day.</summary>
public sealed class GetGroomingTodayKpisRequestHandler
    : IRequestHandler<GetGroomingTodayKpisRequest, Result<GroomingTodayKpiSnapshot>>
{
    private readonly PetshopDbContext _dbContext;

    /// <summary>Creates the handler.</summary>
    public GetGroomingTodayKpisRequestHandler(PetshopDbContext dbContext) => _dbContext = dbContext;

    /// <inheritdoc />
    public async Task<Result<GroomingTodayKpiSnapshot>> Handle(
        GetGroomingTodayKpisRequest request,
        CancellationToken cancellationToken)
    {
        var groups = await _dbContext.GroomingAppointments
            .AsNoTracking()
            .Where(a => a.Date >= request.PeriodStartUtc && a.Date < request.PeriodEndUtc)
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToListAsync(cancellationToken);

        return Result.Success(new GroomingTodayKpiSnapshot
        {
            ByStatus = groups.ToDictionary(x => x.Status, x => x.Count, StringComparer.Ordinal)
        });
    }
}
