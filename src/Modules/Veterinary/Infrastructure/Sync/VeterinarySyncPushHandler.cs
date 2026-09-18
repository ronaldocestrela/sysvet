using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Application.Messaging;
using Core.Application.Sync;
using Core.Domain;
using MediatR;
using Veterinary.Application.Appointments.Commands;

namespace Veterinary.Infrastructure.Sync;

/// <summary>
/// Maps veterinary outbox payloads to MediatR commands during sync push.
/// </summary>
public sealed class VeterinarySyncPushHandler : ISyncPushHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IMediator _mediator;

    /// <summary>Creates the handler.</summary>
    public VeterinarySyncPushHandler(IMediator mediator) => _mediator = mediator;

    /// <inheritdoc />
    public object? TryMapCommand(SyncOutboxMessageDto message)
    {
        return message.Type switch
        {
            nameof(ScheduleAppointmentCommand) => WithIdempotency(
                JsonSerializer.Deserialize<ScheduleAppointmentCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(ConfirmAppointmentCommand) => WithIdempotency(
                JsonSerializer.Deserialize<ConfirmAppointmentCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(StartAppointmentCommand) => WithIdempotency(
                JsonSerializer.Deserialize<StartAppointmentCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(CompleteAppointmentCommand) => WithIdempotency(
                JsonSerializer.Deserialize<CompleteAppointmentCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(MarkNoShowAppointmentCommand) => WithIdempotency(
                JsonSerializer.Deserialize<MarkNoShowAppointmentCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(CancelAppointmentCommand) => WithIdempotency(
                JsonSerializer.Deserialize<CancelAppointmentCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(RescheduleAppointmentCommand) => WithIdempotency(
                JsonSerializer.Deserialize<RescheduleAppointmentCommand>(message.Payload, JsonOptions),
                message.Id),
            _ => null
        };
    }

    /// <inheritdoc />
    public async Task<Result> DispatchAsync(object command, CancellationToken cancellationToken)
    {
        switch (command)
        {
            case ScheduleAppointmentCommand schedule:
            {
                var result = await _mediator.Send(schedule, cancellationToken);
                return result.IsSuccess ? Result.Success() : Result.Failure(result.Error);
            }
            case ConfirmAppointmentCommand confirm:
                return await _mediator.Send(confirm, cancellationToken);
            case StartAppointmentCommand start:
                return await _mediator.Send(start, cancellationToken);
            case CompleteAppointmentCommand complete:
                return await _mediator.Send(complete, cancellationToken);
            case MarkNoShowAppointmentCommand noShow:
                return await _mediator.Send(noShow, cancellationToken);
            case CancelAppointmentCommand cancel:
                return await _mediator.Send(cancel, cancellationToken);
            case RescheduleAppointmentCommand reschedule:
                return await _mediator.Send(reschedule, cancellationToken);
            default:
                return Result.Failure(new Error("Sync.HandlerMismatch", "Not a veterinary sync command."));
        }
    }

    private static ScheduleAppointmentCommand? WithIdempotency(ScheduleAppointmentCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static ConfirmAppointmentCommand? WithIdempotency(ConfirmAppointmentCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static StartAppointmentCommand? WithIdempotency(StartAppointmentCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static CompleteAppointmentCommand? WithIdempotency(CompleteAppointmentCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static MarkNoShowAppointmentCommand? WithIdempotency(MarkNoShowAppointmentCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static CancelAppointmentCommand? WithIdempotency(CancelAppointmentCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static RescheduleAppointmentCommand? WithIdempotency(RescheduleAppointmentCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };
}
