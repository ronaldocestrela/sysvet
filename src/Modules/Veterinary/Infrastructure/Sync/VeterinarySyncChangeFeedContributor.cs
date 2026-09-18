using Core.Application.Sync;
using Microsoft.EntityFrameworkCore;
using Veterinary.Domain.Entities;
using Veterinary.Infrastructure.Persistence;

namespace Veterinary.Infrastructure.Sync;

/// <summary>
/// Exposes veterinary agenda rows to the Core sync pull feed.
/// </summary>
public sealed class VeterinarySyncChangeFeedContributor : ISyncChangeFeedContributor
{
    private readonly VeterinaryDbContext _dbContext;

    /// <summary>Creates the contributor.</summary>
    public VeterinarySyncChangeFeedContributor(VeterinaryDbContext dbContext) => _dbContext = dbContext;

    /// <inheritdoc />
    public async Task<SyncContributorChanges> ReadChangesAsync(DateTimeOffset since, int take, CancellationToken cancellationToken)
    {
        var appointments = await _dbContext.Appointments.AsNoTracking().ToListAsync(cancellationToken);
        var appointmentCandidates = appointments
            .Where(a => a.UpdatedAt > since)
            .OrderBy(a => a.UpdatedAt)
            .Take(take + 1)
            .ToList();

        var hasMoreAppointments = appointmentCandidates.Count > take;
        if (hasMoreAppointments)
        {
            appointmentCandidates = appointmentCandidates.Take(take).ToList();
        }

        var slots = await _dbContext.ScheduleSlots.AsNoTracking().ToListAsync(cancellationToken);
        var slotCandidates = slots
            .Where(s => s.UpdatedAt > since)
            .OrderBy(s => s.UpdatedAt)
            .Take(take + 1)
            .ToList();

        var hasMoreSlots = slotCandidates.Count > take;
        if (hasMoreSlots)
        {
            slotCandidates = slotCandidates.Take(take).ToList();
        }

        var maxUpdated = since;
        foreach (var a in appointmentCandidates)
        {
            if (a.UpdatedAt > maxUpdated)
            {
                maxUpdated = a.UpdatedAt;
            }
        }

        foreach (var s in slotCandidates)
        {
            if (s.UpdatedAt > maxUpdated)
            {
                maxUpdated = s.UpdatedAt;
            }
        }

        return new SyncContributorChanges
        {
            Appointments = appointmentCandidates.Select(MapAppointment).ToList(),
            ScheduleSlots = slotCandidates.Select(MapSlot).ToList(),
            MaxUpdatedAt = maxUpdated,
            HasMore = hasMoreAppointments || hasMoreSlots
        };
    }

    private static SyncAppointmentDto MapAppointment(Appointment appointment) =>
        new()
        {
            Id = appointment.Id,
            TutorId = appointment.TutorId,
            PetId = appointment.PetId,
            VeterinarianId = appointment.VeterinarianId,
            Date = appointment.Date,
            DurationInMinutes = appointment.DurationInMinutes,
            Reason = appointment.Reason,
            Status = appointment.Status.ToString(),
            UpdatedAt = appointment.UpdatedAt,
            RowVersion = Convert.ToBase64String(appointment.RowVersion ?? Array.Empty<byte>())
        };

    private static SyncScheduleSlotDto MapSlot(ScheduleSlot slot) =>
        new()
        {
            Id = slot.Id,
            VeterinarianId = slot.VeterinarianId,
            Date = slot.Date,
            StartTime = slot.StartTime,
            EndTime = slot.EndTime,
            IsAvailable = slot.IsAvailable,
            UpdatedAt = slot.UpdatedAt,
            RowVersion = Convert.ToBase64String(slot.RowVersion ?? Array.Empty<byte>())
        };
}
