using Core.Domain;
using Veterinary.Domain;

namespace Veterinary.Domain.Entities;

/// <summary>Physical bed within a ward unit catalog.</summary>
public sealed class Bed : Entity
{
    /// <summary>Parent ward unit.</summary>
    public Guid WardUnitId { get; private set; }

    /// <summary>Short code shown on the execution map (e.g. A1).</summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>Display order within the ward.</summary>
    public int SortOrder { get; private set; }

    /// <summary>Inactive beds are hidden from admission pick lists.</summary>
    public bool IsActive { get; private set; }

    private Bed() { }

    private Bed(Guid id, Guid wardUnitId, string code, int sortOrder)
        : base(id)
    {
        WardUnitId = wardUnitId;
        Code = code;
        SortOrder = sortOrder;
        IsActive = true;
    }

    /// <summary>Creates a bed line for a ward unit.</summary>
    public static Result<Bed> Create(Guid id, Guid wardUnitId, string code, int sortOrder)
    {
        if (wardUnitId == Guid.Empty)
        {
            return Result.Failure<Bed>(ErrorCodes.WardUnit.InvalidIdentifiers);
        }

        if (string.IsNullOrWhiteSpace(code) || code.Length > 20)
        {
            return Result.Failure<Bed>(ErrorCodes.WardUnit.InvalidBedCode);
        }

        return Result.Success(new Bed(id, wardUnitId, code.Trim(), sortOrder));
    }

    /// <summary>Updates bed metadata during catalog replace.</summary>
    internal Result Update(string code, int sortOrder, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length > 20)
        {
            return Result.Failure(ErrorCodes.WardUnit.InvalidBedCode);
        }

        Code = code.Trim();
        SortOrder = sortOrder;
        IsActive = isActive;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Rehydrates from sync without validation.</summary>
    internal static Bed RestoreFromSync(Guid id, Guid wardUnitId, string code, int sortOrder, bool isActive, DateTimeOffset updatedAt)
    {
        var bed = new Bed(id, wardUnitId, code, sortOrder) { IsActive = isActive, UpdatedAt = updatedAt };
        return bed;
    }
}
