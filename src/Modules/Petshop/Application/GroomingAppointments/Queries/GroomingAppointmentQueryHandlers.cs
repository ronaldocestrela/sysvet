using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using Petshop.Application.GroomingAppointments.DTOs;
using Petshop.Domain.Repositories;

namespace Petshop.Application.GroomingAppointments.Queries;

using ErrorCodes = Petshop.Domain.ErrorCodes;

public sealed class GetDailyGroomingScheduleQueryHandler : IRequestHandler<GetDailyGroomingScheduleQuery, Result<List<GroomingAppointmentDto>>>
{
    private readonly IGroomingAppointmentRepository _repository;

    public GetDailyGroomingScheduleQueryHandler(IGroomingAppointmentRepository repository) => _repository = repository;

    public async Task<Result<List<GroomingAppointmentDto>>> Handle(GetDailyGroomingScheduleQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.GetByDayAsync(request.GroomerId, request.Date, cancellationToken);
        var dtos = items.Select(Map).ToList();
        return Result.Success(dtos);
    }

    private static GroomingAppointmentDto Map(Domain.Entities.GroomingAppointment a) =>
        new(a.Id, a.TutorId, a.PetId, a.GroomerId, a.GroomingServiceId, a.Date, a.DurationInMinutes, a.Notes, a.Status);
}

public sealed class GetGroomingAppointmentByIdQueryHandler : IRequestHandler<GetGroomingAppointmentByIdQuery, Result<GroomingAppointmentDto>>
{
    private readonly IGroomingAppointmentRepository _repository;

    public GetGroomingAppointmentByIdQueryHandler(IGroomingAppointmentRepository repository) => _repository = repository;

    public async Task<Result<GroomingAppointmentDto>> Handle(GetGroomingAppointmentByIdQuery request, CancellationToken cancellationToken)
    {
        var appointment = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (appointment is null)
        {
            return Result.Failure<GroomingAppointmentDto>(ErrorCodes.GroomingAppointment.NotFound);
        }

        return Result.Success(new GroomingAppointmentDto(
            appointment.Id,
            appointment.TutorId,
            appointment.PetId,
            appointment.GroomerId,
            appointment.GroomingServiceId,
            appointment.Date,
            appointment.DurationInMinutes,
            appointment.Notes,
            appointment.Status));
    }
}
