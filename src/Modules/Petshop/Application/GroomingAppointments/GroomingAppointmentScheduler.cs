using Core.Domain;
using Petshop.Domain.Entities;
using Petshop.Domain.Repositories;

namespace Petshop.Application.GroomingAppointments;

using ErrorCodes = Petshop.Domain.ErrorCodes;

/// <summary>
/// Implements grooming appointment scheduling with slot booking and record seeding.
/// </summary>
public sealed class GroomingAppointmentScheduler : IGroomingAppointmentScheduler
{
    private readonly IGroomingAppointmentRepository _appointmentRepository;
    private readonly IGroomingSlotRepository _slotRepository;
    private readonly IGroomingServiceRepository _serviceRepository;
    private readonly IGroomingRecordRepository _recordRepository;

    /// <summary>Creates the scheduler with grooming repositories.</summary>
    public GroomingAppointmentScheduler(
        IGroomingAppointmentRepository appointmentRepository,
        IGroomingSlotRepository slotRepository,
        IGroomingServiceRepository serviceRepository,
        IGroomingRecordRepository recordRepository)
    {
        _appointmentRepository = appointmentRepository;
        _slotRepository = slotRepository;
        _serviceRepository = serviceRepository;
        _recordRepository = recordRepository;
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> ScheduleAsync(
        Guid id,
        Guid tutorId,
        Guid petId,
        Guid groomerId,
        Guid groomingServiceId,
        DateTimeOffset date,
        int durationInMinutes,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        var existing = await _appointmentRepository.GetByIdAsync(id, cancellationToken);
        if (existing is not null)
        {
            return Result.Success(existing.Id);
        }

        var service = await _serviceRepository.GetByIdAsync(groomingServiceId, cancellationToken);
        if (service is null || !service.IsActive)
        {
            return Result.Failure<Guid>(ErrorCodes.GroomingService.NotFound);
        }

        var duration = durationInMinutes > 0 ? durationInMinutes : service.DurationInMinutes;

        if (await _appointmentRepository.HasOverlappingAsync(
                groomerId,
                date,
                duration,
                null,
                cancellationToken))
        {
            return Result.Failure<Guid>(ErrorCodes.GroomingAppointment.Overlap);
        }

        var availableSlots = await _slotRepository.GetAvailableSlotsAsync(groomerId, date, cancellationToken);
        var slot = GroomingSlotHelper.FindCoveringSlot(availableSlots, date, duration);
        if (slot is null)
        {
            return Result.Failure<Guid>(ErrorCodes.GroomingAppointment.SlotUnavailable);
        }

        var bookResult = slot.Book();
        if (bookResult.IsFailure)
        {
            return Result.Failure<Guid>(bookResult.Error);
        }

        _slotRepository.Update(slot);

        var appointmentResult = GroomingAppointment.Create(
            id,
            tutorId,
            petId,
            groomerId,
            groomingServiceId,
            date,
            duration,
            notes ?? string.Empty);

        if (appointmentResult.IsFailure)
        {
            return Result.Failure<Guid>(appointmentResult.Error);
        }

        var recordResult = GroomingRecord.Create(
            Guid.NewGuid(),
            appointmentResult.Value.Id,
            groomerId,
            tutorId,
            petId);
        if (recordResult.IsFailure)
        {
            return Result.Failure<Guid>(recordResult.Error);
        }

        var seed = recordResult.Value.SeedSuppliesFromService(service);
        if (seed.IsFailure)
        {
            return Result.Failure<Guid>(seed.Error);
        }

        await _appointmentRepository.AddAsync(appointmentResult.Value, cancellationToken);
        await _recordRepository.AddAsync(recordResult.Value, cancellationToken);
        return Result.Success(appointmentResult.Value.Id);
    }

    /// <inheritdoc />
    public async Task<Result> CancelAndReleaseSlotAsync(Guid groomingAppointmentId, CancellationToken cancellationToken = default)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(groomingAppointmentId, cancellationToken);
        if (appointment is null)
        {
            return Result.Failure(ErrorCodes.GroomingAppointment.NotFound);
        }

        var result = appointment.Cancel();
        if (result.IsFailure)
        {
            return result;
        }

        await ReleaseSlotAsync(appointment, cancellationToken);
        _appointmentRepository.Update(appointment);
        return Result.Success();
    }

    internal static async Task ReleaseSlotAsync(
        GroomingAppointment appointment,
        IGroomingSlotRepository slotRepository,
        CancellationToken cancellationToken)
    {
        var slots = await slotRepository.GetAllSlotsForDayAsync(appointment.GroomerId, appointment.Date, cancellationToken);
        var slot = GroomingSlotHelper.FindCoveringSlot(slots, appointment.Date, appointment.DurationInMinutes);
        if (slot is not null && !slot.IsAvailable)
        {
            slot.CancelBooking();
            slotRepository.Update(slot);
        }
    }

    private Task ReleaseSlotAsync(GroomingAppointment appointment, CancellationToken cancellationToken) =>
        ReleaseSlotAsync(appointment, _slotRepository, cancellationToken);
}
