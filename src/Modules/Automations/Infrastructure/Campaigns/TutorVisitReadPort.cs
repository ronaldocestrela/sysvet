using Automations.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Petshop.Domain.Enums;
using Petshop.Infrastructure.Persistence;
using Veterinary.Domain.Enums;
using Veterinary.Infrastructure.Persistence;

namespace Automations.Infrastructure.Campaigns;

/// <summary>
/// Aggregates completed clinical and grooming visits for segmentation and reporting.
/// </summary>
public sealed class TutorVisitReadPort : ITutorVisitReadPort
{
    private readonly VeterinaryDbContext _veterinaryDbContext;
    private readonly PetshopDbContext _petshopDbContext;

    public TutorVisitReadPort(VeterinaryDbContext veterinaryDbContext, PetshopDbContext petshopDbContext)
    {
        _veterinaryDbContext = veterinaryDbContext;
        _petshopDbContext = petshopDbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TutorLastVisitRow>> ListLastCompletedVisitsAsync(CancellationToken cancellationToken = default)
    {
        var vet = (await _veterinaryDbContext.Appointments.ToListAsync(cancellationToken))
            .Where(a => a.Status == AppointmentStatus.Completed)
            .GroupBy(a => a.TutorId)
            .Select(g => new { TutorId = g.Key, Last = g.Max(a => a.UpdatedAt) });

        var groom = (await _petshopDbContext.GroomingAppointments.ToListAsync(cancellationToken))
            .Where(a => a.Status == GroomingAppointmentStatus.Completed)
            .GroupBy(a => a.TutorId)
            .Select(g => new { TutorId = g.Key, Last = g.Max(a => a.UpdatedAt) });

        var map = new Dictionary<Guid, DateTimeOffset>();
        foreach (var row in vet.Concat(groom))
        {
            if (!map.TryGetValue(row.TutorId, out var existing) || row.Last > existing)
            {
                map[row.TutorId] = row.Last;
            }
        }

        return map.Select(kvp => new TutorLastVisitRow(kvp.Key, kvp.Value)).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CompletedServiceRow>> ListCompletedBetweenAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtcExclusive,
        CancellationToken cancellationToken = default)
    {
        var vet = (await _veterinaryDbContext.Appointments.ToListAsync(cancellationToken))
            .Where(a => a.Status == AppointmentStatus.Completed
                        && a.UpdatedAt >= fromUtc
                        && a.UpdatedAt < toUtcExclusive)
            .Select(a => new CompletedServiceRow(a.TutorId, a.PetId, a.Id, "Appointment", a.UpdatedAt));

        var groom = (await _petshopDbContext.GroomingAppointments.ToListAsync(cancellationToken))
            .Where(a => a.Status == GroomingAppointmentStatus.Completed
                        && a.UpdatedAt >= fromUtc
                        && a.UpdatedAt < toUtcExclusive)
            .Select(a => new CompletedServiceRow(a.TutorId, a.PetId, a.Id, "GroomingAppointment", a.UpdatedAt));

        return vet.Concat(groom).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TutorReturnFrequencyRow>> GetReturnFrequencyAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        var vetVisits = (await _veterinaryDbContext.Appointments.ToListAsync(cancellationToken))
            .Where(a => a.Status == AppointmentStatus.Completed && a.UpdatedAt >= from && a.UpdatedAt <= to)
            .Select(a => new { a.TutorId, a.UpdatedAt });

        var groomVisits = (await _petshopDbContext.GroomingAppointments.ToListAsync(cancellationToken))
            .Where(a => a.Status == GroomingAppointmentStatus.Completed && a.UpdatedAt >= from && a.UpdatedAt <= to)
            .Select(a => new { a.TutorId, a.UpdatedAt });

        return vetVisits
            .Concat(groomVisits)
            .GroupBy(v => v.TutorId)
            .Select(g =>
            {
                var ordered = g.Select(x => x.UpdatedAt).OrderBy(x => x).ToList();
                double? avg = null;
                if (ordered.Count > 1)
                {
                    var gaps = new List<double>();
                    for (var i = 1; i < ordered.Count; i++)
                    {
                        gaps.Add((ordered[i] - ordered[i - 1]).TotalDays);
                    }

                    avg = gaps.Average();
                }

                return new TutorReturnFrequencyRow(
                    g.Key,
                    ordered.Count,
                    ordered.LastOrDefault(),
                    avg);
            })
            .ToList();
    }
}
