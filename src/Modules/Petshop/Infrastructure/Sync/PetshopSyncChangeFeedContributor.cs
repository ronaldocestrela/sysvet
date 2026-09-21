using Core.Application.Sync;
using Microsoft.EntityFrameworkCore;
using Petshop.Infrastructure.Persistence;

namespace Petshop.Infrastructure.Sync;

/// <summary>
/// Exposes Petshop grooming rows to the Core sync pull feed.
/// </summary>
public sealed class PetshopSyncChangeFeedContributor : ISyncChangeFeedContributor
{
    private readonly PetshopDbContext _dbContext;

    public PetshopSyncChangeFeedContributor(PetshopDbContext dbContext) => _dbContext = dbContext;

    public async Task<SyncContributorChanges> ReadChangesAsync(DateTimeOffset since, int take, CancellationToken cancellationToken)
    {
        var appointments = await _dbContext.GroomingAppointments.AsNoTracking().ToListAsync(cancellationToken);
        var appointmentPage = PageByUpdatedAt(appointments, since, take);

        var slots = await _dbContext.GroomingSlots.AsNoTracking().ToListAsync(cancellationToken);
        var slotPage = PageByUpdatedAt(slots, since, take);

        var records = await _dbContext.GroomingRecords.AsNoTracking().Include(r => r.SupplyLines).ToListAsync(cancellationToken);
        var recordPage = PageByUpdatedAt(records, since, take);

        var services = await _dbContext.GroomingServices.AsNoTracking().Include(s => s.DefaultSupplies).ToListAsync(cancellationToken);
        var servicePage = PageByUpdatedAt(services, since, take);

        var maxUpdated = since;
        maxUpdated = Max(maxUpdated, appointmentPage.MaxUpdated);
        maxUpdated = Max(maxUpdated, slotPage.MaxUpdated);
        maxUpdated = Max(maxUpdated, recordPage.MaxUpdated);
        maxUpdated = Max(maxUpdated, servicePage.MaxUpdated);

        return new SyncContributorChanges
        {
            GroomingAppointments = appointmentPage.Items.Select(a => new SyncGroomingAppointmentDto
            {
                Id = a.Id,
                TutorId = a.TutorId,
                PetId = a.PetId,
                GroomerId = a.GroomerId,
                GroomingServiceId = a.GroomingServiceId,
                Date = a.Date,
                DurationInMinutes = a.DurationInMinutes,
                Notes = a.Notes,
                Status = a.Status.ToString(),
                UpdatedAt = a.UpdatedAt,
                RowVersion = Convert.ToBase64String(a.RowVersion ?? Array.Empty<byte>())
            }).ToList(),
            GroomingSlots = slotPage.Items.Select(s => new SyncGroomingSlotDto
            {
                Id = s.Id,
                GroomerId = s.GroomerId,
                Date = s.Date,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                IsAvailable = s.IsAvailable,
                UpdatedAt = s.UpdatedAt
            }).ToList(),
            GroomingRecords = recordPage.Items.Select(r => new SyncGroomingRecordDto
            {
                Id = r.Id,
                GroomingAppointmentId = r.GroomingAppointmentId,
                GroomerId = r.GroomerId,
                TutorId = r.TutorId,
                PetId = r.PetId,
                CoatNotes = r.CoatNotes,
                Status = r.Status.ToString(),
                UpdatedAt = r.UpdatedAt,
                SupplyLines = r.SupplyLines.Select(l => new SyncGroomingRecordSupplyLineDto
                {
                    Id = l.Id,
                    ProductId = l.ProductId,
                    Quantity = l.Quantity
                }).ToList()
            }).ToList(),
            GroomingServices = servicePage.Items.Select(s => new SyncGroomingServiceDto
            {
                Id = s.Id,
                Name = s.Name,
                ServiceType = s.ServiceType.ToString(),
                DurationInMinutes = s.DurationInMinutes,
                PrepaidServiceCode = s.PrepaidServiceCode,
                IsActive = s.IsActive,
                UpdatedAt = s.UpdatedAt,
                DefaultSupplies = s.DefaultSupplies.Select(l => new SyncGroomingServiceSupplyLineDto
                {
                    Id = l.Id,
                    ProductId = l.ProductId,
                    Quantity = l.Quantity
                }).ToList()
            }).ToList(),
            MaxUpdatedAt = maxUpdated,
            HasMore = appointmentPage.HasMore || slotPage.HasMore || recordPage.HasMore || servicePage.HasMore
        };
    }

    private static DateTimeOffset Max(DateTimeOffset a, DateTimeOffset b) => a > b ? a : b;

    private static (IReadOnlyList<T> Items, DateTimeOffset MaxUpdated, bool HasMore) PageByUpdatedAt<T>(
        IEnumerable<T> source,
        DateTimeOffset since,
        int take)
        where T : Core.Domain.Entity
    {
        var candidates = source.Where(x => x.UpdatedAt > since).OrderBy(x => x.UpdatedAt).Take(take + 1).ToList();
        var hasMore = candidates.Count > take;
        if (hasMore)
        {
            candidates = candidates.Take(take).ToList();
        }

        var max = since;
        foreach (var item in candidates)
        {
            if (item.UpdatedAt > max)
            {
                max = item.UpdatedAt;
            }
        }

        return (candidates, max, hasMore);
    }
}
