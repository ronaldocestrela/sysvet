using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using Veterinary.Application.Appointments;
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
        var existing = await _appointmentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (existing is not null)
        {
            return Result.Success(existing.Id);
        }

        if (await _appointmentRepository.HasOverlappingAsync(
                request.VeterinarianId,
                request.Date,
                request.DurationInMinutes,
                null,
                cancellationToken))
        {
            return Result.Failure<Guid>(Veterinary.Domain.ErrorCodes.Appointment.Overlap);
        }

        var availableSlots = await _scheduleSlotRepository.GetAvailableSlotsAsync(
            request.VeterinarianId,
            request.Date,
            cancellationToken);

        var slot = AppointmentSlotHelper.FindCoveringSlot(availableSlots, request.Date, request.DurationInMinutes);
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
            request.Id,
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

        await _appointmentRepository.AddAsync(appointmentResult.Value, cancellationToken);
        return Result.Success(appointmentResult.Value.Id);
    }
}
