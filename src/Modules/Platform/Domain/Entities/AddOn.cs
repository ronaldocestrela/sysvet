using Core.Domain;
using Core.Domain.Entitlements;

namespace Platform.Domain.Entities;

/// <summary>Global add-on product catalog row (schema dbo).</summary>
public sealed class AddOn : Entity
{
    /// <summary>Unique add-on code.</summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>Display name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Monthly list price in BRL.</summary>
    public decimal MonthlyPrice { get; private set; }

    /// <summary>Module unlocked by this add-on.</summary>
    public CommercialModule Module { get; private set; }

#pragma warning disable CS8618
    private AddOn()
    {
    }
#pragma warning restore CS8618

    /// <summary>Creates a catalog add-on.</summary>
    public static Result<AddOn> Create(Guid id, string code, string name, decimal monthlyPrice, CommercialModule module)
    {
        var normalizedCode = code?.Trim() ?? string.Empty;
        if (normalizedCode.Length < 2)
        {
            return Result.Failure<AddOn>(ErrorCodes.Catalog.InvalidCode);
        }

        var displayName = name?.Trim() ?? string.Empty;
        if (displayName.Length < 2)
        {
            return Result.Failure<AddOn>(ErrorCodes.Catalog.InvalidName);
        }

        if (monthlyPrice < 0)
        {
            return Result.Failure<AddOn>(ErrorCodes.Catalog.InvalidPrice);
        }

        return Result.Success(new AddOn
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id,
            Code = normalizedCode,
            Name = displayName,
            MonthlyPrice = monthlyPrice,
            Module = module,
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = new byte[8]
        });
    }
}
