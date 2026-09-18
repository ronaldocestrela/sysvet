namespace Inventory.Application.StockMovements.Dtos;

/// <summary>Stock movement list row.</summary>
public sealed record StockMovementListItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    Guid? ProductLotId,
    string? LotNumber,
    string Type,
    string? AdjustmentDirection,
    decimal Quantity,
    string Reason,
    DateTimeOffset Date,
    Guid? CorrelationId);

/// <summary>Kardex line with running balance.</summary>
public sealed record ProductKardexLineDto(
    Guid MovementId,
    DateTimeOffset Date,
    string Type,
    string? AdjustmentDirection,
    decimal Quantity,
    decimal RunningBalance,
    string Reason,
    Guid? ProductLotId,
    string? LotNumber,
    Guid? CorrelationId);

/// <summary>Alert list projection.</summary>
public sealed record StockAlertDto(
    string Kind,
    Guid ProductId,
    string ProductName,
    string Sku,
    Guid? ProductLotId,
    string? LotNumber,
    decimal? TotalQuantity,
    decimal? ReorderLevel,
    DateTimeOffset? ExpirationDate);
