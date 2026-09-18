using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Inventory.Application.StockMovements.Dtos;
using Inventory.Domain.Entities;
using Inventory.Domain.Services;

namespace Inventory.Application.StockMovements.Queries;

[AuthorizeRequest(AuthorizationPolicies.Authenticated, Permissions.StockRead)]
public sealed record ListStockMovementsQuery(
    Guid? ProductId = null,
    int Page = 1,
    int PageSize = 50) : IQuery<IReadOnlyList<StockMovementListItemDto>>;

[AuthorizeRequest(AuthorizationPolicies.Authenticated, Permissions.StockRead)]
public sealed record GetProductKardexQuery(
    Guid ProductId,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    Guid? ProductLotId = null) : IQuery<IReadOnlyList<ProductKardexLineDto>>;

[AuthorizeRequest(AuthorizationPolicies.Authenticated, Permissions.StockRead)]
public sealed record ListStockAlertsQuery(
    StockAlertKind? Kind = null,
    int HorizonDays = 30,
    int Page = 1,
    int PageSize = 50) : IQuery<IReadOnlyList<StockAlertDto>>;
