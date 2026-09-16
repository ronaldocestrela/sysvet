using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Application.Appointments.Commands;

public class ScheduleAppointmentCommandHandler : IRequestHandler<ScheduleAppointmentCommand, Result<Guid>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IScheduleSlotRepository _scheduleSlotRepository;

    public ScheduleAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        IScheduleSlotRepository scheduleSlotRepository)
    {
        _appointmentRepository = appointmentRepository;
        _scheduleSlotRepository = scheduleSlotRepository;
    }

    public async Task<Result<Guid>> Handle(ScheduleAppointmentCommand request, CancellationToken cancellationToken)
    {
        var startTime = request.Date.TimeOfDay;
        var endTime = startTime.Add(TimeSpan.FromMinutes(request.DurationInMinutes));

        var availableSlots = await _scheduleSlotRepository.GetAvailableSlotsAsync(
            request.VeterinarianId,
            request.Date,
            cancellationToken);

        var slot = availableSlots.FirstOrDefault(s => s.StartTime <= startTime && s.EndTime >= endTime);

        if (slot == null)
        {
            return Result.Failure<Guid>(Veterinary.Domain.ErrorCodes.Appointment.SlotUnavailable);
        }

        slot.Book();
        _scheduleSlotRepository.Update(slot);

        var appointmentResult = Appointment.Create(
            Guid.NewGuid(),
            request.TutorId,
            request.PetId,
            request.VeterinarianId,
            request.Date,
            request.DurationInMinutes,
            request.Reason);

        if (appointmentResult.IsFailure)
        {
            return Result.Failure<Guid>(appointmentResult.Error);
        }

        var appointment = appointmentResult.Value;

        await _appointmentRepository.AddAsync(appointment, cancellationToken);

        return Result.Success(appointment.Id);
    }
}
