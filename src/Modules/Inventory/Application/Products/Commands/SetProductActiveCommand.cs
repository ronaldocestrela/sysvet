using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Inventory.Application.Products.Commands;

/// <summary>Activates or deactivates a product.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.ProductsWrite)]
public sealed record SetProductActiveCommand(Guid ProductId, bool IsActive, Guid IdempotencyKey = default) : IIdempotentCommand;
