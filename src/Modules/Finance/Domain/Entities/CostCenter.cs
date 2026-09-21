using Core.Domain;

namespace Finance.Domain.Entities;

/// <summary>
/// Optional cost center for financial titles.
/// </summary>
public sealed class CostCenter : AggregateRoot
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private CostCenter() { }

    /// <summary>Creates a cost center.</summary>
    public static Result<CostCenter> Create(string code, string name, Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure<CostCenter>(ErrorCodes.CostCenter.InvalidCode);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<CostCenter>(ErrorCodes.CostCenter.InvalidName);
        }

        return Result.Success(new CostCenter
        {
            Id = id ?? Guid.NewGuid(),
            Code = code.Trim(),
            Name = name.Trim(),
            IsActive = true
        });
    }

    /// <summary>Updates name and active flag.</summary>
    public Result Update(string name, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(ErrorCodes.CostCenter.InvalidName);
        }

        Name = name.Trim();
        IsActive = isActive;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Rehydrates from sync.</summary>
    public static CostCenter RestoreFromSync(Guid id, string code, string name, bool isActive, DateTimeOffset updatedAt)
    {
        return new CostCenter
        {
            Id = id,
            Code = code,
            Name = name,
            IsActive = isActive,
            UpdatedAt = updatedAt
        };
    }
}
