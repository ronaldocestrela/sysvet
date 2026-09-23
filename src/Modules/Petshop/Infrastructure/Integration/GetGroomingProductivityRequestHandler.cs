using Core.Application.IntegrationEvents;
using Core.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Petshop.Domain.Enums;
using Petshop.Infrastructure.Persistence;

namespace Petshop.Infrastructure.Integration;

/// <summary>Counts completed grooming appointments by groomer for productivity reports (10.3).</summary>
public sealed class GetGroomingProductivityRequestHandler
    : IRequestHandler<GetGroomingProductivityRequest, Result<GroomingProductivitySnapshot>>
{
    private readonly PetshopDbContext _dbContext;

    /// <summary>Creates the handler.</summary>
    public GetGroomingProductivityRequestHandler(PetshopDbContext dbContext) => _dbContext = dbContext;

    /// <inheritdoc />
    public async Task<Result<GroomingProductivitySnapshot>> Handle(
        GetGroomingProductivityRequest request,
        CancellationToken cancellationToken)
    {
        var rows = await _dbContext.GroomingAppointments
            .AsNoTracking()
            .Where(a => a.Status == GroomingAppointmentStatus.Completed
                        && a.Date >= request.PeriodStartUtc
                        && a.Date < request.PeriodEndUtc)
            .GroupBy(a => a.GroomerId)
            .Select(g => new GroomingProductivityRow(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        return Result.Success(new GroomingProductivitySnapshot { Rows = rows });
    }
}
