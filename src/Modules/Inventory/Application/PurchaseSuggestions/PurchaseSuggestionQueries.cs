using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Inventory.Application.Labels;

namespace Inventory.Application.PurchaseSuggestions;

[AuthorizeRequest(AuthorizationPolicies.Authenticated, Permissions.StockRead)]
public sealed record ListPurchaseSuggestionsQuery(Guid? SupplierId = null) : IQuery<IReadOnlyList<PurchaseSuggestionGroupDto>>;

[AuthorizeRequest(AuthorizationPolicies.Authenticated, Permissions.StockRead)]
public sealed record ExportPurchaseSuggestionsQuery(Guid? SupplierId = null) : IQuery<LabelFileDto>;
