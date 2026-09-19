using Inventory.Domain.Enums;

namespace Inventory.Application.InventoryCounts.Dtos;

/// <summary>Header for inventory count list views.</summary>
public sealed class InventoryCountListItemDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public InventoryCountStatus Status { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public int LineCount { get; init; }
}

/// <summary>Session detail with lines; expected/variance only after submit.</summary>
public sealed class InventoryCountDetailDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public InventoryCountStatus Status { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public IReadOnlyList<InventoryCountLineDto> Lines { get; init; } = Array.Empty<InventoryCountLineDto>();
}

/// <summary>One counted line with optional variance fields.</summary>
public sealed class InventoryCountLineDto
{
    public Guid LineId { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public Guid? ProductLotId { get; init; }
    public string? LotNumber { get; init; }
    public decimal CountedQuantity { get; init; }
    public decimal? ExpectedQuantity { get; init; }
    public decimal? Variance { get; init; }
    public Guid? StockMovementId { get; init; }
}
