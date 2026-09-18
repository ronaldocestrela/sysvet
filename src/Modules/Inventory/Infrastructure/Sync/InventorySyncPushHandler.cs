using System.Text.Json;
using Core.Application.Messaging;
using Core.Application.Sync;
using Core.Domain;
using Inventory.Application.ProductLots.Commands;
using Inventory.Application.Products.Commands;
using Inventory.Application.StockMovements.Commands;
using Inventory.Application.Suppliers.Commands;
using MediatR;

namespace Inventory.Infrastructure.Sync;

/// <summary>
/// Maps inventory outbox payloads to MediatR commands during sync push.
/// </summary>
public sealed class InventorySyncPushHandler : ISyncPushHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IMediator _mediator;

    /// <summary>Creates the handler.</summary>
    public InventorySyncPushHandler(IMediator mediator) => _mediator = mediator;

    /// <inheritdoc />
    public object? TryMapCommand(SyncOutboxMessageDto message) =>
        message.Type switch
        {
            nameof(RegisterProductCommand) => WithIdempotency(JsonSerializer.Deserialize<RegisterProductCommand>(message.Payload, JsonOptions), message.Id),
            nameof(UpdateProductCommand) => WithIdempotency(JsonSerializer.Deserialize<UpdateProductCommand>(message.Payload, JsonOptions), message.Id),
            nameof(SetProductActiveCommand) => WithIdempotency(JsonSerializer.Deserialize<SetProductActiveCommand>(message.Payload, JsonOptions), message.Id),
            nameof(RegisterSupplierCommand) => WithIdempotency(JsonSerializer.Deserialize<RegisterSupplierCommand>(message.Payload, JsonOptions), message.Id),
            nameof(UpdateSupplierCommand) => WithIdempotency(JsonSerializer.Deserialize<UpdateSupplierCommand>(message.Payload, JsonOptions), message.Id),
            nameof(SetSupplierActiveCommand) => WithIdempotency(JsonSerializer.Deserialize<SetSupplierActiveCommand>(message.Payload, JsonOptions), message.Id),
            nameof(RegisterProductLotCommand) => WithIdempotency(JsonSerializer.Deserialize<RegisterProductLotCommand>(message.Payload, JsonOptions), message.Id),
            nameof(UpdateProductLotCommand) => WithIdempotency(JsonSerializer.Deserialize<UpdateProductLotCommand>(message.Payload, JsonOptions), message.Id),
            nameof(SetProductLotActiveCommand) => WithIdempotency(JsonSerializer.Deserialize<SetProductLotActiveCommand>(message.Payload, JsonOptions), message.Id),
            nameof(RegisterStockMovementCommand) => WithIdempotency(JsonSerializer.Deserialize<RegisterStockMovementCommand>(message.Payload, JsonOptions), message.Id),
            nameof(TransferStockCommand) => WithIdempotency(JsonSerializer.Deserialize<TransferStockCommand>(message.Payload, JsonOptions), message.Id),
            _ => null
        };

    /// <inheritdoc />
    public async Task<Result> DispatchAsync(object command, CancellationToken cancellationToken)
    {
        return command switch
        {
            ICommand<Guid> cmdGuid => Map(await _mediator.Send(cmdGuid, cancellationToken)),
            ICommand cmd => await _mediator.Send(cmd, cancellationToken),
            _ => Result.Failure(new Error("Sync.InvalidCommand", "Unsupported inventory sync command."))
        };
    }

    private static Result Map(Result<Guid> result) =>
        result.IsSuccess ? Result.Success() : Result.Failure(result.Error);

    private static T? WithIdempotency<T>(T? command, Guid idempotencyKey) where T : class
    {
        if (command is null)
        {
            return null;
        }

        var prop = typeof(T).GetProperty("IdempotencyKey");
        if (prop is null || prop.PropertyType != typeof(Guid))
        {
            return command;
        }

        prop.SetValue(command, idempotencyKey);
        return command;
    }
}
