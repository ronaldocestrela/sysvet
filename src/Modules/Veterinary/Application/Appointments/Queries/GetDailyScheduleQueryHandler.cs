using Core.Domain;
using Core.Application.Messaging;
using Veterinary.Application.Appointments.DTOs;
using Veterinary.Domain.Repositories;

using MediatR;

namespace Veterinary.Application.Appointments.Queries;

public class GetDailyScheduleQueryHandler : IRequestHandler<GetDailyScheduleQuery, Result<List<AppointmentDto>>>
{
    private readonly IAppointmentRepository _appointmentRepository;

    public GetDailyScheduleQueryHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<Result<List<AppointmentDto>>> Handle(GetDailyScheduleQuery request, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetByVeterinarianAndDateAsync(
            request.VeterinarianId, 
            request.Date, 
            cancellationToken);

        var dtos = appointments.Select(a => new AppointmentDto
        {
            Id = a.Id,
            TutorId = a.TutorId,
            PetId = a.PetId,
            VeterinarianId = a.VeterinarianId,
            Date = a.Date,
            DurationInMinutes = a.DurationInMinutes,
            Status = a.Status.ToString(),
            Notes = a.Notes
        }).OrderBy(a => a.Date).ToList();

        return Result.Success(dtos);
    }
}
