using Core.Domain;
using Finance.Domain;

namespace Finance.Domain.ValueObjects;

/// <summary>
/// Monetary amount for finance (BRL).
/// </summary>
public sealed record Money
{
    public decimal Amount { get; }

    private Money(decimal amount) => Amount = amount;

    /// <summary>Zero BRL amount.</summary>
    public static Money Zero => new(0m);

    /// <summary>Creates a non-negative monetary value.</summary>
    public static Result<Money> Create(decimal amount)
    {
        if (amount < 0)
        {
            return Result.Failure<Money>(ErrorCodes.Money.InvalidAmount);
        }

        return Result.Success(new Money(amount));
    }

    /// <summary>Unsafe factory for trusted persisted values.</summary>
    public static Money FromPersisted(decimal amount) => new(amount);
}
