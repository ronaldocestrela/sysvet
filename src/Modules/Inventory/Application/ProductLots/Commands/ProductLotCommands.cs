using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Inventory.Application.ProductLots.Commands;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.ProductsWrite)]
public sealed record RegisterProductLotCommand(
    Guid ProductId,
    string LotNumber,
    DateTimeOffset? ExpirationDate,
    decimal UnitCost,
    decimal InitialQuantity,
    Guid LotId = default,
    Guid IdempotencyKey = default) : IIdempotentCommand<Guid>;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.ProductsWrite)]
public sealed record UpdateProductLotCommand(
    Guid LotId,
    DateTimeOffset? ExpirationDate,
    decimal UnitCost,
    Guid IdempotencyKey = default) : IIdempotentCommand;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.ProductsWrite)]
public sealed record SetProductLotActiveCommand(Guid LotId, bool IsActive, Guid IdempotencyKey = default) : IIdempotentCommand;
