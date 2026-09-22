using Core.Domain;

namespace Commerce.Domain.ValueObjects;

/// <summary>
/// BRL sale price for commerce offers and order line snapshots.
/// </summary>
public sealed record Money
{
    /// <summary>Amount in BRL.</summary>
    public decimal Amount { get; }

    private Money(decimal amount) => Amount = amount;

    /// <summary>Zero amount.</summary>
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

    /// <summary>Trusted rehydration from persistence.</summary>
    public static Money FromPersisted(decimal amount) => new(amount);
}
