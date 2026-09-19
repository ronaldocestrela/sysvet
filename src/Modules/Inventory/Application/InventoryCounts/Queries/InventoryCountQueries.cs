using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Inventory.Application.InventoryCounts.Dtos;

namespace Inventory.Application.InventoryCounts.Queries;

/// <summary>Lists recent inventory count sessions.</summary>
[AuthorizeRequest(AuthorizationPolicies.Authenticated, Permissions.StockRead)]
public sealed record ListInventoryCountsQuery(int Take = 50) : IQuery<IReadOnlyList<InventoryCountListItemDto>>;

/// <summary>Loads session detail with blind-count rules.</summary>
[AuthorizeRequest(AuthorizationPolicies.Authenticated, Permissions.StockRead)]
public sealed record GetInventoryCountByIdQuery(Guid InventoryCountId) : IQuery<InventoryCountDetailDto>;
