using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Application.Messaging;
using Core.Application.Sync;
using Core.Domain;
using Fiscal.Application.Documents;
using MediatR;

namespace Fiscal.Infrastructure.Sync;

/// <summary>Maps fiscal outbox payloads during sync push (ADR-036).</summary>
public sealed class FiscalSyncPushHandler : ISyncPushHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IMediator _mediator;

    public FiscalSyncPushHandler(IMediator mediator) => _mediator = mediator;

    public object? TryMapCommand(SyncOutboxMessageDto message) =>
        message.Type switch
        {
            nameof(TransmitNfceCommand) => WithIdempotency(
                JsonSerializer.Deserialize<TransmitNfceCommand>(message.Payload, JsonOptions),
                message.Id),
            _ => null
        };

    public async Task<Result> DispatchAsync(object command, CancellationToken cancellationToken) =>
        command switch
        {
            TransmitNfceCommand transmit => Map(await _mediator.Send(transmit, cancellationToken)),
            _ => Result.Failure(new Error("Sync.HandlerMismatch", "Not a fiscal sync command."))
        };

    private static Result Map(Result<bool> result) =>
        result.IsSuccess ? Result.Success() : Result.Failure(result.Error);

    private static TransmitNfceCommand? WithIdempotency(TransmitNfceCommand? command, Guid idempotencyKey)
    {
        if (command is null)
        {
            return null;
        }

        return command with { IdempotencyKey = idempotencyKey };
    }
}
