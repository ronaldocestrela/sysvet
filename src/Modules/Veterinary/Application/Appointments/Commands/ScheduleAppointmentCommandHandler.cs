using Core.Domain;
using Core.Application.Messaging;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;
using IUnitOfWork = Veterinary.Domain.Repositories.IUnitOfWork;

using MediatR;

namespace Veterinary.Application.Appointments.Commands;

public class ScheduleAppointmentCommandHandler : IRequestHandler<ScheduleAppointmentCommand, Result<Guid>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IScheduleSlotRepository _scheduleSlotRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ScheduleAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        IScheduleSlotRepository scheduleSlotRepository,
        IUnitOfWork unitOfWork)
    {
        _appointmentRepository = appointmentRepository;
        _scheduleSlotRepository = scheduleSlotRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(ScheduleAppointmentCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar se o horário está disponível no ScheduleSlot
        var startTime = request.Date.TimeOfDay;
        var endTime = startTime.Add(TimeSpan.FromMinutes(request.DurationInMinutes));

        var availableSlots = await _scheduleSlotRepository.GetAvailableSlotsAsync(
            request.VeterinarianId, 
            request.Date, 
            cancellationToken);
            
        // Regra simples: para agendar, precisamos ter slots cobrindo o período inteiro
        // ou criar uma regra de checagem. Para MVP, assumimos que o request.Date deve iniciar em um Slot disponível.
        var slot = availableSlots.FirstOrDefault(s => s.StartTime <= startTime && s.EndTime >= endTime);
        
        if (slot == null)
        {
            return Result.Failure<Guid>(new Error("Appointment.SlotUnavailable", "The requested time slot is not available."));
        }

        // 2. Bloquear o slot
        slot.Book();
        _scheduleSlotRepository.Update(slot);

        // 3. Criar agendamento
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(appointment.Id);
    }
}
