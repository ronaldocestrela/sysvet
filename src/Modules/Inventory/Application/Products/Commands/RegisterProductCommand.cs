using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;

namespace Inventory.Application.Products.Commands;

[AuthorizeRequest(AuthorizationPolicies.Authenticated)]
public record RegisterProductCommand(
    string Name,
    string Description,
    string Barcode,
    string UnitOfMeasure,
    decimal ReorderLevel
) : ICommand<Guid>;
