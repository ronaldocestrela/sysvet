using Core.Domain;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Enums;

namespace Clients.Infrastructure.Sync;

/// <summary>
/// Applies remote pull DTOs to local SQLite without enqueueing outbox messages.
/// </summary>
public sealed class OfflineSyncPullApplier
{
    private readonly OfflineDbContext _dbContext;

    /// <summary>Creates the applier.</summary>
    public OfflineSyncPullApplier(OfflineDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Upserts tutors and pets from a pull page and advances the sync cursor.
    /// </summary>
    public async Task ApplyAsync(ClientPullChangesResult page, CancellationToken cancellationToken = default)
    {
        _dbContext.SuppressOutbox = true;
        try
        {
            foreach (var dto in page.Tutors)
            {
                await UpsertTutorAsync(dto, cancellationToken);
            }

            foreach (var dto in page.Pets)
            {
                await UpsertPetAsync(dto, cancellationToken);
            }

            foreach (var dto in page.Appointments)
            {
                await UpsertAppointmentAsync(dto, cancellationToken);
            }

            foreach (var dto in page.ScheduleSlots)
            {
                await UpsertScheduleSlotAsync(dto, cancellationToken);
            }

            var state = await _dbContext.SyncState.FindAsync([1], cancellationToken)
                        ?? _dbContext.SyncState.Add(new SyncState()).Entity;
            state.LastPullAt = page.NextSince;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            _dbContext.SuppressOutbox = false;
        }
    }

    private async Task UpsertTutorAsync(ClientSyncTutorDto dto, CancellationToken cancellationToken)
    {
        var tutor = await _dbContext.Tutors.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == dto.Id, cancellationToken);
        if (tutor is null)
        {
            var email = Email.Create(dto.Email);
            var cpf = Cpf.Create(dto.Cpf);
            var phone = Phone.Create(dto.Phone);
            if (email.IsFailure || cpf.IsFailure || phone.IsFailure)
            {
                return;
            }

            var created = Tutor.Create(dto.Name, email.Value, cpf.Value, phone.Value, dto.Id);
            if (created.IsFailure)
            {
                return;
            }

            tutor = created.Value;
            tutor.UpdatedAt = dto.UpdatedAt;
            if (dto.IsDeleted)
            {
                tutor.SoftDelete();
            }

            _dbContext.Tutors.Add(tutor);
            return;
        }

        if (dto.UpdatedAt <= tutor.UpdatedAt)
        {
            return;
        }

        if (dto.IsDeleted)
        {
            tutor.SoftDelete();
            tutor.UpdatedAt = dto.UpdatedAt;
            return;
        }

        var emailResult = Email.Create(dto.Email);
        var phoneResult = Phone.Create(dto.Phone);
        if (emailResult.IsFailure || phoneResult.IsFailure)
        {
            return;
        }

        tutor.Update(dto.Name, emailResult.Value, phoneResult.Value);
        tutor.UpdatedAt = dto.UpdatedAt;
    }

    private async Task UpsertPetAsync(ClientSyncPetDto dto, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<PetSpecies>(dto.Species, out var species))
        {
            return;
        }

        if (!Enum.TryParse<PetSex>(dto.Sex, out var sex))
        {
            return;
        }

        var pet = await _dbContext.Pets.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == dto.Id, cancellationToken);
        if (pet is null)
        {
            var created = Pet.Create(dto.Name, species, dto.Breed, sex, dto.TutorId, dto.Id);
            if (created.IsFailure)
            {
                return;
            }

            pet = created.Value;
            pet.UpdatedAt = dto.UpdatedAt;
            if (dto.IsDeleted)
            {
                pet.SoftDelete();
            }

            _dbContext.Pets.Add(pet);
            return;
        }

        if (dto.UpdatedAt <= pet.UpdatedAt)
        {
            return;
        }

        if (dto.IsDeleted)
        {
            pet.SoftDelete();
            pet.UpdatedAt = dto.UpdatedAt;
            return;
        }

        pet.Update(dto.Name, species, dto.Breed, sex);
        pet.UpdatedAt = dto.UpdatedAt;
    }

    private async Task UpsertAppointmentAsync(ClientSyncAppointmentDto dto, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AppointmentStatus>(dto.Status, out var status))
        {
            return;
        }

        var appointment = await _dbContext.Appointments.FindAsync([dto.Id], cancellationToken);
        if (appointment is null)
        {
            var created = Appointment.RestoreFromSync(
                dto.Id,
                dto.TutorId,
                dto.PetId,
                dto.VeterinarianId,
                dto.Date,
                dto.DurationInMinutes,
                dto.Reason,
                status,
                dto.UpdatedAt);
            _dbContext.Appointments.Add(created);
            return;
        }

        if (dto.UpdatedAt <= appointment.UpdatedAt)
        {
            return;
        }

        appointment.ApplySyncSnapshot(dto.Date, dto.DurationInMinutes, dto.Reason, status, dto.UpdatedAt);
    }

    private async Task UpsertScheduleSlotAsync(ClientSyncScheduleSlotDto dto, CancellationToken cancellationToken)
    {
        var slot = await _dbContext.ScheduleSlots.FindAsync([dto.Id], cancellationToken);
        if (slot is null)
        {
            slot = new ScheduleSlot(dto.Id, dto.VeterinarianId, dto.Date, dto.StartTime, dto.EndTime);
            slot.UpdatedAt = dto.UpdatedAt;
            if (!dto.IsAvailable)
            {
                slot.Block();
            }

            _dbContext.ScheduleSlots.Add(slot);
            return;
        }

        if (dto.UpdatedAt <= slot.UpdatedAt)
        {
            return;
        }

        if (dto.IsAvailable)
        {
            slot.Unblock();
        }
        else if (slot.IsAvailable)
        {
            slot.Block();
        }

        slot.UpdatedAt = dto.UpdatedAt;
    }
}
