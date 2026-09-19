using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Inventory.Domain.Entities;

namespace Inventory.Application.StockMovements.Commands;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.StockWrite)]
public sealed record RegisterStockMovementCommand(
    Guid ProductId,
    MovementType Type,
    decimal Quantity,
    string Reason,
    Guid? ProductLotId = null,
    AdjustmentDirection? AdjustmentDirection = null,
    string? BatchNumber = null,
    DateTimeOffset? ExpirationDate = null,
    Guid? CorrelationId = null,
    Guid MovementId = default,
    Guid IdempotencyKey = default) : IIdempotentCommand<Guid>;
