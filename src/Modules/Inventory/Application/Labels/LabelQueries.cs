using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Inventory.Application.Labels;

/// <summary>Single product label request line.</summary>
public sealed record ProductLabelItemDto(Guid ProductId, int Copies = 1);

/// <summary>Generates product labels in PDF or ZPL.</summary>
[AuthorizeRequest(AuthorizationPolicies.Authenticated, Permissions.ProductsRead)]
public sealed record GenerateProductLabelsQuery(
    IReadOnlyList<ProductLabelItemDto> Items,
    LabelFormat Format) : IQuery<LabelFileDto>;
