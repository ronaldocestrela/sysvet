using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using Veterinary.Application.Appointments.DTOs;
using Veterinary.Domain.Repositories;

namespace Veterinary.Application.Appointments.Queries;

public class GetAppointmentByIdQueryHandler : IRequestHandler<GetAppointmentByIdQuery, Result<AppointmentDto>>
{
    private readonly IAppointmentRepository _appointmentRepository;

    public GetAppointmentByIdQueryHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<Result<AppointmentDto>> Handle(GetAppointmentByIdQuery request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (appointment is null)
        {
            return Result.Failure<AppointmentDto>(Veterinary.Domain.ErrorCodes.Appointment.NotFound);
        }

        return Result.Success(new AppointmentDto
        {
            Id = appointment.Id,
            TutorId = appointment.TutorId,
            PetId = appointment.PetId,
            VeterinarianId = appointment.VeterinarianId,
            Date = appointment.Date,
            DurationInMinutes = appointment.DurationInMinutes,
            Status = appointment.Status.ToString(),
            Reason = appointment.Reason
        });
    }
}
