using Core.Domain;
using Sales.Domain.ValueObjects;
using System;

namespace Sales.Domain.Entities;

public class CashRegister : AggregateRoot
{
    public Guid OpenedByUserId { get; private set; }
    public DateTimeOffset OpenedAt { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }
    public Money OpeningBalance { get; private set; } = Money.Zero;
    public Money ClosingBalance { get; private set; } = Money.Zero;
    public string Status { get; private set; } = "Closed"; // Open, Closed

    private CashRegister() { } // For EF Core

    private CashRegister(Guid openedByUserId, Money openingBalance)
    {
        OpenedByUserId = openedByUserId;
        OpeningBalance = openingBalance;
        OpenedAt = DateTimeOffset.UtcNow;
        Status = "Open";
    }

    public static Result<CashRegister> Open(Guid userId, decimal openingBalance)
    {
        var moneyResult = Money.Create(openingBalance);
        if (!moneyResult.IsSuccess) return Result.Failure<CashRegister>(moneyResult.Error);

        return Result.Success(new CashRegister(userId, moneyResult.Value));
    }

    public Result<bool> Close(decimal actualClosingBalance)
    {
        if (Status == "Closed")
        {
            return Result.Failure<bool>(new Error("CashRegister.AlreadyClosed", "O caixa já está fechado."));
        }

        var moneyResult = Money.Create(actualClosingBalance);
        if (!moneyResult.IsSuccess) return Result.Failure<bool>(moneyResult.Error);

        ClosingBalance = moneyResult.Value;
        ClosedAt = DateTimeOffset.UtcNow;
        Status = "Closed";
        
        return Result.Success(true);
    }
}
