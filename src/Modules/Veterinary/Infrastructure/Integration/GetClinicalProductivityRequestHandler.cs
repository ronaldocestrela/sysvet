using Core.Application.IntegrationEvents;
using Core.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Veterinary.Domain.Enums;
using Veterinary.Infrastructure.Persistence;

namespace Veterinary.Infrastructure.Integration;

/// <summary>Counts completed appointments by veterinarian for productivity reports (10.3).</summary>
public sealed class GetClinicalProductivityRequestHandler
    : IRequestHandler<GetClinicalProductivityRequest, Result<ClinicalProductivitySnapshot>>
{
    private readonly VeterinaryDbContext _dbContext;

    /// <summary>Creates the handler.</summary>
    public GetClinicalProductivityRequestHandler(VeterinaryDbContext dbContext) => _dbContext = dbContext;

    /// <inheritdoc />
    public async Task<Result<ClinicalProductivitySnapshot>> Handle(
        GetClinicalProductivityRequest request,
        CancellationToken cancellationToken)
    {
        var rows = await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.Status == AppointmentStatus.Completed
                        && a.Date >= request.PeriodStartUtc
                        && a.Date < request.PeriodEndUtc)
            .GroupBy(a => a.VeterinarianId)
            .Select(g => new ClinicalProductivityRow(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        return Result.Success(new ClinicalProductivitySnapshot { Rows = rows });
    }
}
