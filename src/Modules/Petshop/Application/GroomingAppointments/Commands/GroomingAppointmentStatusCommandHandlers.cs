using Core.Application.Messaging;
using Core.Application.IntegrationEvents;
using Core.Domain;
using MediatR;
using Petshop.Application.GroomingAppointments;
using Petshop.Domain.Entities;
using Petshop.Domain.Repositories;
namespace Petshop.Application.GroomingAppointments.Commands;

using ErrorCodes = Petshop.Domain.ErrorCodes;

public sealed class ConfirmGroomingAppointmentCommandHandler : IRequestHandler<ConfirmGroomingAppointmentCommand, Result>
{
    private readonly IGroomingAppointmentRepository _repository;

    public ConfirmGroomingAppointmentCommandHandler(IGroomingAppointmentRepository repository) => _repository = repository;

    public Task<Result> Handle(ConfirmGroomingAppointmentCommand request, CancellationToken cancellationToken)
        => GroomingAppointmentStatusTransitions.ApplyAsync(_repository, request.GroomingAppointmentId, a => a.Confirm(), cancellationToken);
}

public sealed class StartGroomingAppointmentCommandHandler : IRequestHandler<StartGroomingAppointmentCommand, Result>
{
    private readonly IGroomingAppointmentRepository _repository;

    public StartGroomingAppointmentCommandHandler(IGroomingAppointmentRepository repository) => _repository = repository;

    public Task<Result> Handle(StartGroomingAppointmentCommand request, CancellationToken cancellationToken)
        => GroomingAppointmentStatusTransitions.ApplyAsync(_repository, request.GroomingAppointmentId, a => a.Start(), cancellationToken);
}

public sealed class MarkGroomingReadyCommandHandler : IRequestHandler<MarkGroomingReadyCommand, Result>
{
    private readonly IGroomingAppointmentRepository _repository;

    public MarkGroomingReadyCommandHandler(IGroomingAppointmentRepository repository) => _repository = repository;

    public Task<Result> Handle(MarkGroomingReadyCommand request, CancellationToken cancellationToken)
        => GroomingAppointmentStatusTransitions.ApplyAsync(_repository, request.GroomingAppointmentId, a => a.MarkReady(), cancellationToken);
}

/// <summary>
/// Completes grooming: debits supplies, optionally consumes prepaid, finalizes the digital record.
/// </summary>
public sealed class CompleteGroomingAppointmentCommandHandler : IRequestHandler<CompleteGroomingAppointmentCommand, Result>
{
    private readonly IGroomingAppointmentRepository _appointmentRepository;
    private readonly IGroomingRecordRepository _recordRepository;
    private readonly IGroomingServiceRepository _serviceRepository;
    private readonly IMediator _mediator;

    public CompleteGroomingAppointmentCommandHandler(
        IGroomingAppointmentRepository appointmentRepository,
        IGroomingRecordRepository recordRepository,
        IGroomingServiceRepository serviceRepository,
        IMediator mediator)
    {
        _appointmentRepository = appointmentRepository;
        _recordRepository = recordRepository;
        _serviceRepository = serviceRepository;
        _mediator = mediator;
    }

    public async Task<Result> Handle(CompleteGroomingAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.GroomingAppointmentId, cancellationToken);
        if (appointment is null)
        {
            return Result.Failure(ErrorCodes.GroomingAppointment.NotFound);
        }

        if (appointment.Status == Domain.Enums.GroomingAppointmentStatus.Completed)
        {
            return Result.Success();
        }

        var complete = appointment.Complete();
        if (complete.IsFailure)
        {
            return complete;
        }

        var record = await _recordRepository.GetByAppointmentIdAsync(appointment.Id, cancellationToken);
        if (record is null)
        {
            return Result.Failure(ErrorCodes.GroomingRecord.NotFound);
        }

        var stockLines = record.SupplyLines
            .Select(l => new ConsumeStockForGroomingLine(l.ProductId, l.Quantity))
            .ToList();

        var stockResult = await _mediator.Send(new ConsumeStockForGroomingRequest(appointment.Id, stockLines), cancellationToken);
        if (stockResult.IsFailure)
        {
            return stockResult;
        }

        var service = await _serviceRepository.GetByIdAsync(appointment.GroomingServiceId, cancellationToken);
        if (service?.PrepaidServiceCode is { Length: > 0 } prepaidCode)
        {
            var prepaidResult = await _mediator.Send(
                new ConsumePrepaidServicePackageRequest(
                    request.GroomingAppointmentId,
                    appointment.PetId,
                    prepaidCode,
                    appointment.Id.ToString()),
                cancellationToken);

            if (prepaidResult.IsFailure &&
                prepaidResult.Error.Code is not ("Package.NotFound" or "Package.InsufficientBalance"))
            {
                return prepaidResult;
            }
        }

