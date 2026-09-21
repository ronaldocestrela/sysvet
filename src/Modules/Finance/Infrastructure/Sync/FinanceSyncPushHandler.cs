using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Application.Sync;
using Core.Domain;
using Finance.Application.Categories;
using Finance.Application.CostCenters;
using Finance.Application.Titles.Commands;
using MediatR;

namespace Finance.Infrastructure.Sync;

/// <summary>
/// Maps finance outbox payloads to MediatR commands during sync push.
/// </summary>
public sealed class FinanceSyncPushHandler : ISyncPushHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IMediator _mediator;

    public FinanceSyncPushHandler(IMediator mediator) => _mediator = mediator;

    public object? TryMapCommand(SyncOutboxMessageDto message) =>
        message.Type switch
        {
            nameof(CreateManualFinancialTitleCommand) => JsonSerializer.Deserialize<CreateManualFinancialTitleCommand>(message.Payload, JsonOptions),
            nameof(SettleFinancialTitleCommand) => JsonSerializer.Deserialize<SettleFinancialTitleCommand>(message.Payload, JsonOptions),
            nameof(CancelFinancialTitleCommand) => JsonSerializer.Deserialize<CancelFinancialTitleCommand>(message.Payload, JsonOptions),
            nameof(UpsertFinancialCategoryCommand) => JsonSerializer.Deserialize<UpsertFinancialCategoryCommand>(message.Payload, JsonOptions),
            nameof(UpsertCostCenterCommand) => JsonSerializer.Deserialize<UpsertCostCenterCommand>(message.Payload, JsonOptions),
            _ => null
        };

    public async Task<Result> DispatchAsync(object command, CancellationToken cancellationToken)
    {
        return command switch
        {
            CreateManualFinancialTitleCommand c => Map(await _mediator.Send(c, cancellationToken)),
            SettleFinancialTitleCommand c => await _mediator.Send(c, cancellationToken),
            CancelFinancialTitleCommand c => await _mediator.Send(c, cancellationToken),
            UpsertFinancialCategoryCommand c => Map(await _mediator.Send(c, cancellationToken)),
            UpsertCostCenterCommand c => Map(await _mediator.Send(c, cancellationToken)),
            _ => Result.Failure(new Error("Sync.HandlerMismatch", "Not a finance sync command."))
        };
    }

    private static Result Map(Result<Guid> result) =>
        result.IsSuccess ? Result.Success() : Result.Failure(result.Error);
}
