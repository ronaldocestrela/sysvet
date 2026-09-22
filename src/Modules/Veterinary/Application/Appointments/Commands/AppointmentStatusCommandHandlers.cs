using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using Veterinary.Application.Appointments;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Application.Appointments.Commands;

public sealed class ConfirmAppointmentCommandHandler : IRequestHandler<ConfirmAppointmentCommand, Result>
{
    private readonly IAppointmentRepository _repository;

    public ConfirmAppointmentCommandHandler(IAppointmentRepository repository) => _repository = repository;

    public Task<Result> Handle(ConfirmAppointmentCommand request, CancellationToken cancellationToken)
        => AppointmentStatusTransitions.ApplyAsync(_repository, request.AppointmentId, a => a.Confirm(), cancellationToken);
}

public sealed class StartAppointmentCommandHandler : IRequestHandler<StartAppointmentCommand, Result>
{
    private readonly IAppointmentRepository _repository;

    public StartAppointmentCommandHandler(IAppointmentRepository repository) => _repository = repository;

    public Task<Result> Handle(StartAppointmentCommand request, CancellationToken cancellationToken)
        => AppointmentStatusTransitions.ApplyAsync(_repository, request.AppointmentId, a => a.Start(), cancellationToken);
}

public sealed class CompleteAppointmentCommandHandler : IRequestHandler<CompleteAppointmentCommand, Result>
{
    private readonly IAppointmentRepository _repository;

    public CompleteAppointmentCommandHandler(IAppointmentRepository repository) => _repository = repository;

    public Task<Result> Handle(CompleteAppointmentCommand request, CancellationToken cancellationToken)
        => AppointmentStatusTransitions.ApplyAsync(_repository, request.AppointmentId, a => a.Complete(), cancellationToken);
}

public sealed class MarkNoShowAppointmentCommandHandler : IRequestHandler<MarkNoShowAppointmentCommand, Result>
{
    private readonly IAppointmentRepository _repository;
    private readonly IScheduleSlotRepository _scheduleSlotRepository;

    public MarkNoShowAppointmentCommandHandler(
        IAppointmentRepository repository,
        IScheduleSlotRepository scheduleSlotRepository)
    {
        _repository = repository;
        _scheduleSlotRepository = scheduleSlotRepository;
    }

    public async Task<Result> Handle(MarkNoShowAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _repository.GetByIdAsync(request.AppointmentId, cancellationToken);
        if (appointment is null)
        {
            return Result.Failure(Veterinary.Domain.ErrorCodes.Appointment.NotFound);
        }

        var result = appointment.MarkNoShow();
        if (result.IsFailure)
        {
            return result;
        }

        await AppointmentScheduler.ReleaseSlotAsync(appointment, _scheduleSlotRepository, cancellationToken);
        _repository.Update(appointment);
        return Result.Success();
    }
}

public sealed class CancelAppointmentCommandHandler : IRequestHandler<CancelAppointmentCommand, Result>
{
    private readonly IAppointmentScheduler _scheduler;

    /// <summary>Creates the handler with the shared appointment scheduler.</summary>
    public CancelAppointmentCommandHandler(IAppointmentScheduler scheduler) => _scheduler = scheduler;

    /// <inheritdoc />
    public Task<Result> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken) =>
        _scheduler.CancelAndReleaseSlotAsync(request.AppointmentId, cancellationToken);
}

internal static class AppointmentStatusTransitions
{
    internal static async Task<Result> ApplyAsync(
        IAppointmentRepository repository,
        Guid appointmentId,
        Func<Appointment, Result> transition,
        CancellationToken cancellationToken)
    {
        var appointment = await repository.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment is null)
        {
            return Result.Failure(Veterinary.Domain.ErrorCodes.Appointment.NotFound);
        }

        var result = transition(appointment);
        if (result.IsFailure)
        {
            return result;
        }

        repository.Update(appointment);
        return Result.Success();
    }

    internal static Task ReleaseSlotAsync(
        Appointment appointment,
        IScheduleSlotRepository scheduleSlotRepository,
        CancellationToken cancellationToken) =>
        AppointmentScheduler.ReleaseSlotAsync(appointment, scheduleSlotRepository, cancellationToken);
}