        var finalize = record.FinalizeRecord();
        if (finalize.IsFailure)
        {
            return finalize;
        }

        _appointmentRepository.Update(appointment);
        _recordRepository.Update(record);
        return Result.Success();
    }
}

public sealed class CancelGroomingAppointmentCommandHandler : IRequestHandler<CancelGroomingAppointmentCommand, Result>
{
    private readonly IGroomingAppointmentRepository _repository;
    private readonly IGroomingSlotRepository _slotRepository;

    public CancelGroomingAppointmentCommandHandler(
        IGroomingAppointmentRepository repository,
        IGroomingSlotRepository slotRepository)
    {
        _repository = repository;
        _slotRepository = slotRepository;
    }

    public async Task<Result> Handle(CancelGroomingAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _repository.GetByIdAsync(request.GroomingAppointmentId, cancellationToken);
        if (appointment is null)
        {
            return Result.Failure(ErrorCodes.GroomingAppointment.NotFound);
        }

        var result = appointment.Cancel();
        if (result.IsFailure)
        {
            return result;
        }

        await GroomingAppointmentStatusTransitions.ReleaseSlotAsync(appointment, _slotRepository, cancellationToken);
        _repository.Update(appointment);
        return Result.Success();
    }
}

public sealed class MarkNoShowGroomingAppointmentCommandHandler : IRequestHandler<MarkNoShowGroomingAppointmentCommand, Result>
{
    private readonly IGroomingAppointmentRepository _repository;
    private readonly IGroomingSlotRepository _slotRepository;

    public MarkNoShowGroomingAppointmentCommandHandler(
        IGroomingAppointmentRepository repository,
        IGroomingSlotRepository slotRepository)
    {
        _repository = repository;
        _slotRepository = slotRepository;
    }

    public async Task<Result> Handle(MarkNoShowGroomingAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _repository.GetByIdAsync(request.GroomingAppointmentId, cancellationToken);
        if (appointment is null)
        {
            return Result.Failure(ErrorCodes.GroomingAppointment.NotFound);
        }

        var result = appointment.MarkNoShow();
        if (result.IsFailure)
        {
            return result;
        }

        await GroomingAppointmentStatusTransitions.ReleaseSlotAsync(appointment, _slotRepository, cancellationToken);
        _repository.Update(appointment);
        return Result.Success();
    }
}

public sealed class RescheduleGroomingAppointmentCommandHandler : IRequestHandler<RescheduleGroomingAppointmentCommand, Result>
{
    private readonly IGroomingAppointmentRepository _repository;

    public RescheduleGroomingAppointmentCommandHandler(IGroomingAppointmentRepository repository) => _repository = repository;

    public async Task<Result> Handle(RescheduleGroomingAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _repository.GetByIdAsync(request.GroomingAppointmentId, cancellationToken);
        if (appointment is null)
        {
            return Result.Failure(ErrorCodes.GroomingAppointment.NotFound);
        }

        var result = appointment.Reschedule(request.NewDate);
        if (result.IsFailure)
        {
            return result;
        }

        _repository.Update(appointment);
        return Result.Success();
    }
}

internal static class GroomingAppointmentStatusTransitions
{
    internal static async Task<Result> ApplyAsync(
        IGroomingAppointmentRepository repository,
        Guid appointmentId,
        Func<GroomingAppointment, Result> transition,
        CancellationToken cancellationToken)
    {
        var appointment = await repository.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment is null)
        {
            return Result.Failure(ErrorCodes.GroomingAppointment.NotFound);
        }

        var result = transition(appointment);
        if (result.IsFailure)
        {
            return result;
        }

        repository.Update(appointment);
        return Result.Success();
    }

    internal static async Task ReleaseSlotAsync(
        GroomingAppointment appointment,
        IGroomingSlotRepository slotRepository,
        CancellationToken cancellationToken)
    {
        var slots = await slotRepository.GetAllSlotsForDayAsync(appointment.GroomerId, appointment.Date, cancellationToken);
        var slot = GroomingSlotHelper.FindCoveringSlot(slots, appointment.Date, appointment.DurationInMinutes);
        if (slot is not null && !slot.IsAvailable)
        {
            slot.CancelBooking();
            slotRepository.Update(slot);
        }
    }
}
