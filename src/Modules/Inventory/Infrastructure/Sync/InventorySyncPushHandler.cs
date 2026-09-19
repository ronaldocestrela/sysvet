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
            nameof(RegisterStockLossCommand) => WithIdempotency(JsonSerializer.Deserialize<RegisterStockLossCommand>(message.Payload, JsonOptions), message.Id),
            nameof(FractionatePackageCommand) => WithIdempotency(JsonSerializer.Deserialize<FractionatePackageCommand>(message.Payload, JsonOptions), message.Id),
            nameof(RegisterSupplierReturnCommand) => WithIdempotency(JsonSerializer.Deserialize<RegisterSupplierReturnCommand>(message.Payload, JsonOptions), message.Id),
            _ => null
        };

    /// <inheritdoc />
    public async Task<Result> DispatchAsync(object command, CancellationToken cancellationToken)
    {
        return command switch
        {
            RegisterProductCommand registerProduct => Map(await _mediator.Send(registerProduct, cancellationToken)),
            UpdateProductCommand updateProduct => await _mediator.Send(updateProduct, cancellationToken),
            SetProductActiveCommand setProductActive => await _mediator.Send(setProductActive, cancellationToken),
            RegisterSupplierCommand registerSupplier => Map(await _mediator.Send(registerSupplier, cancellationToken)),
            UpdateSupplierCommand updateSupplier => await _mediator.Send(updateSupplier, cancellationToken),
            SetSupplierActiveCommand setSupplierActive => await _mediator.Send(setSupplierActive, cancellationToken),
            RegisterProductLotCommand registerLot => Map(await _mediator.Send(registerLot, cancellationToken)),
            UpdateProductLotCommand updateLot => await _mediator.Send(updateLot, cancellationToken),
            SetProductLotActiveCommand setLotActive => await _mediator.Send(setLotActive, cancellationToken),
            RegisterStockMovementCommand registerMovement => Map(await _mediator.Send(registerMovement, cancellationToken)),
            TransferStockCommand transfer => await _mediator.Send(transfer, cancellationToken),
            RegisterStockLossCommand loss => await _mediator.Send(loss, cancellationToken),
            FractionatePackageCommand fractionate => await _mediator.Send(fractionate, cancellationToken),
            RegisterSupplierReturnCommand supplierReturn => await _mediator.Send(supplierReturn, cancellationToken),
            _ => Result.Failure(new Error("Sync.HandlerMismatch", "Not an inventory sync command."))
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
