using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Inventory.Domain.Entities;

namespace Inventory.Application.StockMovements.Commands;

[AuthorizeRequest(AuthorizationPolicies.Authenticated)]
public record RegisterStockMovementCommand(
    Guid ProductId,
    MovementType Type,
    decimal Quantity,
    string? BatchNumber,
    DateTimeOffset? ExpirationDate,
    string Reason
) : ICommand<Guid>;
