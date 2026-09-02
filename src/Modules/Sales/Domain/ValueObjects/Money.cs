using Core.Domain;
using System;

namespace Sales.Domain.ValueObjects;

/// <summary>
/// Value Object representing a monetary amount in BRL.
/// </summary>
public record Money
{
    public decimal Amount { get; }
    public string Currency { get; } = "BRL";

    private Money(decimal amount, string currency = "BRL")
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Zero => new(0m);

    public static Result<Money> Create(decimal amount)
    {
        if (amount < 0)
        {
            return Result.Failure<Money>(new Error("Money.InvalidAmount", "O valor não pode ser negativo."));
        }
        return Result.Success(new Money(amount));
    }

    public static Money CreateUnsafe(decimal amount) => new(amount);

    public static Money operator +(Money left, Money right) => new(left.Amount + right.Amount);
    
    public static Money operator -(Money left, Money right)
    {
        if (left.Amount < right.Amount)
        {
            throw new InvalidOperationException("O resultado da subtração não pode ser negativo para este Value Object.");
        }
        return new Money(left.Amount - right.Amount);
    }
    
    public override string ToString() => $"{Currency} {Amount:N2}";
}
