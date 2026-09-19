using Core.Domain;
using Inventory.Domain.Enums;

namespace Inventory.Domain.Entities;

/// <summary>
/// Physical inventory session from blind count through approved stock adjustments.
/// </summary>
public class InventoryCount : AggregateRoot
{
    public string Code { get; private set; } = string.Empty;
    public InventoryCountStatus Status { get; private set; }

    /// <summary>Count lines (EF navigation).</summary>
    public List<InventoryCountLine> Lines { get; private set; } = new();

    private InventoryCount() { }

    /// <summary>
    /// Starts a new blind count session.
    /// </summary>
    public static Result<InventoryCount> Start(string code, Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure<InventoryCount>(ErrorCodes.InventoryCount.InvalidCode);
        }

        var normalized = code.Trim();
        if (normalized.Length is < 3 or > 30)
        {
            return Result.Failure<InventoryCount>(ErrorCodes.InventoryCount.InvalidCode);
        }

        return Result.Success(new InventoryCount
        {
            Id = id ?? Guid.NewGuid(),
            Code = normalized,
            Status = InventoryCountStatus.InProgress
        });
    }

    /// <summary>
    /// Adds quantity to an existing line or creates one for the product/lot key.
    /// </summary>
    public Result<Guid> AddOrIncrementLine(Guid productId, Guid? productLotId, decimal quantityToAdd)
    {
        if (Status != InventoryCountStatus.InProgress)
        {
            return Result.Failure<Guid>(ErrorCodes.InventoryCount.InvalidStatus);
        }

        if (quantityToAdd <= 0)
        {
            return Result.Failure<Guid>(ErrorCodes.InventoryCount.InvalidQuantity);
        }

        var existing = FindLine(productId, productLotId);
        if (existing is not null)
        {
            existing.IncrementCounted(quantityToAdd);
            UpdatedAt = DateTimeOffset.UtcNow;
            return Result.Success(existing.Id);
        }

        var line = InventoryCountLine.Create(Id, productId, productLotId, quantityToAdd);
        Lines.Add(line);
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success(line.Id);
    }

    /// <summary>
    /// Sets absolute counted quantity for a line while still in progress.
    /// </summary>
    public Result UpdateLineQuantity(Guid lineId, decimal countedQuantity)
    {
        if (Status != InventoryCountStatus.InProgress)
        {
            return Result.Failure(ErrorCodes.InventoryCount.InvalidStatus);
        }

        if (countedQuantity <= 0)
        {
            return Result.Failure(ErrorCodes.InventoryCount.InvalidQuantity);
        }

        var line = Lines.FirstOrDefault(l => l.Id == lineId);
        if (line is null)
        {
            return Result.Failure(ErrorCodes.InventoryCount.LineNotFound);
        }

        line.SetCountedQuantity(countedQuantity);
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Removes a line during blind count.
    /// </summary>
    public Result RemoveLine(Guid lineId)
    {
        if (Status != InventoryCountStatus.InProgress)
        {
            return Result.Failure(ErrorCodes.InventoryCount.InvalidStatus);
        }

        var line = Lines.FirstOrDefault(l => l.Id == lineId);
        if (line is null)
        {
            return Result.Failure(ErrorCodes.InventoryCount.LineNotFound);
        }

        Lines.Remove(line);
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Freezes expected on-hand and computes variances for review.
    /// </summary>
    public Result Submit(IReadOnlyDictionary<Guid, decimal> expectedByLineId)
    {
        if (Status != InventoryCountStatus.InProgress)
        {
            return Result.Failure(ErrorCodes.InventoryCount.InvalidStatus);
        }

        if (Lines.Count == 0)
        {
            return Result.Failure(ErrorCodes.InventoryCount.EmptyLines);
        }

        foreach (var line in Lines)
        {
            if (!expectedByLineId.TryGetValue(line.Id, out var expected))
            {
                return Result.Failure(ErrorCodes.InventoryCount.LineNotFound);
            }

            line.ApplySubmitSnapshot(expected);
        }

        Status = InventoryCountStatus.Submitted;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Marks the session approved after stock movements were applied.
    /// </summary>
    public Result Approve()
    {
        if (Status != InventoryCountStatus.Submitted)
        {
            return Result.Failure(ErrorCodes.InventoryCount.InvalidStatus);
        }

        if (Lines.Count == 0)
        {
            return Result.Failure(ErrorCodes.InventoryCount.EmptyLines);
        }

        if (Lines.Any(l => l.ExpectedQuantity is null))
        {
            return Result.Failure(ErrorCodes.InventoryCount.NotSubmitted);
        }

        Status = InventoryCountStatus.Approved;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Cancels the session without applying stock changes.
    /// </summary>
    public Result Cancel()
    {
        if (Status is InventoryCountStatus.Approved or InventoryCountStatus.Cancelled)
        {
            return Result.Failure(ErrorCodes.InventoryCount.InvalidStatus);
        }

        Status = InventoryCountStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    private InventoryCountLine? FindLine(Guid productId, Guid? productLotId) =>
        Lines.FirstOrDefault(l => l.ProductId == productId && l.ProductLotId == productLotId);
}
