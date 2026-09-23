using Core.Application.IntegrationEvents;
using Core.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Veterinary.Infrastructure.Persistence;

namespace Veterinary.Infrastructure.Integration;

/// <summary>Counts clinical appointments scheduled on the business day.</summary>
public sealed class GetClinicalAppointmentsTodayKpisRequestHandler
    : IRequestHandler<GetClinicalAppointmentsTodayKpisRequest, Result<ClinicalAppointmentsTodayKpiSnapshot>>
{
    private readonly VeterinaryDbContext _dbContext;

    /// <summary>Creates the handler.</summary>
    public GetClinicalAppointmentsTodayKpisRequestHandler(VeterinaryDbContext dbContext) => _dbContext = dbContext;

    /// <inheritdoc />
    public async Task<Result<ClinicalAppointmentsTodayKpiSnapshot>> Handle(
        GetClinicalAppointmentsTodayKpisRequest request,
        CancellationToken cancellationToken)
    {
        var groups = await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.Date >= request.PeriodStartUtc && a.Date < request.PeriodEndUtc)
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToListAsync(cancellationToken);

        return Result.Success(new ClinicalAppointmentsTodayKpiSnapshot
        {
            ByStatus = groups.ToDictionary(x => x.Status, x => x.Count, StringComparer.Ordinal)
        });
    }
}
