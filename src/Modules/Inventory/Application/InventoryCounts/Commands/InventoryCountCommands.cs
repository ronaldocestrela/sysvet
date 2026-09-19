using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Inventory.Application.InventoryCounts.Commands;

/// <summary>Starts a new blind inventory count session.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.StockWrite)]
public sealed record StartInventoryCountCommand(Guid IdempotencyKey = default) : IIdempotentCommand<Guid>;

/// <summary>Adds or increments a count line by barcode or product/lot.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.StockWrite)]
public sealed record AddInventoryCountLineCommand(
    Guid InventoryCountId,
    string? Barcode,
    Guid? ProductId,
    Guid? ProductLotId,
    decimal QuantityToAdd = 1m,
    Guid IdempotencyKey = default) : IIdempotentCommand<Guid>;

/// <summary>Sets absolute counted quantity for a line.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.StockWrite)]
public sealed record UpdateInventoryCountLineCommand(
    Guid InventoryCountId,
    Guid LineId,
    decimal CountedQuantity,
    Guid IdempotencyKey = default) : IIdempotentCommand;

/// <summary>Removes a line during blind count.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.StockWrite)]
public sealed record RemoveInventoryCountLineCommand(
    Guid InventoryCountId,
    Guid LineId,
    Guid IdempotencyKey = default) : IIdempotentCommand;

/// <summary>Submits session and reveals variances.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.StockWrite)]
public sealed record SubmitInventoryCountCommand(
    Guid InventoryCountId,
    Guid IdempotencyKey = default) : IIdempotentCommand;

/// <summary>Applies stock adjustments and closes the session.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.StockWrite)]
public sealed record ApproveInventoryCountCommand(
    Guid InventoryCountId,
    Guid IdempotencyKey = default) : IIdempotentCommand;

/// <summary>Cancels session without stock impact.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.StockWrite)]
public sealed record CancelInventoryCountCommand(
    Guid InventoryCountId,
    Guid IdempotencyKey = default) : IIdempotentCommand;
