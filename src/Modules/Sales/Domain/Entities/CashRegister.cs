using Core.Domain;
using Sales.Domain.Enums;
using Sales.Domain.ValueObjects;

namespace Sales.Domain.Entities;

/// <summary>
/// Cash register session opened by an operator before PDV sales.
/// </summary>
public class CashRegister : AggregateRoot
{
    public Guid OpenedByUserId { get; private set; }
    public DateTimeOffset OpenedAt { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }
    public Money OpeningBalance { get; private set; } = Money.Zero;
    public Money ClosingBalance { get; private set; } = Money.Zero;
    public CashRegisterStatus Status { get; private set; } = CashRegisterStatus.Closed;

    private CashRegister() { }

    private CashRegister(Guid openedByUserId, Money openingBalance)
    {
        OpenedByUserId = openedByUserId;
        OpeningBalance = openingBalance;
        OpenedAt = DateTimeOffset.UtcNow;
        Status = CashRegisterStatus.Open;
    }

    /// <summary>
    /// Opens a new register session for the operator.
    /// </summary>
    public static Result<CashRegister> Open(Guid userId, decimal openingBalance)
    {
        var moneyResult = Money.Create(openingBalance);
        if (!moneyResult.IsSuccess)
        {
            return Result.Failure<CashRegister>(moneyResult.Error);
        }

        return Result.Success(new CashRegister(userId, moneyResult.Value));
    }

    /// <summary>
    /// Closes the session with the counted cash in drawer.
    /// </summary>
    public Result<bool> Close(decimal actualClosingBalance)
    {
        if (Status == CashRegisterStatus.Closed)
        {
            return Result.Failure<bool>(ErrorCodes.CashRegister.AlreadyClosed);
        }

        var moneyResult = Money.Create(actualClosingBalance);
        if (!moneyResult.IsSuccess)
        {
            return Result.Failure<bool>(moneyResult.Error);
        }

        ClosingBalance = moneyResult.Value;
        ClosedAt = DateTimeOffset.UtcNow;
        Status = CashRegisterStatus.Closed;

        return Result.Success(true);
    }
}
