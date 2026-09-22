using Core.Application.Messaging;
using Core.Domain;
using MediatR;

namespace Veterinary.Application.Appointments.Commands;

/// <summary>MediatR adapter for staff clinical appointment scheduling.</summary>
public class ScheduleAppointmentCommandHandler : IRequestHandler<ScheduleAppointmentCommand, Result<Guid>>
{
    private readonly IAppointmentScheduler _scheduler;

    /// <summary>Creates the handler with the shared appointment scheduler.</summary>
    public ScheduleAppointmentCommandHandler(IAppointmentScheduler scheduler) => _scheduler = scheduler;

    /// <inheritdoc />
    public Task<Result<Guid>> Handle(ScheduleAppointmentCommand request, CancellationToken cancellationToken) =>
        _scheduler.ScheduleAsync(
            request.Id,
            request.TutorId,
            request.PetId,
            request.VeterinarianId,
            request.Date,
            request.DurationInMinutes,
            request.Reason,
            cancellationToken);
}
