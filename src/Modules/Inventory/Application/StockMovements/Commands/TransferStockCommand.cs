using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Inventory.Application.StockMovements.Commands;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.StockWrite)]
public sealed record TransferStockCommand(
    Guid ProductId,
    Guid SourceLotId,
    Guid DestinationLotId,
    decimal Quantity,
    string Reason,
    Guid CorrelationId = default,
    Guid IdempotencyKey = default) : IIdempotentCommand<Guid>;
