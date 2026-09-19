using Inventory.Application.InventoryCounts.Dtos;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;

namespace Inventory.Application.InventoryCounts;

/// <summary>
/// Maps inventory count aggregates to API DTOs with blind-count rules.
/// </summary>
public static class InventoryCountMapper
{
    /// <summary>
    /// Builds detail DTO; hides expected/variance while session is in progress.
    /// </summary>
    public static InventoryCountDetailDto MapDetail(
        InventoryCount session,
        IReadOnlyDictionary<Guid, (string Name, string Sku)> productLabels,
        IReadOnlyDictionary<Guid, string> lotNumbers)
    {
        var blind = session.Status == InventoryCountStatus.InProgress;
        return new InventoryCountDetailDto
        {
            Id = session.Id,
            Code = session.Code,
            Status = session.Status,
            UpdatedAt = session.UpdatedAt,
            Lines = session.Lines.Select(line =>
            {
                string? lotNumber = null;
                if (line.ProductLotId is not null)
                {
                    lotNumbers.TryGetValue(line.ProductLotId.Value, out lotNumber);
                }

                productLabels.TryGetValue(line.ProductId, out var label);

                return new InventoryCountLineDto
                {
                    LineId = line.Id,
                    ProductId = line.ProductId,
                    ProductName = label.Name ?? string.Empty,
                    Sku = label.Sku ?? string.Empty,
                    ProductLotId = line.ProductLotId,
                    LotNumber = lotNumber,
                    CountedQuantity = line.CountedQuantity,
                    ExpectedQuantity = blind ? null : line.ExpectedQuantity,
                    Variance = blind ? null : line.Variance,
                    StockMovementId = line.StockMovementId
                };
            }).ToList()
        };
    }
}
