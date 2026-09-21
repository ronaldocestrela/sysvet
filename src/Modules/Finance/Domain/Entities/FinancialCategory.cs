using Core.Domain;
using Finance.Domain.Enums;

namespace Finance.Domain.Entities;

/// <summary>
/// Chart category for classifying payables and receivables.
/// </summary>
public sealed class FinancialCategory : AggregateRoot
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public CategoryDirection Direction { get; private set; }
    public bool IsSystem { get; private set; }
    public bool IsActive { get; private set; } = true;

    private FinancialCategory() { }

    /// <summary>Creates a tenant-defined category.</summary>
    public static Result<FinancialCategory> Create(
        string code,
        string name,
        CategoryDirection direction,
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure<FinancialCategory>(ErrorCodes.Category.InvalidCode);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<FinancialCategory>(ErrorCodes.Category.InvalidName);
        }

        return Result.Success(new FinancialCategory
        {
            Id = id ?? Guid.NewGuid(),
            Code = code.Trim(),
            Name = name.Trim(),
            Direction = direction,
            IsSystem = false,
            IsActive = true
        });
    }

    /// <summary>Creates or restores a system category used by integration handlers.</summary>
    public static FinancialCategory CreateSystem(
        string code,
        string name,
        CategoryDirection direction,
        Guid? id = null)
    {
        return new FinancialCategory
        {
            Id = id ?? Guid.NewGuid(),
            Code = code.Trim(),
            Name = name.Trim(),
            Direction = direction,
            IsSystem = true,
            IsActive = true
        };
    }

    /// <summary>Updates display name and direction for non-system categories.</summary>
    public Result Update(string name, CategoryDirection direction, bool isActive)
    {
        if (IsSystem)
        {
            return Result.Failure(ErrorCodes.Category.SystemImmutable);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(ErrorCodes.Category.InvalidName);
        }

        Name = name.Trim();
        Direction = direction;
        IsActive = isActive;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Rehydrates from sync.</summary>
    public static FinancialCategory RestoreFromSync(
        Guid id,
        string code,
        string name,
        CategoryDirection direction,
        bool isSystem,
        bool isActive,
        DateTimeOffset updatedAt)
    {
        return new FinancialCategory
        {
            Id = id,
            Code = code,
            Name = name,
            Direction = direction,
            IsSystem = isSystem,
            IsActive = isActive,
            UpdatedAt = updatedAt
        };
    }
}
