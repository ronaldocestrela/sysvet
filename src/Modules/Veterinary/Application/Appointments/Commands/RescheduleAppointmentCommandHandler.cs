using Core.Application.Messaging;
using Core.Domain;
using MediatR;
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
        if (appointment == null)
        {
            return Result.Failure(Veterinary.Domain.ErrorCodes.Appointment.NotFound);
        }

        var startTime = request.NewDate.TimeOfDay;
        var endTime = startTime.Add(TimeSpan.FromMinutes(appointment.DurationInMinutes));

        var availableSlots = await _scheduleSlotRepository.GetAvailableSlotsAsync(
            appointment.VeterinarianId,
            request.NewDate,
            cancellationToken);

        var slot = availableSlots.FirstOrDefault(s => s.StartTime <= startTime && s.EndTime >= endTime);
        if (slot == null)
        {
            return Result.Failure(Veterinary.Domain.ErrorCodes.Appointment.SlotUnavailable);
        }

        var oldDate = appointment.Date;

        var rescheduleResult = appointment.Reschedule(request.NewDate);
        if (rescheduleResult.IsFailure)
        {
            return rescheduleResult;
        }

        slot.Book();
        _scheduleSlotRepository.Update(slot);

        _appointmentRepository.Update(appointment);

        return Result.Success();
    }
}
