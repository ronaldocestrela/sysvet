using Core.Domain;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Application.Appointments;

/// <summary>
/// Implements clinical appointment scheduling with slot booking and overlap checks.
/// </summary>
public sealed class AppointmentScheduler : IAppointmentScheduler
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IScheduleSlotRepository _scheduleSlotRepository;

    /// <summary>Creates the scheduler with appointment and slot repositories.</summary>
    public AppointmentScheduler(
        IAppointmentRepository appointmentRepository,
        IScheduleSlotRepository scheduleSlotRepository)
    {
        _appointmentRepository = appointmentRepository;
        _scheduleSlotRepository = scheduleSlotRepository;
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> ScheduleAsync(
        Guid id,
        Guid tutorId,
        Guid petId,
        Guid veterinarianId,
        DateTimeOffset date,
        int durationInMinutes,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var existing = await _appointmentRepository.GetByIdAsync(id, cancellationToken);
        if (existing is not null)
        {
            return Result.Success(existing.Id);
        }

        if (await _appointmentRepository.HasOverlappingAsync(
                veterinarianId,
                date,
                durationInMinutes,
                null,
                cancellationToken))
        {
            return Result.Failure<Guid>(Veterinary.Domain.ErrorCodes.Appointment.Overlap);
        }

        var availableSlots = await _scheduleSlotRepository.GetAvailableSlotsAsync(
            veterinarianId,
            date,
            cancellationToken);

        var slot = AppointmentSlotHelper.FindCoveringSlot(availableSlots, date, durationInMinutes);
        if (slot is null)
        {
            return Result.Failure<Guid>(Veterinary.Domain.ErrorCodes.Appointment.SlotUnavailable);
        }

        var bookResult = slot.Book();
        if (bookResult.IsFailure)
        {
            return Result.Failure<Guid>(bookResult.Error);
        }

        _scheduleSlotRepository.Update(slot);

        var appointmentResult = Appointment.Create(
            id,
            tutorId,
            petId,
            veterinarianId,
            date,
            durationInMinutes,
            reason);

        if (appointmentResult.IsFailure)
        {
            return Result.Failure<Guid>(appointmentResult.Error);
        }

        await _appointmentRepository.AddAsync(appointmentResult.Value, cancellationToken);
        return Result.Success(appointmentResult.Value.Id);
    }

    /// <inheritdoc />
    public async Task<Result> CancelAndReleaseSlotAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment is null)
        {
            return Result.Failure(Veterinary.Domain.ErrorCodes.Appointment.NotFound);
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
        Appointment appointment,
        IScheduleSlotRepository scheduleSlotRepository,
        CancellationToken cancellationToken)
    {
        var slots = await scheduleSlotRepository.GetAllSlotsForDayAsync(
            appointment.VeterinarianId,
            appointment.Date,
            cancellationToken);

        var slot = AppointmentSlotHelper.FindCoveringSlot(slots, appointment.Date, appointment.DurationInMinutes);
        if (slot is not null && !slot.IsAvailable)
        {
            slot.CancelBooking();
            scheduleSlotRepository.Update(slot);
        }
    }

    private Task ReleaseSlotAsync(Appointment appointment, CancellationToken cancellationToken) =>
        ReleaseSlotAsync(appointment, _scheduleSlotRepository, cancellationToken);
}
