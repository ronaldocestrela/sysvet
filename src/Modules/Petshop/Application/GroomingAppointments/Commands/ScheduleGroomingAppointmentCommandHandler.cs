using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using Petshop.Application.GroomingAppointments;

namespace Petshop.Application.GroomingAppointments.Commands;

/// <summary>
/// Books a grooming appointment and opens a draft digital record seeded from the service catalog.
/// </summary>
public sealed class ScheduleGroomingAppointmentCommandHandler : IRequestHandler<ScheduleGroomingAppointmentCommand, Result<Guid>>
{
    private readonly IGroomingAppointmentScheduler _scheduler;

    /// <summary>Creates the handler with the shared grooming scheduler.</summary>
    public ScheduleGroomingAppointmentCommandHandler(IGroomingAppointmentScheduler scheduler) => _scheduler = scheduler;

    /// <inheritdoc />
    public Task<Result<Guid>> Handle(ScheduleGroomingAppointmentCommand request, CancellationToken cancellationToken) =>
        _scheduler.ScheduleAsync(
            request.Id,
            request.TutorId,
            request.PetId,
            request.GroomerId,
            request.GroomingServiceId,
            request.Date,
            request.DurationInMinutes,
            request.Notes,
            cancellationToken);
}
