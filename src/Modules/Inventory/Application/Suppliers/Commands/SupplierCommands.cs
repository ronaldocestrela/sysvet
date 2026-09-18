using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Inventory.Application.Suppliers.Commands;

/// <summary>Registers a supplier.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.ProductsWrite)]
public sealed record RegisterSupplierCommand(
    string LegalName,
    string TradeName,
    string Document,
    string? ContactEmail,
    string? ContactPhone,
    Guid SupplierId = default,
    Guid IdempotencyKey = default) : IIdempotentCommand<Guid>;

/// <summary>Updates supplier data.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.ProductsWrite)]
public sealed record UpdateSupplierCommand(
    Guid SupplierId,
    string LegalName,
    string TradeName,
    string? ContactEmail,
    string? ContactPhone,
    Guid IdempotencyKey = default) : IIdempotentCommand;

/// <summary>Activates or deactivates supplier.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.ProductsWrite)]
public sealed record SetSupplierActiveCommand(Guid SupplierId, bool IsActive, Guid IdempotencyKey = default) : IIdempotentCommand;
