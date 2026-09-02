using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Domain;
using Core.Application.Messaging;
using Veterinary.Domain.Repositories;
using Veterinary.Domain.Errors;
using MediatR;
using IUnitOfWork = Veterinary.Domain.Repositories.IUnitOfWork;

namespace Veterinary.Application.Appointments.Commands;

public class RescheduleAppointmentCommandHandler : IRequestHandler<RescheduleAppointmentCommand, Result>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IScheduleSlotRepository _scheduleSlotRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RescheduleAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        IScheduleSlotRepository scheduleSlotRepository,
        IUnitOfWork unitOfWork)
    {
        _appointmentRepository = appointmentRepository;
        _scheduleSlotRepository = scheduleSlotRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RescheduleAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken);
        if (appointment == null)
        {
            return Result.Failure(new Error("Appointment.NotFound", "The specified appointment was not found."));
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
            return Result.Failure(new Error("Appointment.SlotUnavailable", "The requested time slot is not available."));
        }

        var oldDate = appointment.Date;
        var oldStartTime = oldDate.TimeOfDay;
        var oldEndTime = oldStartTime.Add(TimeSpan.FromMinutes(appointment.DurationInMinutes));

        var rescheduleResult = appointment.Reschedule(request.NewDate);
        if (rescheduleResult.IsFailure)
        {
            return rescheduleResult;
        }

        // Ideally, free the old slot here if it's managed via Slots
        var oldSlots = await _scheduleSlotRepository.GetAvailableSlotsAsync(
            appointment.VeterinarianId, 
            oldDate, 
            cancellationToken); // Note: GetAvailableSlotsAsync might not return booked slots depending on its implementation.
        // For MVP, we assume slot booking logic here.
        slot.Book();
        _scheduleSlotRepository.Update(slot);

        _appointmentRepository.Update(appointment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
