using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Application.Messaging;
using Core.Application.Sync;
using Core.Domain;
using MediatR;
using Sales.Application.CashRegisters.Commands;
using Sales.Application.Orders.Commands;

namespace Sales.Infrastructure.Sync;

/// <summary>
/// Maps sales outbox payloads to MediatR commands during sync push (ADR-026).
/// </summary>
public sealed class SalesSyncPushHandler : ISyncPushHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IMediator _mediator;

    /// <summary>Creates the handler.</summary>
    public SalesSyncPushHandler(IMediator mediator) => _mediator = mediator;

    /// <inheritdoc />
    public object? TryMapCommand(SyncOutboxMessageDto message) =>
        message.Type switch
        {
            nameof(OpenCashRegisterCommand) => WithIdempotency(JsonSerializer.Deserialize<OpenCashRegisterCommand>(message.Payload, JsonOptions), message.Id),
            nameof(CloseCashRegisterCommand) => WithIdempotency(JsonSerializer.Deserialize<CloseCashRegisterCommand>(message.Payload, JsonOptions), message.Id),
            nameof(CreateOrderCommand) => WithIdempotency(JsonSerializer.Deserialize<CreateOrderCommand>(message.Payload, JsonOptions), message.Id),
            nameof(PayOrderCommand) => WithIdempotency(JsonSerializer.Deserialize<PayOrderCommand>(message.Payload, JsonOptions), message.Id),
            nameof(RefundOrderPaymentCommand) => WithIdempotency(JsonSerializer.Deserialize<RefundOrderPaymentCommand>(message.Payload, JsonOptions), message.Id),
            _ => null
        };

    /// <inheritdoc />
    public async Task<Result> DispatchAsync(object command, CancellationToken cancellationToken)
    {
        return command switch
        {
            OpenCashRegisterCommand open => Map(await _mediator.Send(open, cancellationToken)),
            CloseCashRegisterCommand close => await _mediator.Send(close, cancellationToken),
            CreateOrderCommand create => Map(await _mediator.Send(create, cancellationToken)),
            PayOrderCommand pay => Map(await _mediator.Send(pay, cancellationToken)),
            RefundOrderPaymentCommand refund => MapGuid(await _mediator.Send(refund, cancellationToken)),
            _ => Result.Failure(new Error("Sync.HandlerMismatch", "Not a sales sync command."))
        };
    }

    private static Result Map(Result<Guid> result) =>
        result.IsSuccess ? Result.Success() : Result.Failure(result.Error);

    private static Result Map(Result<bool> result) =>
        result.IsSuccess ? Result.Success() : Result.Failure(result.Error);

    private static Result MapGuid(Result<Guid> result) =>
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
