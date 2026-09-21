using Core.Domain;
using Sales.Domain.Enums;
using Sales.Domain.ValueObjects;

namespace Sales.Domain.Entities;

/// <summary>
/// Sangria or supply recorded against an open cash register session.
/// </summary>
public sealed class CashMovement : Entity
{
    public Guid CashRegisterId { get; private set; }
    public CashMovementKind Kind { get; private set; }
    public Money Amount { get; private set; } = Money.Zero;
    public string Reason { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; private set; }

    private CashMovement() { }

    internal static Result<CashMovement> Create(
        Guid cashRegisterId,
        CashMovementKind kind,
        decimal amount,
        string reason,
        DateTimeOffset? occurredAt = null,
        Guid? id = null)
    {
        if (cashRegisterId == Guid.Empty)
        {
            return Result.Failure<CashMovement>(ErrorCodes.CashRegister.InvalidId);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure<CashMovement>(ErrorCodes.CashRegister.InvalidReason);
        }

        var moneyResult = Money.Create(amount);
        if (moneyResult.IsFailure)
        {
            return Result.Failure<CashMovement>(moneyResult.Error);
        }

        if (moneyResult.Value.Amount <= 0)
        {
            return Result.Failure<CashMovement>(ErrorCodes.CashRegister.InvalidMovementAmount);
        }

        return Result.Success(new CashMovement
        {
            Id = id ?? Guid.NewGuid(),
            CashRegisterId = cashRegisterId,
            Kind = kind,
            Amount = moneyResult.Value,
            Reason = reason.Trim(),
            OccurredAt = occurredAt ?? DateTimeOffset.UtcNow
        });
    }

    /// <summary>Rehydrates from persistence or sync.</summary>
    public static CashMovement Restore(
        Guid id,
        Guid cashRegisterId,
        CashMovementKind kind,
        decimal amount,
        string reason,
        DateTimeOffset occurredAt,
        DateTimeOffset updatedAt)
    {
        return new CashMovement
        {
            Id = id,
            CashRegisterId = cashRegisterId,
            Kind = kind,
            Amount = Money.CreateUnsafe(amount),
            Reason = reason,
            OccurredAt = occurredAt,
            UpdatedAt = updatedAt
        };
    }
}
