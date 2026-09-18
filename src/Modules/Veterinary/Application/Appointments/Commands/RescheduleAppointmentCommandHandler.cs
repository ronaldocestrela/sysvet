using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using Veterinary.Application.Appointments;
using Veterinary.Domain.Repositories;

namespace Veterinary.Application.Appointments.Commands;

public class RescheduleAppointmentCommandHandler : IRequestHandler<RescheduleAppointmentCommand, Result>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IScheduleSlotRepository _scheduleSlotRepository;

    public RescheduleAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        IScheduleSlotRepository scheduleSlotRepository)
    {
        _appointmentRepository = appointmentRepository;
        _scheduleSlotRepository = scheduleSlotRepository;
    }

    public async Task<Result> Handle(RescheduleAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken);
        if (appointment is null)
        {
            return Result.Failure(Veterinary.Domain.ErrorCodes.Appointment.NotFound);
        }

        if (await _appointmentRepository.HasOverlappingAsync(
                appointment.VeterinarianId,
                request.NewDate,
                appointment.DurationInMinutes,
                appointment.Id,
                cancellationToken))
        {
            return Result.Failure(Veterinary.Domain.ErrorCodes.Appointment.Overlap);
        }

        var oldDate = appointment.Date;
        var allOldDaySlots = await _scheduleSlotRepository.GetAllSlotsForDayAsync(
            appointment.VeterinarianId,
            oldDate,
            cancellationToken);

        var oldSlot = AppointmentSlotHelper.FindCoveringSlot(allOldDaySlots, oldDate, appointment.DurationInMinutes);
        if (oldSlot is not null && !oldSlot.IsAvailable)
        {
            oldSlot.CancelBooking();
            _scheduleSlotRepository.Update(oldSlot);
        }

        var availableSlots = await _scheduleSlotRepository.GetAvailableSlotsAsync(
            appointment.VeterinarianId,
            request.NewDate,
            cancellationToken);

        var newSlot = AppointmentSlotHelper.FindCoveringSlot(availableSlots, request.NewDate, appointment.DurationInMinutes);
        if (newSlot is null)
        {
            return Result.Failure(Veterinary.Domain.ErrorCodes.Appointment.SlotUnavailable);
        }

        var bookResult = newSlot.Book();
        if (bookResult.IsFailure)
        {
            return bookResult;
        }

        _scheduleSlotRepository.Update(newSlot);

        var rescheduleResult = appointment.Reschedule(request.NewDate);
        if (rescheduleResult.IsFailure)
        {
            return rescheduleResult;
        }

        if (request.OccurredAt.HasValue)
        {
            appointment.UpdatedAt = request.OccurredAt.Value;
        }

        _appointmentRepository.Update(appointment);
        return Result.Success();
    }
}
