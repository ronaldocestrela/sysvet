using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Application.Messaging;
using Core.Application.Sync;
using Core.Domain;
using MediatR;
using Petshop.Application.GroomingAppointments.Commands;
using Petshop.Application.GroomingRecords;
using Petshop.Application.GroomingServices;
using Petshop.Application.GroomingSlots.Commands;

namespace Petshop.Infrastructure.Sync;

/// <summary>
/// Maps Petshop outbox payloads to MediatR commands during sync push.
/// </summary>
public sealed class PetshopSyncPushHandler : ISyncPushHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IMediator _mediator;

    public PetshopSyncPushHandler(IMediator mediator) => _mediator = mediator;

    public object? TryMapCommand(SyncOutboxMessageDto message) =>
        message.Type switch
        {
            nameof(ScheduleGroomingAppointmentCommand) => WithIdempotency(
                JsonSerializer.Deserialize<ScheduleGroomingAppointmentCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(ConfirmGroomingAppointmentCommand) => WithIdempotency(
                JsonSerializer.Deserialize<ConfirmGroomingAppointmentCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(StartGroomingAppointmentCommand) => WithIdempotency(
                JsonSerializer.Deserialize<StartGroomingAppointmentCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(CompleteGroomingAppointmentCommand) => WithIdempotency(
                JsonSerializer.Deserialize<CompleteGroomingAppointmentCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(CancelGroomingAppointmentCommand) => WithIdempotency(
                JsonSerializer.Deserialize<CancelGroomingAppointmentCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(MarkNoShowGroomingAppointmentCommand) => WithIdempotency(
                JsonSerializer.Deserialize<MarkNoShowGroomingAppointmentCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(RescheduleGroomingAppointmentCommand) => WithIdempotency(
                JsonSerializer.Deserialize<RescheduleGroomingAppointmentCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(UpdateGroomingRecordCommand) => WithIdempotency(
                JsonSerializer.Deserialize<UpdateGroomingRecordCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(UpsertGroomingServiceCommand) => WithIdempotency(
                JsonSerializer.Deserialize<UpsertGroomingServiceCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(DefineGroomingDailyAvailabilityCommand) => WithIdempotency(
                JsonSerializer.Deserialize<DefineGroomingDailyAvailabilityCommand>(message.Payload, JsonOptions),
                message.Id),
            _ => null
        };

    public async Task<Result> DispatchAsync(object command, CancellationToken cancellationToken)
    {
        switch (command)
        {
            case ScheduleGroomingAppointmentCommand schedule:
            {
                var result = await _mediator.Send(schedule, cancellationToken);
                return result.IsSuccess ? Result.Success() : Result.Failure(result.Error);
            }
            case ConfirmGroomingAppointmentCommand confirm:
                return await _mediator.Send(confirm, cancellationToken);
            case StartGroomingAppointmentCommand start:
                return await _mediator.Send(start, cancellationToken);
            case CompleteGroomingAppointmentCommand complete:
                return await _mediator.Send(complete, cancellationToken);
            case CancelGroomingAppointmentCommand cancel:
                return await _mediator.Send(cancel, cancellationToken);
            case MarkNoShowGroomingAppointmentCommand noShow:
                return await _mediator.Send(noShow, cancellationToken);
            case RescheduleGroomingAppointmentCommand reschedule:
                return await _mediator.Send(reschedule, cancellationToken);
            case UpdateGroomingRecordCommand updateRecord:
                return await _mediator.Send(updateRecord, cancellationToken);
            case UpsertGroomingServiceCommand upsertService:
            {
                var upsert = await _mediator.Send(upsertService, cancellationToken);
                return upsert.IsSuccess ? Result.Success() : Result.Failure(upsert.Error);
            }
            case DefineGroomingDailyAvailabilityCommand defineSlots:
                return await _mediator.Send(defineSlots, cancellationToken);
            default:
                return Result.Failure(new Error("Sync.UnknownCommand", "Unsupported Petshop sync command."));
        }
    }

    private static ScheduleGroomingAppointmentCommand? WithIdempotency(ScheduleGroomingAppointmentCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static ConfirmGroomingAppointmentCommand? WithIdempotency(ConfirmGroomingAppointmentCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static StartGroomingAppointmentCommand? WithIdempotency(StartGroomingAppointmentCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static CompleteGroomingAppointmentCommand? WithIdempotency(CompleteGroomingAppointmentCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static CancelGroomingAppointmentCommand? WithIdempotency(CancelGroomingAppointmentCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static MarkNoShowGroomingAppointmentCommand? WithIdempotency(MarkNoShowGroomingAppointmentCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static RescheduleGroomingAppointmentCommand? WithIdempotency(RescheduleGroomingAppointmentCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static UpdateGroomingRecordCommand? WithIdempotency(UpdateGroomingRecordCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static UpsertGroomingServiceCommand? WithIdempotency(UpsertGroomingServiceCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static DefineGroomingDailyAvailabilityCommand? WithIdempotency(DefineGroomingDailyAvailabilityCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };
}
